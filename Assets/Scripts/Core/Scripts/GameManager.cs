using UnityEngine;
using AFKS.Shared.Utils;
using AFKS.Shared.Events;
using AFKS.Shared.Interfaces;

namespace AFKS.Core
{
    /// <summary>
    /// 게임 전체를 관리하는 핵심 매니저
    /// 싱글톤 패턴으로 구현
    /// </summary>
    public class GameManager : MonoBehaviour, ISaveable
    {
        // === SINGLETON ===
        private static GameManager instance;
        public static GameManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<GameManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("GameManager");
                        instance = go.AddComponent<GameManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }
        
        // === PROPERTIES ===
        [Header("🎮 게임 설정")]
        [SerializeField, Tooltip("게임 전역 설정 데이터 (GameConfig ScriptableObject)")] private GameConfig gameConfig;
        
        [Header("📊 현재 게임 상태")]
        [SerializeField, Tooltip("현재 게임 상태 (MainMenu, Playing, Paused 등)")] private GameState currentState = GameState.MainMenu;
        [SerializeField, Range(0, 10), Tooltip("현재 진행 중인 스테이지 인덱스")] private int currentStageIndex = 0;
        [SerializeField, Tooltip("게임 진행 시간 (초)")] private float gameTime = 0f;
        
        // === PERFORMANCE MONITORING ===
        private float performanceMonitorTimer = 0f;
        
        [Header("📡 이벤트")]
        [SerializeField, Tooltip("게임 상태 변경 시 발생하는 이벤트")] private GameEvent onGameStateChanged;
        [SerializeField, Tooltip("게임 일시정지 시 발생하는 이벤트")] private GameEvent onGamePaused;
        [SerializeField, Tooltip("게임 재개 시 발생하는 이벤트")] private GameEvent onGameResumed;
        
        // === RUNTIME EVENTS ===
        public static readonly GameEvent<GameState> OnGameStateChanged = new GameEvent<GameState>();
        public static readonly GameEvent<int> OnStageChanged = new GameEvent<int>();
        public static readonly GameEvent<float> OnGameTimeUpdated = new GameEvent<float>();
        
        // === PROPERTIES ===
        public GameState CurrentState => currentState;
        public int CurrentStageIndex => currentStageIndex;
        public float GameTime => gameTime;
        public bool IsPaused { get; private set; }
        public string SaveID => "GameManager";
        
        // === UNITY LIFECYCLE ===
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeGame();
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            // 초기 상태 설정
            ChangeGameState(GameState.MainMenu);
        }
        
        private void Update()
        {
            if (!IsPaused && currentState == GameState.Playing)
            {
                gameTime += Time.deltaTime;
                OnGameTimeUpdated.Raise(gameTime);
            }
            
            // 성능 모니터링 (최적화: 1초마다 실행)
            performanceMonitorTimer += Time.unscaledDeltaTime;
            if (performanceMonitorTimer >= 1f)
            {
                MonitorPerformance();
                performanceMonitorTimer = 0f;
            }
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
                PauseGame();
            else
                ResumeGame();
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && currentState == GameState.Playing)
                PauseGame();
            else if (hasFocus && IsPaused)
                ResumeGame();
        }
        
        // === INITIALIZATION ===
        private void InitializeGame()
        {
            // 프레임레이트 설정
            Application.targetFrameRate = Constants.TARGET_FRAME_RATE;
            
            // 화면 꺼짐 방지
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            
            Debug.Log("[게임매니저] 게임 초기화 성공");
        }
        
        // === GAME STATE MANAGEMENT ===
        public void ChangeGameState(GameState newState)
        {
            if (currentState == newState) return;
            
            GameState previousState = currentState;
            currentState = newState;
            
            Debug.Log($"[게임매니저] 상태 변경: {previousState} -> {newState}");
            
            // 이벤트 발생
            onGameStateChanged?.Raise();
            OnGameStateChanged.Raise(newState);
            
            // 상태별 처리
            HandleStateChange(previousState, newState);
        }
        
        private void HandleStateChange(GameState previousState, GameState newState)
        {
            switch (newState)
            {
                case GameState.MainMenu:
                    Time.timeScale = 1f;
                    IsPaused = false;
                    break;
                    
                case GameState.Playing:
                    Time.timeScale = 1f;
                    IsPaused = false;
                    break;
                    
                case GameState.Paused:
                    Time.timeScale = 0f;
                    IsPaused = true;
                    break;
                    
                case GameState.GameOver:
                    Time.timeScale = 0f;
                    break;
                    
                case GameState.Loading:
                    // 로딩 상태에서는 게임 진행 일시정지
                    break;
            }
        }
        
        // === STAGE MANAGEMENT ===
        public void ChangeStage(int stageIndex)
        {
            if (stageIndex < 0 || stageIndex >= Constants.TOTAL_STAGES)
            {
                Debug.LogError($"[게임매니저] 잘못된 스테이지 인덱스: {stageIndex}");
                return;
            }
            
            currentStageIndex = stageIndex;
            OnStageChanged.Raise(stageIndex);
            
            Debug.Log($"[게임매니저] 스테이지 변경: {stageIndex}");
        }
        
        public void NextStage()
        {
            ChangeStage(currentStageIndex + 1);
        }
        
        public void PreviousStage()
        {
            ChangeStage(currentStageIndex - 1);
        }
        
        // === GAME CONTROL ===
        public void StartNewGame()
        {
            gameTime = 0f;
            currentStageIndex = 0;
            ChangeGameState(GameState.Playing);
            Debug.Log("[게임매니저] 새 게임 시작");
        }
        
        public void PauseGame()
        {
            if (currentState == GameState.Playing)
            {
                IsPaused = true;
                Time.timeScale = 0f;
                onGamePaused?.Raise();
                Debug.Log("[게임매니저] 게임 일시정지");
            }
        }
        
        public void ResumeGame()
        {
            if (IsPaused)
            {
                IsPaused = false;
                Time.timeScale = 1f;
                onGameResumed?.Raise();
                Debug.Log("[게임매니저] 게임 재개");
            }
        }
        
        public void EndGame()
        {
            ChangeGameState(GameState.GameOver);
            Debug.Log("[게임매니저] 게임 종료");
        }
        
        public void ReturnToMainMenu()
        {
            ChangeGameState(GameState.MainMenu);
            Debug.Log("[게임매니저] 메인 메뉴로 돌아감");
        }
        
        // === PERFORMANCE MONITORING ===
        private void MonitorPerformance()
        {
            // 메모리 사용량 체크
            long memoryUsage = System.GC.GetTotalMemory(false) / (1024 * 1024); // MB 단위
            
            if (memoryUsage > Constants.MEMORY_THRESHOLD_MB)
            {
                Debug.LogWarning($"[게임매니저] 높은 메모리 사용량 감지: {memoryUsage}MB");
                // 필요 시 가비지 컬렉션 강제 실행
                System.GC.Collect();
            }
            
            // 프레임레이트 체크
            if (Application.targetFrameRate != Constants.TARGET_FRAME_RATE)
            {
                Application.targetFrameRate = Constants.TARGET_FRAME_RATE;
            }
        }
        
        // === SAVE SYSTEM ===
        public string GetSaveData()
        {
            GameSaveData saveData = new GameSaveData
            {
                currentStageIndex = this.currentStageIndex,
                gameTime = this.gameTime,
                gameState = this.currentState
            };
            
            return JsonUtility.ToJson(saveData);
        }
        
        public void LoadSaveData(string data)
        {
            if (data.IsNullOrEmpty()) return;
            
            try
            {
                GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(data);
                currentStageIndex = saveData.currentStageIndex;
                gameTime = saveData.gameTime;
                ChangeGameState(saveData.gameState);
                
                Debug.Log("[게임매니저] 저장 데이터 로드 성공");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[게임매니저] 저장 데이터 로드 실패: {e.Message}");
            }
        }
    }
    
    // === ENUMS ===
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        Loading,
        GameOver
    }
    
    // === SAVE DATA ===
    [System.Serializable]
    public class GameSaveData
    {
        public int currentStageIndex;
        public float gameTime;
        public GameState gameState;
    }
}