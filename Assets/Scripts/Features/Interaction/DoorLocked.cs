using UnityEngine;
using AFKS.Core.Services;
using AFKS.Core.Services.Input;
using AFKS.Core.Services.Inventory;
using AFKS.Core.Utils;

namespace AFKS.Features.Interaction
{
	/// <summary>
	/// 특정 키 아이템을 보유해야 열리는 문. 열리면 설정된 스테이지로 이동 트리거와 함께 사용할 수 있음.
	/// </summary>
	[AddComponentMenu("AFKS/Interaction/Door (Locked)")]
	public sealed class DoorLocked : MonoBehaviour
	{
		#region 필드
		[SerializeField] private string requiredItemId = "Key_Bundle";
		[SerializeField] private GameObject lockedHint;
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
			if (clicked != gameObject) return;
			bool hasKey = inventory != null && inventory.Has(requiredItemId);
			if (!hasKey)
			{
				if (lockedHint != null) lockedHint.SetActive(true);
				Log.Info($"문이 잠겨있습니다. 필요 아이템: {requiredItemId}");
				return;
			}
			// 열림: 별도의 이동 트리거나 애니메이션과 연동
			if (lockedHint != null) lockedHint.SetActive(false);
		}
		#endregion
	}
}


