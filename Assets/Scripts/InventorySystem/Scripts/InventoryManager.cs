using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using AFKS.Shared.Events;
using AFKS.Shared.Interfaces;
using AFKS.Shared.Utils;

namespace AFKS.InventorySystem
{
    /// <summary>
    /// 인벤토리 관리를 담당하는 매니저
    /// </summary>
    public class InventoryManager : MonoBehaviour, ISaveable
    {
        [Header("🎒 인벤토리 설정")]
        [SerializeField, Range(5, 20), Tooltip("최대 인벤토리 슬롯 개수")] private int maxSlots = 10;
        [SerializeField, Tooltip("동일한 아이템 중복 소지 허용 여부")] private bool allowDuplicates = false;
        [SerializeField, Tooltip("인벤토리 변경 시 자동 저장 여부")] private bool autoSave = true;
        
        [Header("📦 아이템 데이터베이스")]
        [SerializeField, Tooltip("게임에서 사용할 모든 아이템 데이터 목록")] private List<ItemData> itemDatabase = new List<ItemData>();
        
        [Header("📡 이벤트")]
        [SerializeField, Tooltip("아이템 추가 시 발생하는 게임 이벤트")] private GameEvent onItemAdded;
        [SerializeField, Tooltip("아이템 제거 시 발생하는 게임 이벤트")] private GameEvent onItemRemoved;
        [SerializeField, Tooltip("아이템 사용 시 발생하는 게임 이벤트")] private GameEvent onItemUsed;
        [SerializeField, Tooltip("인벤토리 변경 시 발생하는 게임 이벤트")] private GameEvent onInventoryChanged;
        
        // === RUNTIME EVENTS ===
        public static readonly GameEvent<InventoryItem> OnItemAdded = new GameEvent<InventoryItem>();
        public static readonly GameEvent<InventoryItem> OnItemRemoved = new GameEvent<InventoryItem>();
        public static readonly GameEvent<string> OnItemUsed = new GameEvent<string>();
        public static readonly GameEvent<List<InventoryItem>> OnInventoryChanged = new GameEvent<List<InventoryItem>>();
        
        // === PROPERTIES ===
        public int MaxSlots => maxSlots;
        public int UsedSlots => inventoryItems.Count;
        public int AvailableSlots => maxSlots - UsedSlots;
        public bool IsFull => UsedSlots >= maxSlots;
        public string SaveID => "InventoryManager";
        public List<InventoryItem> Items => new List<InventoryItem>(inventoryItems);
        
        // === PRIVATE FIELDS ===
        private List<InventoryItem> inventoryItems = new List<InventoryItem>();
        private Dictionary<string, ItemData> itemDataCache = new Dictionary<string, ItemData>();
        
        // === SINGLETON ACCESS ===
        private static InventoryManager instance;
        public static InventoryManager Instance
        {
            get
            {
                if (instance == null)
                    instance = FindFirstObjectByType<InventoryManager>();
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
                InitializeInventory();
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }
        
        // === INITIALIZATION ===
        private void InitializeInventory()
        {
            BuildItemDataCache();
            Debug.Log($"[InventoryManager] Initialized with {maxSlots} slots");
        }
        
        /// <summary>
        /// 아이템 데이터 캐시 구성
        /// </summary>
        private void BuildItemDataCache()
        {
            itemDataCache.Clear();
            
            foreach (var itemData in itemDatabase)
            {
                if (itemData != null && !itemDataCache.ContainsKey(itemData.ItemId))
                {
                    itemDataCache[itemData.ItemId] = itemData;
                }
            }
            
            Debug.Log($"[InventoryManager] Cached {itemDataCache.Count} item types");
        }
        
        // === ITEM MANAGEMENT ===
        
        /// <summary>
        /// 아이템 추가
        /// </summary>
        /// <param name="itemId">아이템 ID</param>
        /// <param name="quantity">수량</param>
        /// <returns>성공적으로 추가되었으면 true</returns>
        public bool AddItem(string itemId, int quantity = 1)
        {
            if (string.IsNullOrEmpty(itemId) || quantity <= 0)
            {
                Debug.LogWarning("[InventoryManager] Invalid add item parameters");
                return false;
            }
            
            ItemData itemData = GetItemData(itemId);
            if (itemData == null)
            {
                Debug.LogError($"[InventoryManager] Item data not found: {itemId}");
                return false;
            }
            
            return AddItemInternal(itemData, quantity);
        }
        
        /// <summary>
        /// 아이템 추가 (내부 구현)
        /// </summary>
        private bool AddItemInternal(ItemData itemData, int quantity)
        {
            // 스택 가능한 아이템인 경우 기존 아이템에 추가
            if (itemData.IsStackable)
            {
                var existingItem = FindItem(itemData.ItemId);
                if (existingItem != null)
                {
                    int addableQuantity = Mathf.Min(quantity, itemData.MaxStackSize - existingItem.Quantity);
                    if (addableQuantity > 0)
                    {
                        existingItem.Quantity += addableQuantity;
                        NotifyItemAdded(existingItem);
                        
                        // 남은 수량이 있으면 새 슬롯에 추가
                        int remainingQuantity = quantity - addableQuantity;
                        if (remainingQuantity > 0)
                        {
                            return AddNewItemSlot(itemData, remainingQuantity);
                        }
                        
                        return true;
                    }
                }
            }
            
            // 새 슬롯에 아이템 추가
            return AddNewItemSlot(itemData, quantity);
        }
        
        /// <summary>
        /// 새 슬롯에 아이템 추가
        /// </summary>
        private bool AddNewItemSlot(ItemData itemData, int quantity)
        {
            // 중복 허용하지 않는 경우 체크
            if (!allowDuplicates && HasItem(itemData.ItemId))
            {
                Debug.LogWarning($"[InventoryManager] Item already exists and duplicates not allowed: {itemData.ItemId}");
                return false;
            }
            
            // 인벤토리 공간 확인
            if (IsFull)
            {
                Debug.LogWarning("[InventoryManager] Inventory is full");
                return false;
            }
            
            // 새 아이템 생성
            InventoryItem newItem = new InventoryItem(itemData, quantity);
            inventoryItems.Add(newItem);
            
            // 사운드 재생
            if (itemData.PickupSound != null)
            {
                // AudioManager.Instance.PlaySFX(itemData.PickupSound);
            }
            
            NotifyItemAdded(newItem);
            
            Debug.Log($"[InventoryManager] Added item: {itemData.ItemName} x{quantity}");
            return true;
        }
        
        /// <summary>
        /// 아이템 제거
        /// </summary>
        /// <param name="itemId">아이템 ID</param>
        /// <param name="quantity">제거할 수량</param>
        /// <returns>성공적으로 제거되었으면 true</returns>
        public bool RemoveItem(string itemId, int quantity = 1)
        {
            var item = FindItem(itemId);
            if (item == null)
            {
                Debug.LogWarning($"[InventoryManager] Item not found for removal: {itemId}");
                return false;
            }
            
            if (item.Quantity < quantity)
            {
                Debug.LogWarning($"[InventoryManager] Not enough items to remove: {itemId}");
                return false;
            }
            
            item.Quantity -= quantity;
            
            // 수량이 0이 되면 아이템 삭제
            if (item.Quantity <= 0)
            {
                inventoryItems.Remove(item);
            }
            
            // 사운드 재생
            if (item.ItemData.DropSound != null)
            {
                // AudioManager.Instance.PlaySFX(item.ItemData.DropSound);
            }
            
            NotifyItemRemoved(item);
            
            Debug.Log($"[InventoryManager] Removed item: {item.ItemData.ItemName} x{quantity}");
            return true;
        }
        
        /// <summary>
        /// 아이템 사용
        /// </summary>
        /// <param name="itemId">아이템 ID</param>
        /// <returns>성공적으로 사용되었으면 true</returns>
        public bool UseItem(string itemId)
        {
            var item = FindItem(itemId);
            if (item == null)
            {
                Debug.LogWarning($"[InventoryManager] Item not found for use: {itemId}");
                return false;
            }
            
            ItemData itemData = item.ItemData;
            
            // 사용 가능 여부 확인
            if (!itemData.IsUsable)
            {
                Debug.LogWarning($"[InventoryManager] Item is not usable: {itemId}");
                return false;
            }
            
            // 사용 조건 확인
            string[] currentItemIds = inventoryItems.Select(i => i.ItemData.ItemId).ToArray();
            if (!itemData.CanUse(currentItemIds))
            {
                Debug.LogWarning($"[InventoryManager] Item use conditions not met: {itemId}");
                return false;
            }
            
            // 아이템 효과 실행
            itemData.ExecuteEffects();
            
            // 사운드 재생
            if (itemData.UseSound != null)
            {
                // AudioManager.Instance.PlaySFX(itemData.UseSound);
            }
            
            // 소비성 아이템인 경우 제거
            if (itemData.IsConsumable)
            {
                RemoveItem(itemId, 1);
            }
            
            NotifyItemUsed(itemId);
            
            Debug.Log($"[InventoryManager] Used item: {itemData.ItemName}");
            return true;
        }
        
        // === QUERY METHODS ===
        
        /// <summary>
        /// 아이템 보유 여부 확인
        /// </summary>
        /// <param name="itemId">아이템 ID</param>
        /// <returns>보유하고 있으면 true</returns>
        public bool HasItem(string itemId)
        {
            return FindItem(itemId) != null;
        }
        
        /// <summary>
        /// 아이템 수량 반환
        /// </summary>
        /// <param name="itemId">아이템 ID</param>
        /// <returns>보유 수량</returns>
        public int GetItemQuantity(string itemId)
        {
            var item = FindItem(itemId);
            return item?.Quantity ?? 0;
        }
        
        /// <summary>
        /// 특정 타입의 아이템들 반환
        /// </summary>
        /// <param name="itemType">아이템 타입</param>
        /// <returns>해당 타입의 아이템 목록</returns>
        public List<InventoryItem> GetItemsByType(ItemType itemType)
        {
            return inventoryItems.Where(item => item.ItemData.ItemType == itemType).ToList();
        }
        
        /// <summary>
        /// 키 아이템들 반환
        /// </summary>
        /// <returns>키 아이템 목록</returns>
        public List<InventoryItem> GetKeyItems()
        {
            return inventoryItems.Where(item => item.ItemData.IsKeyItem).ToList();
        }
        
        /// <summary>
        /// 사용 가능한 아이템들 반환
        /// </summary>
        /// <returns>사용 가능한 아이템 목록</returns>
        public List<InventoryItem> GetUsableItems()
        {
            string[] currentItemIds = inventoryItems.Select(i => i.ItemData.ItemId).ToArray();
            return inventoryItems.Where(item => 
                item.ItemData.IsUsable && 
                item.ItemData.CanUse(currentItemIds)
            ).ToList();
        }
        
        // === UTILITY METHODS ===
        
        /// <summary>
        /// 아이템 찾기
        /// </summary>
        private InventoryItem FindItem(string itemId)
        {
            return inventoryItems.FirstOrDefault(item => item.ItemData.ItemId == itemId);
        }
        
        /// <summary>
        /// 아이템 데이터 반환
        /// </summary>
        public ItemData GetItemData(string itemId)
        {
            itemDataCache.TryGetValue(itemId, out ItemData itemData);
            return itemData;
        }
        
        /// <summary>
        /// 인벤토리 정리
        /// </summary>
        public void SortInventory()
        {
            inventoryItems.Sort((a, b) => 
            {
                // 키 아이템 우선
                if (a.ItemData.IsKeyItem != b.ItemData.IsKeyItem)
                    return a.ItemData.IsKeyItem ? -1 : 1;
                
                // 타입별 정렬
                if (a.ItemData.ItemType != b.ItemData.ItemType)
                    return a.ItemData.ItemType.CompareTo(b.ItemData.ItemType);
                
                // 희귀도별 정렬
                if (a.ItemData.Rarity != b.ItemData.Rarity)
                    return b.ItemData.Rarity.CompareTo(a.ItemData.Rarity);
                
                // 이름별 정렬
                return a.ItemData.ItemName.CompareTo(b.ItemData.ItemName);
            });
            
            NotifyInventoryChanged();
            Debug.Log("[InventoryManager] Inventory sorted");
        }
        
        /// <summary>
        /// 인벤토리 초기화
        /// </summary>
        public void ClearInventory()
        {
            inventoryItems.Clear();
            NotifyInventoryChanged();
            Debug.Log("[InventoryManager] Inventory cleared");
        }
        
        // === NOTIFICATION METHODS ===
        
        private void NotifyItemAdded(InventoryItem item)
        {
            onItemAdded?.Raise();
            OnItemAdded.Raise(item);
            NotifyInventoryChanged();
        }
        
        private void NotifyItemRemoved(InventoryItem item)
        {
            onItemRemoved?.Raise();
            OnItemRemoved.Raise(item);
            NotifyInventoryChanged();
        }
        
        private void NotifyItemUsed(string itemId)
        {
            onItemUsed?.Raise();
            OnItemUsed.Raise(itemId);
        }
        
        private void NotifyInventoryChanged()
        {
            onInventoryChanged?.Raise();
            OnInventoryChanged.Raise(new List<InventoryItem>(inventoryItems));
            
            if (autoSave)
            {
                // SaveManager.SaveGame();
            }
        }
        
        // === SAVE SYSTEM ===
        public string GetSaveData()
        {
            InventorySaveData saveData = new InventorySaveData
            {
                items = inventoryItems.Select(item => new ItemSaveData
                {
                    itemId = item.ItemData.ItemId,
                    quantity = item.Quantity
                }).ToArray()
            };
            
            return JsonUtility.ToJson(saveData);
        }
        
        public void LoadSaveData(string data)
        {
            if (data.IsNullOrEmpty()) return;
            
            try
            {
                InventorySaveData saveData = JsonUtility.FromJson<InventorySaveData>(data);
                
                ClearInventory();
                
                foreach (var itemSave in saveData.items)
                {
                    AddItem(itemSave.itemId, itemSave.quantity);
                }
                
                Debug.Log("[InventoryManager] Save data loaded successfully");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[InventoryManager] Failed to load save data: {e.Message}");
            }
        }
    }
    
    // === INVENTORY ITEM ===
    [System.Serializable]
    public class InventoryItem
    {
        [SerializeField] private ItemData itemData;
        [SerializeField] private int quantity;
        
        public ItemData ItemData => itemData;
        public int Quantity 
        { 
            get => quantity; 
            set => quantity = Mathf.Max(0, value); 
        }
        
        public InventoryItem(ItemData itemData, int quantity = 1)
        {
            this.itemData = itemData;
            this.quantity = quantity;
        }
    }
    
    // === SAVE DATA ===
    [System.Serializable]
    public class InventorySaveData
    {
        public ItemSaveData[] items;
    }
    
    [System.Serializable]
    public class ItemSaveData
    {
        public string itemId;
        public int quantity;
    }
}