using System.Collections.Generic;

namespace AFKS.Core.Services
{
	public readonly struct ItemId
	{
		public readonly string Value;
		public ItemId(string value) { Value = value; }
		public override string ToString() => Value;
	}

	public sealed class InventoryService
	{
		private readonly EventBus _bus;
		private readonly HashSet<string> _items = new HashSet<string>();

		public InventoryService(EventBus bus)
		{
			_bus = bus;
		}

		public bool TryAdd(ItemId id)
		{
			if (string.IsNullOrEmpty(id.Value)) return false;
			if (_items.Add(id.Value))
			{
				_bus.Publish(new Events.ItemPickedEvent(id.Value));
				return true;
			}
			return false;
		}

		public bool Has(ItemId id) => _items.Contains(id.Value);

		public bool TryUseOn(ItemId item, string targetId)
		{
			if (!_items.Contains(item.Value)) return false;
			_bus.Publish(new Events.ItemUsedOnEvent(item.Value, targetId));
			return true;
		}

		public IEnumerable<string> Enumerate() => _items;
	}
}


