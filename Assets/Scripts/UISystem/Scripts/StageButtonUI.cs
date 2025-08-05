using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AFKS.StageSystem;

namespace AFKS.UISystem
{
    /// <summary>
    /// 개별 스테이지 버튼 UI
    /// StageNavigationUI에서 분리하여 독립성 향상
    /// </summary>
    public class StageButtonUI : MonoBehaviour
    {
        [Header("🎯 UI 요소들")]
        [SerializeField] private Button button;
        [SerializeField] private TextMeshProUGUI stageNameText;
        [SerializeField] private TextMeshProUGUI stageIndexText;
        [SerializeField] private Image buttonImage;
        [SerializeField] private Image lockIcon;
        
        // === PRIVATE FIELDS ===
        private int stageIndex;
        private StageData stageData;
        private StageNavigationUI navigationUI;
        
        // === PROPERTIES ===
        public int StageIndex => stageIndex;
        public StageData StageData => stageData;
        public bool IsInitialized => navigationUI != null && stageData != null;
        
        /// <summary>
        /// 스테이지 버튼 초기화
        /// </summary>
        public void Initialize(int index, StageData data, StageNavigationUI navUI)
        {
            stageIndex = index;
            stageData = data;
            navigationUI = navUI;
            
            SetupComponents();
            SetupButton();
            UpdateButtonState();
            
            Debug.Log($"[StageButtonUI] 스테이지 버튼 {index} 초기화 완료: {data?.StageName}");
        }
        
        /// <summary>
        /// UI 컴포넌트 자동 설정
        /// </summary>
        private void SetupComponents()
        {
            // 컴포넌트 자동 찾기
            if (button == null) button = GetComponent<Button>();
            if (buttonImage == null) buttonImage = GetComponent<Image>();
            
            // 텍스트 컴포넌트들 자동 찾기
            var texts = GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length > 0) 
            {
                stageNameText = texts[0];
                if (texts.Length > 1) stageIndexText = texts[1];
            }
            
            // 락 아이콘 찾기 (자식 오브젝트에서)
            if (lockIcon == null)
            {
                Transform lockTransform = transform.Find("LockIcon");
                if (lockTransform != null)
                {
                    lockIcon = lockTransform.GetComponent<Image>();
                }
            }
            
            // 컴포넌트 유효성 검증
            if (button == null)
            {
                Debug.LogWarning($"[StageButtonUI] {gameObject.name}에서 Button 컴포넌트를 찾을 수 없습니다.");
            }
            
            if (stageNameText == null)
            {
                Debug.LogWarning($"[StageButtonUI] {gameObject.name}에서 스테이지 이름 텍스트를 찾을 수 없습니다.");
            }
        }
        
        /// <summary>
        /// 버튼 설정 및 이벤트 연결
        /// </summary>
        private void SetupButton()
        {
            if (button != null)
            {
                // 기존 리스너 제거 후 새로 추가
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(OnButtonClicked);
            }
            
            // 텍스트 설정
            if (stageNameText != null && stageData != null)
            {
                stageNameText.text = stageData.StageName;
            }
            
            if (stageIndexText != null)
            {
                stageIndexText.text = $"Stage {stageIndex + 1}";
            }
        }
        
        /// <summary>
        /// 버튼 상태 업데이트 (색상, 상호작용성, 잠금 상태)
        /// </summary>
        public void UpdateButtonState()
        {
            if (navigationUI == null || !IsInitialized) return;
            
            bool isInteractable = navigationUI.IsStageButtonInteractable(stageIndex);
            Color buttonColor = navigationUI.GetStageButtonColor(stageIndex);
            
            // 버튼 상호작용 설정
            if (button != null)
            {
                button.interactable = isInteractable;
            }
            
            // 색상 설정
            if (buttonImage != null)
            {
                buttonImage.color = buttonColor;
            }
            
            // 잠금 아이콘 표시/숨김
            UpdateLockIcon();
            
            // 텍스트 색상도 업데이트
            UpdateTextColors(isInteractable);
        }
        
        /// <summary>
        /// 잠금 아이콘 상태 업데이트
        /// </summary>
        private void UpdateLockIcon()
        {
            if (lockIcon != null)
            {
                bool isLocked = !StageManager.Instance.CanAccessStage(stageIndex);
                lockIcon.gameObject.SetActive(isLocked);
                
                if (isLocked)
                {
                    lockIcon.color = Color.white;
                }
            }
        }
        
        /// <summary>
        /// 텍스트 색상 업데이트
        /// </summary>
        private void UpdateTextColors(bool isInteractable)
        {
            Color textColor = isInteractable ? Color.white : Color.gray;
            
            if (stageNameText != null)
            {
                stageNameText.color = textColor;
            }
            
            if (stageIndexText != null)
            {
                stageIndexText.color = textColor;
            }
        }
        
        /// <summary>
        /// 버튼 클릭 시 호출
        /// </summary>
        private void OnButtonClicked()
        {
            if (navigationUI == null || !IsInitialized)
            {
                Debug.LogWarning("[StageButtonUI] 초기화되지 않은 버튼이 클릭되었습니다.");
                return;
            }
            
            Debug.Log($"[StageButtonUI] 스테이지 {stageIndex} 버튼 클릭됨");
            navigationUI.RequestStageChange(stageIndex);
        }
        
        /// <summary>
        /// 버튼 강조 효과 (선택적)
        /// </summary>
        public void SetHighlighted(bool highlighted)
        {
            if (buttonImage == null) return;
            
            if (highlighted)
            {
                // 강조 효과 (밝기 증가)
                Color highlightColor = buttonImage.color * 1.2f;
                highlightColor.a = buttonImage.color.a;
                buttonImage.color = highlightColor;
            }
            else
            {
                // 원래 색상으로 복원
                UpdateButtonState();
            }
        }
        
        /// <summary>
        /// 애니메이션 효과 (선택적)
        /// </summary>
        public void PlayClickAnimation()
        {
            if (button == null) return;
            
            // 간단한 스케일 애니메이션
            StartCoroutine(ClickAnimationCoroutine());
        }
        
        /// <summary>
        /// 클릭 애니메이션 코루틴
        /// </summary>
        private System.Collections.IEnumerator ClickAnimationCoroutine()
        {
            Vector3 originalScale = transform.localScale;
            Vector3 pressedScale = originalScale * 0.95f;
            
            // 축소
            float elapsed = 0f;
            float duration = 0.1f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.localScale = Vector3.Lerp(originalScale, pressedScale, t);
                yield return null;
            }
            
            // 복원
            elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.localScale = Vector3.Lerp(pressedScale, originalScale, t);
                yield return null;
            }
            
            transform.localScale = originalScale;
        }
        
        /// <summary>
        /// 디버그 정보 출력
        /// </summary>
        [ContextMenu("디버그 정보 출력")]
        public void PrintDebugInfo()
        {
            Debug.Log($"=== StageButtonUI Debug Info ===");
            Debug.Log($"Stage Index: {stageIndex}");
            Debug.Log($"Stage Name: {stageData?.StageName ?? "null"}");
            Debug.Log($"Is Initialized: {IsInitialized}");
            Debug.Log($"Button Interactable: {button?.interactable ?? false}");
            Debug.Log($"Lock Icon Active: {lockIcon?.gameObject.activeSelf ?? false}");
        }
        
        /// <summary>
        /// 정리 작업 (OnDestroy에서 호출)
        /// </summary>
        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
            }
        }
    }
}