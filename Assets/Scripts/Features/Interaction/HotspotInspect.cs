using UnityEngine;
using AFKS.Core.Services;
using AFKS.Core.Services.Input;
using AFKS.Core.UI;
using UnityEngine.UI;

namespace AFKS.Features.Interaction
{
    /// <summary>
    /// 자신을 클릭하면 클로즈업 뷰어에 지정된 스프라이트를 표시합니다.
    /// </summary>
    [AddComponentMenu("AFKS/Interaction/Hotspot Inspect")]
    public sealed class HotspotInspect : MonoBehaviour
    {
        #region 필드
        [SerializeField]
        [InspectorName("클로즈업 스프라이트")]
        [Tooltip("클로즈업으로 표시할 스프라이트 이미지입니다.")]
        private Sprite closeupSprite;

        [Header("UI 프리팹 소환(줌 연출)")]
        [SerializeField]
        [InspectorName("UI 프리팹 소환 사용")]
        [Tooltip("이미지 뷰어 대신 UI 프리팹을 소환하고 확대 애니메이션을 재생합니다.")]
        private bool spawnUIPrefab = true;

        [SerializeField]
        [InspectorName("소환할 패널 프리팹")]
        [Tooltip("MainCanvas 하위에 소환할 CloseupPanel 프리팹(Chain/문 버튼 포함)")]
        private GameObject closeupPanelPrefab;

        [SerializeField]
        [InspectorName("대상 Canvas")]
        [Tooltip("없으면 첫 번째 Overlay Canvas를 자동 탐색합니다.")]
        private Canvas targetCanvas;

        [SerializeField]
        [InspectorName("줌 연출(초)")]
        private float zoomSeconds = 0.25f;

        [SerializeField]
        [InspectorName("시작 스케일")] private Vector2 startScale01 = new Vector2(0.3f, 0.3f);

        [SerializeField]
        [InspectorName("Easing 曲선")] private AnimationCurve zoomCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [SerializeField]
        [InspectorName("제목(옵션)")]
        [Tooltip("텍스트가 필요할 때 표시할 제목(옵션)")]
        private string title;

        private IInputService inputService;
        private CloseupViewer closeupViewer;
        #endregion

        #region 유니티 수명주기
        private void Awake()
        {
            ServiceLocator.TryGet<IInputService>(out inputService);
            // CloseupViewer는 씬 내에서 1개 존재한다고 가정하고 찾아서 캐시(신규 API)
            closeupViewer = UnityEngine.Object.FindFirstObjectByType<CloseupViewer>(FindObjectsInactive.Include);
        }

        private void OnEnable()
        {
            if (inputService != null)
            {
                inputService.ObjectClicked += OnObjectClicked;
            }
        }

        private void OnDisable()
        {
            if (inputService != null)
            {
                inputService.ObjectClicked -= OnObjectClicked;
            }
        }
        #endregion

        #region 이벤트 핸들러
        private void OnObjectClicked(GameObject clicked)
        {
            if (clicked != gameObject)
            {
                return;
            }
            Debug.Log("[HotspotInspect] Clicked = self; trying to open closeup.");

            if (spawnUIPrefab && closeupPanelPrefab != null)
            {
                TrySpawnUIPrefabWithZoom();
                return;
            }

            // 이미지 뷰어 경로(레거시)
            if (closeupViewer == null)
            {
                closeupViewer = UnityEngine.Object.FindFirstObjectByType<CloseupViewer>(FindObjectsInactive.Include);
            }
            if (closeupViewer != null)
            {
                if (closeupSprite != null)
                {
                    closeupViewer.Show(closeupSprite, string.IsNullOrEmpty(title) ? null : title);
                }
                else
                {
                    closeupViewer.Show();
                }
                return;
            }

            // 코어(UI/CloseupViewer) 부재 시 간단한 폴백 팝업을 사용
            FallbackPopup(closeupSprite, title);
        }
        #endregion

        #region 폴백 UI
        private static void FallbackPopup(Sprite sprite, string title)
        {
            var go = new GameObject("_FallbackCloseupCanvas");
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = short.MaxValue;
            var cg = go.AddComponent<CanvasGroup>();
            cg.alpha = 1f; cg.blocksRaycasts = true; cg.interactable = true;

            var imgGO = new GameObject("Image");
            imgGO.transform.SetParent(go.transform, false);
            var img = imgGO.AddComponent<UnityEngine.UI.Image>();
            img.sprite = sprite;
            var rt = img.rectTransform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

            go.AddComponent<FallbackCloser>();
        }
        #endregion

        #region 마우스 폴백(스테이지 단독 실행 지원)
        private void OnMouseDown()
        {
            // InputService가 존재하면 폴백 경로는 사용하지 않습니다(중복 트리거 방지)
            if (inputService != null) return;
            // InputService가 없거나 레이어 마스크로 누락되어도 콜라이더만 있으면 동작하도록 폴백
            if (!enabled || !gameObject.activeInHierarchy) return;
            if (spawnUIPrefab && closeupPanelPrefab != null)
            {
                TrySpawnUIPrefabWithZoom();
                return;
            }

            if (closeupViewer == null)
                closeupViewer = UnityEngine.Object.FindFirstObjectByType<CloseupViewer>(FindObjectsInactive.Include);
            if (closeupViewer != null)
            {
                if (closeupSprite != null) closeupViewer.Show(closeupSprite, string.IsNullOrEmpty(title) ? null : title);
                else closeupViewer.Show();
            }
            else FallbackPopup(closeupSprite, title);
        }
        #endregion

        #region UI Prefab Spawn + Zoom
        private void TrySpawnUIPrefabWithZoom()
        {
            if (targetCanvas == null)
            {
                var canvases = UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                foreach (var c in canvases)
                {
                    if (c.isActiveAndEnabled && c.renderMode == RenderMode.ScreenSpaceOverlay)
                    {
                        targetCanvas = c; break;
                    }
                }
                if (targetCanvas == null && canvases.Length > 0) targetCanvas = canvases[0];
            }
            if (targetCanvas == null)
            {
                Debug.LogWarning("[HotspotInspect] No Canvas found to spawn UI prefab.");
                return;
            }

            // 이미 열려 있으면 다시 열지 않기(동일 캔버스 내 한 개만)
            if (targetCanvas.transform.GetComponentInChildren<ChainCloseupController>(true) != null)
            {
                return;
            }

            GameObject panel = null;
            RectTransform rt = null;
            CanvasGroup cg = null;

            // 프리팹이 없으면 같은 캔버스 내 기존 CloseupPanel을 찾아 활성화 후 재사용
            if (closeupPanelPrefab == null)
            {
                var existingController = targetCanvas.transform.GetComponentInChildren<ChainCloseupController>(true);
                if (existingController != null)
                {
                    panel = existingController.gameObject;
                    if (!panel.activeSelf) panel.SetActive(true);
                    rt = panel.GetComponent<RectTransform>();
                    cg = panel.GetComponent<CanvasGroup>();
                }
                else
                {
                    Debug.LogWarning("[HotspotInspect] No prefab set and no existing CloseupPanel found under Canvas.");
                    return;
                }
            }
            else
            {
                panel = Instantiate(closeupPanelPrefab, targetCanvas.transform);
                panel.transform.SetAsLastSibling();
                rt = panel.GetComponent<RectTransform>();
                if (rt == null) rt = panel.AddComponent<RectTransform>();
            }

            // 시작 위치: 클릭된 오브젝트의 화면 좌표 → 캔버스 로컬 좌표
            var cam = Camera.main;
            Vector3 screen = cam != null ? cam.WorldToScreenPoint(transform.position) : new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
            RectTransform canvasRt = targetCanvas.transform as RectTransform;
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRt, screen, targetCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : cam, out localPoint);

            // 초기 상태 적용
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = localPoint;
            rt.localScale = new Vector3(startScale01.x, startScale01.y, 1f);

            // 페이드/입력 제어
            if (cg == null)
            {
                cg = panel.GetComponent<CanvasGroup>();
                if (cg == null) cg = panel.AddComponent<CanvasGroup>();
            }
            cg.alpha = 0f; cg.blocksRaycasts = true; cg.interactable = false;

            // 애니메이션 중에는 전역 입력 잠금(중복 생성 방지)
            if (inputService != null) inputService.Lock(true);

            StartCoroutine(AnimateZoom(rt, cg));
        }

        private System.Collections.IEnumerator AnimateZoom(RectTransform rt, CanvasGroup cg)
        {
            float t = 0f;
            Vector2 startPos = rt.anchoredPosition;
            Vector3 startScale = rt.localScale;
            while (t < zoomSeconds)
            {
                t += Time.unscaledDeltaTime;
                float p = zoomSeconds > 0f ? Mathf.Clamp01(t / zoomSeconds) : 1f;
                float e = zoomCurve != null ? zoomCurve.Evaluate(p) : p;
                rt.anchoredPosition = Vector2.LerpUnclamped(startPos, Vector2.zero, e);
                rt.localScale = Vector3.LerpUnclamped(startScale, Vector3.one, e);
                cg.alpha = Mathf.LerpUnclamped(0f, 1f, e);
                yield return null;
            }
            rt.anchoredPosition = Vector2.zero; rt.localScale = Vector3.one;
            // 줌 종료 후 전체 화면으로 스트레치하여 UI가 월드 클릭을 가로채도록 함
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            cg.alpha = 1f; cg.blocksRaycasts = true; cg.interactable = true;

            // 입력 잠금 해제
            if (inputService != null) inputService.Lock(false);
        }
        #endregion
    }

    internal sealed class FallbackCloser : MonoBehaviour
    {
        private void Update()
        {
            if (UnityEngine.Input.GetMouseButtonDown(0) || UnityEngine.Input.GetKeyDown(KeyCode.Escape))
            {
                Destroy(gameObject);
            }
        }
    }
}



