using UnityEngine;

namespace AFKS.InventorySystem
{
    /// <summary>
    /// 아이템 정보를 담는 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "ItemData", menuName = "AFKS/Inventory/Item Data")]
    public class ItemData : ScriptableObject
    {
        [Header("📋 기본 정보")]
        [SerializeField, Tooltip("고유 아이템 ID (예: cross, flashlight)")] private string itemId;
        [SerializeField, Tooltip("화면에 표시될 아이템 이름")] private string itemName;
        [SerializeField, TextArea(3, 5), Tooltip("아이템 설명 및 용도")] private string description;
        
        [Header("🎨 비주얼")]
        [SerializeField, Tooltip("인벤토리에 표시될 아이콘 (512x512)")] private Sprite icon;
        [SerializeField, Tooltip("상세보기용 이미지 (1024x1024)")] private Sprite detailImage;
        [SerializeField, Tooltip("아이콘 색조 (기본: 흰색)")] private Color iconTint = Color.white;
        
        [Header("🏷️ 타입")]
        [SerializeField, Tooltip("아이템 분류 (Key, Consumable, Quest 등)")] private ItemType itemType;
        [SerializeField, Tooltip("아이템 희귀도")] private ItemRarity rarity = ItemRarity.Common;
        [SerializeField, Tooltip("같은 아이템을 여러 개 소지 가능한지")] private bool isStackable = false;
        [SerializeField, Tooltip("최대 소지 가능 개수")] private int maxStackSize = 1;
        
        [Header("🔧 사용법")]
        [SerializeField, Tooltip("사용할 수 있는 아이템인지")] private bool isUsable = false;
        [SerializeField, Tooltip("사용 시 소모되는 아이템인지")] private bool isConsumable = false;
        [SerializeField, Tooltip("스토리 진행에 필수인 중요 아이템인지")] private bool isKeyItem = false;
        
        [Header("🎵 오디오")]
        [SerializeField, Tooltip("아이템 획득 시 재생되는 소리")] private AudioClip pickupSound;
        [SerializeField, Tooltip("아이템 사용 시 재생되는 소리")] private AudioClip useSound;
        [SerializeField, Tooltip("아이템 버릴 때 재생되는 소리")] private AudioClip dropSound;
        
        [Header("✨ 효과")]
        [SerializeField, Tooltip("아이템 사용 시 발생하는 효과들")] private ItemEffect[] effects;
        
        [Header("📝 조건")]
        [SerializeField, Tooltip("이 아이템을 사용하기 위해 필요한 다른 아이템들")] private string[] requiredItems;
        [SerializeField, Tooltip("이 아이템과 함께 사용할 수 없는 아이템들")] private string[] incompatibleItems;
        
        // === PROPERTIES ===
        public string ItemId => itemId;
        public string ItemName => itemName;
        public string Description => description;
        public Sprite Icon => icon;
        public Sprite DetailImage => detailImage;
        public Color IconTint => iconTint;
        public ItemType ItemType => itemType;
        public ItemRarity Rarity => rarity;
        public bool IsStackable => isStackable;
        public int MaxStackSize => maxStackSize;
        public bool IsUsable => isUsable;
        public bool IsConsumable => isConsumable;
        public bool IsKeyItem => isKeyItem;
        public AudioClip PickupSound => pickupSound;
        public AudioClip UseSound => useSound;
        public AudioClip DropSound => dropSound;
        public ItemEffect[] Effects => effects;
        public string[] RequiredItems => requiredItems;
        public string[] IncompatibleItems => incompatibleItems;
        
        // === VALIDATION ===
        private void OnValidate()
        {
            // ID 자동 생성 (비어있을 때만)
            if (string.IsNullOrEmpty(itemId))
            {
                itemId = name.ToLower().Replace(" ", "_");
            }
            
            // 스택 크기 검증
            if (isStackable)
            {
                maxStackSize = Mathf.Max(1, maxStackSize);
            }
            else
            {
                maxStackSize = 1;
            }
            
            // 소비 아이템은 반드시 사용 가능해야 함
            if (isConsumable && !isUsable)
            {
                isUsable = true;
            }
        }
        
        // === UTILITY METHODS ===
        
        /// <summary>
        /// 아이템 사용 가능 여부 확인
        /// </summary>
        /// <param name="currentItems">현재 보유 아이템 목록</param>
        /// <returns>사용 가능하면 true</returns>
        public bool CanUse(string[] currentItems)
        {
            if (!isUsable) return false;
            
            // 필요 아이템 확인
            foreach (string requiredItem in requiredItems)
            {
                bool hasRequired = false;
                foreach (string currentItem in currentItems)
                {
                    if (currentItem == requiredItem)
                    {
                        hasRequired = true;
                        break;
                    }
                }
                
                if (!hasRequired) return false;
            }
            
            // 호환되지 않는 아이템 확인
            foreach (string incompatibleItem in incompatibleItems)
            {
                foreach (string currentItem in currentItems)
                {
                    if (currentItem == incompatibleItem)
                        return false;
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// 아이템 효과 실행
        /// </summary>
        public void ExecuteEffects()
        {
            foreach (var effect in effects)
            {
                effect.Execute();
            }
        }
        
        /// <summary>
        /// 희귀도에 따른 색상 반환
        /// </summary>
        /// <returns>희귀도 색상</returns>
        public Color GetRarityColor()
        {
            switch (rarity)
            {
                case ItemRarity.Common: return Color.white;
                case ItemRarity.Uncommon: return Color.green;
                case ItemRarity.Rare: return Color.blue;
                case ItemRarity.Epic: return Color.magenta;
                case ItemRarity.Legendary: return Color.yellow;
                default: return Color.white;
            }
        }
    }
    
    // === ITEM EFFECT ===
    [System.Serializable]
    public class ItemEffect
    {
        [Header("Effect Info")]
        public string effectId;
        public string description;
        
        [Header("Type")]
        public EffectType effectType;
        
        [Header("Values")]
        public float value;
        public string stringValue;
        public bool boolValue;
        
        [Header("Duration")]
        public bool isPermanent = true;
        public float duration = 0f;
        
        /// <summary>
        /// 효과 실행
        /// </summary>
        public void Execute()
        {
            switch (effectType)
            {
                case EffectType.UnlockArea:
                    ExecuteUnlockArea();
                    break;
                    
                case EffectType.TriggerEvent:
                    ExecuteTriggerEvent();
                    break;
                    
                case EffectType.ShowMessage:
                    ExecuteShowMessage();
                    break;
                    
                case EffectType.PlaySound:
                    ExecutePlaySound();
                    break;
                    
                case EffectType.ChangeStage:
                    ExecuteChangeStage();
                    break;
                    
                default:
                    Debug.LogWarning($"[ItemEffect] Unknown effect type: {effectType}");
                    break;
            }
        }
        
        private void ExecuteUnlockArea()
        {
            // 지역 잠금 해제
            Debug.Log($"[ItemEffect] Unlocked area: {stringValue}");
        }
        
        private void ExecuteTriggerEvent()
        {
            // 이벤트 트리거
            Debug.Log($"[ItemEffect] Triggered event: {stringValue}");
        }
        
        private void ExecuteShowMessage()
        {
            // 메시지 표시
            Debug.Log($"[ItemEffect] Show message: {stringValue}");
        }
        
        private void ExecutePlaySound()
        {
            // 사운드 재생
            Debug.Log($"[ItemEffect] Play sound: {stringValue}");
        }
        
        private void ExecuteChangeStage()
        {
            // 스테이지 변경
            Debug.Log($"[ItemEffect] Change stage: {value}");
        }
    }
    
    // === ENUMS ===
    public enum ItemType
    {
        Collectible,    // 수집품 (십자가, 허가증 등)
        Tool,          // 도구 (손전등, 열쇠 등)
        Document,      // 문서 (편지, 사진 등)
        Key,           // 열쇠
        Special        // 특수 아이템
    }
    
    public enum ItemRarity
    {
        Common,        // 일반
        Uncommon,      // 일반적이지 않음
        Rare,          // 희귀
        Epic,          // 서사급
        Legendary      // 전설급
    }
    
    public enum EffectType
    {
        UnlockArea,    // 지역 잠금 해제
        TriggerEvent,  // 이벤트 트리거
        ShowMessage,   // 메시지 표시
        PlaySound,     // 사운드 재생
        ChangeStage    // 스테이지 변경
    }
}