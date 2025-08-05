using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using AFKS.ItemSystem;

namespace AFKS.UISystem
{
    /// <summary>
    /// 간단한 아이템 슬롯 UI (열쇠, 손전등 전용)
    /// </summary>
    public class InventorySlotUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("🖼️ UI 컴포넌트")]
        [SerializeField, Tooltip("아이템 아이콘을 표시할 Image 컴포넌트")] private Image itemIcon;
        [SerializeField, Tooltip("아이템 이름을 표시할 TextMeshPro 컴포넌트")] private TextMeshProUGUI itemNameText;
        [SerializeField, Tooltip("슬롯 배경 Image 컴포넌트")] private Image backgroundImage;
        [SerializeField, Tooltip("선택 상태를 표시할 하이라이트 Image")] private Image selectionHighlight;
        
        [Header("🎨 색상 설정")]
        [SerializeField, Tooltip("기본 상태의 슬롯 색상")] private Color normalColor = Color.white;
        [SerializeField, Tooltip("마우스 호버 시 슬롯 색상")] private Color hoverColor = Color.yellow;
        [SerializeField, Tooltip("선택 상태의 슬롯 색상")] private Color selectedColor = Color.green;
        [SerializeField, Tooltip("빈 슬롯의 색상")] private Color emptyColor = Color.gray;
        [SerializeField, Tooltip("보유한 아이템 색상")] private Color hasItemColor = Color.white;
        [SerializeField, Tooltip("미보유 아이템 색상")] private Color noItemColor = new Color(0.5f, 0.5f, 0.5f, 0.7f);
        
        [Header("🔑 아이템 설정")]
        [SerializeField, Tooltip("이 슬롯의 아이템 ID (key 또는 flashlight)")] private string assignedItemId;
        [SerializeField, Tooltip("아이템 기본 아이콘")] private Sprite defaultIcon;
        [SerializeField, Tooltip("아이템 이름")] private string itemDisplayName;
        
        // === PROPERTIES ===
        public int SlotIndex { get; private set; }
        public string ItemId => assignedItemId;
        public bool HasItem => GetHasItem();
        public bool IsSelected { get; private set; }
        
        // === PRIVATE FIELDS ===
        private bool isHovered = false;
        private ItemManager itemManager;
        
        // === UNITY LIFECYCLE ===
        private void Start()
        {
            Initialize();
        }
        
        // === INITIALIZATION ===
        
        /// <summary>
        /// 아이템 슬롯 초기화
        /// </summary>
        /// <param name="index">슬롯 인덱스</param>
        /// <param name="itemId">할당된 아이템 ID</param>
        public void Initialize(int index = -1, string itemId = "")
        {
            if (index >= 0) SlotIndex = index;
            if (!string.IsNullOrEmpty(itemId)) assignedItemId = itemId;
            
            itemManager = ItemManager.Instance;
            SetupComponents();
            UpdateVisual();
            
            Debug.Log($"[인벤토리슬롯UI] 슬롯 {SlotIndex} 초기화 완료 - 아이템: {assignedItemId}");
        }
        
        /// <summary>
        /// 컴포넌트 설정
        /// </summary>
        private void SetupComponents()
        {
            // 필요한 컴포넌트들이 없으면 자동으로 찾기
            if (itemIcon == null)
                itemIcon = transform.Find("ItemIcon")?.GetComponent<Image>();
            
            if (itemNameText == null)
                itemNameText = transform.Find("ItemNameText")?.GetComponent<TextMeshProUGUI>();
            
            if (backgroundImage == null)
                backgroundImage = GetComponent<Image>();
            
            if (selectionHighlight == null)
                selectionHighlight = transform.Find("SelectionHighlight")?.GetComponent<Image>();
            
            // 선택 하이라이트 초기화
            if (selectionHighlight != null)
            {
                selectionHighlight.gameObject.SetActive(false);
            }
            
            // 기본 아이콘 및 이름 설정
            SetupDefaultItemInfo();
        }
        
        /// <summary>
        /// 기본 아이템 정보 설정
        /// </summary>
        private void SetupDefaultItemInfo()
        {
            if (string.IsNullOrEmpty(assignedItemId)) return;
            
            switch (assignedItemId.ToLower())
            {
                case "key":
                case "열쇠":
                    if (string.IsNullOrEmpty(itemDisplayName))
                        itemDisplayName = "열쇠";
                    break;
                case "flashlight":
                case "손전등":
                    if (string.IsNullOrEmpty(itemDisplayName))
                        itemDisplayName = "손전등";
                    break;
            }
        }
        
        // === ITEM MANAGEMENT ===
        
        /// <summary>
        /// 아이템 보유 여부 확인
        /// </summary>
        private bool GetHasItem()
        {
            if (itemManager == null || string.IsNullOrEmpty(assignedItemId))
                return false;
                
            return itemManager.HasKeyItem(assignedItemId);
        }
        
        /// <summary>
        /// 시각적 요소 업데이트
        /// </summary>
        public void UpdateVisual()
        {
            bool hasItem = HasItem;
            
            // 아이콘 업데이트
            if (itemIcon != null)
            {
                if (defaultIcon != null)
                {
                    itemIcon.sprite = defaultIcon;
                    itemIcon.color = hasItem ? hasItemColor : noItemColor;
                }
                else
                {
                    itemIcon.sprite = null;
                    itemIcon.color = Color.clear;
                }
            }
            
            // 이름 텍스트 업데이트
            if (itemNameText != null)
            {
                itemNameText.text = hasItem ? itemDisplayName : $"[미보유] {itemDisplayName}";
                itemNameText.color = hasItem ? hasItemColor : noItemColor;
            }
            
            // 배경 색상 업데이트
            UpdateBackgroundColor();
        }
        
        /// <summary>
        /// 배경 색상 업데이트
        /// </summary>
        private void UpdateBackgroundColor()
        {
            if (backgroundImage == null) return;
            
            Color targetColor;
            
            if (IsSelected)
                targetColor = selectedColor;
            else if (isHovered)
                targetColor = hoverColor;
            else if (HasItem)
                targetColor = normalColor;
            else
                targetColor = emptyColor;
            
            backgroundImage.color = targetColor;
        }
        
        // === SELECTION MANAGEMENT ===
        
        /// <summary>
        /// 슬롯 선택 상태 설정
        /// </summary>
        /// <param name="selected">선택 여부</param>
        public void SetSelected(bool selected)
        {
            IsSelected = selected;
            
            if (selectionHighlight != null)
            {
                selectionHighlight.gameObject.SetActive(selected);
            }
            
            UpdateBackgroundColor();
        }
        
        // === EVENT HANDLERS ===
        
        /// <summary>
        /// 마우스 클릭 처리
        /// </summary>
        /// <param name="eventData">포인터 이벤트 데이터</param>
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                HandleLeftClick();
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                HandleRightClick();
            }
        }
        
        /// <summary>
        /// 좌클릭 처리
        /// </summary>
        private void HandleLeftClick()
        {
            if (!HasItem)
            {
                Debug.Log($"[인벤토리슬롯UI] {itemDisplayName}을(를) 아직 보유하지 않았습니다");
                return;
            }
            
            Debug.Log($"[인벤토리슬롯UI] {itemDisplayName} 선택됨");
            
            // 다른 슬롯들의 선택 해제 (필요시 UIManager에서 처리)
            SetSelected(!IsSelected);
            
            // 아이템 사용 (실제로는 보유 확인만)
            if (itemManager != null)
            {
                bool used = itemManager.UseKeyItem(assignedItemId);
                if (used)
                {
                    Debug.Log($"[인벤토리슬롯UI] {itemDisplayName} 사용 가능 확인됨");
                }
            }
        }
        
        /// <summary>
        /// 우클릭 처리
        /// </summary>
        private void HandleRightClick()
        {
            ShowItemInfo();
        }
        
        /// <summary>
        /// 마우스 호버 시작
        /// </summary>
        /// <param name="eventData">포인터 이벤트 데이터</param>
        public void OnPointerEnter(PointerEventData eventData)
        {
            isHovered = true;
            UpdateBackgroundColor();
            
            // 호버 시 간단한 정보 표시
            if (HasItem)
            {
                Debug.Log($"[인벤토리슬롯UI] {itemDisplayName}: 사용 가능");
            }
            else
            {
                Debug.Log($"[인벤토리슬롯UI] {itemDisplayName}: 아직 획득하지 않음");
            }
        }
        
        /// <summary>
        /// 마우스 호버 종료
        /// </summary>
        /// <param name="eventData">포인터 이벤트 데이터</param>
        public void OnPointerExit(PointerEventData eventData)
        {
            isHovered = false;
            UpdateBackgroundColor();
        }
        
        // === UTILITY METHODS ===
        
        /// <summary>
        /// 아이템 정보 표시
        /// </summary>
        public void ShowItemInfo()
        {
            if (string.IsNullOrEmpty(assignedItemId)) return;
            
            // 간단한 아이템 정보 표시
            switch (assignedItemId.ToLower())
            {
                case "key":
                case "열쇠":
                    Debug.Log("[인벤토리슬롯UI] 열쇠: 문을 여는 데 사용됩니다");
                    break;
                case "flashlight":
                case "손전등":
                    Debug.Log("[인벤토리슬롯UI] 손전등: 어둠을 밝히는 데 사용됩니다");
                    break;
                default:
                    Debug.LogWarning($"[인벤토리슬롯UI] 알 수 없는 아이템: {assignedItemId}");
                    break;
            }
        }
        
        /// <summary>
        /// 슬롯 새로고침 (외부에서 호출)
        /// </summary>
        public void RefreshSlot()
        {
            UpdateVisual();
        }
        
        // === DEBUG METHODS ===
        
        /// <summary>
        /// 에디터에서 테스트용 시각 업데이트
        /// </summary>
        [ContextMenu("시각 업데이트 테스트")]
        public void DebugUpdateVisual()
        {
            if (Application.isPlaying)
            {
                UpdateVisual();
            }
        }
        
        /// <summary>
        /// 슬롯 정보 출력
        /// </summary>
        [ContextMenu("슬롯 정보 출력")]
        public void DebugPrintSlotInfo()
        {
            Debug.Log($"[인벤토리슬롯UI] 슬롯 {SlotIndex}: {assignedItemId} - 보유: {HasItem}, 선택: {IsSelected}");
        }
    }
}