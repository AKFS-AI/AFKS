using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using AFKS.Core.Services.Input;
using AFKS.Core.UI;
using AFKS.Core.Events;

namespace AFKS.Core.Services.Scene
{
    /// <summary>
    /// 페이드와 입력 잠금을 동반한 스테이지(Additive) 로드/언로드를 수행합니다.
    /// Core 씬에 배치된 FadeCanvas 참조가 필요합니다.
    /// </summary>
    [AddComponentMenu("AFKS/씬/씬 전환 서비스")]
    public sealed class SceneService : MonoBehaviour, ISceneService
    {
        [SerializeField]
        [InspectorName("페이드 캔버스")]
        [Tooltip("전환 시 화면을 어둡게/밝게 페이드할 캔버스입니다.")]
        private FadeCanvas fadeCanvas;

        [SerializeField]
        [InspectorName("기본 페이드 시간(초)")]
        [Tooltip("전환 시 기본으로 사용할 페이드 시간(초)입니다.")]
        private float defaultFadeSeconds = 0.35f;

        private IInputService inputService;

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

        private void OnStageChangeRequested(string targetStageId)
        {
            StartCoroutine(TransitionToStageCoroutine(targetStageId));
        }

        private IEnumerator TransitionToStageCoroutine(string targetStageId)
        {
            if (inputService != null) inputService.Lock(true);
            yield return FadeOutAsync(defaultFadeSeconds);

            string currentStage = GetActiveStageName();
            if (!string.IsNullOrEmpty(targetStageId))
            {
                yield return LoadStageAdditiveAsync(targetStageId, activateOnLoad: true);
            }

            if (!string.IsNullOrEmpty(currentStage))
            {
                yield return UnloadStageAsync(currentStage);
            }

            yield return FadeInAsync(defaultFadeSeconds);
            if (inputService != null) inputService.Lock(false);
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
            var op = SceneManager.LoadSceneAsync(stageId, LoadSceneMode.Additive);
            while (!op.isDone) yield return null;

            if (activateOnLoad)
            {
                yield return ActivateLoadedStageAsync(stageId);
            }

            GameEvents.RaiseStageLoaded(stageId);
        }

        public IEnumerator ActivateLoadedStageAsync(string stageId)
        {
            var scene = SceneManager.GetSceneByName(stageId);
            if (scene.IsValid())
            {
                SceneManager.SetActiveScene(scene);
            }
            yield break;
        }

        public IEnumerator UnloadStageAsync(string stageId)
        {
            var op = SceneManager.UnloadSceneAsync(stageId);
            if (op != null)
            {
                while (!op.isDone) yield return null;
                GameEvents.RaiseStageUnloaded(stageId);
            }
        }
    }
}


