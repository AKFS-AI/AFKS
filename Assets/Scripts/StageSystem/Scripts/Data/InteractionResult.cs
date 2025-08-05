using UnityEngine;
using AFKS.ItemSystem;

namespace AFKS.StageSystem
{
    /// <summary>
    /// 상호작용 실행 결과 데이터
    /// </summary>
    [System.Serializable]
    public class InteractionResult
    {
        [Header("📋 결과 타입")]
        [SerializeField, Tooltip("0: 일반, 1: 아이템, 2: 스테이지 변경")] 
        public int resultType = 0;
        [SerializeField, TextArea(2, 3)] public string message;
        
        [Header("🎵 사운드")]
        [SerializeField] public AudioClip soundEffect;
        [SerializeField] public string[] additionalActions;
        
        [Header("📦 아이템 (Legacy - 호환성)")]
#pragma warning disable CS0618 // Type or member is obsolete
        [SerializeField] public ItemData itemToAdd;
#pragma warning restore CS0618 // Type or member is obsolete
        
        [Header("🎒 아이템 관련 (Legacy)")]
        [SerializeField] public bool giveItem = false;
        [SerializeField] public string itemId;
        
        [Header("🚪 스테이지 변경")]
        [SerializeField] public bool changeStage = false;
        [SerializeField] public int targetStageIndex = -1;
        [SerializeField] public int nextStageIndex = -1; // 호환성
        
        [Header("😱 공포 이벤트")]
        [SerializeField] public bool triggerHorrorEvent = false;
        [SerializeField] public string horrorEventId;
        
        [Header("💬 대화")]
        [SerializeField] public bool showDialog = false;
        [SerializeField, TextArea(3, 5)] public string dialogText;
        
        [Header("🔊 오디오 재생")]
        [SerializeField] public bool playAudio = false;
        [SerializeField] public AudioClip audioClip;
        
        [Header("⚙️ 커스텀 액션")]
        [SerializeField] public bool executeCustomAction = false;
        [SerializeField] public string customActionId;
        
        /// <summary>
        /// 상호작용 결과 실행
        /// </summary>
        public void ExecuteResult()
        {
            // 메시지 표시
            if (!string.IsNullOrEmpty(message))
            {
                Debug.Log($"[InteractionResult] {message}");
                // UI 메시지 시스템과 연동 시 여기에 추가
            }
            
            // 사운드 재생
            if (soundEffect != null && AFKS.AudioSystem.AudioManager.Instance != null)
            {
                AFKS.AudioSystem.AudioManager.Instance.PlaySFX(soundEffect);
            }
            
            // 아이템 지급 (새로운 방식 - itemId 사용)
            if (giveItem && !string.IsNullOrEmpty(itemId))
            {
                if (AFKS.ItemSystem.ItemManager.Instance != null)
                {
                    AFKS.ItemSystem.ItemManager.Instance.ObtainKeyItem(itemId);
                }
            }
            
            // 아이템 지급 (Legacy 방식 - itemToAdd 사용, 호환성)
#pragma warning disable CS0618 // Type or member is obsolete
            if (itemToAdd != null)
            {
                Debug.Log($"[InteractionResult] Legacy 아이템 지급: {itemToAdd.name}");
                // 향후 InventoryManager와 연동 시 여기에 추가
                // InventoryManager.Instance.AddItem(itemToAdd);
            }
#pragma warning restore CS0618 // Type or member is obsolete
            
            // 스테이지 변경
            if (changeStage && targetStageIndex >= 0)
            {
                if (AFKS.StageSystem.StageManager.Instance != null)
                {
                    AFKS.StageSystem.StageManager.Instance.ChangeStage(targetStageIndex);
                }
            }
            
            // 공포 이벤트 트리거
            if (triggerHorrorEvent && !string.IsNullOrEmpty(horrorEventId))
            {
                if (AFKS.HorrorSystem.HorrorEventManager.Instance != null)
                {
                    AFKS.HorrorSystem.HorrorEventManager.Instance.TriggerEvent(horrorEventId);
                }
            }
            
            // 대화 표시
            if (showDialog && !string.IsNullOrEmpty(dialogText))
            {
                Debug.Log($"[Dialog] {dialogText}");
                // 대화 시스템과 연동 시 여기에 추가
            }
            
            // 오디오 재생
            if (playAudio && audioClip != null)
            {
                if (AFKS.AudioSystem.AudioManager.Instance != null)
                {
                    AFKS.AudioSystem.AudioManager.Instance.PlaySFX(audioClip);
                }
            }
            
            // 커스텀 액션 실행
            if (executeCustomAction && !string.IsNullOrEmpty(customActionId))
            {
                ExecuteCustomAction(customActionId);
            }
        }
        
        /// <summary>
        /// 커스텀 액션 실행 (확장 가능)
        /// </summary>
        private void ExecuteCustomAction(string actionId)
        {
            switch (actionId.ToLower())
            {
                case "unlock_door":
                    Debug.Log("[CustomAction] 문이 열렸습니다!");
                    break;
                case "toggle_light":
                    Debug.Log("[CustomAction] 조명이 전환되었습니다!");
                    break;
                default:
                    Debug.LogWarning($"[CustomAction] 알 수 없는 액션: {actionId}");
                    break;
            }
        }
        
        /// <summary>
        /// 결과 유효성 검증
        /// </summary>
        public bool IsValid()
        {
            if (changeStage && targetStageIndex < 0)
            {
                Debug.LogWarning("[InteractionResult] 스테이지 변경이 활성화되었지만 대상 스테이지가 유효하지 않습니다.");
                return false;
            }
            
            if (triggerHorrorEvent && string.IsNullOrEmpty(horrorEventId))
            {
                Debug.LogWarning("[InteractionResult] 공포 이벤트 트리거가 활성화되었지만 이벤트 ID가 없습니다.");
                return false;
            }
            
            return true;
        }
    }
}