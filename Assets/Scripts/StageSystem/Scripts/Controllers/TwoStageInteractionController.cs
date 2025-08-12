using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using AFKS.Shared.Interfaces;
using AFKS.Shared.Events;
using AFKS.UISystem;
using AFKS.Shared.Utils;

namespace AFKS.StageSystem
{
    /// <summary>
    /// 2단계 상호작용 컨트롤러
    /// 1단계: 철문 클릭 → 확대
    /// 2단계: 확대된 상태에서 쇠사슬 멀티 클릭 → 스테이지 전환
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class TwoStageInteractionController : MonoBehaviour, IInteractable, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Header("🎯 1단계: 초기 상호작용 (철문)")]
        [SerializeField, Tooltip("1단계 상호작용할 이미지 (철문)")] private Image doorImage;
        [SerializeField, Tooltip("1단계 상호작용 ID")] private string doorInteractionId = Constants.InteractionIds.HospitalDoor;
        [SerializeField, Tooltip("1단계 표시 이름")] private string doorDisplayName = "병원 철문";
        
        [Header("🔍 확대 설정")]
        [SerializeField, Range(1.5f, 4f), Tooltip("확대 배율")] private float zoomScale = 2.5f;
        [SerializeField, Tooltip("확대 중심점 오프셋")] private Vector2 zoomFocusOffset = Vector2.zero;
        
        [Header("⛓️ 2단계: 쇠사슬 상호작용")]
        [SerializeField, Tooltip("2단계 상호작용할 이미지 (쇠사슬)")] private Image chainImage;
        [SerializeField, Tooltip("쇠사슬 상호작용 ID")] private string chainInteractionId = Constants.InteractionIds.HospitalDoorChain;
        [SerializeField, Tooltip("쇠사슬 표시 이름")] private string chainDisplayName = "철문의 쇠사슬";
        [SerializeField, Tooltip("쇠사슬 해제에 필요한 클릭 횟수")] private int requiredChainClicks = 5;
        [SerializeField, Tooltip("다음 스테이지 인덱스")] private int nextStageIndex = 1;
        
        [Header("🎨 시각적 피드백")]
        [SerializeField, Tooltip("호버 색상")] private Color hoverColor = new Color(1, 1, 0, 0.3f);
        [SerializeField, Tooltip("호버 크기 배율")] private float hoverScale = 1.05f;
        [SerializeField, Tooltip("클릭 피드백 지속시간")] private float clickFeedbackDuration = 0.2f;
        
        [Header("🔊 오디오")]
        [SerializeField, Tooltip("철문 클릭 사운드")] private AudioClip doorClickSound;
        [SerializeField, Tooltip("쇠사슬 클릭 사운드")] private AudioClip chainClickSound;
        [SerializeField, Tooltip("쇠사슬 해제 사운드")] private AudioClip chainBreakSound;
        
        [Header("💬 메시지")]
        [SerializeField, Tooltip("철문 클릭 시 메시지")] private string doorClickMessage = "문을 자세히 살펴보자...";
        [SerializeField, Tooltip("쇠사슬 클릭 진행 메시지")] private string chainProgressMessage = "쇠사슬을 부수고 있다... ({0}/{1})";
        [SerializeField, Tooltip("문 해제 완료 메시지")] private string doorUnlockMessage = "쇠사슬이 끊어졌다! 문이 열렸다!";
        
        // === RUNTIME EVENTS ===
        public static readonly GameEvent<string> OnDoorClicked = new GameEvent<string>();
        public static readonly GameEvent<string> OnChainClicked = new GameEvent<string>();
        public static readonly GameEvent<string> OnDoorUnlocked = new GameEvent<string>();
        
        // === PROPERTIES ===
        public bool IsStage1Active => currentStage == InteractionStage.Stage1;
        public bool IsStage2Active => currentStage == InteractionStage.Stage2;
        public bool IsCompleted => currentStage == InteractionStage.Completed;
        public int CurrentChainClicks => currentChainClicks;
        public int RequiredChainClicks => requiredChainClicks;
        
        // === PRIVATE FIELDS ===
        private InteractionStage currentStage = InteractionStage.Stage1;
        private int currentChainClicks = 0;
        private bool isHovered = false;
        private bool isInteracting = false;
        
        private Vector3 originalDoorScale;
        private Color originalDoorColor;
        private Vector3 originalChainScale;
        private Color originalChainColor;
        
        private Coroutine hoverCoroutine;
        private bool isInitialized = false;
        
        // === ENUMS ===
        private enum InteractionStage
        {
            Stage1,     // 철문 클릭 대기
            Stage2,     // 쇠사슬 클릭 진행
            Completed   // 상호작용 완료
        }
        
        // === UNITY LIFECYCLE ===
        
        private void Awake()
        {
            SetupComponents();
        }
        
        private void Start()
        {
            Initialize();
        }
        
        private void Update()
        {
            // 2단계에서 ESC 키로 줌 해제 및 1단계로 복귀
            if (currentStage == InteractionStage.Stage2 && AFKS.Shared.Utils.InputHelper.WasEscapePressedThisFrame())
            {
                ReturnToStage1();
            }
        }
        
        // === INITIALIZATION ===
        
        /// <summary>
        /// 컴포넌트 설정
        /// </summary>
        private void SetupComponents()
        {
            // 철문 이미지 설정
            if (doorImage == null)
            {
                doorImage = GetComponent<Image>();
            }
            
            if (doorImage != null)
            {
                originalDoorScale = doorImage.transform.localScale;
                originalDoorColor = doorImage.color;
            }
            
            // 쇠사슬 이미지 설정
            if (chainImage != null)
            {
                originalChainScale = chainImage.transform.localScale;
                originalChainColor = chainImage.color;
                
                // 2단계가 아닐 때는 쇠사슬 비활성화
                chainImage.gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// 초기화
        /// </summary>
        private void Initialize()
        {
            currentStage = InteractionStage.Stage1;
            currentChainClicks = 0;
            isInitialized = true;
            
            // 줌 상태 변경 이벤트 구독
            if (ZoomController.Instance != null)
            {
                ZoomController.OnZoomStateChanged.AddListener(OnZoomStateChanged);
            }
            
            Debug.Log($"[TwoStageInteraction] 초기화 완료: {doorDisplayName}");
        }
        
        // === IINTERACTABLE IMPLEMENTATION ===
        
        public void OnInteract()
        {
            if (!CanInteract() || isInteracting) return;
            
            StartCoroutine(ExecuteInteraction());
        }
        
        public bool CanInteract()
        {
            return isInitialized && gameObject.activeInHierarchy && currentStage != InteractionStage.Completed;
        }
        
        public InteractionType GetInteractionType()
        {
            return InteractionType.Click;
        }
        
        // === POINTER EVENT HANDLERS ===
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!CanInteract()) return;
            
            isHovered = true;
            
            if (hoverCoroutine != null)
                StopCoroutine(hoverCoroutine);
            hoverCoroutine = StartCoroutine(HoverEffect(true));
        }
        
        public void OnPointerExit(PointerEventData eventData)
        {
            if (isHovered)
            {
                isHovered = false;
                
                if (hoverCoroutine != null)
                    StopCoroutine(hoverCoroutine);
                hoverCoroutine = StartCoroutine(HoverEffect(false));
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
            isInteracting = true;
            
            switch (currentStage)
            {
                case InteractionStage.Stage1:
                    yield return StartCoroutine(ExecuteStage1Interaction());
                    break;
                    
                case InteractionStage.Stage2:
                    yield return StartCoroutine(ExecuteStage2Interaction());
                    break;
            }
            
            isInteracting = false;
        }
        
        /// <summary>
        /// 1단계 상호작용: 철문 클릭 → 확대
        /// </summary>
        private IEnumerator ExecuteStage1Interaction()
        {
            Debug.Log($"[TwoStageInteraction] 1단계 실행: {doorDisplayName} 클릭");
            
            // 사운드 재생
            if (doorClickSound != null && AFKS.AudioSystem.AudioManager.Instance != null)
            {
                AFKS.AudioSystem.AudioManager.Instance.PlaySFX(doorClickSound);
            }
            
            // 클릭 피드백 효과
            yield return StartCoroutine(ClickFeedback(doorImage));
            
            // 메시지 표시
            if (!string.IsNullOrEmpty(doorClickMessage))
            {
                Debug.Log($"[TwoStageInteraction] {doorClickMessage}");
                // TODO: UI 메시지 시스템과 연동
            }
            
            // 확대 실행
            if (ZoomController.Instance != null)
            {
                Vector2 focusPoint = zoomFocusOffset;
                ZoomController.Instance.ZoomToImage(doorImage, zoomScale, focusPoint);
                
                // 확대 완료까지 대기
                yield return new WaitForSeconds(0.8f);
                
                // 2단계로 전환
                TransitionToStage2();
            }
            else
            {
                Debug.LogWarning("[TwoStageInteraction] ZoomController를 찾을 수 없습니다!");
            }
            
            // 이벤트 발생
            OnDoorClicked.Raise(doorInteractionId);
            AFKS.Shared.Events.EventBus.DoorClicked.Raise(doorInteractionId);
        }
        
        /// <summary>
        /// 2단계 상호작용: 쇠사슬 멀티 클릭
        /// </summary>
        private IEnumerator ExecuteStage2Interaction()
        {
            Debug.Log($"[TwoStageInteraction] 2단계 실행: {chainDisplayName} 클릭 ({currentChainClicks + 1}/{requiredChainClicks})");
            
            // 사운드 재생
            if (chainClickSound != null && AFKS.AudioSystem.AudioManager.Instance != null)
            {
                AFKS.AudioSystem.AudioManager.Instance.PlaySFX(chainClickSound);
            }
            
            // 클릭 피드백 효과
            yield return StartCoroutine(ClickFeedback(chainImage));
            
            // 클릭 카운트 증가
            currentChainClicks++;
            
            // 진행 상황 메시지
            string progressMsg = chainProgressMessage.Replace("{0}", currentChainClicks.ToString())
                                                    .Replace("{1}", requiredChainClicks.ToString());
            Debug.Log($"[TwoStageInteraction] {progressMsg}");
            
            // 이벤트 발생
            OnChainClicked.Raise(chainInteractionId);
            AFKS.Shared.Events.EventBus.ChainClicked.Raise(chainInteractionId);
            
            // 필요한 클릭 수에 도달했는지 확인
            if (currentChainClicks >= requiredChainClicks)
            {
                yield return StartCoroutine(CompleteInteraction());
            }
        }
        
        /// <summary>
        /// 상호작용 완료 처리
        /// </summary>
        private IEnumerator CompleteInteraction()
        {
            Debug.Log($"[TwoStageInteraction] 상호작용 완료: {doorDisplayName}");
            
            // 완료 사운드 재생
            if (chainBreakSound != null && AFKS.AudioSystem.AudioManager.Instance != null)
            {
                AFKS.AudioSystem.AudioManager.Instance.PlaySFX(chainBreakSound);
            }
            
            // 완료 메시지
            if (!string.IsNullOrEmpty(doorUnlockMessage))
            {
                Debug.Log($"[TwoStageInteraction] {doorUnlockMessage}");
            }
            
            // 상태 변경
            currentStage = InteractionStage.Completed;
            
            // 이벤트 발생
            OnDoorUnlocked.Raise(doorInteractionId);
            AFKS.Shared.Events.EventBus.DoorUnlocked.Raise(doorInteractionId);
            
            // 잠시 대기
            yield return new WaitForSeconds(1.5f);
            
            // 줌 해제
            if (ZoomController.Instance != null)
            {
                ZoomController.Instance.ZoomOut();
                yield return new WaitForSeconds(0.8f);
            }
            
            // 다음 스테이지로 이동 (잠금 해제 포함)
            if (nextStageIndex >= 0 && StageManager.Instance != null)
            {
                StageManager.Instance.UnlockStage(nextStageIndex);
                StageManager.Instance.ChangeStage(nextStageIndex);
            }
        }
        
        // === STAGE MANAGEMENT ===
        
        /// <summary>
        /// 2단계로 전환
        /// </summary>
        private void TransitionToStage2()
        {
            currentStage = InteractionStage.Stage2;
            
            // 쇠사슬 이미지 활성화
            if (chainImage != null)
            {
                chainImage.gameObject.SetActive(true);
            }
            
            Debug.Log("[TwoStageInteraction] 2단계로 전환: 쇠사슬 클릭 가능");
        }
        
        /// <summary>
        /// 1단계로 복귀
        /// </summary>
        private void ReturnToStage1()
        {
            if (currentStage != InteractionStage.Stage2) return;
            
            Debug.Log("[TwoStageInteraction] 1단계로 복귀");
            
            currentStage = InteractionStage.Stage1;
            currentChainClicks = 0;
            
            // 쇠사슬 이미지 비활성화
            if (chainImage != null)
            {
                chainImage.gameObject.SetActive(false);
            }
            
            // 줌 해제
            if (ZoomController.Instance != null)
            {
                ZoomController.Instance.ZoomOut();
            }
        }
        
        /// <summary>
        /// 줌 상태 변경 이벤트 처리
        /// </summary>
        private void OnZoomStateChanged(bool isZoomed)
        {
            // 줌이 해제되면 1단계로 복귀
            if (!isZoomed && currentStage == InteractionStage.Stage2)
            {
                ReturnToStage1();
            }
        }
        
        // === VISUAL EFFECTS ===
        
        /// <summary>
        /// 호버 효과
        /// </summary>
        private IEnumerator HoverEffect(bool isEntering)
        {
            Image targetImg = (currentStage == InteractionStage.Stage1) ? doorImage : chainImage;
            if (targetImg == null) yield break;
            
            float duration = 0.2f;
            float elapsed = 0f;
            
            Vector3 originalScale = (currentStage == InteractionStage.Stage1) ? originalDoorScale : originalChainScale;
            Vector3 startScale = targetImg.transform.localScale;
            Vector3 targetScale = isEntering ? originalScale * hoverScale : originalScale;
            
            Color originalColor = (currentStage == InteractionStage.Stage1) ? originalDoorColor : originalChainColor;
            Color startColor = targetImg.color;
            Color targetColor = isEntering ? 
                Color.Lerp(originalColor, hoverColor, 0.5f) : originalColor;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                targetImg.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
                targetImg.color = Color.Lerp(startColor, targetColor, t);
                
                yield return null;
            }
            
            targetImg.transform.localScale = targetScale;
            targetImg.color = targetColor;
        }
        
        /// <summary>
        /// 클릭 피드백 효과
        /// </summary>
        private IEnumerator ClickFeedback(Image targetImg)
        {
            if (targetImg == null) yield break;
            
            Vector3 originalScale = targetImg.transform.localScale;
            Vector3 pressedScale = originalScale * 0.95f;
            
            float halfDuration = clickFeedbackDuration / 2f;
            
            // 축소
            float elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / halfDuration;
                
                targetImg.transform.localScale = Vector3.Lerp(originalScale, pressedScale, t);
                yield return null;
            }
            
            // 복원
            elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / halfDuration;
                
                targetImg.transform.localScale = Vector3.Lerp(pressedScale, originalScale, t);
                yield return null;
            }
            
            targetImg.transform.localScale = originalScale;
        }
        
        // === PUBLIC UTILITY METHODS ===
        
        /// <summary>
        /// 상호작용 리셋
        /// </summary>
        public void ResetInteraction()
        {
            currentStage = InteractionStage.Stage1;
            currentChainClicks = 0;
            isHovered = false;
            isInteracting = false;
            
            if (chainImage != null)
            {
                chainImage.gameObject.SetActive(false);
            }
            
            if (ZoomController.Instance != null)
            {
                ZoomController.Instance.ResetZoom();
            }
            
            Debug.Log("[TwoStageInteraction] 상호작용 리셋 완료");
        }
        
        /// <summary>
        /// 디버그 정보 출력
        /// </summary>
        [ContextMenu("디버그 정보 출력")]
        public void PrintDebugInfo()
        {
            Debug.Log($"=== TwoStageInteraction Debug Info ===");
            Debug.Log($"Current Stage: {currentStage}");
            Debug.Log($"Chain Clicks: {currentChainClicks}/{requiredChainClicks}");
            Debug.Log($"Is Hovered: {isHovered}");
            Debug.Log($"Is Interacting: {isInteracting}");
            Debug.Log($"Door Image: {doorImage?.name ?? "null"}");
            Debug.Log($"Chain Image: {chainImage?.name ?? "null"}");
        }
        
        // === CLEANUP ===
        
        private void OnDestroy()
        {
            if (hoverCoroutine != null)
            {
                StopCoroutine(hoverCoroutine);
            }
            
            // 이벤트 구독 해제
            if (ZoomController.Instance != null)
            {
                ZoomController.OnZoomStateChanged.RemoveListener(OnZoomStateChanged);
            }
        }
    }
}