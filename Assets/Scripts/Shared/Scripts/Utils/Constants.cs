namespace AFKS.Shared.Utils
{
    /// <summary>
    /// 게임 전역 상수
    /// </summary>
    public static class Constants
    {
        // === GAME SETTINGS ===
        public const int TARGET_FRAME_RATE = 60;
        public const float MEMORY_THRESHOLD_MB = 8192f; // PC 게임 기준 8GB 임계값 (16GB RAM 환경 고려)
        public const int MAX_PRELOAD_STAGES = 2;
        
        // === STAGE SETTINGS ===
        public const int TOTAL_STAGES = 6;
        public const float STAGE_TRANSITION_DURATION = 1f;
        public const float FADE_DURATION = 0.5f;
        
        // === HORROR SETTINGS ===
        public const float JUMPSCARE_DURATION = 2f;
        public const float HORROR_COOLDOWN = 5f;
        public const int MAX_CONCURRENT_HORROR_EVENTS = 1;
        
        // === AUDIO SETTINGS ===
        public const float DEFAULT_BGM_VOLUME = 0.7f;
        public const float DEFAULT_SFX_VOLUME = 0.8f;
        public const float AUDIO_FADE_DURATION = 1f;
        
        // === UI SETTINGS ===
        public const float UI_ANIMATION_DURATION = 0.3f;
        public const int INVENTORY_MAX_SLOTS = 2; // 열쇠, 손전등 전용
        
        // === INPUT SETTINGS ===
        public const float DOUBLE_CLICK_TIME = 0.3f;
        public const float HOLD_TIME_THRESHOLD = 0.5f;
        
        // === PATHS ===
        public const string STAGES_DATA_PATH = "StageData/";
        public const string ITEMS_DATA_PATH = "ItemData/";
        public const string HORROR_DATA_PATH = "HorrorData/";
        public const string AUDIO_DATA_PATH = "AudioData/";
        
        // === LAYER NAMES ===
        public const string UI_LAYER = "UI";
        public const string INTERACTABLE_LAYER = "Interactable";
        public const string BACKGROUND_LAYER = "Background";
        
        // === TAGS ===
        public const string PLAYER_TAG = "Player";
        public const string INTERACTABLE_TAG = "Interactable";
        public const string HORROR_TRIGGER_TAG = "HorrorTrigger";

        // === INTERACTION IDS (중앙 관리) ===
        public static class InteractionIds
        {
            public const string HospitalDoor = "hospital_door";
            public const string HospitalDoorChain = "hospital_door_chain";
            public const string CrossPickup = "cross_pickup";
            public const string CctvMonitor = "cctv_monitor";
        }

        // === KEY ITEM IDS (중앙 관리) ===
        public static class ItemIds
        {
            public const string Key = "key";
            public const string Flashlight = "flashlight";
        }
    }
}