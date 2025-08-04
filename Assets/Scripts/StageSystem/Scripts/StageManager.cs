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
        
        [Header("📡 이벤트")]
        [SerializeField, Tooltip("스테이지 변경 시 발생하는 게임 이벤트")] private GameEvent onStageChanged;
        [SerializeField, Tooltip("스테이지 전환 시작 시 발생하는 게임 이벤트")] private GameEvent onStageTransitionStarted;
        [SerializeField, Tooltip("스테이지 전환 완료 시 발생하는 게임 이벤트")] private GameEvent onStageTransitionCompleted;
        
        // === RUNTIME EVENTS ===
        public static readonly GameEvent<int> OnStageChanged = new GameEvent<int>();
        public static readonly GameEvent<StageData> OnStageDataLoaded = new GameEvent<StageData>();
        public static readonly GameEvent<float> OnTransitionProgress = new GameEvent<float>();
        
        // === PROPERTIES ===
        public StageData CurrentStage => GetStageData(currentStageIndex);
        public int CurrentStageIndex => currentStageIndex;
        public int TotalStages => stages.Count;
        public bool IsTransitioning { get; private set; }
        public string SaveID => "StageManager";
        
        // === PRIVATE FIELDS ===
        private Dictionary<int, Sprite> preloadedBackgrounds = new Dictionary<int, Sprite>();
        private List<StageInteractionController> interactionControllers = new List<StageInteractionController>();
        private bool isInitialized = false;
        
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
            
            // 첫 번째 스테이지 로드
            LoadStageImmediate(currentStageIndex);
            
            // 프리로딩 시작
            if (enablePreloading)
            {
                StartCoroutine(PreloadAdjacentStages());
            }
            
            isInitialized = true;
            Debug.Log($"[StageManager] Initialized with {stages.Count} stages");
        }
        
        private void ValidateStages()
        {
            for (int i = 0; i < stages.Count; i++)
            {
                if (stages[i] == null)
                {
                    Debug.LogError($"[StageManager] Stage at index {i} is null!");
                    continue;
                }
                
                if (stages[i].BackgroundImage == null)
                {
                    Debug.LogWarning($"[StageManager] Stage {i} ({stages[i].StageName}) has no background image!");
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
                Debug.LogWarning("[StageManager] Already transitioning, ignoring stage change request");
                return;
            }
            
            if (!IsValidStageIndex(stageIndex))
            {
                Debug.LogError($"[StageManager] Invalid stage index: {stageIndex}");
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
                Debug.Log("[StageManager] Already at the last stage");
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
                Debug.Log("[StageManager] Already at the first stage");
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
            
            Debug.Log($"[StageManager] Loaded stage: {stageData.StageName}");
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
            
            Debug.Log($"[StageManager] Loaded stage async: {stageData.StageName}");
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
                Debug.Log($"[StageManager] Preloaded background for stage {stageIndex}");
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
                
                Debug.Log("[StageManager] Save data loaded successfully");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[StageManager] Failed to load save data: {e.Message}");
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