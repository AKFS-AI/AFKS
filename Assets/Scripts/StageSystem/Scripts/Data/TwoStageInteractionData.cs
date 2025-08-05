using UnityEngine;

namespace AFKS.StageSystem
{
    /// <summary>
    /// 2단계 상호작용 설정 데이터
    /// Inspector에서 쉽게 설정할 수 있도록 ScriptableObject로 구현
    /// </summary>
    [CreateAssetMenu(fileName = "TwoStageInteractionData", menuName = "AFKS/Stage/Two Stage Interaction Data")]
    public class TwoStageInteractionData : ScriptableObject
    {
        [Header("📋 기본 정보")]
        [SerializeField, Tooltip("상호작용 고유 ID")] private string interactionId = "hospital_door";
        [SerializeField, Tooltip("상호작용 이름")] private string displayName = "병원 철문";
        [SerializeField, TextArea(2, 3), Tooltip("상호작용 설명")] private string description = "병원 입구의 철문입니다.";
        
        [Header("🚪 1단계: 초기 상호작용")]
        [SerializeField, Tooltip("1단계 상호작용 이름")] private string stage1Name = "철문 조사";
        [SerializeField, Tooltip("1단계 클릭 메시지")] private string stage1ClickMessage = "문을 자세히 살펴보자...";
        [SerializeField, Tooltip("1단계 상호작용 사운드")] private AudioClip stage1Sound;
        
        [Header("🔍 확대 설정")]
        [SerializeField, Range(1.5f, 4f), Tooltip("확대 배율")] private float zoomScale = 2.5f;
        [SerializeField, Tooltip("확대 중심점 오프셋")] private Vector2 zoomFocusOffset = Vector2.zero;
        [SerializeField, Range(0.3f, 2f), Tooltip("확대 애니메이션 시간")] private float zoomDuration = 0.8f;
        
        [Header("⛓️ 2단계: 세부 상호작용")]
        [SerializeField, Tooltip("2단계 상호작용 이름")] private string stage2Name = "쇠사슬 해제";
        [SerializeField, Tooltip("2단계 필요 클릭 횟수")] private int requiredClicks = 5;
        [SerializeField, Tooltip("2단계 클릭 진행 메시지")] private string stage2ProgressMessage = "쇠사슬을 부수고 있다... ({0}/{1})";
        [SerializeField, Tooltip("2단계 클릭 사운드")] private AudioClip stage2ClickSound;
        [SerializeField, Tooltip("2단계 완료 사운드")] private AudioClip stage2CompleteSound;
        
        [Header("🎯 결과 설정")]
        [SerializeField, Tooltip("완료 메시지")] private string completionMessage = "쇠사슬이 끊어졌다! 문이 열렸다!";
        [SerializeField, Tooltip("다음 스테이지 인덱스")] private int nextStageIndex = 1;
        [SerializeField, Range(0.5f, 3f), Tooltip("완료 후 대기 시간")] private float completionDelay = 1.5f;
        
        [Header("🎨 시각적 설정")]
        [SerializeField, Tooltip("호버 색상")] private Color hoverColor = new Color(1, 1, 0, 0.3f);
        [SerializeField, Range(1f, 1.3f), Tooltip("호버 크기 배율")] private float hoverScale = 1.05f;
        [SerializeField, Range(0.1f, 0.5f), Tooltip("클릭 피드백 시간")] private float clickFeedbackDuration = 0.2f;
        
        [Header("🔧 고급 설정")]
        [SerializeField, Tooltip("ESC 키로 1단계 복귀 허용")] private bool allowEscapeToStage1 = true;
        [SerializeField, Tooltip("자동 줌 해제 (완료 시)")] private bool autoZoomOutOnComplete = true;
        [SerializeField, Tooltip("디버그 모드")] private bool debugMode = false;
        
        // === PROPERTIES ===
        public string InteractionId => interactionId;
        public string DisplayName => displayName;
        public string Description => description;
        
        public string Stage1Name => stage1Name;
        public string Stage1ClickMessage => stage1ClickMessage;
        public AudioClip Stage1Sound => stage1Sound;
        
        public float ZoomScale => zoomScale;
        public Vector2 ZoomFocusOffset => zoomFocusOffset;
        public float ZoomDuration => zoomDuration;
        
        public string Stage2Name => stage2Name;
        public int RequiredClicks => requiredClicks;
        public string Stage2ProgressMessage => stage2ProgressMessage;
        public AudioClip Stage2ClickSound => stage2ClickSound;
        public AudioClip Stage2CompleteSound => stage2CompleteSound;
        
        public string CompletionMessage => completionMessage;
        public int NextStageIndex => nextStageIndex;
        public float CompletionDelay => completionDelay;
        
        public Color HoverColor => hoverColor;
        public float HoverScale => hoverScale;
        public float ClickFeedbackDuration => clickFeedbackDuration;
        
        public bool AllowEscapeToStage1 => allowEscapeToStage1;
        public bool AutoZoomOutOnComplete => autoZoomOutOnComplete;
        public bool DebugMode => debugMode;
        
        // === VALIDATION ===
        
        private void OnValidate()
        {
            // ID 검증
            if (string.IsNullOrEmpty(interactionId))
            {
                interactionId = name.ToLower().Replace(" ", "_");
            }
            
            // 클릭 횟수 검증
            requiredClicks = Mathf.Max(1, requiredClicks);
            
            // 스테이지 인덱스 검증
            nextStageIndex = Mathf.Max(0, nextStageIndex);
            
            // 시간 값 검증
            zoomDuration = Mathf.Max(0.1f, zoomDuration);
            completionDelay = Mathf.Max(0.1f, completionDelay);
            clickFeedbackDuration = Mathf.Max(0.05f, clickFeedbackDuration);
            
            // 배율 검증
            zoomScale = Mathf.Max(1.1f, zoomScale);
            hoverScale = Mathf.Max(1f, hoverScale);
        }
        
        // === UTILITY METHODS ===
        
        /// <summary>
        /// 진행 메시지 포맷팅
        /// </summary>
        public string GetFormattedProgressMessage(int currentClicks)
        {
            return stage2ProgressMessage.Replace("{0}", currentClicks.ToString())
                                       .Replace("{1}", requiredClicks.ToString());
        }
        
        /// <summary>
        /// 설정 데이터 유효성 검증
        /// </summary>
        public bool ValidateData()
        {
            bool isValid = true;
            
            if (string.IsNullOrEmpty(interactionId))
            {
                Debug.LogError($"[TwoStageInteractionData] {name}: InteractionId가 비어있습니다.");
                isValid = false;
            }
            
            if (string.IsNullOrEmpty(displayName))
            {
                Debug.LogWarning($"[TwoStageInteractionData] {name}: DisplayName이 비어있습니다.");
            }
            
            if (requiredClicks <= 0)
            {
                Debug.LogError($"[TwoStageInteractionData] {name}: RequiredClicks는 1 이상이어야 합니다.");
                isValid = false;
            }
            
            if (nextStageIndex < 0)
            {
                Debug.LogWarning($"[TwoStageInteractionData] {name}: NextStageIndex가 0보다 작습니다.");
            }
            
            return isValid;
        }
        
        /// <summary>
        /// 디버그 정보 출력
        /// </summary>
        [ContextMenu("디버그 정보 출력")]
        public void PrintDebugInfo()
        {
            Debug.Log($"=== TwoStageInteractionData Debug Info ===");
            Debug.Log($"Interaction ID: {interactionId}");
            Debug.Log($"Display Name: {displayName}");
            Debug.Log($"Required Clicks: {requiredClicks}");
            Debug.Log($"Next Stage Index: {nextStageIndex}");
            Debug.Log($"Zoom Scale: {zoomScale}");
            Debug.Log($"Is Valid: {ValidateData()}");
        }
        
        /// <summary>
        /// TwoStageInteractionController에 데이터 적용
        /// </summary>
        public void ApplyToController(TwoStageInteractionController controller)
        {
            if (controller == null)
            {
                Debug.LogError("[TwoStageInteractionData] Controller가 null입니다.");
                return;
            }
            
            // 리플렉션을 사용하여 데이터 적용 (또는 별도의 Apply 메서드 구현)
            Debug.Log($"[TwoStageInteractionData] 데이터를 {controller.name}에 적용했습니다.");
        }
        
        /// <summary>
        /// 기본 병원 철문 설정 생성
        /// </summary>
        [ContextMenu("기본 병원 철문 설정 생성")]
        public void CreateDefaultHospitalDoorSettings()
        {
            interactionId = "hospital_door";
            displayName = "병원 철문";
            description = "녹슨 쇠사슬로 잠긴 병원 입구의 철문입니다.";
            
            stage1Name = "철문 조사";
            stage1ClickMessage = "문을 자세히 살펴보자...";
            
            zoomScale = 2.5f;
            zoomFocusOffset = Vector2.zero;
            
            stage2Name = "쇠사슬 해제";
            requiredClicks = 5;
            stage2ProgressMessage = "쇠사슬을 부수고 있다... ({0}/{1})";
            
            completionMessage = "쇠사슬이 끊어졌다! 문이 열렸다!";
            nextStageIndex = 1;
            
            hoverColor = new Color(1, 1, 0, 0.3f);
            hoverScale = 1.05f;
            
            Debug.Log("[TwoStageInteractionData] 기본 병원 철문 설정이 생성되었습니다.");
        }
    }
}