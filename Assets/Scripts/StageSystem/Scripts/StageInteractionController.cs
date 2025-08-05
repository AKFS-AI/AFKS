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
    /// 스테이지 내 상호작용 포인트를 관리하는 컨트롤러
    /// </summary>
    public class StageInteractionController : MonoBehaviour, IInteractable, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Header("⚙️ 설정")]
        [SerializeField, Tooltip("이 상호작용 포인트의 데이터")] private InteractionPoint interactionData;
        [SerializeField, Tooltip("디버그 모드 활성화 (콘솔 로그 출력)")] private bool debugMode = false;
        
        [Header("🖼️ 비주얼 컴포넌트")]
        [SerializeField, Tooltip("상호작용 영역을 표시할 Image 컴포넌트")] private Image visualImage;
        [SerializeField, Tooltip("클릭 이벤트를 처리할 Button 컴포넌트")] private Button interactionButton;
        
        // === RUNTIME EVENTS ===
        public static readonly GameEvent<string> OnInteractionTriggered = new GameEvent<string>();
        public static readonly GameEvent<string> OnInteractionHover = new GameEvent<string>();
        
        // === PROPERTIES ===
        public InteractionPoint InteractionData => interactionData;
        public bool IsHovered { get; private set; }
        public bool IsInteracting { get; private set; }
        
        // === PRIVATE FIELDS ===
        private Vector3 originalScale;
        private Color originalColor;
        private Coroutine hoverCoroutine;
        private bool isInitialized = false;
        
        // === CLICK COUNTER SYSTEM ===
        private static Dictionary<string, int> clickCounters = new Dictionary<string, int>();
        private static Dictionary<string, int> requiredClickCounts = new Dictionary<string, int>();
        
        // === INITIALIZATION ===
        
        /// <summary>
        /// 상호작용 포인트 초기화
        /// </summary>
        /// <param name="data">상호작용 데이터</param>
        public void Initialize(InteractionPoint data)
        {
            interactionData = data;
            SetupComponents();
            SetupVisual();
            UpdateInteractionState();
            
            // 클릭 카운터 초기화
            InitializeClickCounter();
            
            isInitialized = true;
            
            if (debugMode)
            {
                Debug.Log($"[StageInteractionController] Initialized: {data.id}");
            }
        }
        
        /// <summary>
        /// 컴포넌트 설정
        /// </summary>
        private void SetupComponents()
        {
            // Image 컴포넌트 설정
            if (visualImage == null)
            {
                visualImage = GetComponent<Image>();
                if (visualImage == null)
                    visualImage = gameObject.AddComponent<Image>();
            }
            
            // Button 컴포넌트 설정
            if (interactionButton == null)
            {
                interactionButton = GetComponent<Button>();
                if (interactionButton == null)
                    interactionButton = gameObject.AddComponent<Button>();
            }
            
            // 원본 값 저장
            originalScale = transform.localScale;
            originalColor = visualImage.color;
        }
        
        /// <summary>
        /// 시각적 요소 설정
        /// </summary>
        private void SetupVisual()
        {
            if (interactionData == null) return;
            
            // 가시성 설정
            gameObject.SetActive(interactionData.isVisible);
            
            // 호버 스프라이트가 있으면 설정
            if (interactionData.hoverSprite != null)
            {
                visualImage.sprite = interactionData.hoverSprite;
            }
            
            // 투명하게 설정 (디버그 모드가 아닐 때)
            if (!debugMode)
            {
                Color transparentColor = originalColor;
                transparentColor.a = 0f;
                visualImage.color = transparentColor;
            }
            else
            {
                // 디버그 모드에서는 반투명하게 표시
                Color debugColor = Color.yellow;
                debugColor.a = 0.3f;
                visualImage.color = debugColor;
            }
        }
        
        // === UPDATE METHODS ===
        
        /// <summary>
        /// 상호작용 상태 업데이트
        /// </summary>
        public void UpdateInteractionState()
        {
            if (!isInitialized || interactionData == null) return;
            
            bool canInteract = CanInteract();
            
            // 버튼 상호작용 가능 여부 설정
            if (interactionButton != null)
            {
                interactionButton.interactable = canInteract;
            }
            
            // 시각적 피드백
            if (!canInteract)
            {
                // 상호작용 불가능할 때의 시각적 처리
                Color disabledColor = originalColor;
                disabledColor.a = debugMode ? 0.1f : 0f;
                visualImage.color = disabledColor;
            }
        }
        
        // === IINTERACTABLE IMPLEMENTATION ===
        
        public void OnInteract()
        {
            if (!CanInteract() || IsInteracting) return;
            
            StartCoroutine(ExecuteInteraction());
        }
        
        public bool CanInteract()
        {
            if (interactionData == null) return false;
            
            return interactionData.CanInteract();
        }
        
        public InteractionType GetInteractionType()
        {
            return interactionData?.interactionType ?? InteractionType.Click;
        }
        
        // === POINTER EVENT HANDLERS ===
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!CanInteract()) return;
            
            IsHovered = true;
            
            // 호버 사운드 재생
            if (interactionData.hoverSound != null)
            {
                // AudioManager.Instance.PlaySFX(interactionData.hoverSound);
            }
            
            // 호버 효과 시작
            if (hoverCoroutine != null)
                StopCoroutine(hoverCoroutine);
            hoverCoroutine = StartCoroutine(HoverEffect(true));
            
            // 이벤트 발생
            OnInteractionHover.Raise(interactionData.id);
            
            if (debugMode)
            {
                Debug.Log($"[StageInteractionController] Hover enter: {interactionData.id}");
            }
        }
        
        public void OnPointerExit(PointerEventData eventData)
        {
            if (!CanInteract()) return;
            
            IsHovered = false;
            
            // 호버 효과 종료
            if (hoverCoroutine != null)
                StopCoroutine(hoverCoroutine);
            hoverCoroutine = StartCoroutine(HoverEffect(false));
            
            if (debugMode)
            {
                Debug.Log($"[StageInteractionController] Hover exit: {interactionData.id}");
            }
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            OnInteract();
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
                Debug.Log($"[StageInteractionController] Executing interaction: {interactionData.id}");
            }
            
            // 상호작용 사운드 재생
            if (interactionData.interactionSound != null)
            {
                // AudioManager.Instance.PlaySFX(interactionData.interactionSound);
            }
            
            // 클릭 피드백 효과
            yield return StartCoroutine(ClickFeedback());
            
            // 상호작용 결과 처리
            ProcessInteractionResult();
            
            // 이벤트 발생
            OnInteractionTriggered.Raise(interactionData.id);
            
            // 짧은 딜레이 후 상호작용 완료
            yield return new WaitForSeconds(0.1f);
            
            IsInteracting = false;
        }
        
        /// <summary>
        /// 상호작용 결과 처리
        /// </summary>
        private void ProcessInteractionResult()
        {
            if (interactionData?.result == null) return;
            
            var result = interactionData.result;
            
            // 클릭 카운터 업데이트 및 체크
            if (HandleClickCounter())
            {
                return; // 카운터가 처리되었으면 다른 액션은 실행하지 않음
            }
            
            // 아이템 지급
            if (result.itemToAdd != null)
            {
                // InventoryManager.Instance.AddItem(result.itemToAdd);
                Debug.Log($"[StageInteractionController] Give item: {result.itemToAdd.name}");
            }
            
            // 스테이지 변경 (resultType 2: 다음 스테이지로 이동)
            if (result.resultType == 2 && result.nextStageIndex >= 0)
            {
                if (StageManager.Instance != null)
                {
                    StageManager.Instance.ChangeStage(result.nextStageIndex);
                }
            }
            
            // 메시지 표시
            if (!string.IsNullOrEmpty(result.message))
            {
                // UIManager나 NotificationManager로 메시지 표시
                Debug.Log($"[StageInteractionController] Message: {result.message}");
            }
        }
        
        /// <summary>
        /// 커스텀 액션 실행
        /// </summary>
        private void ExecuteCustomAction(string actionId)
        {
            // 게임별 커스텀 액션 구현
            switch (actionId)
            {
                case "unlock_door":
                    Debug.Log("[StageInteractionController] Custom action: unlock_door");
                    break;
                    
                case "turn_on_light":
                    Debug.Log("[StageInteractionController] Custom action: turn_on_light");
                    break;
                    
                default:
                    Debug.LogWarning($"[StageInteractionController] Unknown custom action: {actionId}");
                    break;
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
            Vector3 targetScale = isEntering ? originalScale * interactionData.hoverScale : originalScale;
            
            Color startColor = visualImage.color;
            Color targetColor = isEntering ? interactionData.hoverColor : originalColor;
            
            // 디버그 모드가 아닐 때는 색상 변경하지 않음
            if (!debugMode)
            {
                targetColor.a = isEntering ? 0.3f : 0f;
            }
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;
                
                // 스케일 애니메이션
                transform.localScale = Vector3.Lerp(startScale, targetScale, t);
                
                // 색상 애니메이션
                visualImage.color = Color.Lerp(startColor, targetColor, t);
                
                yield return null;
            }
            
            transform.localScale = targetScale;
            visualImage.color = targetColor;
        }
        
        /// <summary>
        /// 클릭 피드백 효과
        /// </summary>
        private IEnumerator ClickFeedback()
        {
            float duration = 0.1f;
            Vector3 pressedScale = originalScale * 0.95f;
            
            // 눌림 효과
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;
                
                transform.localScale = Vector3.Lerp(originalScale, pressedScale, t);
                yield return null;
            }
            
            // 복원 효과
            elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;
                
                transform.localScale = Vector3.Lerp(pressedScale, originalScale, t);
                yield return null;
            }
            
            transform.localScale = originalScale;
        }
        
        // === CLICK COUNTER METHODS ===
        
        /// <summary>
        /// 클릭 카운터 초기화
        /// </summary>
        private void InitializeClickCounter()
        {
            if (interactionData == null) return;
            
            // entrance_door는 5번 클릭이 필요
            if (interactionData.id == "entrance_door")
            {
                requiredClickCounts[interactionData.id] = 5;
                if (!clickCounters.ContainsKey(interactionData.id))
                {
                    clickCounters[interactionData.id] = 0;
                }
            }
        }
        
        /// <summary>
        /// 클릭 카운터 처리
        /// </summary>
        /// <returns>카운터가 처리되었으면 true</returns>
        private bool HandleClickCounter()
        {
            if (!requiredClickCounts.ContainsKey(interactionData.id))
                return false;
                
            // 클릭 카운트 증가
            clickCounters[interactionData.id]++;
            int currentCount = clickCounters[interactionData.id];
            int requiredCount = requiredClickCounts[interactionData.id];
            
            Debug.Log($"[StageInteractionController] {interactionData.id} 클릭: {currentCount}/{requiredCount}");
            
            // 메시지 표시 (진행 상황)
            string progressMessage = interactionData.result.message.Replace("{0}", currentCount.ToString());
            Debug.Log($"[StageInteractionController] {progressMessage}");
            
            // 필요한 클릭 수에 도달했는지 확인
            if (currentCount >= requiredCount)
            {
                Debug.Log($"[StageInteractionController] {interactionData.id}: 클릭 조건 완료! 다음 스테이지로 이동");
                
                // 다음 스테이지로 이동
                if (StageManager.Instance != null && interactionData.result.nextStageIndex >= 0)
                {
                    StageManager.Instance.ChangeStage(interactionData.result.nextStageIndex);
                }
                
                // 카운터 리셋
                clickCounters[interactionData.id] = 0;
                return true;
            }
            
            return true; // 카운터가 처리되었음을 표시
        }
        
        /// <summary>
        /// 현재 클릭 카운트 가져오기
        /// </summary>
        public int GetCurrentClickCount()
        {
            if (!clickCounters.ContainsKey(interactionData.id))
                return 0;
            return clickCounters[interactionData.id];
        }
        
        /// <summary>
        /// 필요한 클릭 카운트 가져오기
        /// </summary>
        public int GetRequiredClickCount()
        {
            if (!requiredClickCounts.ContainsKey(interactionData.id))
                return 1;
            return requiredClickCounts[interactionData.id];
        }
        
        // === UTILITY METHODS ===
        
        /// <summary>
        /// 디버그 모드 토글
        /// </summary>
        public void ToggleDebugMode()
        {
            debugMode = !debugMode;
            SetupVisual();
        }
        
        /// <summary>
        /// 상호작용 데이터 업데이트
        /// </summary>
        public void UpdateInteractionData(InteractionPoint newData)
        {
            interactionData = newData;
            SetupVisual();
            UpdateInteractionState();
        }
        
        // === UNITY LIFECYCLE ===
        
        private void Update()
        {
            // 상호작용 상태를 주기적으로 업데이트
            if (isInitialized && Time.frameCount % 30 == 0) // 30프레임마다
            {
                UpdateInteractionState();
            }
        }
        
        private void OnDestroy()
        {
            // 코루틴 정리
            if (hoverCoroutine != null)
            {
                StopCoroutine(hoverCoroutine);
            }
        }
        
        // === GIZMOS ===
        
        private void OnDrawGizmos()
        {
            if (!debugMode || interactionData == null) return;
            
            // 상호작용 영역 시각화
            Gizmos.color = CanInteract() ? Color.green : Color.red;
            Gizmos.DrawWireCube(transform.position, new Vector3(interactionData.size.x, interactionData.size.y, 0.1f));
            
            // ID 표시
            if (Application.isPlaying)
            {
#if UNITY_EDITOR
                UnityEditor.Handles.Label(transform.position, interactionData.id);
#endif
            }
        }
    }
}