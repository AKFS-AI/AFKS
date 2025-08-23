using System.Collections.Generic;

namespace AFKS.Core.Services.Inventory
{
	/// <summary>
	/// 간단한 아이템 인벤토리 인터페이스. 키/문서/소품 등을 보관.
	/// </summary>
	public interface IInventoryService
	{
		bool Has(string itemId);
		void Add(string itemId);
		bool Remove(string itemId);
		IReadOnlyCollection<string> Items { get; }
	}
}


