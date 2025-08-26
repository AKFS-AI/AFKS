using System.Collections.Generic;

namespace AFKS.Core.Services
{
	public sealed class ProgressService
	{
		private readonly EventBus _bus;
		private readonly HashSet<string> _flags = new HashSet<string>();
		private readonly Dictionary<string, int> _ints = new Dictionary<string, int>();
		public int CurrentStageIndex { get; private set; }

		public ProgressService(EventBus bus)
		{
			_bus = bus;
		}

		public bool GetFlag(string flag) => _flags.Contains(flag);
		public void SetFlag(string flag, bool value = true)
		{
			if (value)
			{
				if (_flags.Add(flag))
					_bus.Publish(new Events.FlagChangedEvent(flag, true));
			}
			else
			{
				if (_flags.Remove(flag))
					_bus.Publish(new Events.FlagChangedEvent(flag, false));
			}
		}

		public void SetStage(int index)
		{
			if (index == CurrentStageIndex) return;
			CurrentStageIndex = index;
			_bus.Publish(new Events.StageChangedEvent(index));
		}

		public int AddAndGet(string key, int delta)
		{
			if (!_ints.TryGetValue(key, out var v)) v = 0;
			v += delta;
			_ints[key] = v;
			return v;
		}

		public int GetInt(string key)
		{
			return _ints.TryGetValue(key, out var v) ? v : 0;
		}
	}
}


