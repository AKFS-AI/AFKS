using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using AFKS.Shared.Interfaces;
using AFKS.Shared.Events;
using AFKS.Shared.Utils;

namespace AFKS.StageSystem
{
    /// <summary>
    /// 이미지 기반 픽셀 퍼펙트 상호작용 컨트롤러
    /// 파일명 단축: PixelPerfectInteractionController → PixelInteractionController
    /// </summary>
    public class PixelInteractionController : MonoBehaviour, IInteractable, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Header("🎯 이미지 기반 설정")]
        [SerializeField, Tooltip("상호작용할 이미지")] private Image targetImage;
        [SerializeField, Range(0f, 1f), Tooltip("투명도 임계값")] private float alphaThreshold = 0.1f;
        [SerializeField, Tooltip("디버그 모드 (클릭 영역 시각화)")] private bool debugMode = false;
        
        [Header("⚙️ 상호작용 설정")]
        [SerializeField, Tooltip("고유 ID")] private string interactionId;
        [SerializeField, Tooltip("표시될 이름")] private string displayName;
        [SerializeField, TextArea(2, 4), Tooltip("설명")] private string description;
        [SerializeField, Tooltip("필요한 클릭 횟수")] private int requiredClickCount = 1;
        [SerializeField, Tooltip("대상 스테이지 인덱스")] private int targetStageIndex = -1;
        [SerializeField, Tooltip("결과 메시지")] private string resultMessage = "";
        
        [Header("🎨 시각적 피드백")]
        [SerializeField, Tooltip("호버 색상")] private Color hoverColor = new Color(1, 1, 0, 0.5f);
        [SerializeField, Tooltip("호버 크기 배율")] private float hoverScale = 1.1f;
        [SerializeField, Tooltip("클릭 피드백 지속시간")] private float clickFeedbackDuration = 0.2f;
        
        [Header("🔊 오디오")]
        [SerializeField, Tooltip("호버 사운드")] private AudioClip hoverSound;
        [SerializeField, Tooltip("클릭 사운드")] private AudioClip clickSound;
        
        // === PIXEL PERFECT OPTIMIZATION ===
        [Header("🚀 최적화 설정")]
        [SerializeField, Tooltip("미리 계산된 마스크 사용")] private bool usePreCalculatedMask = true;
        [SerializeField, Tooltip("마스크 해상도")] private int maskResolution = 128;
        
        // === RUNTIME EVENTS ===
        public static readonly GameEvent<string> OnPixelInteractionTriggered = new GameEvent<string>();
        public static readonly GameEvent<string> OnPixelInteractionHover = new GameEvent<string>();
        
        // === PROPERTIES ===
        public string InteractionId => interactionId;
        public string DisplayName => displayName;
        public string Description => description;
        public int RequiredClickCount => requiredClickCount;
        public int TargetStageIndex => targetStageIndex;
        public bool IsHovered { get; private set; }
        public bool IsInteracting { get; private set; }
        
        // === PRIVATE FIELDS ===
        private bool[,] clickableMask;
        private int maskWidth, maskHeight;
        private Vector3 originalScale;
        private Color originalColor;
        private Coroutine hoverCoroutine;
        private bool isInitialized = false;
        
        // === CLICK COUNTER SYSTEM ===
        private static Dictionary<string, int> clickCounters = new Dictionary<string, int>();
        private static Dictionary<string, int> requiredClickCounts = new Dictionary<string, int>();
        
        // === INITIALIZATION ===
        
        /// <summary>
        /// 상호작용 컨트롤러 초기화
        /// </summary>
        public void Initialize()
        {
            SetupComponents();
            PreCalculateClickableMask();
            InitializeClickCounter();
            SetupVisualFeedback();
            
            isInitialized = true;
            
            if (debugMode)
            {
                Debug.Log($"[픽셀상호작용] 초기화 완료: {interactionId}");
            }
        }
        
        /// <summary>
        /// 컴포넌트 설정
        /// </summary>
        private void SetupComponents()
        {
            // Image 컴포넌트 확인
            if (targetImage == null)
            {
                targetImage = GetComponent<Image>();
                if (targetImage == null)
                {
                    Debug.LogError($"[픽셀상호작용] {gameObject.name}: 이미지 컴포넌트를 찾을 수 없습니다!");
                    return;
                }
            }
            
            // 원본 값 저장
            originalScale = transform.localScale;
            originalColor = targetImage.color;
            
            // 텍스처 읽기 가능 여부 확인
            if (targetImage.sprite?.texture != null && !targetImage.sprite.texture.isReadable)
            {
                Debug.LogWarning($"[픽셀상호작용] 텍스처 {targetImage.sprite.texture.name}을 읽을 수 없습니다! Import 설정에서 Read/Write를 활성화하세요.");
            }
        }
        
        /// <summary>
        /// 클릭 가능한 영역 마스크 미리 계산
        /// </summary>
        private void PreCalculateClickableMask()
        {
            if (!usePreCalculatedMask || targetImage?.sprite?.texture == null) return;
            
            var sprite = targetImage.sprite;
            var texture = sprite.texture;
            
            if (!texture.isReadable) return;
            
            // 마스크 해상도 설정
            maskWidth = Mathf.Min(maskResolution, (int)sprite.rect.width);
            maskHeight = Mathf.Min(maskResolution, (int)sprite.rect.height);
            
            clickableMask = new bool[maskWidth, maskHeight];
            
            // 마스크 계산
            for (int x = 0; x < maskWidth; x++)
            {
                for (int y = 0; y < maskHeight; y++)
                {
                    float uvX = (float)x / maskWidth;
                    float uvY = (float)y / maskHeight;
                    
                    int pixelX = Mathf.FloorToInt(uvX * sprite.rect.width + sprite.rect.x);
                    int pixelY = Mathf.FloorToInt(uvY * sprite.rect.height + sprite.rect.y);
                    
                    Color pixel = texture.GetPixel(pixelX, pixelY);
                    clickableMask[x, y] = pixel.a > alphaThreshold;
                }
            }
            
            if (debugMode)
            {
                Debug.Log($"[픽셀상호작용] {interactionId}에 대해 {maskWidth}x{maskHeight} 마스크를 미리 계산했습니다.");
            }
        }
        
        /// <summary>
        /// 클릭 카운터 초기화
        /// </summary>
        private void InitializeClickCounter()
        {
            if (string.IsNullOrEmpty(interactionId)) return;
            
            requiredClickCounts[interactionId] = requiredClickCount;
            
            if (!clickCounters.ContainsKey(interactionId))
            {
                clickCounters[interactionId] = 0;
            }
        }
        
        /// <summary>
        /// 시각적 피드백 설정
        /// </summary>
        private void SetupVisualFeedback()
        {
            // 디버그 모드에서 반투명 오버레이 추가
            if (debugMode && targetImage != null)
            {
                Color debugColor = originalColor;
                debugColor.a = 0.3f;
                targetImage.color = debugColor;
            }
        }
        
        // === PIXEL PERFECT CLICK DETECTION ===
        
        /// <summary>
        /// 픽셀 퍼펙트 클릭 감지
        /// </summary>
        private bool IsPixelClickable(Vector2 screenPosition)
        {
            // Screen 좌표를 로컬 좌표로 변환
            Vector2 localPoint;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                targetImage.rectTransform, screenPosition, Camera.main, out localPoint))
            {
                return false;
            }
            
            var rect = targetImage.rectTransform.rect;
            float uvX = (localPoint.x - rect.xMin) / rect.width;
            float uvY = (localPoint.y - rect.yMin) / rect.height;
            
            // 범위 체크
            if (uvX < 0 || uvX > 1 || uvY < 0 || uvY > 1) return false;
            
            if (usePreCalculatedMask && clickableMask != null)
            {
                // 미리 계산된 마스크 사용 (빠름)
                int maskX = Mathf.Clamp(Mathf.FloorToInt(uvX * maskWidth), 0, maskWidth - 1);
                int maskY = Mathf.Clamp(Mathf.FloorToInt(uvY * maskHeight), 0, maskHeight - 1);
                
                return clickableMask[maskX, maskY];
            }
            else
            {
                // 실시간 픽셀 체크 (정확함)
                return CheckPixelAlphaRealtime(uvX, uvY);
            }
        }
        
        /// <summary>
        /// 실시간 픽셀 알파 체크
        /// </summary>
        private bool CheckPixelAlphaRealtime(float uvX, float uvY)
        {
            var sprite = targetImage.sprite;
            if (sprite?.texture == null || !sprite.texture.isReadable) return true;
            
            var texture = sprite.texture;
            int pixelX = Mathf.FloorToInt(uvX * sprite.rect.width + sprite.rect.x);
            int pixelY = Mathf.FloorToInt(uvY * sprite.rect.height + sprite.rect.y);
            
            Color pixelColor = texture.GetPixel(pixelX, pixelY);
            return pixelColor.a > alphaThreshold;
        }
        
        // === IINTERACTABLE IMPLEMENTATION ===
        
        public void OnInteract()
        {
            if (!CanInteract() || IsInteracting) return;
            
            StartCoroutine(ExecuteInteraction());
        }
        
        public bool CanInteract()
        {
            return isInitialized && gameObject.activeInHierarchy;
        }
        
        public InteractionType GetInteractionType()
        {
            return InteractionType.Click;
        }
        
        // === POINTER EVENT HANDLERS ===
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!CanInteract()) return;
            
            if (IsPixelClickable(eventData.position))
            {
                IsHovered = true;
                
                // 호버 사운드 재생
                if (hoverSound != null && AFKS.AudioSystem.AudioManager.Instance != null)
                {
                    AFKS.AudioSystem.AudioManager.Instance.PlaySFX(hoverSound);
                }
                
                // 호버 효과 시작
                if (hoverCoroutine != null)
                    StopCoroutine(hoverCoroutine);
                hoverCoroutine = StartCoroutine(HoverEffect(true));
                
                // 이벤트 발생
                OnPixelInteractionHover.Raise(interactionId);
                
                if (debugMode)
                {
                    Debug.Log($"[픽셀상호작용] 마우스 진입: {interactionId}");
                }
            }
        }
        
        public void OnPointerExit(PointerEventData eventData)
        {
            if (IsHovered)
            {
                IsHovered = false;
                
                // 호버 효과 종료
                if (hoverCoroutine != null)
                    StopCoroutine(hoverCoroutine);
                hoverCoroutine = StartCoroutine(HoverEffect(false));
                
                if (debugMode)
                {
                    Debug.Log($"[픽셀상호작용] 마우스 나감: {interactionId}");
                }
            }
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            if (!CanInteract()) return;
            
            if (IsPixelClickable(eventData.position))
            {
                OnInteract();
            }
        }
        
        // === INTERACTION EXECUTION ===
        
        /// <summary>
        /// 상호작용 실행
        /// </summary>
        private IEnumerator ExecuteInteraction()
        {
            IsInteracting = true;
            
            if (debugMode)
            {
                Debug.Log($"[픽셀상호작용] 상호작용 실행: {interactionId}");
            }
            
            // 클릭 사운드 재생
            if (clickSound != null && AFKS.AudioSystem.AudioManager.Instance != null)
            {
                AFKS.AudioSystem.AudioManager.Instance.PlaySFX(clickSound);
            }
            
            // 클릭 피드백 효과
            yield return StartCoroutine(ClickFeedback());
            
            // 클릭 카운터 처리
            HandleClickCounter();
            
            // 이벤트 발생
            OnPixelInteractionTriggered.Raise(interactionId);
            
            // 짧은 딜레이 후 상호작용 완료
            yield return new WaitForSeconds(0.1f);
            
            IsInteracting = false;
        }
        
        /// <summary>
        /// 클릭 카운터 처리
        /// </summary>
        private void HandleClickCounter()
        {
            if (!requiredClickCounts.ContainsKey(interactionId)) return;
            
            // 클릭 카운트 증가
            clickCounters[interactionId]++;
            int currentCount = clickCounters[interactionId];
            int requiredCount = requiredClickCounts[interactionId];
            
            Debug.Log($"[픽셀상호작용] {interactionId} 클릭: {currentCount}/{requiredCount}");
            
            // 진행 상황 메시지
            if (!string.IsNullOrEmpty(resultMessage))
            {
                string progressMessage = resultMessage.Replace("{0}", currentCount.ToString()).Replace("{1}", requiredCount.ToString());
                Debug.Log($"[픽셀상호작용] {progressMessage}");
            }
            
            // 필요한 클릭 수에 도달했는지 확인
            if (currentCount >= requiredCount)
            {
                Debug.Log($"[픽셀상호작용] {interactionId}: 클릭 조건 완료!");
                
                // 다음 스테이지로 이동
                if (targetStageIndex >= 0 && StageManager.Instance != null)
                {
                    StageManager.Instance.ChangeStage(targetStageIndex);
                }
            }
        }
        
        // === VISUAL EFFECTS ===
        
        /// <summary>
        /// 호버 효과
        /// </summary>
        private IEnumerator HoverEffect(bool isEntering)
        {
            float duration = 0.2f;
            float elapsedTime = 0f;
            
            Vector3 startScale = transform.localScale;
            Vector3 targetScale = isEntering ? originalScale * hoverScale : originalScale;
            
            Color startColor = targetImage.color;
            Color targetColor = isEntering ? hoverColor : originalColor;
            
            // 디버그 모드가 아닐 때는 색상 변경하지 않음
            if (!debugMode)
            {
                targetColor = originalColor;
            }
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;
                
                // 스케일 애니메이션
                transform.localScale = Vector3.Lerp(startScale, targetScale, t);
                
                // 색상 애니메이션 (디버그 모드에서만)
                if (debugMode)
                {
                    targetImage.color = Color.Lerp(startColor, targetColor, t);
                }
                
                yield return null;
            }
            
            transform.localScale = targetScale;
            if (debugMode)
            {
                targetImage.color = targetColor;
            }
        }
        
        /// <summary>
        /// 클릭 피드백 효과
        /// </summary>
        private IEnumerator ClickFeedback()
        {
            float duration = clickFeedbackDuration;
            Vector3 pressedScale = originalScale * 0.95f;
            
            // 눌림 효과
            float elapsedTime = 0f;
            while (elapsedTime < duration / 2)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / (duration / 2);
                
                transform.localScale = Vector3.Lerp(originalScale, pressedScale, t);
                yield return null;
            }
            
            // 복원 효과
            elapsedTime = 0f;
            while (elapsedTime < duration / 2)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / (duration / 2);
                
                transform.localScale = Vector3.Lerp(pressedScale, originalScale, t);
                yield return null;
            }
            
            transform.localScale = originalScale;
        }
        
        // === PUBLIC METHODS ===
        
        /// <summary>
        /// 현재 클릭 카운트 가져오기
        /// </summary>
        public int GetCurrentClickCount()
        {
            if (!clickCounters.ContainsKey(interactionId))
                return 0;
            return clickCounters[interactionId];
        }
        
        /// <summary>
        /// 클릭 카운터 리셋
        /// </summary>
        public void ResetClickCounter()
        {
            if (clickCounters.ContainsKey(interactionId))
            {
                clickCounters[interactionId] = 0;
            }
        }
        
        /// <summary>
        /// 상태 정보 가져오기
        /// </summary>
        public InteractionState GetCurrentState()
        {
            return new InteractionState
            {
                interactionId = this.interactionId,
                currentClickCount = GetCurrentClickCount(),
                isCompleted = GetCurrentClickCount() >= requiredClickCount,
                isVisible = gameObject.activeSelf
            };
        }
        
        /// <summary>
        /// 상태 복원
        /// </summary>
        public void RestoreState(InteractionState state)
        {
            if (state.interactionId != this.interactionId) return;
            
            clickCounters[interactionId] = state.currentClickCount;
            gameObject.SetActive(state.isVisible);
        }
        
        // === UNITY LIFECYCLE ===
        
        private void OnDestroy()
        {
            // 코루틴 정리
            if (hoverCoroutine != null)
            {
                StopCoroutine(hoverCoroutine);
            }
            
            // 해당 인스턴스의 데이터 정리 (메모리 누수 방지)
            if (!string.IsNullOrEmpty(interactionId))
            {
                clickCounters.Remove(interactionId);
                requiredClickCounts.Remove(interactionId);
            }
        }
        
        /// <summary>
        /// 애플리케이션 종료 시 static Dictionary 정리 (메모리 누수 방지)
        /// </summary>
        private void OnApplicationQuit()
        {
            ClearAllStaticData();
        }
        
        /// <summary>
        /// 모든 static 데이터 정리 (메모리 누수 방지)
        /// </summary>
        public static void ClearAllStaticData()
        {
            clickCounters.Clear();
            requiredClickCounts.Clear();
            Debug.Log("[PixelInteractionController] 모든 static 데이터가 정리되었습니다.");
        }
        
        // === GIZMOS ===
        
        private void OnDrawGizmos()
        {
            if (!debugMode || targetImage == null) return;
            
            // 클릭 가능한 영역 시각화
            Gizmos.color = CanInteract() ? Color.green : Color.red;
            
            var rect = targetImage.rectTransform.rect;
            Vector3 worldPos = transform.TransformPoint(rect.center);
            Vector3 size = new Vector3(rect.width, rect.height, 0.1f);
            
            Gizmos.DrawWireCube(worldPos, size);
            
            // ID 표시
            if (Application.isPlaying)
            {
#if UNITY_EDITOR
                UnityEditor.Handles.Label(worldPos, interactionId);
#endif
            }
        }
    }
    
    // === DATA STRUCTURES ===
    
    [System.Serializable]
    public class InteractionState
    {
        public string interactionId;
        public int currentClickCount;
        public bool isCompleted;
        public bool isVisible;
        public Vector3 position;
    }
}