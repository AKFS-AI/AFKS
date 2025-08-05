using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using AFKS.StageSystem;
using AFKS.Shared.Events;

namespace AFKS.UISystem
{
    /// <summary>
    /// 스테이지 선택 및 네비게이션 UI
    /// </summary>
    public class StageNavigationUI : MonoBehaviour
    {
        [Header("🎯 UI 요소들")]
        [SerializeField, Tooltip("스테이지 선택 패널")] private GameObject stageSelectionPanel;
        [SerializeField, Tooltip("스테이지 버튼 부모")] private Transform stageButtonParent;
        [SerializeField, Tooltip("스테이지 버튼 프리팹")] private GameObject stageButtonPrefab;
        [SerializeField, Tooltip("현재 스테이지 표시")] private TextMeshProUGUI currentStageText;
        [SerializeField, Tooltip("네비게이션 버튼들")] private Button[] navigationButtons;
        
        [Header("⚙️ 네비게이션 설정")]
        [SerializeField, Tooltip("퀵 네비게이션 키")] private KeyCode quickNavKey = KeyCode.M;
        [SerializeField, Tooltip("애니메이션 지속시간")] private float animationDuration = 0.3f;
        [SerializeField, Tooltip("자동 숨김 시간")] private float autoHideTime = 10f;
        
        [Header("🎨 버튼 색상 설정")]
        [SerializeField, Tooltip("잠긴 스테이지 색상")] private Color lockedStageColor = Color.gray;
        [SerializeField, Tooltip("잠금 해제된 스테이지 색상")] private Color unlockedStageColor = Color.white;
        [SerializeField, Tooltip("현재 스테이지 색상")] private Color currentStageColor = Color.green;
        [SerializeField, Tooltip("일방통행 제한 색상")] private Color restrictedStageColor = Color.red;
        
        // === PRIVATE FIELDS ===
        private StageManager stageManager;
        private List<StageButtonUI> stageButtons = new List<StageButtonUI>();
        private CanvasGroup panelCanvasGroup;
        private Coroutine autoHideCoroutine;
        private bool isVisible = false;
        
        // === INITIALIZATION ===
        
        private void Start()
        {
            Initialize();
        }
        
        private void Initialize()
        {
            stageManager = StageManager.Instance;
            if (stageManager == null)
            {
                Debug.LogError("[스테이지네비게이션UI] StageManager 인스턴스를 찾을 수 없습니다!");
                return;
            }
            
            SetupUI();
            CreateStageButtons();
            SubscribeToEvents();
            UpdateUI();
            
            // 초기에는 숨김
            SetPanelVisibility(false, false);
            
            Debug.Log("[스테이지네비게이션UI] 초기화 성공");
        }
        
        private void SetupUI()
        {
            // CanvasGroup 설정
            if (stageSelectionPanel != null)
            {
                panelCanvasGroup = stageSelectionPanel.GetComponent<CanvasGroup>();
                if (panelCanvasGroup == null)
                {
                    panelCanvasGroup = stageSelectionPanel.AddComponent<CanvasGroup>();
                }
            }
            
            // 네비게이션 버튼들 설정
            if (navigationButtons != null)
            {
                for (int i = 0; i < navigationButtons.Length; i++)
                {
                    int index = i;
                    if (navigationButtons[i] != null)
                    {
                        navigationButtons[i].onClick.AddListener(() => OnNavigationButtonClicked(index));
                    }
                }
            }
        }
        
        private void CreateStageButtons()
        {
            if (stageButtonParent == null || stageButtonPrefab == null) return;
            
            // 기존 버튼들 정리
            foreach (Transform child in stageButtonParent)
            {
                Destroy(child.gameObject);
            }
            stageButtons.Clear();
            
            // 각 스테이지별 버튼 생성
            for (int i = 0; i < stageManager.Stages.Count; i++)
            {
                CreateStageButton(i);
            }
        }
        
        private void CreateStageButton(int stageIndex)
        {
            var stageData = stageManager.Stages[stageIndex];
            if (stageData == null) return;
            
            // 버튼 인스턴스 생성
            var buttonObj = Instantiate(stageButtonPrefab, stageButtonParent);
            buttonObj.name = $"StageButton_{stageIndex}";
            
            // StageButtonUI 컴포넌트 설정
            var stageButton = buttonObj.GetComponent<StageButtonUI>();
            if (stageButton == null)
            {
                stageButton = buttonObj.AddComponent<StageButtonUI>();
            }
            
            stageButton.Initialize(stageIndex, stageData, this);
            stageButtons.Add(stageButton);
        }
        
        private void SubscribeToEvents()
        {
            StageManager.OnStageChanged.AddListener(OnStageChanged);
            StageManager.OnStageUnlocked.AddListener(OnStageUnlocked);
            StageManager.OnTransitionProgress.AddListener(OnStageTransitionProgress);
        }
        
        private void OnDestroy()
        {
            StageManager.OnStageChanged.RemoveListener(OnStageChanged);
            StageManager.OnStageUnlocked.RemoveListener(OnStageUnlocked);
            StageManager.OnTransitionProgress.RemoveListener(OnStageTransitionProgress);
        }
        
        // === UI MANAGEMENT ===
        
        private void Update()
        {
            // 최적화: 패널이 보이지 않을 때는 ESC 키 체크 불필요
            if (isVisible && Input.GetKeyDown(KeyCode.Escape))
            {
                SetPanelVisibility(false);
            }
            
            // 퀵 네비게이션 키 확인 (최적화: 게임 플레이 중일 때만)
            if (Input.GetKeyDown(quickNavKey))
            {
                ToggleStageSelection();
            }
        }
        
        /// <summary>
        /// 스테이지 선택 패널 토글
        /// </summary>
        public void ToggleStageSelection()
        {
            SetPanelVisibility(!isVisible);
        }
        
        /// <summary>
        /// 패널 가시성 설정
        /// </summary>
        public void SetPanelVisibility(bool visible, bool animate = true)
        {
            if (panelCanvasGroup == null) return;
            
            isVisible = visible;
            
            if (autoHideCoroutine != null)
            {
                StopCoroutine(autoHideCoroutine);
                autoHideCoroutine = null;
            }
            
            if (animate)
            {
                StartCoroutine(AnimatePanel(visible));
            }
            else
            {
                panelCanvasGroup.alpha = visible ? 1f : 0f;
                panelCanvasGroup.interactable = visible;
                panelCanvasGroup.blocksRaycasts = visible;
                stageSelectionPanel.SetActive(visible);
            }
            
            if (visible)
            {
                UpdateUI();
                
                // 자동 숨김 타이머 시작
                if (autoHideTime > 0)
                {
                    autoHideCoroutine = StartCoroutine(AutoHideCoroutine());
                }
            }
        }
        
        /// <summary>
        /// 패널 애니메이션
        /// </summary>
        private IEnumerator AnimatePanel(bool show)
        {
            float startAlpha = panelCanvasGroup.alpha;
            float targetAlpha = show ? 1f : 0f;
            float elapsedTime = 0f;
            
            if (show)
            {
                stageSelectionPanel.SetActive(true);
            }
            
            while (elapsedTime < animationDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / animationDuration;
                
                panelCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                yield return null;
            }
            
            panelCanvasGroup.alpha = targetAlpha;
            panelCanvasGroup.interactable = show;
            panelCanvasGroup.blocksRaycasts = show;
            
            if (!show)
            {
                stageSelectionPanel.SetActive(false);
            }
        }
        
        /// <summary>
        /// 자동 숨김 코루틴
        /// </summary>
        private IEnumerator AutoHideCoroutine()
        {
            yield return new WaitForSeconds(autoHideTime);
            
            if (isVisible)
            {
                SetPanelVisibility(false);
            }
        }
        
        /// <summary>
        /// UI 업데이트
        /// </summary>
        public void UpdateUI()
        {
            if (stageManager == null) return;
            
            // 현재 스테이지 텍스트 업데이트
            UpdateCurrentStageText();
            
            // 모든 스테이지 버튼 업데이트
            foreach (var stageButton in stageButtons)
            {
                stageButton.UpdateButtonState();
            }
        }
        
        private void UpdateCurrentStageText()
        {
            if (currentStageText == null) return;
            
            var currentStage = stageManager.CurrentStage;
            if (currentStage != null)
            {
                currentStageText.text = $"현재 위치: {currentStage.StageName}";
            }
        }
        
        // === EVENT HANDLERS ===
        
        private void OnStageChanged(int newStageIndex)
        {
            UpdateUI();
            
            // 패널이 열려있으면 업데이트 후 자동으로 닫기
            if (isVisible)
            {
                StartCoroutine(DelayedHide());
            }
        }
        
        private IEnumerator DelayedHide()
        {
            yield return new WaitForSeconds(1f);
            SetPanelVisibility(false);
        }
        
        private void OnStageUnlocked(int stageIndex)
        {
            UpdateUI();
            
            // 잠금 해제 알림 (선택적)
            Debug.Log($"[스테이지네비게이션UI] 스테이지 {stageIndex} 잠금 해제!");
        }
        
        private void OnStageTransitionProgress(float progress)
        {
            // 전환 진행률 표시 (선택적)
            // 프로그레스 바나 로딩 인디케이터 업데이트
        }
        
        private void OnNavigationButtonClicked(int buttonIndex)
        {
            switch (buttonIndex)
            {
                case 0: // 이전 스테이지
                    stageManager.GoToPreviousStage();
                    break;
                    
                case 1: // 다음 스테이지
                    stageManager.GoToNextStage();
                    break;
                    
                case 2: // 스테이지 선택
                    ToggleStageSelection();
                    break;
                    
                case 3: // 메인 메뉴
                    // SceneController.LoadMainMenu();
                    break;
            }
        }
        
        // === PUBLIC API ===
        
        /// <summary>
        /// 특정 스테이지로 이동 요청
        /// </summary>
        public void RequestStageChange(int stageIndex)
        {
            if (stageManager.CanAccessStage(stageIndex))
            {
                stageManager.ChangeStage(stageIndex);
                SetPanelVisibility(false);
            }
            else
            {
                Debug.LogWarning($"[스테이지네비게이션UI] 스테이지 {stageIndex}에 접근할 수 없습니다");
                // 알림 UI 표시 (선택적)
            }
        }
        
        /// <summary>
        /// 스테이지 버튼 색상 가져오기
        /// </summary>
        public Color GetStageButtonColor(int stageIndex)
        {
            if (stageIndex == stageManager.CurrentStageIndex)
            {
                return currentStageColor;
            }
            else if (!stageManager.CanAccessStage(stageIndex))
            {
                return lockedStageColor;
            }
            else if (!stageManager.CanTransitionToStage(stageManager.CurrentStageIndex, stageIndex))
            {
                return restrictedStageColor;
            }
            else
            {
                return unlockedStageColor;
            }
        }
        
        /// <summary>
        /// 스테이지 버튼 상호작용 가능 여부
        /// </summary>
        public bool IsStageButtonInteractable(int stageIndex)
        {
            return stageManager.CanAccessStage(stageIndex) && 
                   stageManager.CanTransitionToStage(stageManager.CurrentStageIndex, stageIndex) &&
                   !stageManager.IsTransitioning;
        }
    }
}