using UnityEngine;

namespace AFKS.Core.Data
{
	[CreateAssetMenu(menuName = "AFKS/Stage Definition")]
	public sealed class StageDefinition : ScriptableObject
	{
		public int stageIndex;
		public Sprite backgroundSprite;
		public AudioClip ambientSfx;
		public OverlayObjectDefinition[] overlays; // UI overlay objects to toggle/swap
		public HotspotDefinition[] hotspots;
		public ZoomDefinition[] zooms;
		public GimmickDefinition[] gimmicks;
	}

	[System.Serializable]
	public sealed class HotspotDefinition
	{
		public string id;
		public Rect rect; // viewport-space rect [0..1]
		public string[] requiredItemIds;
		public string[] setFlags;
		public string playSfx;
		public string showZoomId;
		public int goToStageIndex = -1; // -1 = stay
		public InteractionSequenceDefinition sequence;
	}

	[System.Serializable]
	public sealed class OverlayObjectDefinition
	{
		public string id;
		public Sprite sprite;
		public Rect rect; // UI normalized rect
		public bool initiallyVisible = true;
		public int sortingOrder;
	}

	public enum ConditionType { Flag, Item, CounterAtLeast }
	public enum ActionType { SetFlag, PlaySfx, ShowZoom, ToggleOverlay, SwapOverlaySprite, GoToStage }

	[System.Serializable]
	public sealed class InteractionSequenceDefinition
	{
		public SequenceStep[] steps;
	}

	[System.Serializable]
	public sealed class SequenceStep
	{
		public int onClickCount = 1; // trigger at this click count
		public Condition[] conditions;
		public SequenceAction[] actions;
	}

	[System.Serializable]
	public sealed class Condition
	{
		public ConditionType type;
		public string key; // flag or item id or counter id
		public int threshold; // for CounterAtLeast
	}

	[System.Serializable]
	public sealed class SequenceAction
	{
		public ActionType type;
		public string a; // flag id / sfx id / overlay id / zoom id
		public string b; // optional extra
		public int i; // stage index or int value
		public bool v; // toggle value
		public Sprite sprite; // for swap
	}

	[System.Serializable]
	public sealed class ZoomDefinition
	{
		public string id;
		public Sprite sprite;
		[TextArea]
		public string caption;
		public string canPickupItemId; // optional
	}

	public enum GimmickType { DoorLock, CCTV, EKG, Ultrasound, Incubator, MusicBox }

	[System.Serializable]
	public sealed class GimmickDefinition
	{
		public string id;
		public GimmickType type;
		public string[] parameters; // simple k=v pairs or ids
	}
}


