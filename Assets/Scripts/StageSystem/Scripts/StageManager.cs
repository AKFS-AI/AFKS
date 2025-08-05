using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using AFKS.Shared.Utils;
using AFKS.Shared.Events;
using AFKS.Shared.Interfaces;
using AFKS.Core;

namespace AFKS.StageSystem
{
    /// <summary>
    /// 스테이지 관리 및 전환을 담당하는 매니저
    /// </summary>
    public class StageManager : MonoBehaviour, ISaveable
    {
        [Header("🎭 스테이지 설정")]
        [SerializeField, Tooltip("게임에서 사용할 모든 스테이지 데이터 목록")] private List<StageData> stages = new List<StageData>();
        [SerializeField, Range(0, 10), Tooltip("현재 활성화된 스테이지 인덱스")] private int currentStageIndex = 0;
        
        [Header("🖼️ UI 참조")]
        [SerializeField, Tooltip("스테이지 배경 이미지를 표시할 Image 컴포넌트")] private Image backgroundImage;
        [SerializeField, Tooltip("스테이지 전체 Canvas Group")] private CanvasGroup stageCanvas;
        [SerializeField, Tooltip("스테이지 전환 효과용 Canvas Group")] private CanvasGroup transitionCanvas;
        
        [Header("🌊 전환 설정")]
        [SerializeField, Range(0.1f, 5f), Tooltip("스테이지 전환 애니메이션 시간 (초)")] private float transitionDuration = 1f;
        [SerializeField, Tooltip("스테이지 미리 로드 기능 활성화 여부")] private bool enablePreloading = true;
        [SerializeField, Range(1, 5), Tooltip("미리 로드할 스테이지 개수")] private int maxPreloadStages = 2;
        
        [Header("🔒 고급 스테이지 시스템")]
        [SerializeField, Tooltip("잠금 해제된 스테이지들")] private HashSet<int> unlockedStages = new HashSet<int>();
        [SerializeField, Tooltip("픽셀 퍼펙트 상호작용 지원")] private bool enablePixelPerfectInteraction = true;
        [SerializeField, Tooltip("자동 상태 저장 간격 (초)")] private float autoSaveInterval = 30f;

        [Header("📡 이벤트")]
        [SerializeField, Tooltip("스테이지 변경 시 발생하는 게임 이벤트")] private GameEvent onStageChanged;
        [SerializeField, Tooltip("스테이지 전환 시작 시 발생하는 게임 이벤트")] private GameEvent onStageTransitionStarted;
        [SerializeField, Tooltip("스테이지 전환 완료 시 발생하는 게임 이벤트")] private GameEvent onStageTransitionCompleted;
        
        // === RUNTIME EVENTS ===
        public static readonly GameEvent<int> OnStageChanged = new GameEvent<int>();
        public static readonly GameEvent<StageData> OnStageDataLoaded = new GameEvent<StageData>();
        public static readonly GameEvent<float> OnTransitionProgress = new GameEvent<float>();
        public static readonly GameEvent<int> OnStageUnlocked = new GameEvent<int>();
        
        // === PROPERTIES ===
        public StageData CurrentStage => GetStageData(currentStageIndex);
        public StageData CurrentStageData => GetStageData(currentStageIndex);
        public int CurrentStageIndex => currentStageIndex;
        public int TotalStages => stages.Count;
        public bool IsTransitioning { get; private set; }
        public string SaveID => "StageManager";
        
        /// <summary>
        /// 스테이지 데이터 배열 (읽기 전용 접근)
        /// </summary>
        public List<StageData> Stages => stages;
        
        // === PRIVATE FIELDS ===
        private Dictionary<int, Sprite> preloadedBackgrounds = new Dictionary<int, Sprite>();
        private List<StageInteractionController> interactionControllers = new List<StageInteractionController>();
        private Dictionary<int, List<PixelPerfectInteractionController>> stageInteractionPoints = new Dictionary<int, List<PixelPerfectInteractionController>>();
        private bool isInitialized = false;
        private Coroutine autoSaveCoroutine;
        
        // === SINGLETON ACCESS ===
        private static StageManager instance;
        public static StageManager Instance
        {
            get
            {
                if (instance == null)
                    instance = FindFirstObjectByType<StageManager>();
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
            }
            else if (instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }
        
        private void Start()
        {
            InitializeStageManager();
        }
        
        // === INITIALIZATION ===
        private void InitializeStageManager()
        {
            if (isInitialized) return;
            
            // 초기 검증
            ValidateStages();
            
            // 잠금 해제된 스테이지 로드
            LoadUnlockedStages();
            
            // 첫 번째 스테이지 잠금 해제 및 로드
            UnlockStage(0);
            LoadStageImmediate(currentStageIndex);
            
            // 픽셀 퍼펙트 상호작용 시스템 초기화
            if (enablePixelPerfectInteraction)
            {
                InitializePixelPerfectInteractions();
            }
            
            // 프리로딩 시작
            if (enablePreloading)
            {
                StartCoroutine(PreloadAdjacentStages());
            }
            
            // 자동 저장 시작
            if (autoSaveInterval > 0)
            {
                autoSaveCoroutine = StartCoroutine(AutoSaveCoroutine());
            }
            
            isInitialized = true;
            Debug.Log($"[스테이지매니저] {stages.Count}개 스테이지로 초기화 완료, 픽셀퍼펙트: {enablePixelPerfectInteraction}");
        }
        
        private void ValidateStages()
        {
            for (int i = 0; i < stages.Count; i++)
            {
                if (stages[i] == null)
                {
                    Debug.LogError($"[스테이지매니저] 인덱스 {i}의 스테이지가 null입니다!");
                    continue;
                }
                
                if (stages[i].BackgroundImage == null)
                {
                    Debug.LogWarning($"[스테이지매니저] 스테이지 {i} ({stages[i].StageName})에 배경 이미지가 없습니다!");
                }
            }
        }
        
        // === STAGE MANAGEMENT ===
        
        /// <summary>
        /// 특정 스테이지로 전환
        /// </summary>
        /// <param name="stageIndex">스테이지 인덱스</param>
        /// <param name="useTransition">전환 효과 사용 여부</param>
        public void ChangeStage(int stageIndex, bool useTransition = true)
        {
            if (IsTransitioning)
            {
                Debug.LogWarning("[스테이지매니저] 이미 전환 중입니다. 스테이지 변경 요청을 무시합니다.");
                return;
            }
            
            if (!IsValidStageIndex(stageIndex))
            {
                Debug.LogError($"[스테이지매니저] 잘못된 스테이지 인덱스: {stageIndex}");
                return;
            }
            
            if (useTransition)
            {
                StartCoroutine(TransitionToStage(stageIndex));
            }
            else
            {
                LoadStageImmediate(stageIndex);
            }
        }
        
        /// <summary>
        /// 다음 스테이지로 이동
        /// </summary>
        public void NextStage()
        {
            int nextIndex = currentStageIndex + 1;
            if (IsValidStageIndex(nextIndex))
            {
                ChangeStage(nextIndex);
            }
            else
            {
                Debug.Log("[스테이지매니저] 이미 마지막 스테이지입니다.");
            }
        }
        
        /// <summary>
        /// 이전 스테이지로 이동
        /// </summary>
        public void PreviousStage()
        {
            int previousIndex = currentStageIndex - 1;
            if (IsValidStageIndex(previousIndex))
            {
                ChangeStage(previousIndex);
            }
            else
            {
                Debug.Log("[스테이지매니저] 이미 첫 번째 스테이지입니다.");
            }
        }
        
        /// <summary>
        /// 첫 번째 스테이지로 리셋
        /// </summary>
        public void ResetToFirstStage()
        {
            ChangeStage(0);
        }
        
        // === PRIVATE METHODS ===
        
        /// <summary>
        /// 전환 효과와 함께 스테이지 변경
        /// </summary>
        private IEnumerator TransitionToStage(int stageIndex)
        {
            IsTransitioning = true;
            onStageTransitionStarted?.Raise();
            
            // 페이드 아웃
            yield return StartCoroutine(FadeOut());
            
            // 스테이지 로드
            yield return StartCoroutine(LoadStageAsync(stageIndex));
            
            // 페이드 인
            yield return StartCoroutine(FadeIn());
            
            IsTransitioning = false;
            onStageTransitionCompleted?.Raise();
            OnTransitionProgress.Raise(1f);
        }
        
        /// <summary>
        /// 즉시 스테이지 로드 (전환 효과 없음)
        /// </summary>
        private void LoadStageImmediate(int stageIndex)
        {
            if (!IsValidStageIndex(stageIndex)) return;
            
            StageData stageData = stages[stageIndex];
            currentStageIndex = stageIndex;
            
            // 배경 이미지 설정
            SetBackgroundImage(stageData);
            
            // 상호작용 포인트 생성
            SetupInteractionPoints(stageData);
            
            // 이벤트 발생
            onStageChanged?.Raise();
            OnStageChanged.Raise(stageIndex);
            OnStageDataLoaded.Raise(stageData);
            
            // GameManager에 알림
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ChangeStage(stageIndex);
            }
            
            Debug.Log($"[스테이지매니저] 스테이지 로드 완료: {stageData.StageName}");
        }
        
        /// <summary>
        /// 비동기 스테이지 로드
        /// </summary>
        private IEnumerator LoadStageAsync(int stageIndex)
        {
            OnTransitionProgress.Raise(0.2f);
            
            StageData stageData = stages[stageIndex];
            currentStageIndex = stageIndex;
            
            // 배경 이미지 로드
            yield return StartCoroutine(LoadBackgroundAsync(stageData));
            OnTransitionProgress.Raise(0.5f);
            
            // 상호작용 포인트 설정
            SetupInteractionPoints(stageData);
            OnTransitionProgress.Raise(0.7f);
            
            // 이벤트 발생
            onStageChanged?.Raise();
            OnStageChanged.Raise(stageIndex);
            OnStageDataLoaded.Raise(stageData);
            
            // GameManager에 알림
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ChangeStage(stageIndex);
            }
            
            OnTransitionProgress.Raise(0.9f);
            
            Debug.Log($"[스테이지매니저] 스테이지 비동기 로드 완료: {stageData.StageName}");
        }
        
        /// <summary>
        /// 배경 이미지 설정
        /// </summary>
        private void SetBackgroundImage(StageData stageData)
        {
            if (backgroundImage == null || stageData.BackgroundImage == null) return;
            
            backgroundImage.sprite = stageData.BackgroundImage;
            
            // 색상 조정
            if (stageData.EnableDarkMode)
            {
                backgroundImage.color = stageData.AmbientColor;
            }
            else
            {
                backgroundImage.color = Color.white;
            }
        }
        
        /// <summary>
        /// 비동기 배경 이미지 로드
        /// </summary>
        private IEnumerator LoadBackgroundAsync(StageData stageData)
        {
            // 프리로드된 이미지가 있으면 사용
            if (preloadedBackgrounds.ContainsKey(stageData.StageIndex))
            {
                backgroundImage.sprite = preloadedBackgrounds[stageData.StageIndex];
            }
            else
            {
                // 직접 로드
                backgroundImage.sprite = stageData.BackgroundImage;
            }
            
            // 색상 조정
            if (stageData.EnableDarkMode)
            {
                backgroundImage.color = stageData.AmbientColor;
            }
            else
            {
                backgroundImage.color = Color.white;
            }
            
            yield return null; // 프레임 대기
        }
        
        /// <summary>
        /// 상호작용 포인트 설정
        /// </summary>
        private void SetupInteractionPoints(StageData stageData)
        {
            // 기존 상호작용 컨트롤러 정리
            foreach (var controller in interactionControllers)
            {
                if (controller != null)
                    Destroy(controller.gameObject);
            }
            interactionControllers.Clear();
            
            // 새로운 상호작용 포인트 생성
            foreach (var interactionPoint in stageData.InteractionPoints)
            {
                CreateInteractionPoint(interactionPoint);
            }
        }
        
        /// <summary>
        /// 상호작용 포인트 생성
        /// </summary>
        private void CreateInteractionPoint(InteractionPoint interactionPoint)
        {
            // GameObject 생성
            GameObject pointObject = new GameObject($"InteractionPoint_{interactionPoint.id}");
            pointObject.transform.SetParent(stageCanvas.transform, false);
            
            // RectTransform 설정
            RectTransform rectTransform = pointObject.AddComponent<RectTransform>();
            rectTransform.anchoredPosition = interactionPoint.position;
            rectTransform.sizeDelta = interactionPoint.size;
            
            // StageInteractionController 추가
            StageInteractionController controller = pointObject.AddComponent<StageInteractionController>();
            controller.Initialize(interactionPoint);
            
            interactionControllers.Add(controller);
        }
        
        /// <summary>
        /// 인접 스테이지 프리로딩
        /// </summary>
        private IEnumerator PreloadAdjacentStages()
        {
            for (int i = 1; i <= maxPreloadStages; i++)
            {
                // 다음 스테이지 프리로드
                int nextIndex = currentStageIndex + i;
                if (IsValidStageIndex(nextIndex))
                {
                    yield return StartCoroutine(PreloadStageBackground(nextIndex));
                }
                
                // 이전 스테이지 프리로드
                int prevIndex = currentStageIndex - i;
                if (IsValidStageIndex(prevIndex))
                {
                    yield return StartCoroutine(PreloadStageBackground(prevIndex));
                }
            }
        }
        
        /// <summary>
        /// 특정 스테이지 배경 프리로드
        /// </summary>
        private IEnumerator PreloadStageBackground(int stageIndex)
        {
            if (preloadedBackgrounds.ContainsKey(stageIndex)) yield break;
            
            StageData stageData = stages[stageIndex];
            if (stageData?.BackgroundImage != null)
            {
                preloadedBackgrounds[stageIndex] = stageData.BackgroundImage;
                Debug.Log($"[스테이지매니저] 스테이지 {stageIndex} 배경을 미리 로드했습니다.");
            }
            
            yield return null;
        }
        
        // === TRANSITION EFFECTS ===
        
        /// <summary>
        /// 페이드 아웃
        /// </summary>
        private IEnumerator FadeOut()
        {
            if (transitionCanvas != null)
            {
                yield return StartCoroutine(transitionCanvas.FadeIn(transitionDuration * 0.5f));
            }
            else
            {
                yield return new WaitForSeconds(transitionDuration * 0.5f);
            }
        }
        
        /// <summary>
        /// 페이드 인
        /// </summary>
        private IEnumerator FadeIn()
        {
            if (transitionCanvas != null)
            {
                yield return StartCoroutine(transitionCanvas.FadeOut(transitionDuration * 0.5f));
            }
            else
            {
                yield return new WaitForSeconds(transitionDuration * 0.5f);
            }
        }
        
        // === UTILITY METHODS ===
        
        /// <summary>
        /// 유효한 스테이지 인덱스인지 확인
        /// </summary>
        private bool IsValidStageIndex(int index)
        {
            return index >= 0 && index < stages.Count;
        }
        
        /// <summary>
        /// 스테이지 데이터 반환
        /// </summary>
        public StageData GetStageData(int index)
        {
            return IsValidStageIndex(index) ? stages[index] : null;
        }
        
            /// <summary>
    /// 스테이지 이름으로 인덱스 찾기
    /// </summary>
    public int GetStageIndex(string stageName)
    {
        for (int i = 0; i < stages.Count; i++)
        {
            if (stages[i]?.StageName == stageName)
                return i;
        }
        return -1;
    }
    
    /// <summary>
    /// 두 스테이지 간 전환 가능 여부 확인
    /// </summary>
    /// <param name="fromStageIndex">출발 스테이지 인덱스</param>
    /// <param name="toStageIndex">도착 스테이지 인덱스</param>
    /// <returns>전환 가능 여부</returns>
    public bool CanTransitionToStage(int fromStageIndex, int toStageIndex)
    {
        // 기본 검증
        if (!IsValidStageIndex(fromStageIndex) || !IsValidStageIndex(toStageIndex))
            return false;
            
        // 현재 전환 중이면 불가
        if (IsTransitioning)
            return false;
            
        // 대상 스테이지가 잠금 해제되어 있는지 확인
        if (!CanAccessStage(toStageIndex))
            return false;
            
        // 자기 자신으로의 전환은 불가 (이미 그 스테이지에 있음)
        if (fromStageIndex == toStageIndex)
            return false;
            
        // 스테이지 데이터 검증
        var fromStage = GetStageData(fromStageIndex);
        var toStage = GetStageData(toStageIndex);
        
        if (fromStage == null || toStage == null)
            return false;
            
        // 일방통행 제한 확인 (스테이지 데이터에 제한 정보가 있다면)
        // 예: 특정 조건을 만족해야만 이동 가능한 스테이지
        if (HasTransitionRestriction(fromStageIndex, toStageIndex))
            return false;
            
        return true;
    }
    
    /// <summary>
    /// 스테이지 간 전환 제한 확인
    /// </summary>
    private bool HasTransitionRestriction(int fromStageIndex, int toStageIndex)
    {
        // 거리 제한: 인접한 스테이지로만 이동 가능한 경우
        int distance = Mathf.Abs(toStageIndex - fromStageIndex);
        
        // 기본적으로 인접 스테이지(거리 1) 또는 첫 번째 스테이지로만 이동 가능
        if (distance > 1 && toStageIndex != 0)
        {
            return true; // 제한 있음
        }
        
        // 추가 제한 로직 (예: 특정 조건 미충족)
        // 여기에 게임 특화 로직 추가 가능
        
        return false; // 제한 없음
    }
        
        // === SAVE SYSTEM ===
        public string GetSaveData()
        {
            StageSaveData saveData = new StageSaveData
            {
                currentStageIndex = this.currentStageIndex
            };
            
            return JsonUtility.ToJson(saveData);
        }
        
        public void LoadSaveData(string data)
        {
            if (data.IsNullOrEmpty()) return;
            
            try
            {
                StageSaveData saveData = JsonUtility.FromJson<StageSaveData>(data);
                ChangeStage(saveData.currentStageIndex, false);
                
                Debug.Log("[스테이지매니저] 저장 데이터를 성공적으로 로드했습니다.");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[스테이지매니저] 저장 데이터 로드 실패: {e.Message}");
            }
        }
        
        // === 고급 스테이지 기능들 ===
    
    /// <summary>
    /// 스테이지 잠금 해제
    /// </summary>
    public void UnlockStage(int stageIndex)
    {
        if (IsValidStageIndex(stageIndex) && !unlockedStages.Contains(stageIndex))
        {
            unlockedStages.Add(stageIndex);
            SaveUnlockedStages();
            OnStageUnlocked.Raise(stageIndex);
            Debug.Log($"[스테이지매니저] 스테이지 {stageIndex} 잠금 해제!");
        }
    }
    
    /// <summary>
    /// 스테이지 접근 가능 여부 확인
    /// </summary>
    public bool CanAccessStage(int stageIndex)
    {
        return IsValidStageIndex(stageIndex) && unlockedStages.Contains(stageIndex);
    }
    
    /// <summary>
    /// 다음 스테이지로 이동 (잠금 해제 포함)
    /// </summary>
    public void GoToNextStage()
    {
        int nextIndex = currentStageIndex + 1;
        if (IsValidStageIndex(nextIndex))
        {
            UnlockStage(nextIndex);
            ChangeStage(nextIndex);
        }
    }
    
    /// <summary>
    /// 이전 스테이지로 이동
    /// </summary>
    public void GoToPreviousStage()
    {
        PreviousStage();
    }
    
    /// <summary>
    /// 픽셀 퍼펙트 상호작용 시스템 초기화
    /// </summary>
    private void InitializePixelPerfectInteractions()
    {
        // 각 스테이지별 픽셀 퍼펙트 상호작용 포인트 수집
        var allPixelPerfectControllers = FindObjectsByType<PixelPerfectInteractionController>(FindObjectsSortMode.None);
        
        foreach (var controller in allPixelPerfectControllers)
        {
            // 상호작용 컨트롤러가 속한 스테이지 확인 (GameObject 이름 또는 태그로)
            int stageIndex = GetStageIndexFromController(controller);
            if (stageIndex >= 0)
            {
                if (!stageInteractionPoints.ContainsKey(stageIndex))
                {
                    stageInteractionPoints[stageIndex] = new List<PixelPerfectInteractionController>();
                }
                stageInteractionPoints[stageIndex].Add(controller);
            }
        }
    }
    
    /// <summary>
    /// 컨트롤러가 속한 스테이지 인덱스 확인
    /// </summary>
    private int GetStageIndexFromController(PixelPerfectInteractionController controller)
    {
        // GameObject 이름에서 스테이지 인덱스 추출 (예: "Stage0_InteractionPoints")
        Transform parent = controller.transform.parent;
        while (parent != null)
        {
            if (parent.name.StartsWith("Stage") && parent.name.Contains("_"))
            {
                string indexStr = parent.name.Substring(5, parent.name.IndexOf('_') - 5);
                if (int.TryParse(indexStr, out int stageIndex))
                {
                    return stageIndex;
                }
            }
            parent = parent.parent;
        }
        return -1;
    }
    
    /// <summary>
    /// 자동 저장 코루틴
    /// </summary>
    private IEnumerator AutoSaveCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(autoSaveInterval);
            SaveStageStates();
        }
    }
    
    /// <summary>
    /// 스테이지 상태들 저장
    /// </summary>
    private void SaveStageStates()
    {
        foreach (var kvp in stageInteractionPoints)
        {
            int stageIndex = kvp.Key;
            var controllers = kvp.Value;
            
            foreach (var controller in controllers)
            {
                if (controller != null)
                {
                    var state = controller.GetCurrentState();
                    // 상태 저장 로직 (필요에 따라 PlayerPrefs 또는 SaveSystem 사용)
                    PlayerPrefs.SetString($"PixelPerfect_{controller.InteractionId}", JsonUtility.ToJson(state));
                }
            }
        }
    }
    
    /// <summary>
    /// 잠금 해제된 스테이지들 저장
    /// </summary>
    private void SaveUnlockedStages()
    {
        string unlockedStagesStr = string.Join(",", unlockedStages);
        PlayerPrefs.SetString("UnlockedStages", unlockedStagesStr);
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// 잠금 해제된 스테이지들 로드
    /// </summary>
    private void LoadUnlockedStages()
    {
        string unlockedStagesStr = PlayerPrefs.GetString("UnlockedStages", "0");
        unlockedStages.Clear();
        
        if (!string.IsNullOrEmpty(unlockedStagesStr))
        {
            string[] indices = unlockedStagesStr.Split(',');
            foreach (string indexStr in indices)
            {
                if (int.TryParse(indexStr, out int index))
                {
                    unlockedStages.Add(index);
                }
            }
        }
    }
    
    // === SAVE DATA ===
    [System.Serializable]
    public class StageSaveData
    {
        public int currentStageIndex;
    }
    }
}