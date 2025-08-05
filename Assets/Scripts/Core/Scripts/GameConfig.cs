using UnityEngine;

namespace AFKS.Core
{
    /// <summary>
    /// 게임 전체 설정을 담는 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "GameConfig", menuName = "AFKS/Core/Game Config")]
    public class GameConfig : ScriptableObject
    {
        [Header("⚡ 성능 설정")]
        [SerializeField, Tooltip("목표 프레임레이트 (60 권장)")] private int targetFrameRate = 60;
        [SerializeField, Tooltip("메모리 사용량 임계치 (MB)")] private float memoryThresholdMB = 100f;
        [SerializeField, Tooltip("수직 동기화 활성화")] private bool enableVSync = true;
        
        [Header("🎮 게임 설정")]
        [SerializeField, Tooltip("자동 저장 기능 활성화")] private bool enableAutosave = true;
        [SerializeField, Tooltip("자동 저장 간격 (초)")] private float autosaveInterval = 30f;
        [SerializeField, Tooltip("최대 세이브 슬롯 개수")] private int maxSaveSlots = 3;
        
        [Header("🎵 오디오 설정")]
        [SerializeField, Range(0f, 1f), Tooltip("기본 BGM 볼륨")] private float defaultBGMVolume = 0.7f;
        [SerializeField, Range(0f, 1f), Tooltip("기본 효과음 볼륨")] private float defaultSFXVolume = 0.8f;
        [SerializeField, Tooltip("오디오 이펙트 활성화")] private bool enableAudioEffects = true;
        [SerializeField, Tooltip("최대 오디오 캐시 크기")] private int maxAudioCacheSize = 50;
        
        [Header("😱 공포 설정")]
        [SerializeField, Tooltip("공포 이벤트 간 최소 대기 시간 (초)")] private float horrorCooldown = 5f;
        [SerializeField, Tooltip("동시 실행 가능한 공포 이벤트 최대 개수")] private int maxConcurrentHorrorEvents = 1;
        [SerializeField, Tooltip("점프스케어 효과 활성화")] private bool enableJumpscares = true;
        
        [Header("🖼️ UI 설정")]
        [SerializeField, Tooltip("UI 애니메이션 지속 시간 (초)")] private float uiAnimationDuration = 0.3f;
        [SerializeField, Tooltip("UI 애니메이션 활성화")] private bool enableUIAnimations = true;
        [SerializeField, Tooltip("최대 아이템 슬롯 개수")] private int maxItemSlots = 2;
        [SerializeField, Tooltip("햅틱 피드백 활성화 (모바일)")] private bool enableHapticFeedback = false;
        
        [Header("🖱️ 입력 설정")]
        [SerializeField, Tooltip("더블클릭 인식 시간 (초)")] private float doubleClickTime = 0.3f;
        [SerializeField, Tooltip("길게 누르기 인식 시간 (초)")] private float holdTimeThreshold = 0.5f;
        [SerializeField, Tooltip("터치 입력 활성화")] private bool enableTouchInput = true;
        
        [Header("🚪 스테이지 설정")]
        [SerializeField, Tooltip("미리 로드할 스테이지 개수")] private int maxPreloadStages = 2;
        [SerializeField, Tooltip("스테이지 전환 애니메이션 시간 (초)")] private float stageTransitionDuration = 1f;
        [SerializeField, Tooltip("스테이지 미리 로드 활성화")] private bool enablePreloading = true;
        
        // === PROPERTIES ===
        public int TargetFrameRate => targetFrameRate;
        public float MemoryThresholdMB => memoryThresholdMB;
        public bool EnableVSync => enableVSync;
        
        public bool EnableAutosave => enableAutosave;
        public float AutosaveInterval => autosaveInterval;
        public int MaxSaveSlots => maxSaveSlots;
        
        public float DefaultBGMVolume => defaultBGMVolume;
        public float DefaultSFXVolume => defaultSFXVolume;
        public bool EnableAudioEffects => enableAudioEffects;
        public int MaxAudioCacheSize => maxAudioCacheSize;
        
        public float HorrorCooldown => horrorCooldown;
        public int MaxConcurrentHorrorEvents => maxConcurrentHorrorEvents;
        public bool EnableJumpscares => enableJumpscares;
        
        public float UIAnimationDuration => uiAnimationDuration;
        public bool EnableUIAnimations => enableUIAnimations;
        public int MaxItemSlots => maxItemSlots;
        public bool EnableHapticFeedback => enableHapticFeedback;
        
        public float DoubleClickTime => doubleClickTime;
        public float HoldTimeThreshold => holdTimeThreshold;
        public bool EnableTouchInput => enableTouchInput;
        
        public int MaxPreloadStages => maxPreloadStages;
        public float StageTransitionDuration => stageTransitionDuration;
        public bool EnablePreloading => enablePreloading;
        
        // === VALIDATION ===
        private void OnValidate()
        {
            targetFrameRate = Mathf.Clamp(targetFrameRate, 30, 120);
            memoryThresholdMB = Mathf.Max(memoryThresholdMB, 50f);
            
            autosaveInterval = Mathf.Max(autosaveInterval, 10f);
            maxSaveSlots = Mathf.Clamp(maxSaveSlots, 1, 10);
            
            defaultBGMVolume = Mathf.Clamp01(defaultBGMVolume);
            defaultSFXVolume = Mathf.Clamp01(defaultSFXVolume);
            
            horrorCooldown = Mathf.Max(horrorCooldown, 1f);
            maxConcurrentHorrorEvents = Mathf.Max(maxConcurrentHorrorEvents, 1);
            
            uiAnimationDuration = Mathf.Max(uiAnimationDuration, 0.1f);
            
            doubleClickTime = Mathf.Clamp(doubleClickTime, 0.1f, 1f);
            holdTimeThreshold = Mathf.Clamp(holdTimeThreshold, 0.1f, 2f);
            
            maxPreloadStages = Mathf.Clamp(maxPreloadStages, 1, 5);
            stageTransitionDuration = Mathf.Max(stageTransitionDuration, 0.1f);
        }
    }
}