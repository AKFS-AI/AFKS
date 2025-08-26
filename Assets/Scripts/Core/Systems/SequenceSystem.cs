using UnityEngine;
using AFKS.Core.Services;
using AFKS.Core.Events;
using AFKS.Core.Data;

namespace AFKS.Core.Systems
{
	public sealed class SequenceSystem : MonoBehaviour
	{
		private EventBus _bus;
		private StageController _stage;
		private ProgressService _progress;

		public void Initialize(EventBus bus, StageController stage, ProgressService progress)
		{
			_bus = bus; _stage = stage; _progress = progress;
			_bus.Subscribe<HotspotClickedEvent>(OnHotspotClicked);
		}

		private void OnHotspotClicked(HotspotClickedEvent e)
		{
			var def = GetHotspot(e.hotspotId);
			if (def == null || def.sequence == null || def.sequence.steps == null) return;
			var countKey = $"clicks::{e.hotspotId}";
			var clicks = _progress.AddAndGet(countKey, 1);
			for (int i = 0; i < def.sequence.steps.Length; i++)
			{
				var step = def.sequence.steps[i];
				if (step.onClickCount != clicks) continue;
				if (!CheckConditions(step.conditions)) continue;
				ExecuteActions(step.actions);
			}
		}

		private HotspotDefinition GetHotspot(string id)
		{
			var s = _stage.CurrentDefinition;
			if (s == null || s.hotspots == null) return null;
			for (int i = 0; i < s.hotspots.Length; i++)
				if (s.hotspots[i] != null && s.hotspots[i].id == id) return s.hotspots[i];
			return null;
		}

		private bool CheckConditions(Condition[] conds)
		{
			if (conds == null || conds.Length == 0) return true;
			for (int i = 0; i < conds.Length; i++)
			{
				var c = conds[i];
				switch (c.type)
				{
					case ConditionType.Flag:
						if (!_progress.GetFlag(c.key)) return false; break;
					case ConditionType.Item:
						// Inventory not required for current use-cases; flags preferred
						break;
					case ConditionType.CounterAtLeast:
						if (_progress.GetInt(c.key) < c.threshold) return false; break;
				}
			}
			return true;
		}

		private void ExecuteActions(SequenceAction[] actions)
		{
			if (actions == null) return;
			for (int i = 0; i < actions.Length; i++)
			{
				var a = actions[i];
				switch (a.type)
				{
					case ActionType.SetFlag:
						_progress.SetFlag(a.a, true); break;
					case ActionType.PlaySfx:
						_bus.Publish(new PlaySfxEvent(a.a)); break;
					case ActionType.ShowZoom:
						_bus.Publish(new ShowZoomRequest(a.a)); break;
					case ActionType.ToggleOverlay:
						_stage.SetOverlayVisible(a.a, a.v); break;
					case ActionType.SwapOverlaySprite:
						_stage.SwapOverlaySprite(a.a, a.sprite); break;
					case ActionType.GoToStage:
						_stage.LoadStageByIndex(a.i); break;
				}
			}
		}

		public readonly struct ShowZoomRequest
		{
			public readonly string zoomId;
			public ShowZoomRequest(string id) { zoomId = id; }
		}
	}
}

