using UnityEngine;
using AFKS.Core.Services;
using AFKS.Core.Services.Input;
using AFKS.Core.Services.Inventory;
using AFKS.Features.Items;
using AFKS.Core.Events;

namespace AFKS.Features.Interaction
{
	/// <summary>
	/// 클릭 시 아이템을 인벤토리에 추가하고, 필요 시 클로즈업을 띄웁니다.
	/// </summary>
	[AddComponentMenu("AFKS/Interaction/Pickup Item")]
	public sealed class PickupItem : MonoBehaviour
	{
		#region 필드
		[SerializeField] private ItemData itemData;
		[SerializeField] private bool showCloseup = true;
		private IInputService inputService;
		private IInventoryService inventory;
		#endregion

		#region 유니티 수명주기
		private void Awake()
		{
			ServiceLocator.TryGet<IInputService>(out inputService);
			ServiceLocator.TryGet<IInventoryService>(out inventory);
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
			if (clicked != gameObject || itemData == null) return;
			if (inventory != null)
			{
				inventory.Add(itemData.ItemId);
			}
			GameEvents.RaiseItemPicked(itemData.ItemId);
			if (showCloseup)
			{
				// 필요 시 CloseupViewer를 찾아 표시 (씬에 하나 있다고 가정)
				var viewer = Object.FindFirstObjectByType<AFKS.Core.UI.CloseupViewer>();
				if (viewer != null) viewer.Show(itemData);
			}
			// 획득 후 씬에서 제거
			Destroy(gameObject);
		}
		#endregion
	}
}


