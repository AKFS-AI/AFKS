using UnityEngine;
using System.Collections.Generic;
using AFKS.Shared.Interfaces;
using AFKS.ItemSystem;

namespace AFKS.StageSystem
{
    /// <summary>
    /// 스테이지 정보를 담는 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "StageData", menuName = "AFKS/Stage/Stage Data")]
    public class StageData : ScriptableObject
    {
        [Header("📋 기본 정보")]
        [SerializeField, Tooltip("스테이지 번호 (0부터 시작)")] private int stageIndex;
        [SerializeField, Tooltip("스테이지 이름 (예: 병원 정문)")] private string stageName;
        [SerializeField, TextArea(3, 5), Tooltip("스테이지 설명 및 스토리")] private string stageDescription;
        
        [Header("🎨 비주얼")]
        [SerializeField, Tooltip("스테이지 배경 이미지")] private Sprite backgroundImage;
        [SerializeField, Tooltip("환경 색조 (기본: 흰색)")] private Color ambientColor = Color.white;
        [SerializeField, Tooltip("다크 모드 활성화 여부")] private bool enableDarkMode = false;
        
        [Header("🎵 오디오")]
        [SerializeField, Tooltip("배경 음악 (BGM)")] private AudioClip backgroundMusic;
        [SerializeField, Tooltip("환경 음향 (Ambient)")] private AudioClip ambientSound;
        [SerializeField, Range(0f, 1f), Tooltip("음악 볼륨 (0~1)")] private float musicVolume = 0.7f;
        [SerializeField, Range(0f, 1f), Tooltip("환경음 볼륨 (0~1)")] private float ambientVolume = 0.5f;
        
        [Header("🖱️ 상호작용")]
        [SerializeField, Tooltip("클릭 가능한 상호작용 포인트들")] private List<InteractionPoint> interactionPoints = new List<InteractionPoint>();
        
        [Header("😱 공포 이벤트")]
        [SerializeField, Tooltip("이 스테이지에서 발생하는 공포 이벤트들")] private List<HorrorEventData> horrorEvents = new List<HorrorEventData>();
        
        [Header("🚪 스테이지 흐름")]
        [SerializeField, Tooltip("이 스테이지 잠금 해제 조건들")] private List<StageCondition> unlockConditions = new List<StageCondition>();
        [SerializeField, Tooltip("게임 시작 시 기본 잠금 해제 여부")] private bool isUnlockedByDefault = false;
        [SerializeField, Tooltip("다음 스테이지 번호 (-1이면 마지막)")] private int nextStageIndex = -1;
        
        [Header("📦 추가 리소스")]
        [SerializeField, Tooltip("추가 이미지 리소스들")] private List<Sprite> additionalImages = new List<Sprite>();
        [SerializeField, Tooltip("추가 오디오 리소스들")] private List<AudioClip> additionalSounds = new List<AudioClip>();
        
        // === PROPERTIES ===
        public int StageIndex => stageIndex;
        public string StageName => stageName;
        public string StageDescription => stageDescription;
        public Sprite BackgroundImage => backgroundImage;
        public Color AmbientColor => ambientColor;
        public bool EnableDarkMode => enableDarkMode;
        
        public AudioClip BackgroundMusic => backgroundMusic;
        public AudioClip AmbientSound => ambientSound;
        public float MusicVolume => musicVolume;
        public float AmbientVolume => ambientVolume;
        
        public List<InteractionPoint> InteractionPoints => interactionPoints;
        public List<HorrorEventData> HorrorEvents => horrorEvents;
        public List<StageCondition> UnlockConditions => unlockConditions;
        public bool IsUnlockedByDefault => isUnlockedByDefault;
        public int NextStageIndex => nextStageIndex;
        
        public List<Sprite> AdditionalImages => additionalImages;
        public List<AudioClip> AdditionalSounds => additionalSounds;
        
        // === VALIDATION ===
        private void OnValidate()
        {
            stageIndex = Mathf.Max(0, stageIndex);
            musicVolume = Mathf.Clamp01(musicVolume);
            ambientVolume = Mathf.Clamp01(ambientVolume);
            
            // 상호작용 포인트들의 ID 중복 체크
            HashSet<string> ids = new HashSet<string>();
            foreach (var point in interactionPoints)
            {
                if (!string.IsNullOrEmpty(point.id) && !ids.Add(point.id))
                {
                    Debug.LogWarning($"[StageData] Duplicate interaction ID found: {point.id}");
                }
            }
        }
        
        // === UTILITY METHODS ===
        
        /// <summary>
        /// 특정 ID의 상호작용 포인트 찾기
        /// </summary>
        public InteractionPoint GetInteractionPoint(string id)
        {
            foreach (var point in interactionPoints)
            {
                if (point.id == id)
                    return point;
            }
            return null;
        }
        
        /// <summary>
        /// 스테이지 잠금 해제 조건 확인
        /// </summary>
        public bool CheckUnlockConditions()
        {
            if (isUnlockedByDefault) return true;
            
            foreach (var condition in unlockConditions)
            {
                if (!condition.IsConditionMet())
                    return false;
            }
            
            return true;
        }
    }
    
    // === INTERACTION POINT ===
    [System.Serializable]
    public class InteractionPoint
    {
        [Header("Basic Info")]
        public string id;
        public string displayName;
        [TextArea(2, 3)] public string description;
        
        [Header("Position")]
        public Vector2 position;
        public Vector2 size = Vector2.one;
        
        [Header("Interaction")]
        public InteractionType interactionType;
        public bool isEnabled = true;
        public bool isVisible = true;
        
        [Header("Visual Feedback")]
        public Sprite hoverSprite;
        public Color hoverColor = Color.white;
        public float hoverScale = 1.1f;
        
        [Header("Audio")]
        public AudioClip interactionSound;
        public AudioClip hoverSound;
        
        [Header("Conditions")]
        public List<string> requiredItems = new List<string>();
        public List<StageCondition> enableConditions = new List<StageCondition>();
        
        [Header("Results")]
        public InteractionResult result;
        
        /// <summary>
        /// 상호작용 가능 여부 확인
        /// </summary>
        public bool CanInteract()
        {
            if (!isEnabled) return false;
            
            // 필요 아이템 체크 (나중에 인벤토리 시스템과 연동)
            // foreach (string itemId in requiredItems)
            // {
            //     if (!InventoryManager.Instance.HasItem(itemId))
            //         return false;
            // }
            
            // 활성화 조건 체크
            foreach (var condition in enableConditions)
            {
                if (!condition.IsConditionMet())
                    return false;
            }
            
            return true;
        }
    }
    
    // === INTERACTION RESULT ===
    [System.Serializable]
    public class InteractionResult
    {
        [Header("Result Type")]
        public int resultType = 0; // 0: Normal, 1: Item, 2: Stage Change
        [TextArea(2, 3)] public string message;
        #pragma warning disable CS0618 // Type or member is obsolete
        public ItemData itemToAdd;
        #pragma warning restore CS0618 // Type or member is obsolete
        public int nextStageIndex = -1;
        public AudioClip soundEffect;
        public string[] additionalActions;
        
        [Header("Legacy Fields")]
        public bool giveItem = false;
        public string itemId;
        
        [Header("Stage")]
        public bool changeStage = false;
        public int targetStageIndex = -1;
        
        [Header("Horror")]
        public bool triggerHorrorEvent = false;
        public string horrorEventId;
        
        [Header("Dialog")]
        public bool showDialog = false;
        [TextArea(3, 5)] public string dialogText;
        
        [Header("Audio")]
        public bool playAudio = false;
        public AudioClip audioClip;
        
        [Header("Custom")]
        public bool executeCustomAction = false;
        public string customActionId;
    }
    
    // === HORROR EVENT DATA ===
    [System.Serializable]
    public class HorrorEventData
    {
        [Header("Basic Info")]
        public string eventId;
        public string eventName;
        [TextArea(2, 3)] public string description;
        
        [Header("Trigger")]
        public HorrorTriggerType triggerType;
        public float triggerDelay = 0f;
        public string triggerCondition; // 조건 ID
        
        [Header("Visual")]
        public Sprite jumpscareImage;
        public Vector2 imagePosition = Vector2.zero;
        public Vector2 imageSize = Vector2.one;
        public float displayDuration = 2f;
        
        [Header("Audio")]
        public AudioClip horrorSound;
        public float volume = 0.8f;
        
        [Header("Effects")]
        public bool enableScreenShake = true;
        public float shakeIntensity = 0.5f;
        public float shakeDuration = 1f;
        
        [Header("Settings")]
        public bool canRepeat = false;
        public float cooldownTime = 10f;
    }
    
    // === STAGE CONDITION ===
    [System.Serializable]
    public class StageCondition
    {
        [Header("Condition")]
        public ConditionType conditionType;
        public string targetId;
        public string requiredValue;
        
        [Header("Logic")]
        public bool invertCondition = false;
        
        /// <summary>
        /// 조건 확인 (실제 게임 상태와 연동)
        /// </summary>
        public bool IsConditionMet()
        {
            bool result = false;
            
            switch (conditionType)
            {
                case ConditionType.HasItem:
                    // ItemManager 사용 (열쇠, 손전등 전용)
                    if (AFKS.ItemSystem.ItemManager.Instance != null)
                    {
                        result = AFKS.ItemSystem.ItemManager.Instance.HasKeyItem(targetId);
                    }
                    else
                    {
                        result = false; // 관리자가 없으면 false
                    }
                    break;
                    
                case ConditionType.StageCompleted:
                    if (AFKS.Core.GameManager.Instance != null && int.TryParse(targetId, out int stageIndex))
                    {
                        result = AFKS.StageSystem.StageManager.Instance?.CurrentStageIndex >= stageIndex;
                    }
                    else
                    {
                        result = false;
                    }
                    break;
                    
                case ConditionType.VariableEquals:
                    // 게임 변수 시스템이 구현되면 연동, 현재는 PlayerPrefs 사용
                    string savedValue = UnityEngine.PlayerPrefs.GetString($"GameVar_{targetId}", "");
                    result = savedValue == requiredValue;
                    break;
                    
                case ConditionType.Always:
                    result = true;
                    break;
                    
                case ConditionType.Never:
                    result = false;
                    break;
                    
                default:
                    UnityEngine.Debug.LogWarning($"[StageCondition] Unknown condition type: {conditionType}");
                    result = false;
                    break;
            }
            
            return invertCondition ? !result : result;
        }
    }
    
    // === ENUMS ===
    public enum HorrorTriggerType
    {
        OnStageEnter,       // 스테이지 진입 시
        OnInteraction,      // 특정 상호작용 시
        OnTimer,           // 시간 경과 시
        OnCondition,       // 특정 조건 달성 시
        Manual             // 수동 트리거
    }
    
    public enum ConditionType
    {
        Always,            // 항상 참
        Never,             // 항상 거짓
        HasItem,           // 아이템 보유
        StageCompleted,    // 스테이지 완료
        VariableEquals     // 변수 값 일치
    }
}