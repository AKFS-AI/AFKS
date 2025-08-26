namespace AFKS.Core.Events
{
	public readonly struct ItemPickedEvent
	{
		public readonly string itemId;
		public ItemPickedEvent(string itemId) { this.itemId = itemId; }
	}

	public readonly struct ItemUsedOnEvent
	{
		public readonly string itemId;
		public readonly string targetId;
		public ItemUsedOnEvent(string itemId, string targetId)
		{
			this.itemId = itemId; this.targetId = targetId;
		}
	}

	public readonly struct FlagChangedEvent
	{
		public readonly string flag;
		public readonly bool value;
		public FlagChangedEvent(string flag, bool value) { this.flag = flag; this.value = value; }
	}

	public readonly struct StageChangedEvent
	{
		public readonly int index;
		public StageChangedEvent(int index) { this.index = index; }
	}

	public readonly struct JumpscareEvent
	{
		public readonly string id;
		public JumpscareEvent(string id) { this.id = id; }
	}

	public readonly struct PlaySfxEvent
	{
		public readonly string sfxId;
		public PlaySfxEvent(string sfxId) { this.sfxId = sfxId; }
	}

	public readonly struct HotspotClickedEvent
	{
		public readonly string hotspotId;
		public HotspotClickedEvent(string hotspotId) { this.hotspotId = hotspotId; }
	}
}


