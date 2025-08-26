using UnityEngine;
using AFKS.Core.Services;
using AFKS.Core.Events;
using AFKS.Core.Data;
using AFKS.Core.Systems;

namespace AFKS.Core.Features.Hospital.Gimmicks
{
	public interface IGimmick
	{
		void Initialize(EventBus bus, StageController stage, GimmickDefinition def);
	}

	public sealed class DoorLockGimmick : MonoBehaviour, IGimmick
	{
		private EventBus _bus; private StageController _stage; private GimmickDefinition _def;
		public void Initialize(EventBus bus, StageController stage, GimmickDefinition def) { _bus = bus; _stage = stage; _def = def; }
	}

	public sealed class CctvGimmick : MonoBehaviour, IGimmick
	{
		private EventBus _bus; private StageController _stage; private GimmickDefinition _def;
		public void Initialize(EventBus bus, StageController stage, GimmickDefinition def) { _bus = bus; _stage = stage; _def = def; }
	}

	public sealed class EkgGimmick : MonoBehaviour, IGimmick
	{
		private EventBus _bus; private StageController _stage; private GimmickDefinition _def;
		public void Initialize(EventBus bus, StageController stage, GimmickDefinition def) { _bus = bus; _stage = stage; _def = def; }
	}

	public sealed class UltrasoundGimmick : MonoBehaviour, IGimmick
	{
		private EventBus _bus; private StageController _stage; private GimmickDefinition _def;
		public void Initialize(EventBus bus, StageController stage, GimmickDefinition def) { _bus = bus; _stage = stage; _def = def; }
	}

	public sealed class IncubatorGimmick : MonoBehaviour, IGimmick
	{
		private EventBus _bus; private StageController _stage; private GimmickDefinition _def;
		public void Initialize(EventBus bus, StageController stage, GimmickDefinition def) { _bus = bus; _stage = stage; _def = def; }
	}

	public sealed class MusicBoxGimmick : MonoBehaviour, IGimmick
	{
		private EventBus _bus; private StageController _stage; private GimmickDefinition _def;
		public void Initialize(EventBus bus, StageController stage, GimmickDefinition def) { _bus = bus; _stage = stage; _def = def; }
	}
}


