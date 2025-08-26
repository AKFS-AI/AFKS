using System;
using System.Collections.Generic;

namespace AFKS.Core.Services
{
	/// <summary>
	/// Type-safe lightweight event bus.
	/// </summary>
	public sealed class EventBus
	{
		private readonly Dictionary<Type, List<Delegate>> _subscribers = new Dictionary<Type, List<Delegate>>();

		public void Publish<T>(T evt)
		{
			var type = typeof(T);
			if (_subscribers.TryGetValue(type, out var list))
			{
				// copy to avoid mutation during invoke
				var snapshot = list.ToArray();
				for (int i = 0; i < snapshot.Length; i++)
				{
					var d = snapshot[i] as Action<T>;
					d?.Invoke(evt);
				}
			}
		}

		public IDisposable Subscribe<T>(Action<T> handler)
		{
			var type = typeof(T);
			if (!_subscribers.TryGetValue(type, out var list))
			{
				list = new List<Delegate>();
				_subscribers[type] = list;
			}
			list.Add(handler);
			return new Subscription(() => Unsubscribe(handler));
		}

		private void Unsubscribe<T>(Action<T> handler)
		{
			var type = typeof(T);
			if (_subscribers.TryGetValue(type, out var list))
			{
				list.Remove(handler);
				if (list.Count == 0)
				{
					_subscribers.Remove(type);
				}
			}
		}

		private sealed class Subscription : IDisposable
		{
			private readonly Action _dispose;
			private bool _isDisposed;
			public Subscription(Action dispose) { _dispose = dispose; }
			public void Dispose()
			{
				if (_isDisposed) return;
				_isDisposed = true;
				_dispose?.Invoke();
			}
		}
	}
}


