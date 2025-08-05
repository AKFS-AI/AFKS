using UnityEngine;

namespace AFKS.StageSystem
{
    /// <summary>
    /// 공포 이벤트 데이터
    /// </summary>
    [System.Serializable]
    public class HorrorEventData
    {
        [Header("📋 기본 정보")]
        [SerializeField] public string eventId;
        [SerializeField] public string eventName;
        [SerializeField, TextArea(2, 3)] public string description;
        
        [Header("🎯 트리거 설정")]
        [SerializeField] public HorrorTriggerType triggerType;
        [SerializeField, Range(0f, 10f)] public float triggerDelay = 0f;
        [SerializeField] public string triggerCondition; // 조건 ID
        
        [Header("🖼️ 시각적 요소")]
        [SerializeField] public Sprite jumpscareImage;
        [SerializeField] public Vector2 imagePosition = Vector2.zero;
        [SerializeField] public Vector2 imageSize = Vector2.one;
        [SerializeField, Range(0.1f, 10f)] public float displayDuration = 2f;
        
        [Header("🔊 오디오")]
        [SerializeField] public AudioClip horrorSound;
        [SerializeField, Range(0f, 1f)] public float volume = 0.8f;
        
        [Header("⚡ 특수 효과")]
        [SerializeField] public bool enableScreenShake = true;
        [SerializeField, Range(0f, 2f)] public float shakeIntensity = 0.5f;
        [SerializeField, Range(0.1f, 5f)] public float shakeDuration = 1f;
        
        [Header("⚙️ 재사용 설정")]
        [SerializeField] public bool canRepeat = false;
        [SerializeField, Range(5f, 60f)] public float cooldownTime = 10f;
        
        /// <summary>
        /// 공포 이벤트 유효성 검증
        /// </summary>
        public bool IsValid()
        {
            if (string.IsNullOrEmpty(eventId))
            {
                Debug.LogWarning("[HorrorEventData] 이벤트 ID가 비어있습니다.");
                return false;
            }
            
            if (string.IsNullOrEmpty(eventName))
            {
                Debug.LogWarning($"[HorrorEventData] 이벤트 '{eventId}'의 이름이 비어있습니다.");
                return false;
            }
            
            if (jumpscareImage == null)
            {
                Debug.LogWarning($"[HorrorEventData] 이벤트 '{eventId}'의 점프스케어 이미지가 없습니다.");
                return false;
            }
            
            if (displayDuration <= 0)
            {
                Debug.LogWarning($"[HorrorEventData] 이벤트 '{eventId}'의 표시 시간이 유효하지 않습니다.");
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// 트리거 조건 확인
        /// </summary>
        public bool CanTrigger()
        {
            switch (triggerType)
            {
                case HorrorTriggerType.OnStageEnter:
                    return true; // 스테이지 진입 시 항상 가능
                    
                case HorrorTriggerType.OnInteraction:
                    return !string.IsNullOrEmpty(triggerCondition);
                    
                case HorrorTriggerType.OnTimer:
                    return triggerDelay > 0;
                    
                case HorrorTriggerType.OnCondition:
                    return !string.IsNullOrEmpty(triggerCondition);
                    
                case HorrorTriggerType.Manual:
                    return true; // 수동 트리거는 항상 가능
                    
                default:
                    Debug.LogWarning($"[HorrorEventData] 알 수 없는 트리거 타입: {triggerType}");
                    return false;
            }
        }
        
        /// <summary>
        /// 디버그 정보 반환
        /// </summary>
        public string GetDebugInfo()
        {
            return $"HorrorEvent[{eventId}] - {eventName} " +
                   $"(Trigger: {triggerType}, Duration: {displayDuration}s, Repeat: {canRepeat})";
        }
        
        /// <summary>
        /// 이벤트 복사본 생성 (런타임 수정용)
        /// </summary>
        public HorrorEventData CreateCopy()
        {
            var copy = new HorrorEventData();
            copy.eventId = this.eventId;
            copy.eventName = this.eventName;
            copy.description = this.description;
            copy.triggerType = this.triggerType;
            copy.triggerDelay = this.triggerDelay;
            copy.triggerCondition = this.triggerCondition;
            copy.jumpscareImage = this.jumpscareImage;
            copy.imagePosition = this.imagePosition;
            copy.imageSize = this.imageSize;
            copy.displayDuration = this.displayDuration;
            copy.horrorSound = this.horrorSound;
            copy.volume = this.volume;
            copy.enableScreenShake = this.enableScreenShake;
            copy.shakeIntensity = this.shakeIntensity;
            copy.shakeDuration = this.shakeDuration;
            copy.canRepeat = this.canRepeat;
            copy.cooldownTime = this.cooldownTime;
            return copy;
        }
    }
    
    /// <summary>
    /// 공포 이벤트 트리거 타입
    /// </summary>
    public enum HorrorTriggerType
    {
        OnStageEnter,       // 스테이지 진입 시
        OnInteraction,      // 특정 상호작용 시
        OnTimer,           // 시간 경과 시
        OnCondition,       // 특정 조건 달성 시
        Manual             // 수동 트리거
    }
}