using UnityEngine;
using AFKS.Shared.Events;
using AFKS.Shared.Interfaces;

namespace AFKS.ItemSystem
{
    /// <summary>
    /// 오직 열쇠와 손전등만 관리하는 간단한 아이템 매니저
    /// </summary>
    public class ItemManager : MonoBehaviour, ISaveable
    {
        [Header("🔑 아이템 상태")]
        [SerializeField, Tooltip("열쇠 보유 여부")] private bool hasKey = false;
        [SerializeField, Tooltip("손전등 보유 여부")] private bool hasFlashlight = false;
        
        [Header("🎵 오디오")]
        [SerializeField, Tooltip("열쇠 획득 시 사운드")] private AudioClip keyPickupSound;
        [SerializeField, Tooltip("손전등 획득 시 사운드")] private AudioClip flashlightPickupSound;
        [SerializeField, Tooltip("아이템 사용 시 사운드")] private AudioClip useSound;
        
        [Header("⚙️ 설정")]
        [SerializeField, Tooltip("아이템 변경 시 자동 저장 여부")] private bool autoSave = true;
        
        // === RUNTIME EVENTS ===
        public static readonly GameEvent<string> OnKeyItemObtained = new GameEvent<string>();
        public static readonly GameEvent<string> OnKeyItemUsed = new GameEvent<string>();
        public static readonly GameEvent OnInventoryChanged = new GameEvent();
        
        // === PROPERTIES ===
        public string SaveID => "ItemManager";
        public bool HasKey => hasKey;
        public bool HasFlashlight => hasFlashlight;
        public bool HasAllItems => hasKey && hasFlashlight;
        public int ItemCount => (hasKey ? 1 : 0) + (hasFlashlight ? 1 : 0);
        public int ObtainedItemCount => ItemCount; // 호환성
        
        // === SINGLETON ACCESS ===
        private static ItemManager instance;
        public static ItemManager Instance
        {
            get
            {
                if (instance == null)
                    instance = FindFirstObjectByType<ItemManager>();
                return instance;
            }
        }
        
        // === UNITY LIFECYCLE ===
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                LoadSavedState();
                Debug.Log("[아이템매니저] 초기화 완료 - 열쇠: " + hasKey + ", 손전등: " + hasFlashlight);
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }
        
        // === ITEM MANAGEMENT ===
        
        /// <summary>
        /// 열쇠 획득
        /// </summary>
        public void ObtainKey()
        {
            if (hasKey)
            {
                Debug.LogWarning("[아이템매니저] 이미 열쇠를 보유하고 있습니다");
                return;
            }
            
            hasKey = true;
            PlayPickupSound(keyPickupSound);
            
            Debug.Log("[아이템매니저] 열쇠 획득!");
            
            // 이벤트 발생
            OnKeyItemObtained.Raise("key");
            OnInventoryChanged.Raise();
            
            // 자동 저장
            if (autoSave)
                SaveState();
        }
        
        /// <summary>
        /// 손전등 획득
        /// </summary>
        public void ObtainFlashlight()
        {
            if (hasFlashlight)
            {
                Debug.LogWarning("[아이템매니저] 이미 손전등을 보유하고 있습니다");
                return;
            }
            
            hasFlashlight = true;
            PlayPickupSound(flashlightPickupSound);
            
            Debug.Log("[아이템매니저] 손전등 획득!");
            
            // 이벤트 발생
            OnKeyItemObtained.Raise("flashlight");
            OnInventoryChanged.Raise();
            
            // 자동 저장
            if (autoSave)
                SaveState();
        }
        
        /// <summary>
        /// 아이템 ID로 획득 (호환성)
        /// </summary>
        public void ObtainKeyItem(string itemId)
        {
            switch (itemId.ToLower())
            {
                case "key":
                case "열쇠":
                    ObtainKey();
                    break;
                case "flashlight":
                case "손전등":
                    ObtainFlashlight();
                    break;
                default:
                    Debug.LogWarning($"[아이템매니저] 알 수 없는 아이템 ID: {itemId}");
                    break;
            }
        }
        
        /// <summary>
        /// 열쇠 사용 (실제로는 보유 여부만 확인)
        /// </summary>
        public bool UseKey()
        {
            if (!hasKey)
            {
                Debug.LogWarning("[아이템매니저] 열쇠를 보유하지 않았습니다");
                return false;
            }
            
            PlayUseSound();
            Debug.Log("[아이템매니저] 열쇠 사용");
            
            OnKeyItemUsed.Raise("key");
            return true;
        }
        
        /// <summary>
        /// 손전등 사용 (실제로는 보유 여부만 확인)
        /// </summary>
        public bool UseFlashlight()
        {
            if (!hasFlashlight)
            {
                Debug.LogWarning("[아이템매니저] 손전등을 보유하지 않았습니다");
                return false;
            }
            
            PlayUseSound();
            Debug.Log("[아이템매니저] 손전등 사용");
            
            OnKeyItemUsed.Raise("flashlight");
            return true;
        }
        
        /// <summary>
        /// 아이템 보유 여부 확인
        /// </summary>
        public bool HasKeyItem(string itemId)
        {
            switch (itemId.ToLower())
            {
                case "key":
                case "열쇠":
                    return hasKey;
                case "flashlight":
                case "손전등":
                    return hasFlashlight;
                default:
                    Debug.LogWarning($"[아이템매니저] 알 수 없는 아이템 ID: {itemId}");
                    return false;
            }
        }
        
        /// <summary>
        /// 아이템 사용 (호환성)
        /// </summary>
        public bool UseKeyItem(string itemId, int stageIndex = -1)
        {
            switch (itemId.ToLower())
            {
                case "key":
                case "열쇠":
                    return UseKey();
                case "flashlight":
                case "손전등":
                    return UseFlashlight();
                default:
                    Debug.LogWarning($"[아이템매니저] 알 수 없는 아이템 ID: {itemId}");
                    return false;
            }
        }
        
        // === UTILITY METHODS ===
        
        /// <summary>
        /// 획득 사운드 재생
        /// </summary>
        private void PlayPickupSound(AudioClip clip)
        {
            if (clip != null && AFKS.AudioSystem.AudioManager.Instance != null)
            {
                AFKS.AudioSystem.AudioManager.Instance.PlaySFX(clip);
            }
        }
        
        /// <summary>
        /// 사용 사운드 재생
        /// </summary>
        private void PlayUseSound()
        {
            if (useSound != null && AFKS.AudioSystem.AudioManager.Instance != null)
            {
                AFKS.AudioSystem.AudioManager.Instance.PlaySFX(useSound);
            }
        }
        
        /// <summary>
        /// 모든 아이템 리셋 (디버그용)
        /// </summary>
        [ContextMenu("디버그: 모든 아이템 리셋")]
        public void DebugResetAllKeyItems()
        {
            hasKey = false;
            hasFlashlight = false;
            
            OnInventoryChanged.Raise();
            
            if (autoSave)
                SaveState();
                
            Debug.Log("[아이템매니저] 모든 아이템 리셋 완료");
        }
        
        /// <summary>
        /// 모든 아이템 획득 (디버그용)
        /// </summary>
        [ContextMenu("디버그: 모든 아이템 획득")]
        public void DebugObtainAllKeyItems()
        {
            hasKey = true;
            hasFlashlight = true;
            
            OnInventoryChanged.Raise();
            
            if (autoSave)
                SaveState();
                
            Debug.Log("[아이템매니저] 모든 아이템 획득 완료");
        }
        
        // === SAVE SYSTEM ===
        
        /// <summary>
        /// ISaveable 인터페이스 구현: 저장 데이터 반환
        /// </summary>
        public string GetSaveData()
        {
            var saveData = new SaveData
            {
                hasKey = this.hasKey,
                hasFlashlight = this.hasFlashlight
            };
            return JsonUtility.ToJson(saveData);
        }
        
        /// <summary>
        /// ISaveable 인터페이스 구현: 저장 데이터 로드
        /// </summary>
        public void LoadSaveData(string data)
        {
            try
            {
                if (string.IsNullOrEmpty(data))
                {
                    // 기본값으로 초기화
                    hasKey = false;
                    hasFlashlight = false;
                    return;
                }
                
                var saveData = JsonUtility.FromJson<SaveData>(data);
                hasKey = saveData.hasKey;
                hasFlashlight = saveData.hasFlashlight;
                
                Debug.Log("[아이템매니저] 저장 데이터 로드 성공");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[아이템매니저] 저장 데이터 로드 실패: {e.Message}");
                // 기본값으로 초기화
                hasKey = false;
                hasFlashlight = false;
            }
        }
        
        /// <summary>
        /// 기존 PlayerPrefs 방식 저장 (호환성)
        /// </summary>
        public void SaveState()
        {
            PlayerPrefs.SetInt("ItemManager_HasKey", hasKey ? 1 : 0);
            PlayerPrefs.SetInt("ItemManager_HasFlashlight", hasFlashlight ? 1 : 0);
            PlayerPrefs.Save();
        }
        
        /// <summary>
        /// 기존 PlayerPrefs 방식 로드 (호환성)
        /// </summary>
        public void LoadState()
        {
            LoadSavedState();
        }
        
        private void LoadSavedState()
        {
            try
            {
                hasKey = PlayerPrefs.GetInt("ItemManager_HasKey", 0) == 1;
                hasFlashlight = PlayerPrefs.GetInt("ItemManager_HasFlashlight", 0) == 1;
                
                Debug.Log("[아이템매니저] 저장 데이터 로드 성공");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[아이템매니저] 저장 데이터 로드 실패: {e.Message}");
                // 기본값으로 초기화
                hasKey = false;
                hasFlashlight = false;
            }
        }
        
        // === SAVE DATA STRUCTURE ===
        
        /// <summary>
        /// JSON 직렬화를 위한 저장 데이터 구조
        /// </summary>
        [System.Serializable]
        private class SaveData
        {
            public bool hasKey;
            public bool hasFlashlight;
        }
        
        /// <summary>
        /// 게임 상태 정보 가져오기
        /// </summary>
        public string GetStateInfo()
        {
            return $"열쇠: {(hasKey ? "보유" : "미보유")}, 손전등: {(hasFlashlight ? "보유" : "미보유")}";
        }
    }
}