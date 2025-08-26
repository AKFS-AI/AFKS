using System.Collections.Generic;
using UnityEngine;
using AFKS.Core.Composition;

namespace AFKS.Core.Mono
{
	[DisallowMultipleComponent]
	public sealed class InventoryServiceComponent : MonoBehaviour
	{
		public Core.Services.InventoryService Service { get; private set; }

		[SerializeField] private string[] _debugItems;

		private void Awake()
		{
			if (GameRuntime.Instance == null)
			{
				var root = new GameObject("GameRuntime");
				root.AddComponent<GameRuntime>();
			}
			Service = GameRuntime.Instance.Inventory;
		}

		// Debug helper to reflect current items in inspector (read-only)
		private void OnValidate()
		{
			#if UNITY_EDITOR
			if (Application.isPlaying || GameRuntime.Instance == null) return;
			#endif
		}

		public void RefreshDebug()
		{
			var list = new List<string>();
			if (GameRuntime.Instance != null)
			{
				foreach (var it in GameRuntime.Instance.Inventory.Enumerate()) list.Add(it);
			}
			_debugItems = list.ToArray();
		}
	}
}


