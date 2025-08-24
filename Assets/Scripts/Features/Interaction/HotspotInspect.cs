using UnityEngine;
using AFKS.Core.Services;
using AFKS.Core.Services.Input;
using AFKS.Core.UI;

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


