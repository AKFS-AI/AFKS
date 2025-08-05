using UnityEngine;

namespace AFKS.ItemSystem
{
    /// <summary>
    /// 아이템 정보를 담는 간단한 ScriptableObject (열쇠, 손전등용)
    /// </summary>
    [CreateAssetMenu(fileName = "ItemData", menuName = "AFKS/Item/Item Data")]
    public class ItemData : ScriptableObject
    {
        [Header("📋 기본 정보")]
        [SerializeField, Tooltip("고유 아이템 ID (key, flashlight)")] private string itemId;
        [SerializeField, Tooltip("화면에 표시될 아이템 이름")] private string itemName;
        [SerializeField, TextArea(2, 3), Tooltip("아이템 설명")] private string description;
        
        [Header("🎨 비주얼")]
        [SerializeField, Tooltip("인벤토리에 표시될 아이콘")] private Sprite icon;
        [SerializeField, Tooltip("아이콘 색조 (기본: 흰색)")] private Color iconTint = Color.white;
        
        [Header("🎵 오디오")]
        [SerializeField, Tooltip("아이템 획득 시 재생되는 소리")] private AudioClip pickupSound;
        [SerializeField, Tooltip("아이템 사용 시 재생되는 소리")] private AudioClip useSound;
        
        // === PROPERTIES ===
        public string ItemId => itemId;
        public string ItemName => itemName;
        public string Description => description;
        public Sprite Icon => icon;
        public Color IconTint => iconTint;
        public AudioClip PickupSound => pickupSound;
        public AudioClip UseSound => useSound;
        
        // === VALIDATION ===
        private void OnValidate()
        {
            // ID 자동 생성 (비어있을 때만)
            if (string.IsNullOrEmpty(itemId))
            {
                itemId = name.ToLower().Replace(" ", "_");
            }
            
            // 아이템 이름 자동 설정
            if (string.IsNullOrEmpty(itemName))
            {
                itemName = name;
            }
            
            // 열쇠와 손전등만 허용
            if (!string.IsNullOrEmpty(itemId))
            {
                string id = itemId.ToLower();
                if (id != "key" && id != "flashlight" && id != "열쇠" && id != "손전등")
                {
                    Debug.LogWarning($"[ItemData] '{itemId}'는 지원되지 않는 아이템입니다. 'key' 또는 'flashlight'만 사용 가능합니다.");
                }
            }
        }
        
        /// <summary>
        /// 이 아이템이 열쇠인지 확인
        /// </summary>
        public bool IsKey()
        {
            string id = itemId.ToLower();
            return id == "key" || id == "열쇠";
        }
        
        /// <summary>
        /// 이 아이템이 손전등인지 확인
        /// </summary>
        public bool IsFlashlight()
        {
            string id = itemId.ToLower();
            return id == "flashlight" || id == "손전등";
        }
    }
}