using UnityEngine;
using System.Collections.Generic;
using AFKS.Shared.Interfaces;
using AFKS.ItemSystem;

namespace AFKS.StageSystem
{
    /// <summary>
    /// 스테이지 정보를 담는 ScriptableObject
    /// 개선됨: 관련 클래스들이 별도 파일로 분리되어 더 명확한 구조
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
                    Debug.LogWarning($"[StageData] 중복된 상호작용 ID 발견: {point.id}");
                }
            }
            
            // 공포 이벤트 ID 중복 체크
            HashSet<string> eventIds = new HashSet<string>();
            foreach (var horrorEvent in horrorEvents)
            {
                if (!string.IsNullOrEmpty(horrorEvent.eventId) && !eventIds.Add(horrorEvent.eventId))
                {
                    Debug.LogWarning($"[StageData] 중복된 공포 이벤트 ID 발견: {horrorEvent.eventId}");
                }
            }
            
            // 조건들의 유효성 검증
            foreach (var condition in unlockConditions)
            {
                if (!condition.IsValid())
                {
                    Debug.LogWarning($"[StageData] 잘못된 잠금 해제 조건이 발견되었습니다.");
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
        /// 특정 ID의 공포 이벤트 찾기
        /// </summary>
        public HorrorEventData GetHorrorEvent(string eventId)
        {
            foreach (var horrorEvent in horrorEvents)
            {
                if (horrorEvent.eventId == eventId)
                    return horrorEvent;
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
        
        /// <summary>
        /// 활성화된 상호작용 포인트들만 반환
        /// </summary>
        public List<InteractionPoint> GetActiveInteractionPoints()
        {
            List<InteractionPoint> activePoints = new List<InteractionPoint>();
            
            foreach (var point in interactionPoints)
            {
                if (point.isEnabled && point.CanInteract())
                {
                    activePoints.Add(point);
                }
            }
            
            return activePoints;
        }
        
        /// <summary>
        /// 트리거 가능한 공포 이벤트들만 반환
        /// </summary>
        public List<HorrorEventData> GetTriggableHorrorEvents()
        {
            List<HorrorEventData> triggableEvents = new List<HorrorEventData>();
            
            foreach (var horrorEvent in horrorEvents)
            {
                if (horrorEvent.IsValid() && horrorEvent.CanTrigger())
                {
                    triggableEvents.Add(horrorEvent);
                }
            }
            
            return triggableEvents;
        }
        
        /// <summary>
        /// 스테이지 완전성 검증
        /// </summary>
        public bool ValidateStageData()
        {
            bool isValid = true;
            
            // 기본 정보 검증
            if (string.IsNullOrEmpty(stageName))
            {
                Debug.LogError($"[StageData] 스테이지 {stageIndex}의 이름이 비어있습니다.");
                isValid = false;
            }
            
            if (backgroundImage == null)
            {
                Debug.LogWarning($"[StageData] 스테이지 {stageIndex}에 배경 이미지가 없습니다.");
            }
            
            // 상호작용 포인트 검증
            foreach (var point in interactionPoints)
            {
                if (string.IsNullOrEmpty(point.id))
                {
                    Debug.LogError($"[StageData] 스테이지 {stageIndex}에 ID가 없는 상호작용 포인트가 있습니다.");
                    isValid = false;
                }
            }
            
            // 공포 이벤트 검증
            foreach (var horrorEvent in horrorEvents)
            {
                if (!horrorEvent.IsValid())
                {
                    Debug.LogError($"[StageData] 스테이지 {stageIndex}에 잘못된 공포 이벤트가 있습니다: {horrorEvent.eventId}");
                    isValid = false;
                }
            }
            
            return isValid;
        }
        
        /// <summary>
        /// 디버그 정보 출력
        /// </summary>
        [ContextMenu("디버그 정보 출력")]
        public void PrintDebugInfo()
        {
            Debug.Log($"=== StageData Debug Info ===");
            Debug.Log($"Stage {stageIndex}: {stageName}");
            Debug.Log($"Interaction Points: {interactionPoints.Count}");
            Debug.Log($"Horror Events: {horrorEvents.Count}");
            Debug.Log($"Unlock Conditions: {unlockConditions.Count}");
            Debug.Log($"Is Valid: {ValidateStageData()}");
        }
    }
}