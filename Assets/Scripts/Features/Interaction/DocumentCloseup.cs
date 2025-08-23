using UnityEngine;
using AFKS.Core.Services;
using AFKS.Core.Services.Input;
using AFKS.Core.UI;
using AFKS.Features.Items;

namespace AFKS.Features.Interaction
{
	/// <summary>
	/// 클릭 시 문서/사진을 클로즈업으로 표시.
	/// </summary>
	[AddComponentMenu("AFKS/Interaction/Document Closeup")]
	public sealed class DocumentCloseup : MonoBehaviour
	{
		#region 필드
		[SerializeField] private ItemData documentItem;
		private IInputService inputService;
		#endregion

		#region 유니티 수명주기
		private void Awake()
		{
			ServiceLocator.TryGet<IInputService>(out inputService);
		}

		private void OnEnable()
		{
			if (inputService != null) inputService.ObjectClicked += OnObjectClicked;
		}

		private void OnDisable()
		{
			if (inputService != null) inputService.ObjectClicked -= OnObjectClicked;
		}
		#endregion

		#region 이벤트 핸들러
		private void OnObjectClicked(GameObject clicked)
		{
			if (clicked != gameObject) return;
			var viewer = Object.FindFirstObjectByType<CloseupViewer>();
			if (viewer != null)
			{
				if (documentItem != null) viewer.Show(documentItem);
			}
		}
		#endregion
	}
}


