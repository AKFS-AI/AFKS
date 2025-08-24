using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using AFKS.Core.Services.Input;
using AFKS.Core.UI;
using AFKS.Core.Events;
using AFKS.Core.Services;
using AFKS.Core.Utils;
using AFKS.Features.Stage;

namespace AFKS.Core.Services.Scene
{
    /// <summary>
    /// 페이드와 입력 잠금을 동반한 스테이지(Additive) 로드/언로드를 수행합니다.
    /// Core 씬에 배치된 FadeCanvas 참조가 필요합니다.
    /// </summary>
    [AddComponentMenu("AFKS/Scene/Scene Service")]
    public sealed class SceneService : MonoBehaviour, ISceneService
    {
        #region 필드
        [SerializeField]
        [InspectorName("페이드 캔버스")]
        [Tooltip("전환 시 화면을 어둡게/밝게 페이드할 캔버스입니다.")]
        private FadeCanvas fadeCanvas;

        [SerializeField]
        [InspectorName("기본 페이드 시간(초)")]
        [Tooltip("전환 시 기본으로 사용할 페이드 시간(초)입니다.")]
        private float defaultFadeSeconds = 0.35f;

        private IInputService inputService;
        private bool lastLoadSucceeded;
        private bool isTransitioning;
        private string currentStageId;
        #endregion

        #region 유니티 수명주기
        private void Awake()
        {
            ServiceLocator.Register<ISceneService>(this, overwriteExisting: true);
            inputService = ServiceLocator.TryGet<IInputService>(out var svc) ? svc : null;
        }

        private void OnEnable()
        {
            GameEvents.StageChangeRequested += OnStageChangeRequested;
        }

        private void OnDisable()
        {
            GameEvents.StageChangeRequested -= OnStageChangeRequested;
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<ISceneService>();
        }
        #endregion

        #region 이벤트 핸들러
        private void OnStageChangeRequested(string targetStageId)
        {
            if (isTransitioning)
            {
                Log.Warn($"[SceneService] 전환 진행 중 중복 요청 무시: '{targetStageId}'");
                return;
            }
            StartCoroutine(TransitionToStageCoroutine(targetStageId));
        }
        #endregion

        #region 코루틴
        private IEnumerator TransitionToStageCoroutine(string targetStageId)
        {
            isTransitioning = true;
            try
            {
                if (inputService != null) inputService.Lock(true);
                yield return FadeOutAsync(defaultFadeSeconds);

                string prevStage = string.IsNullOrEmpty(currentStageId) ? GetActiveStageName() : currentStageId;
                if (!string.IsNullOrEmpty(targetStageId))
                {
                    yield return LoadStageAdditiveAsync(targetStageId, activateOnLoad: true);
                    if (!lastLoadSucceeded)
                    {
                        Log.Error($"[SceneService] 스테이지 로드 실패: '{targetStageId}'. Build Settings 또는 Assets/Scenes 등록을 확인하세요.");
                        // 실패 시 기존 스테이지 유지, 즉시 페이드 인 후 해제
                        yield return FadeInAsync(defaultFadeSeconds);
                        if (inputService != null) inputService.Lock(false);
                        yield break;
                    }
                    currentStageId = targetStageId;
                }

                if (!string.IsNullOrEmpty(prevStage))
                {
                    yield return UnloadStageAsync(prevStage);
                }

                yield return FadeInAsync(defaultFadeSeconds);
                if (inputService != null) inputService.Lock(false);
            }
            finally
            {
                isTransitioning = false;
            }
        }

        private static string GetActiveStageName()
        {
            // 가정: Core가 먼저 로드되어 있으며, 활성화된 가장 마지막 Additive 씬을 스테이지로 간주합니다.
            for (int i = SceneManager.sceneCount - 1; i >= 0; i--)
            {
                var sc = SceneManager.GetSceneAt(i);
                if (sc.isLoaded && sc.name != UnityEngine.SceneManagement.SceneManager.GetActiveScene().name)
                {
                    // 휴리스틱: 현재 활성 씬 외에 로드된 씬을 스테이지로 취급
                    return sc.name;
                }
            }
            return null;
        }

        public IEnumerator FadeOutAsync(float seconds)
        {
            if (fadeCanvas != null)
            {
                yield return fadeCanvas.FadeOut(seconds);
            }
        }

        public IEnumerator FadeInAsync(float seconds)
        {
            if (fadeCanvas != null)
            {
                yield return fadeCanvas.FadeIn(seconds);
            }
        }

        public IEnumerator LoadStageAdditiveAsync(string stageId, bool activateOnLoad = false)
        {
            lastLoadSucceeded = false;

            // 사전 검증: 빌드 세팅/에셋번들에 로드 가능 여부 확인
            if (!Application.CanStreamedLevelBeLoaded(stageId))
            {
                Log.Error($"[SceneService] 씬 '{stageId}' 을(를) 로드할 수 없습니다. File > Build Settings에 씬을 추가했는지 확인하세요.");
                yield break;
            }

            var op = SceneManager.LoadSceneAsync(stageId, LoadSceneMode.Additive);
            if (op == null)
            {
                Log.Error($"[SceneService] LoadSceneAsync가 null을 반환했습니다: '{stageId}'");
                yield break;
            }
            while (!op.isDone) yield return null;

            if (activateOnLoad)
            {
                yield return ActivateLoadedStageAsync(stageId);
            }

            lastLoadSucceeded = true;
            GameEvents.RaiseStageLoaded(stageId);
        }

        public IEnumerator ActivateLoadedStageAsync(string stageId)
        {
            var scene = SceneManager.GetSceneByName(stageId);
            if (scene.IsValid())
            {
                SceneManager.SetActiveScene(scene);
                DisableDuplicatedGlobalComponents(scene);
                InvokeStageInitialize(scene);
            }
            yield break;
        }

        public IEnumerator UnloadStageAsync(string stageId)
        {
            var scene = SceneManager.GetSceneByName(stageId);
            if (scene.IsValid())
            {
                InvokeStageTeardown(scene);
            }
            var op = SceneManager.UnloadSceneAsync(stageId);
            if (op != null)
            {
                while (!op.isDone) yield return null;
                GameEvents.RaiseStageUnloaded(stageId);
            }
        }
        #endregion

        #region 내부 메서드
        private static void InvokeStageInitialize(UnityEngine.SceneManagement.Scene scene)
        {
            var roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                var root = roots[i].GetComponent<StageRoot>();
                if (root != null)
                {
                    root.Initialize();
                    break;
                }
            }
        }

        private static void InvokeStageTeardown(UnityEngine.SceneManagement.Scene scene)
        {
            var roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                var root = roots[i].GetComponent<StageRoot>();
                if (root != null)
                {
                    root.Teardown();
                    break;
                }
            }
        }
        #endregion

        #region 유틸리티
        /// <summary>
        /// Additive 로드 시 중복 생길 수 있는 전역 컴포넌트(EventSystem/AudioListener)를 정리합니다.
        /// Core 상주 인스턴스를 유지하고, 방금 로드된 스테이지 씬의 중복 인스턴스는 비활성화합니다.
        /// </summary>
        private static void DisableDuplicatedGlobalComponents(UnityEngine.SceneManagement.Scene loadedScene)
        {
            // EventSystem: 하나만 활성화되도록, 방금 로드된 씬의 것은 비활성화
            var eventSystems = Object.FindObjectsByType<UnityEngine.EventSystems.EventSystem>(FindObjectsSortMode.None);
            if (eventSystems != null && eventSystems.Length > 1)
            {
                for (int i = 0; i < eventSystems.Length; i++)
                {
                    var es = eventSystems[i];
                    if (es != null && es.gameObject.scene == loadedScene)
                    {
                        es.enabled = false;
                    }
                }
            }

            // AudioListener: 하나만 활성화되도록, 방금 로드된 씬의 것은 비활성화
            var listeners = Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
            if (listeners != null && listeners.Length > 1)
            {
                for (int i = 0; i < listeners.Length; i++)
                {
                    var al = listeners[i];
                    if (al != null && al.gameObject.scene == loadedScene)
                    {
                        al.enabled = false;
                    }
                }
            }
        }
        #endregion
    }
}


