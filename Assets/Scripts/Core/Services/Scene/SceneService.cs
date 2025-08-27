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
        [SerializeField]
        [Tooltip("전환 흐름과 로드/언로드 단계를 상세 로그로 출력합니다.")]
        private bool debugLog = false;
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
            if (debugLog) Log.Info($"[SceneService] StageChangeRequested -> {targetStageId}");
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
                if (debugLog) Log.Info("[SceneService] FadeOut start");
                yield return FadeOutAsync(defaultFadeSeconds);

                string prevStage = string.IsNullOrEmpty(currentStageId) ? GetActiveStageName() : currentStageId;
                if (!string.IsNullOrEmpty(targetStageId))
                {
                    // 씬 전환 직전, 이전 씬의 배경/환경 사운드를 정리해 누수 방지
                    if (AFKS.Core.Services.ServiceLocator.TryGet<AFKS.Core.Services.Audio.IAudioService>(out var audioSvc))
                    {
                        audioSvc.StopBGM(0.25f);
                    }
                    if (debugLog) Log.Info($"[SceneService] LoadAdditive -> {targetStageId}");
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
                    if (debugLog) Log.Info($"[SceneService] Unload -> {prevStage}");
                    yield return UnloadStageAsync(prevStage);
                }

                if (debugLog) Log.Info("[SceneService] FadeIn start");
                yield return FadeInAsync(defaultFadeSeconds);
                if (inputService != null) inputService.Lock(false);

                // 전환 완료 시점에 세이브(진행 저장). 메뉴로 이동/메뉴 도착 시에는 스킵
                if (!string.Equals(currentStageId, "Menu", System.StringComparison.Ordinal))
                {
                    if (AFKS.Core.Services.ServiceLocator.TryGet<AFKS.Core.Services.Save.ISaveService>(out var save))
                    {
                        try { save.SaveAll(); }
                        catch (System.Exception e)
                        {
                            Log.Warn($"[SceneService] 전환 완료 후 저장 실패: {e.Message}");
                        }
                    }
                }
            }
            finally
            {
                isTransitioning = false;
                if (debugLog) Log.Info("[SceneService] Transition completed");
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
            if (debugLog) Log.Info("[SceneService] Loading in progress...");
            while (!op.isDone) yield return null;

            if (activateOnLoad)
            {
                if (debugLog) Log.Info("[SceneService] ActivateLoadedStageAsync");
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
                // 진행 상태 업데이트(스테이지 ID 보고)
                if (AFKS.Core.Services.ServiceLocator.TryGet<AFKS.Core.Services.GameState.IGameStateService>(out var gs))
                {
                    gs.SetCurrentStage(stageId);
                }
                DisableDuplicatedGlobalComponents(scene);
                InvokeStageInitialize(scene);
                // 만약 새 씬에 AutoBGMPlayer가 없다면 잔여 BGM을 강제 정지해 메뉴 BGM이 이어지지 않도록 한다
                if (!SceneContainsComponent<AFKS.Core.Audio.AutoBGMPlayer>(scene))
                {
                    if (AFKS.Core.Services.ServiceLocator.TryGet<AFKS.Core.Services.Audio.IAudioService>(out var audioSvc))
                    {
                        audioSvc.StopBGM(0.0f);
                    }
                }
                if (debugLog) Log.Info("[SceneService] Stage activated and initialized");
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
                if (debugLog) Log.Info("[SceneService] Unloading in progress...");
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
            // EventSystem: 전역에서 정확히 1개만 활성 유지
            // Unity가 중복 시 자동 비활성화할 수 있으므로, 여기서 명시적으로 하나를 선택/활성화하고 나머지를 비활성화합니다.
            var eventSystems = Object.FindObjectsByType<UnityEngine.EventSystems.EventSystem>(FindObjectsSortMode.None);
            if (eventSystems != null && eventSystems.Length > 0)
            {
                UnityEngine.EventSystems.EventSystem keep = null;
                // 우선순위 변경: 방금 활성화한 스테이지(loadedScene)의 EventSystem을 유지하여 해당 씬 UI가 이벤트를 확실히 받도록 함
                for (int i = 0; i < eventSystems.Length; i++)
                {
                    var es = eventSystems[i];
                    if (es == null) continue;
                    if (es.gameObject.scene == loadedScene)
                    {
                        keep = es;
                        break;
                    }
                }
                // 폴백: 로드된 씬에 없으면 첫 번째를 유지
                if (keep == null) keep = eventSystems[0];

                // 선택된 것은 활성화(게임오브젝트도 활성), 나머지는 비활성화
                for (int i = 0; i < eventSystems.Length; i++)
                {
                    var es = eventSystems[i];
                    if (es == null) continue;
                    bool isKeep = es == keep;
                    es.enabled = isKeep;
                    if (isKeep && !es.gameObject.activeSelf)
                    {
                        es.gameObject.SetActive(true);
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

        private static bool SceneContainsComponent<T>(UnityEngine.SceneManagement.Scene scene) where T : Component
        {
            var roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                var comps = roots[i].GetComponentsInChildren<T>(true);
                if (comps != null && comps.Length > 0) return true;
            }
            return false;
        }
        #endregion
    }
}


