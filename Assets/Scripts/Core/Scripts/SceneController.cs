using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using AFKS.Shared.Events;
using AFKS.Shared.Utils;
using AFKS.Shared.Core;

namespace AFKS.Core
{
    /// <summary>
    /// 씬 전환 및 관리를 담당하는 컨트롤러
    /// BaseSingleton을 상속받아 싱글톤 패턴 구현
    /// </summary>
    public class SceneController : BaseSingleton<SceneController>
    {
        [Header("🎬 씬 설정")]
        [SerializeField, Tooltip("메인 메뉴 씬 이름")] private string mainMenuSceneName = "MainMenu";
        [SerializeField, Tooltip("게임 메인 씬 이름")] private string gameSceneName = "Main";
        
        [Header("⏳ 로딩 설정")]
        [SerializeField, Range(0.5f, 5f), Tooltip("최소 로딩 표시 시간 (초)")] private float minimumLoadingTime = 1f;
        [SerializeField, Tooltip("페이드 전환 효과 활성화 여부")] private bool enableFadeTransition = true;
        [SerializeField, Range(0.1f, 2f), Tooltip("페이드 전환 시간 (초)")] private float fadeTransitionDuration = 0.5f;
        
        // 이벤트는 전역 EventBus 또는 정적 GameEvent<T>를 사용
        
        // === RUNTIME EVENTS ===
        public static readonly GameEvent<string> OnSceneLoadStarted = new GameEvent<string>();
        public static readonly GameEvent<string> OnSceneLoadCompleted = new GameEvent<string>();
        public static readonly GameEvent<float> OnSceneLoadProgress = new GameEvent<float>();
        
        // === PROPERTIES ===
        public bool IsLoading { get; private set; }
        public float LoadingProgress { get; private set; }
        
        // === SINGLETON - BaseSingleton<T>에서 자동 관리됨 ===
        
        // === UNITY LIFECYCLE ===
        protected override void OnSingletonAwake()
        {
            // SceneController는 별도 초기화 로직 불필요
        }
        
        // === PUBLIC METHODS ===
        
        /// <summary>
        /// 메인 메뉴로 이동
        /// </summary>
        public void LoadMainMenu()
        {
            LoadScene(mainMenuSceneName);
        }
        
        /// <summary>
        /// 게임 씬으로 이동
        /// </summary>
        public void LoadGameScene()
        {
            LoadScene(gameSceneName);
        }
        
        /// <summary>
        /// 특정 씬 로드
        /// </summary>
        /// <param name="sceneName">로드할 씬 이름</param>
        public void LoadScene(string sceneName)
        {
            if (IsLoading)
            {
                Debug.LogWarning($"[SceneController] Already loading scene. Ignoring request for: {sceneName}");
                return;
            }
            
            StartCoroutine(LoadSceneAsync(sceneName));
        }
        
        /// <summary>
        /// 현재 씬 다시 로드
        /// </summary>
        public void ReloadCurrentScene()
        {
            string currentSceneName = SceneManager.GetActiveScene().name;
            LoadScene(currentSceneName);
        }
        
        /// <summary>
        /// 게임 종료
        /// </summary>
        public void QuitGame()
        {
            Debug.Log("[SceneController] Quitting game...");
            
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        
        // === PRIVATE METHODS ===
        
        /// <summary>
        /// 비동기 씬 로딩
        /// </summary>
        private IEnumerator LoadSceneAsync(string sceneName)
        {
            IsLoading = true;
            LoadingProgress = 0f;
            
            // 이벤트 발생
            OnSceneLoadStarted.Raise(sceneName);
            AFKS.Shared.Events.EventBus.GlobalInteraction.Raise($"scene.load.start:{sceneName}");
            
            Debug.Log($"[SceneController] Starting to load scene: {sceneName}");
            
            // 페이드 아웃 시작
            if (enableFadeTransition)
            {
                yield return StartCoroutine(FadeOut());
            }
            
            // 최소 로딩 시간 보장을 위한 타이머
            float startTime = Time.realtimeSinceStartup;
            
            // 씬 로딩 시작
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            asyncLoad.allowSceneActivation = false;
            
            // 로딩 진행률 업데이트
            while (!asyncLoad.isDone)
            {
                // 90%까지는 실제 로딩 진행률 사용
                LoadingProgress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
                OnSceneLoadProgress.Raise(LoadingProgress);
                
                // 로딩이 90% 완료되고 최소 로딩 시간이 지났으면 씬 활성화
                if (asyncLoad.progress >= 0.9f)
                {
                    float elapsedTime = Time.realtimeSinceStartup - startTime;
                    if (elapsedTime >= minimumLoadingTime)
                    {
                        LoadingProgress = 1f;
                        OnSceneLoadProgress.Raise(LoadingProgress);
                        asyncLoad.allowSceneActivation = true;
                    }
                }
                
                yield return null;
            }
            
            // 페이드 인 시작
            if (enableFadeTransition)
            {
                yield return StartCoroutine(FadeIn());
            }
            
            // 로딩 완료
            IsLoading = false;
            OnSceneLoadCompleted.Raise(sceneName);
            AFKS.Shared.Events.EventBus.GlobalInteraction.Raise($"scene.load.done:{sceneName}");
            
            // 씬 로딩 완료 후 static 상호작용 카운터 정리로 누수 방지
            AFKS.StageSystem.StageInteractionController.ClearAllStaticData();
            AFKS.StageSystem.PixelInteractionController.ClearAllStaticData();

            Debug.Log($"[SceneController] Scene loaded successfully: {sceneName}");
        }
        
        /// <summary>
        /// 페이드 아웃 효과
        /// </summary>
        private IEnumerator FadeOut()
        {
            // 페이드 아웃 구현 (UI 매니저와 연동)
            // 여기서는 간단한 대기 시간으로 대체
            yield return new WaitForSeconds(fadeTransitionDuration);
        }
        
        /// <summary>
        /// 페이드 인 효과
        /// </summary>
        private IEnumerator FadeIn()
        {
            // 페이드 인 구현 (UI 매니저와 연동)
            // 여기서는 간단한 대기 시간으로 대체
            yield return new WaitForSeconds(fadeTransitionDuration);
        }
        
        // === UTILITIES ===
        
        /// <summary>
        /// 현재 씬 이름 반환
        /// </summary>
        public string GetCurrentSceneName()
        {
            return SceneManager.GetActiveScene().name;
        }
        
        /// <summary>
        /// 씬이 존재하는지 확인
        /// </summary>
        public bool DoesSceneExist(string sceneName)
        {
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                string sceneNameFromPath = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                
                if (sceneNameFromPath == sceneName)
                    return true;
            }
            
            return false;
        }
        
        // === EVENT HANDLERS ===
        
        /// <summary>
        /// 씬 로드 완료 시 호출
        /// </summary>
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log($"[SceneController] Scene loaded event: {scene.name}");
        }
        
        /// <summary>
        /// 씬 언로드 시 호출
        /// </summary>
        private void OnSceneUnloaded(Scene scene)
        {
            Debug.Log($"[SceneController] Scene unloaded: {scene.name}");
        }
        
        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }
        
        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }
    }
}