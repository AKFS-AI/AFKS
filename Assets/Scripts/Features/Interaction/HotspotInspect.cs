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
            // CloseupViewer는 씬 내에서 1개 존재한다고 가정하고 찾아서 캐시
            closeupViewer = Object.FindObjectOfType<CloseupViewer>(includeInactive: true);
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
            if (clicked != gameObject) return;
            if (closeupViewer == null) return;

            if (closeupSprite != null)
            {
                closeupViewer.Show(closeupSprite, string.IsNullOrEmpty(title) ? null : title);
            }
            else
            {
                closeupViewer.Show();
            }
        }
        #endregion
    }
}


