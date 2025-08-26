using UnityEngine;
using AFKS.Core.Services;
using AFKS.Core.Events;
using AFKS.Core.Mono;

namespace AFKS.Core.Systems
{
	/// <summary>
	/// Screen-space hit test against StageHotspot rect definitions (no colliders).
	/// </summary>
	public sealed class InteractionSystem : MonoBehaviour
	{
		private EventBus _bus;
		private StageController _stage;
		private InventoryService _inventory;
		private ProgressService _progress;
		private ZoomView _zoom;

		public void Initialize(EventBus bus, StageController stage, InventoryService inventory, ProgressService progress, ZoomView zoom)
		{
			_bus = bus; _stage = stage; _inventory = inventory; _progress = progress; _zoom = zoom;
		}

		private void Update()
		{
			if (Input.GetMouseButtonDown(0))
			{
				var mouse = Input.mousePosition;
				TryInteract(mouse);
			}
		}

		private void TryInteract(Vector3 screenPos)
		{
			// Screen-space -> normalized viewport without relying on Camera.main
			var viewport = new Vector2(
				Mathf.Clamp01(screenPos.x / Mathf.Max(1f, Screen.width)),
				Mathf.Clamp01(screenPos.y / Mathf.Max(1f, Screen.height))
			);

			// 씬 베이크 핫스팟 우선 탐색, 없으면 StageHotspot(FBX) 사용
			var baked = GameObject.FindObjectsByType<HotspotRect>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
			for (int i = 0; i < baked.Length; i++)
			{
				var hs = baked[i];
				if (hs != null && hs.rect.Contains(viewport)) { HandleHotspot(BakeToDef(hs)); return; }
			}

			var hotspots = _stage.GetComponentsInChildren<StageHotspot>(true);
			for (int i = 0; i < hotspots.Length; i++)
			{
				var hs = hotspots[i];
				if (hs.Definition != null && hs.Definition.rect.Contains((Vector2)viewport))
				{
					HandleHotspot(hs.Definition);
					break;
				}
			}
		}

		private Data.HotspotDefinition BakeToDef(HotspotRect h)
		{
			return new Data.HotspotDefinition {
				id = h.id,
				rect = h.rect,
				requiredItemIds = h.requiredItemIds,
				setFlags = h.setFlags,
				playSfx = h.playSfx,
				showZoomId = h.showZoomId,
				goToStageIndex = h.goToStageIndex
			};
		}

		private void HandleHotspot(Data.HotspotDefinition def)
		{
			// check item requirement
			if (def.requiredItemIds != null && def.requiredItemIds.Length > 0)
			{
				bool ok = false;
				for (int i = 0; i < def.requiredItemIds.Length; i++)
				{
					if (_inventory.Has(new ItemId(def.requiredItemIds[i]))) { ok = true; break; }
				}
				if (!ok) { return; }
			}

			// If sequence is defined, route to sequence system via event
			if (def.sequence != null && def.sequence.steps != null && def.sequence.steps.Length > 0)
			{
				_bus.Publish(new AFKS.Core.Events.HotspotClickedEvent(def.id));
				return;
			}

			// Default simple behavior for legacy hotspots
			if (def.setFlags != null)
			{
				for (int i = 0; i < def.setFlags.Length; i++)
					_progress.SetFlag(def.setFlags[i]);
			}
			if (!string.IsNullOrEmpty(def.showZoomId)) _zoom.Show(def.showZoomId);
			if (def.goToStageIndex >= 0) _bus.Publish(new AFKS.Core.Events.StageChangedEvent(def.goToStageIndex));
			if (!string.IsNullOrEmpty(def.playSfx)) _bus.Publish(new PlaySfxEvent(def.playSfx));
		}

		// PlaySfxEvent moved to Core.Events.Events
	}
}


