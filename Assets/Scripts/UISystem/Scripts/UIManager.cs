using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using AFKS.Shared.Events;
using AFKS.Shared.Utils;
using AFKS.ItemSystem;

namespace AFKS.UISystem
{
    /// <summary>
    /// UI 시스템을 관리하는 매니저
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("🖼️ 메인 UI 패널")]
        [SerializeField, Tooltip("메인 UI Canvas (모든 UI의 부모)")] private Canvas mainCanvas;
        [SerializeField, Tooltip("게임플레이 패널 (게임 진행 중 UI)")] private CanvasGroup gameplayPanel;
        [SerializeField, Tooltip("인벤토리 패널 (아이템 관리 UI)")] private CanvasGroup inventoryPanel;
        [SerializeField, Tooltip("메뉴 패널 (일시정지/설정 메뉴)")] private CanvasGroup menuPanel;
        [SerializeField, Tooltip("설정 패널 (옵션 설정 UI)")] private CanvasGroup settingsPanel;
        [SerializeField, Tooltip("로딩 패널 (로딩 화면 UI)")] private CanvasGroup loadingPanel;
        
        [Header("📱 오버레이 패널")]
        [SerializeField, Tooltip("페이드 오버레이 (화면 전환 효과)")] private CanvasGroup fadeOverlay;
        [SerializeField, Tooltip("대화 패널 (텍스트 대화창)")] private CanvasGroup dialogPanel;
        [SerializeField, Tooltip("알림 패널 (게임 알림 메시지)")] private CanvasGroup notificationPanel;
        
        [Header("🎒 인벤토리 UI")]
        [SerializeField, Tooltip("인벤토리 슬롯들이 배치될 컨테이너")] private Transform inventoryContainer;
        [SerializeField, Tooltip("인벤토리 슬롯 프리팹 (동적 생성용)")] private GameObject inventorySlotPrefab;
        [SerializeField, Tooltip("선택된 아이템 이름 표시 텍스트")] private TextMeshProUGUI itemNameText;
        [SerializeField, Tooltip("선택된 아이템 설명 표시 텍스트")] private TextMeshProUGUI itemDescriptionText;
        [SerializeField, Tooltip("선택된 아이템 상세 이미지")] private Image itemDetailImage;
        
        [Header("⚙️ 설정")]
        [SerializeField, Range(0.1f, 2f), Tooltip("패널 전환 애니메이션 시간 (초)")] private float panelTransitionDuration = 0.3f;
        [SerializeField, Range(0.1f, 2f), Tooltip("페이드 전환 애니메이션 시간 (초)")] private float fadeTransitionDuration = 0.5f;
        [SerializeField, Tooltip("UI 애니메이션 활성화 여부")] private bool enableUIAnimations = true;
        
        [Header("📡 이벤트")]
        [SerializeField, Tooltip("패널 변경 시 발생하는 게임 이벤트")] private GameEvent onPanelChanged;
        [SerializeField, Tooltip("인벤토리 토글 시 발생하는 게임 이벤트")] private GameEvent onInventoryToggled;
        
        // === RUNTIME EVENTS ===
        public static readonly GameEvent<UIPanel> OnPanelChanged = new GameEvent<UIPanel>();
        public static readonly GameEvent<bool> OnInventoryToggled = new GameEvent<bool>();
        public static readonly GameEvent<string> OnDialogShown = new GameEvent<string>();
        
        // === PROPERTIES ===
        public UIPanel CurrentPanel { get; private set; } = UIPanel.Gameplay;
        public bool IsInventoryOpen { get; private set; }
        public bool IsTransitioning { get; private set; }
        public bool IsDialogOpen => dialogPanel != null && dialogPanel.gameObject.activeInHierarchy;
        
        // === PRIVATE FIELDS ===
        private Dictionary<UIPanel, CanvasGroup> panelMap = new Dictionary<UIPanel, CanvasGroup>();
        private List<InventorySlotUI> inventorySlots = new List<InventorySlotUI>();
        private Coroutine currentTransition;
        
        // === SINGLETON ACCESS ===
        private static UIManager instance;
        public static UIManager Instance
        {
            get
            {
                if (instance == null)
                    instance = FindFirstObjectByType<UIManager>();
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
                InitializeUIManager();
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            SetupEventListeners();
            ShowPanel(UIPanel.Gameplay, false);
        }
        
        private void Update()
        {
            HandleInput();
        }
        
        private void OnDestroy()
        {
            RemoveEventListeners();
        }
        
        // === INITIALIZATION ===
        private void InitializeUIManager()
        {
            SetupPanelMap();
            CreateInventoryUI();
            SetupOverlays();
            
            Debug.Log("[UI매니저] 초기화 완료");
        }
        
        /// <summary>
        /// 패널 맵 설정
        /// </summary>
        private void SetupPanelMap()
        {
            panelMap[UIPanel.Gameplay] = gameplayPanel;
            panelMap[UIPanel.Inventory] = inventoryPanel;
            panelMap[UIPanel.Menu] = menuPanel;
            panelMap[UIPanel.Settings] = settingsPanel;
            panelMap[UIPanel.Loading] = loadingPanel;
        }
        
        /// <summary>
        /// 인벤토리 UI 생성
        /// </summary>
        private void CreateInventoryUI()
        {
            if (inventoryContainer == null || inventorySlotPrefab == null) return;
            
            // 기존 슬롯들 정리
            foreach (Transform child in inventoryContainer)
            {
                Destroy(child.gameObject);
            }
            inventorySlots.Clear();
            
            // 간단한 키 아이템 슬롯들 생성 (열쇠, 손전등)
            if (AFKS.ItemSystem.ItemManager.Instance != null)
            {
                int maxSlots = 2; // 열쇠, 손전등만
                for (int i = 0; i < maxSlots; i++)
                {
                    GameObject slotObject = Instantiate(inventorySlotPrefab, inventoryContainer);
                    InventorySlotUI slotUI = slotObject.GetComponent<InventorySlotUI>();
                    
                    if (slotUI != null)
                    {
                        slotUI.Initialize(i);
                        inventorySlots.Add(slotUI);
                    }
                }
            }
        }
        
        /// <summary>
        /// 오버레이 설정
        /// </summary>
        private void SetupOverlays()
        {
            if (fadeOverlay != null)
            {
                fadeOverlay.alpha = 0f;
                fadeOverlay.gameObject.SetActive(false);
            }
            
            if (dialogPanel != null)
            {
                dialogPanel.alpha = 0f;
                dialogPanel.gameObject.SetActive(false);
            }
            
            if (notificationPanel != null)
            {
                notificationPanel.alpha = 0f;
                notificationPanel.gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// 이벤트 리스너 설정
        /// </summary>
        private void SetupEventListeners()
        {
            if (AFKS.ItemSystem.ItemManager.Instance != null)
            {
                AFKS.ItemSystem.ItemManager.OnKeyItemObtained.AddListener(OnKeyItemObtained);
            }
        }
        
        /// <summary>
        /// 이벤트 리스너 제거
        /// </summary>
        private void RemoveEventListeners()
        {
            if (AFKS.ItemSystem.ItemManager.Instance != null)
            {
                AFKS.ItemSystem.ItemManager.OnKeyItemObtained.RemoveListener(OnKeyItemObtained);
            }
        }
        
        // === INPUT HANDLING ===
        private void HandleInput()
        {
            // ESC 키로 메뉴 토글
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (CurrentPanel == UIPanel.Gameplay)
                    ShowPanel(UIPanel.Menu);
                else if (CurrentPanel == UIPanel.Menu)
                    ShowPanel(UIPanel.Gameplay);
            }
            
            // Tab 키로 인벤토리 토글
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                ToggleInventory();
            }
        }
        
        // === PANEL MANAGEMENT ===
        
        /// <summary>
        /// 패널 표시
        /// </summary>
        /// <param name="panel">표시할 패널</param>
        /// <param name="useAnimation">애니메이션 사용 여부</param>
        public void ShowPanel(UIPanel panel, bool useAnimation = true)
        {
            if (IsTransitioning && useAnimation) return;
            
            if (currentTransition != null)
            {
                StopCoroutine(currentTransition);
            }
            
            if (useAnimation && enableUIAnimations)
            {
                currentTransition = StartCoroutine(TransitionToPanel(panel));
            }
            else
            {
                ShowPanelImmediate(panel);
            }
        }
        
        /// <summary>
        /// 패널 즉시 표시
        /// </summary>
        private void ShowPanelImmediate(UIPanel panel)
        {
            // 모든 패널 숨김
            foreach (var kvp in panelMap)
            {
                if (kvp.Value != null)
                {
                    kvp.Value.alpha = kvp.Key == panel ? 1f : 0f;
                    kvp.Value.interactable = kvp.Key == panel;
                    kvp.Value.blocksRaycasts = kvp.Key == panel;
                    kvp.Value.gameObject.SetActive(kvp.Key == panel);
                }
            }
            
            CurrentPanel = panel;
            
            onPanelChanged?.Raise();
            OnPanelChanged.Raise(panel);
            
            Debug.Log($"[UI매니저] 패널 표시: {panel}");
        }
        
        /// <summary>
        /// 패널 전환 애니메이션
        /// </summary>
        private IEnumerator TransitionToPanel(UIPanel targetPanel)
        {
            IsTransitioning = true;
            
            CanvasGroup currentPanelGroup = panelMap[CurrentPanel];
            CanvasGroup targetPanelGroup = panelMap[targetPanel];
            
            // 타겟 패널 활성화
            if (targetPanelGroup != null)
            {
                targetPanelGroup.gameObject.SetActive(true);
                targetPanelGroup.alpha = 0f;
                targetPanelGroup.interactable = false;
                targetPanelGroup.blocksRaycasts = false;
            }
            
            // 페이드 아웃 & 페이드 인
            float elapsedTime = 0f;
            while (elapsedTime < panelTransitionDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / panelTransitionDuration;
                
                if (currentPanelGroup != null)
                {
                    currentPanelGroup.alpha = 1f - t;
                }
                
                if (targetPanelGroup != null)
                {
                    targetPanelGroup.alpha = t;
                }
                
                yield return null;
            }
            
            // 완료 처리
            if (currentPanelGroup != null)
            {
                currentPanelGroup.alpha = 0f;
                currentPanelGroup.interactable = false;
                currentPanelGroup.blocksRaycasts = false;
                currentPanelGroup.gameObject.SetActive(false);
            }
            
            if (targetPanelGroup != null)
            {
                targetPanelGroup.alpha = 1f;
                targetPanelGroup.interactable = true;
                targetPanelGroup.blocksRaycasts = true;
            }
            
            CurrentPanel = targetPanel;
            IsTransitioning = false;
            
            onPanelChanged?.Raise();
            OnPanelChanged.Raise(targetPanel);
            
            Debug.Log($"[UI매니저] 패널 전환 완료: {targetPanel}");
        }
        
        // === INVENTORY UI ===
        
        /// <summary>
        /// 인벤토리 토글
        /// </summary>
        public void ToggleInventory()
        {
            SetInventoryVisible(!IsInventoryOpen);
        }
        
        /// <summary>
        /// 인벤토리 표시 설정
        /// </summary>
        /// <param name="visible">표시 여부</param>
        public void SetInventoryVisible(bool visible)
        {
            if (IsInventoryOpen == visible) return;
            
            IsInventoryOpen = visible;
            
            if (inventoryPanel != null)
            {
                if (enableUIAnimations)
                {
                    StartCoroutine(visible ? inventoryPanel.FadeIn(panelTransitionDuration) 
                                          : inventoryPanel.FadeOut(panelTransitionDuration));
                }
                else
                {
                    inventoryPanel.gameObject.SetActive(visible);
                    inventoryPanel.alpha = visible ? 1f : 0f;
                }
            }
            
            onInventoryToggled?.Raise();
            OnInventoryToggled.Raise(visible);
            
            Debug.Log($"[UI매니저] 인벤토리 {(visible ? "열림" : "닫힘")}");
        }
        
        /// <summary>
        /// 인벤토리 변경 이벤트 처리
        /// </summary>
        private void OnKeyItemObtained(string itemId)
        {
            UpdateKeyItemDisplay();
        }
        
        /// <summary>
        /// 키 아이템 표시 업데이트 (간단화)
        /// </summary>
        private void UpdateKeyItemDisplay()
        {
            if (AFKS.ItemSystem.ItemManager.Instance == null) return;
            
            // 키 아이템 상태 업데이트
            bool hasKey = AFKS.ItemSystem.ItemManager.Instance.HasKey;
            bool hasFlashlight = AFKS.ItemSystem.ItemManager.Instance.HasFlashlight;
            
            Debug.Log($"[UI매니저] 키 아이템 업데이트 - 열쇠: {hasKey}, 손전등: {hasFlashlight}");
            
            // UI 업데이트 로직 (필요시 구현)
            // 예: 키 아이템 아이콘 활성화/비활성화
        }
        
        // === DIALOG SYSTEM ===
        
        /// <summary>
        /// 대화창 표시
        /// </summary>
        /// <param name="text">표시할 텍스트</param>
        /// <param name="duration">표시 시간 (0이면 무한)</param>
        public void ShowDialog(string text, float duration = 0f)
        {
            if (dialogPanel == null) return;
            
            // 대화창 텍스트 설정
            Text dialogText = dialogPanel.GetComponentInChildren<Text>();
            if (dialogText != null)
            {
                dialogText.text = text;
            }
            
            StartCoroutine(ShowDialogCoroutine(duration));
            
            OnDialogShown.Raise(text);
            Debug.Log($"[UI매니저] 대화창 표시: {text}");
        }
        
        /// <summary>
        /// 대화창 표시 코루틴
        /// </summary>
        private IEnumerator ShowDialogCoroutine(float duration)
        {
            yield return StartCoroutine(dialogPanel.FadeIn(panelTransitionDuration));
            
            if (duration > 0f)
            {
                yield return new WaitForSeconds(duration);
                yield return StartCoroutine(dialogPanel.FadeOut(panelTransitionDuration));
            }
        }
        
        /// <summary>
        /// 대화창 숨김
        /// </summary>
        public void HideDialog()
        {
            if (dialogPanel != null && IsDialogOpen)
            {
                StartCoroutine(dialogPanel.FadeOut(panelTransitionDuration));
            }
        }
        
        // === FADE EFFECTS ===
        
        /// <summary>
        /// 화면 페이드 아웃
        /// </summary>
        public IEnumerator FadeOut(float duration = -1f)
        {
            if (duration < 0f) duration = fadeTransitionDuration;
            
            if (fadeOverlay != null)
            {
                yield return StartCoroutine(fadeOverlay.FadeIn(duration));
            }
        }
        
        /// <summary>
        /// 화면 페이드 인
        /// </summary>
        public IEnumerator FadeIn(float duration = -1f)
        {
            if (duration < 0f) duration = fadeTransitionDuration;
            
            if (fadeOverlay != null)
            {
                yield return StartCoroutine(fadeOverlay.FadeOut(duration));
            }
        }
        
        // === NOTIFICATION SYSTEM ===
        
        /// <summary>
        /// 알림 표시
        /// </summary>
        /// <param name="message">알림 메시지</param>
        /// <param name="duration">표시 시간</param>
        public void ShowNotification(string message, float duration = 3f)
        {
            if (notificationPanel == null) return;
            
            Text notificationText = notificationPanel.GetComponentInChildren<Text>();
            if (notificationText != null)
            {
                notificationText.text = message;
            }
            
            StartCoroutine(ShowNotificationCoroutine(duration));
        }
        
        /// <summary>
        /// 알림 표시 코루틴
        /// </summary>
        private IEnumerator ShowNotificationCoroutine(float duration)
        {
            yield return StartCoroutine(notificationPanel.FadeIn(0.3f));
            yield return new WaitForSeconds(duration);
            yield return StartCoroutine(notificationPanel.FadeOut(0.3f));
        }
        
        // === UTILITY METHODS ===
        
        /// <summary>
        /// 게임플레이 패널로 돌아가기
        /// </summary>
        public void ReturnToGameplay()
        {
            ShowPanel(UIPanel.Gameplay);
        }
        
        /// <summary>
        /// 메뉴 패널 표시
        /// </summary>
        public void ShowMenu()
        {
            ShowPanel(UIPanel.Menu);
        }
        
        /// <summary>
        /// 설정 패널 표시
        /// </summary>
        public void ShowSettings()
        {
            ShowPanel(UIPanel.Settings);
        }
        
        /// <summary>
        /// 로딩 패널 표시
        /// </summary>
        public void ShowLoading(bool show)
        {
            if (show)
                ShowPanel(UIPanel.Loading, false);
            else
                ShowPanel(UIPanel.Gameplay, false);
        }
    }
    
    // === ENUMS ===
    public enum UIPanel
    {
        Gameplay,
        Inventory,
        Menu,
        Settings,
        Loading
    }
}