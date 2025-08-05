using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace AFKS.ItemSystem
{
    /// <summary>
    /// 열쇠와 손전등 상태를 표시하는 간단한 UI
    /// </summary>
    public class ItemUI : MonoBehaviour
    {
        [Header("🎯 UI 요소들")]
        [SerializeField, Tooltip("열쇠 아이콘")] private Image keyIcon;
        [SerializeField, Tooltip("손전등 아이콘")] private Image flashlightIcon;
        [SerializeField, Tooltip("획득 상태 텍스트")] private TextMeshProUGUI statusText;
        [SerializeField, Tooltip("진행률 텍스트")] private TextMeshProUGUI progressText;
        
        [Header("🎨 색상 설정")]
        [SerializeField, Tooltip("획득한 아이템 색상")] private Color obtainedColor = Color.white;
        [SerializeField, Tooltip("미획득 아이템 색상")] private Color notObtainedColor = Color.gray;
        [SerializeField, Tooltip("UI 표시 여부")] private bool showUI = true;
        
        // === PRIVATE FIELDS ===
        private ItemManager itemManager;
        private CanvasGroup canvasGroup;
        
        // === UNITY LIFECYCLE ===
        private void Start()
        {
            Initialize();
            SubscribeToEvents();
        }
        
        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }
        
        // === INITIALIZATION ===
        private void Initialize()
        {
            itemManager = ItemManager.Instance;
            if (itemManager == null)
            {
                Debug.LogError("[아이템UI] ItemManager 인스턴스를 찾을 수 없습니다!");
                return;
            }
            
            SetupUI();
            UpdateUI();
        }
        
        private void SetupUI()
        {
            // CanvasGroup 설정
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            
            SetUIVisibility(showUI);
        }
        
        // === EVENT HANDLING ===
        private void SubscribeToEvents()
        {
            ItemManager.OnKeyItemObtained.AddListener(OnKeyItemObtained);
        }
        
        private void UnsubscribeFromEvents()
        {
            ItemManager.OnKeyItemObtained.RemoveListener(OnKeyItemObtained);
        }
        
        private void OnKeyItemObtained(string itemId)
        {
            UpdateUI();
            
            // 간단한 획득 피드백
            StartCoroutine(ShowObtainedFeedback());
        }
        
        // === UI UPDATES ===
        
        /// <summary>
        /// UI 업데이트
        /// </summary>
        public void UpdateUI()
        {
            if (itemManager == null) return;
            
            bool hasKey = itemManager.HasKey;
            bool hasFlashlight = itemManager.HasFlashlight;
            
            // 아이콘 색상 업데이트
            if (keyIcon != null)
            {
                keyIcon.color = hasKey ? obtainedColor : notObtainedColor;
            }
            
            if (flashlightIcon != null)
            {
                flashlightIcon.color = hasFlashlight ? obtainedColor : notObtainedColor;
            }
            
            // 상태 텍스트 업데이트
            if (statusText != null)
            {
                if (hasKey && hasFlashlight)
                {
                    statusText.text = "모든 도구 준비 완료";
                    statusText.color = Color.green;
                }
                else if (hasKey || hasFlashlight)
                {
                    statusText.text = "일부 도구 확보";
                    statusText.color = Color.yellow;
                }
                else
                {
                    statusText.text = "도구 수집 필요";
                    statusText.color = Color.red;
                }
            }
            
            // 진행률 텍스트 업데이트
            if (progressText != null)
            {
                int obtained = itemManager.ItemCount;
                int total = 2; // 열쇠, 손전등
                progressText.text = $"수집: {obtained}/{total}";
            }
        }
        
        /// <summary>
        /// 획득 피드백 효과
        /// </summary>
        private IEnumerator ShowObtainedFeedback()
        {
            // 간단한 스케일 애니메이션
            Vector3 originalScale = transform.localScale;
            
            // 커지기
            float duration = 0.2f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float scale = Mathf.Lerp(1f, 1.2f, elapsed / duration);
                transform.localScale = originalScale * scale;
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            // 원래 크기로
            elapsed = 0f;
            while (elapsed < duration)
            {
                float scale = Mathf.Lerp(1.2f, 1f, elapsed / duration);
                transform.localScale = originalScale * scale;
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            transform.localScale = originalScale;
        }
        
        // === UI CONTROL ===
        
        /// <summary>
        /// UI 가시성 설정
        /// </summary>
        public void SetUIVisibility(bool visible)
        {
            showUI = visible;
            if (canvasGroup != null)
            {
                canvasGroup.alpha = visible ? 1f : 0f;
                canvasGroup.interactable = visible;
                canvasGroup.blocksRaycasts = visible;
            }
        }
        
        /// <summary>
        /// UI 토글
        /// </summary>
        public void ToggleUI()
        {
            SetUIVisibility(!showUI);
        }
        
        /// <summary>
        /// 아이템 정보 표시 (호버 등에서 사용)
        /// </summary>
        /// <param name="itemId">아이템 ID</param>
        public void ShowItemInfo(string itemId)
        {
            if (itemManager == null) return;
            
            // 간단한 아이템 정보 표시 (열쇠, 손전등만)
            switch (itemId.ToLower())
            {
                case "key":
                case "열쇠":
                    Debug.Log("[아이템UI] 열쇠: 문을 여는 데 사용됩니다");
                    break;
                case "flashlight":
                case "손전등":
                    Debug.Log("[아이템UI] 손전등: 어둠을 밝히는 데 사용됩니다");
                    break;
                default:
                    Debug.LogWarning($"[아이템UI] 알 수 없는 아이템: {itemId}");
                    break;
            }
        }
        
        // === DEBUG METHODS ===
        
        /// <summary>
        /// UI 강제 업데이트 (에디터 전용)
        /// </summary>
        [ContextMenu("UI 강제 업데이트")]
        public void ForceUpdateUI()
        {
            if (Application.isPlaying)
            {
                UpdateUI();
            }
        }
    }
}