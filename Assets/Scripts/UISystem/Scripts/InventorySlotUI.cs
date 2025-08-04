using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using AFKS.InventorySystem;

namespace AFKS.UISystem
{
    /// <summary>
    /// 인벤토리 슬롯 UI 컴포넌트
    /// </summary>
    public class InventorySlotUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("🖼️ UI 컴포넌트")]
        [SerializeField, Tooltip("아이템 아이콘을 표시할 Image 컴포넌트")] private Image itemIcon;
        [SerializeField, Tooltip("아이템 개수를 표시할 Text 컴포넌트")] private Text quantityText;
        [SerializeField, Tooltip("슬롯 배경 Image 컴포넌트")] private Image backgroundImage;
        [SerializeField, Tooltip("선택 상태를 표시할 하이라이트 Image")] private Image selectionHighlight;
        
        [Header("🎨 색상 설정")]
        [SerializeField, Tooltip("기본 상태의 슬롯 색상")] private Color normalColor = Color.white;
        [SerializeField, Tooltip("마우스 호버 시 슬롯 색상")] private Color hoverColor = Color.yellow;
        [SerializeField, Tooltip("선택 상태의 슬롯 색상")] private Color selectedColor = Color.green;
        [SerializeField, Tooltip("빈 슬롯의 색상")] private Color emptyColor = Color.gray;
        
        // === PROPERTIES ===
        public int SlotIndex { get; private set; }
        public InventoryItem CurrentItem { get; private set; }
        public bool IsEmpty => CurrentItem == null;
        public bool IsSelected { get; private set; }
        
        // === PRIVATE FIELDS ===
        private bool isHovered = false;
        
        // === INITIALIZATION ===
        
        /// <summary>
        /// 인벤토리 슬롯 초기화
        /// </summary>
        /// <param name="index">슬롯 인덱스</param>
        public void Initialize(int index)
        {
            SlotIndex = index;
            SetupComponents();
            SetItem(null);
        }
        
        /// <summary>
        /// 컴포넌트 설정
        /// </summary>
        private void SetupComponents()
        {
            // 필요한 컴포넌트들이 없으면 자동으로 찾기
            if (itemIcon == null)
                itemIcon = transform.Find("ItemIcon")?.GetComponent<Image>();
            
            if (quantityText == null)
                quantityText = transform.Find("QuantityText")?.GetComponent<Text>();
            
            if (backgroundImage == null)
                backgroundImage = GetComponent<Image>();
            
            if (selectionHighlight == null)
                selectionHighlight = transform.Find("SelectionHighlight")?.GetComponent<Image>();
            
            // 선택 하이라이트 초기화
            if (selectionHighlight != null)
            {
                selectionHighlight.gameObject.SetActive(false);
            }
        }
        
        // === ITEM MANAGEMENT ===
        
        /// <summary>
        /// 슬롯에 아이템 설정
        /// </summary>
        /// <param name="item">설정할 아이템 (null이면 빈 슬롯)</param>
        public void SetItem(InventoryItem item)
        {
            CurrentItem = item;
            UpdateVisual();
        }
        
        /// <summary>
        /// 시각적 요소 업데이트
        /// </summary>
        private void UpdateVisual()
        {
            if (IsEmpty)
            {
                // 빈 슬롯 처리
                if (itemIcon != null)
                {
                    itemIcon.sprite = null;
                    itemIcon.color = Color.clear;
                }
                
                if (quantityText != null)
                {
                    quantityText.text = "";
                }
                
                if (backgroundImage != null)
                {
                    backgroundImage.color = emptyColor;
                }
            }
            else
            {
                // 아이템이 있는 슬롯 처리
                ItemData itemData = CurrentItem.ItemData;
                
                if (itemIcon != null && itemData.Icon != null)
                {
                    itemIcon.sprite = itemData.Icon;
                    itemIcon.color = itemData.IconTint;
                }
                
                if (quantityText != null)
                {
                    if (itemData.IsStackable && CurrentItem.Quantity > 1)
                    {
                        quantityText.text = CurrentItem.Quantity.ToString();
                    }
                    else
                    {
                        quantityText.text = "";
                    }
                }
                
                if (backgroundImage != null)
                {
                    // 희귀도에 따른 배경색 설정
                    Color rarityColor = itemData.GetRarityColor();
                    rarityColor.a = 0.3f; // 투명도 조정
                    backgroundImage.color = rarityColor;
                }
            }
            
            // 상태에 따른 색상 업데이트
            UpdateStateColor();
        }
        
        /// <summary>
        /// 상태에 따른 색상 업데이트
        /// </summary>
        private void UpdateStateColor()
        {
            if (backgroundImage == null) return;
            
            Color targetColor;
            
            if (IsEmpty)
            {
                targetColor = emptyColor;
            }
            else if (IsSelected)
            {
                targetColor = selectedColor;
            }
            else if (isHovered)
            {
                targetColor = hoverColor;
            }
            else
            {
                targetColor = CurrentItem.ItemData.GetRarityColor();
                targetColor.a = 0.3f;
            }
            
            // 부드러운 색상 전환을 위해서는 코루틴 사용 가능
            backgroundImage.color = targetColor;
        }
        
        // === SELECTION ===
        
        /// <summary>
        /// 슬롯 선택 설정
        /// </summary>
        /// <param name="selected">선택 여부</param>
        public void SetSelected(bool selected)
        {
            IsSelected = selected;
            
            if (selectionHighlight != null)
            {
                selectionHighlight.gameObject.SetActive(selected);
            }
            
            UpdateStateColor();
        }
        
        // === EVENT HANDLERS ===
        
        public void OnPointerClick(PointerEventData eventData)
        {
            if (IsEmpty) return;
            
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                // 좌클릭: 아이템 선택/사용
                HandleLeftClick();
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                // 우클릭: 아이템 정보 표시
                HandleRightClick();
            }
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            isHovered = true;
            UpdateStateColor();
            
            // 아이템 정보 표시
            if (!IsEmpty)
            {
                ShowItemInfo();
            }
        }
        
        public void OnPointerExit(PointerEventData eventData)
        {
            isHovered = false;
            UpdateStateColor();
            
            // 아이템 정보 숨김
            HideItemInfo();
        }
        
        // === CLICK HANDLERS ===
        
        /// <summary>
        /// 좌클릭 처리
        /// </summary>
        private void HandleLeftClick()
        {
            if (IsEmpty) return;
            
            ItemData itemData = CurrentItem.ItemData;
            
            // 사용 가능한 아이템이면 사용
            if (itemData.IsUsable)
            {
                if (InventoryManager.Instance != null)
                {
                    InventoryManager.Instance.UseItem(itemData.ItemId);
                }
            }
            else
            {
                // 선택 토글
                SetSelected(!IsSelected);
                
                // 다른 슬롯들의 선택 해제 (단일 선택)
                DeselectOtherSlots();
            }
            
            Debug.Log($"[InventorySlotUI] Left clicked item: {itemData.ItemName}");
        }
        
        /// <summary>
        /// 우클릭 처리
        /// </summary>
        private void HandleRightClick()
        {
            if (IsEmpty) return;
            
            // 아이템 상세 정보 표시
            ShowDetailedItemInfo();
            
            Debug.Log($"[InventorySlotUI] Right clicked item: {CurrentItem.ItemData.ItemName}");
        }
        
        /// <summary>
        /// 다른 슬롯들의 선택 해제
        /// </summary>
        private void DeselectOtherSlots()
        {
            // 부모의 모든 InventorySlotUI 찾기
            InventorySlotUI[] allSlots = GetComponentsInParent<InventorySlotUI>();
            
            foreach (var slot in allSlots)
            {
                if (slot != this)
                {
                    slot.SetSelected(false);
                }
            }
        }
        
        // === INFO DISPLAY ===
        
        /// <summary>
        /// 아이템 기본 정보 표시
        /// </summary>
        private void ShowItemInfo()
        {
            if (UIManager.Instance != null && CurrentItem != null)
            {
                ItemData itemData = CurrentItem.ItemData;
                string info = $"{itemData.ItemName}";
                
                if (itemData.IsStackable && CurrentItem.Quantity > 1)
                {
                    info += $" x{CurrentItem.Quantity}";
                }
                
                // 툴팁이나 상태바에 정보 표시
                // UIManager.Instance.ShowTooltip(info);
            }
        }
        
        /// <summary>
        /// 아이템 정보 숨김
        /// </summary>
        private void HideItemInfo()
        {
            if (UIManager.Instance != null)
            {
                // UIManager.Instance.HideTooltip();
            }
        }
        
        /// <summary>
        /// 아이템 상세 정보 표시
        /// </summary>
        private void ShowDetailedItemInfo()
        {
            if (UIManager.Instance != null && CurrentItem != null)
            {
                ItemData itemData = CurrentItem.ItemData;
                string detailedInfo = $"{itemData.ItemName}\n\n{itemData.Description}";
                
                // 상세 정보 패널에 표시
                UIManager.Instance.ShowDialog(detailedInfo, 0f); // 0초 = 무한 표시
            }
        }
        
        // === UTILITY ===
        
        /// <summary>
        /// 슬롯 비우기
        /// </summary>
        public void ClearSlot()
        {
            SetItem(null);
        }
        
        /// <summary>
        /// 슬롯 새로고침
        /// </summary>
        public void RefreshSlot()
        {
            UpdateVisual();
        }
    }
}