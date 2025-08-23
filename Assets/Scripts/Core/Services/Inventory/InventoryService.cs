using System.Collections.Generic;
using AFKS.Core.Services;
using UnityEngine;

namespace AFKS.Core.Services.Inventory
{
	/// <summary>
	/// 문자열 ID 기반의 경량 인벤토리 구현. 장면 간 유지 위해 ServiceLocator 등록.
	/// </summary>
	[AddComponentMenu("AFKS/Inventory/Inventory Service")]
	public sealed class InventoryService : MonoBehaviour, IInventoryService
	{
		#region 필드
		[SerializeField]
		private List<string> initialItems = new List<string>();
		private readonly HashSet<string> items = new HashSet<string>();
		#endregion

		#region 유니티 수명주기
		private void Awake()
		{
			for (int i = 0; i < initialItems.Count; i++)
			{
				if (!string.IsNullOrEmpty(initialItems[i])) items.Add(initialItems[i]);
			}
			ServiceLocator.Register<IInventoryService>(this, overwriteExisting: true);
		}

		private void OnDestroy()
		{
			ServiceLocator.Unregister<IInventoryService>();
		}
		#endregion

		#region 공개 API
		public bool Has(string itemId) => items.Contains(itemId);
		public void Add(string itemId) { if (!string.IsNullOrEmpty(itemId)) items.Add(itemId); }
		public bool Remove(string itemId) => items.Remove(itemId);
		public IReadOnlyCollection<string> Items => items;
		#endregion
	}
}


