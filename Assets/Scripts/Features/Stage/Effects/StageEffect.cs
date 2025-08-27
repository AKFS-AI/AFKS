using UnityEngine;

namespace AFKS.Features.Stage.Effects
{
	/// <summary>
	/// StageEvent 완료 시 적용되는 효과의 베이스.
	/// </summary>
	public abstract class StageEffect : ScriptableObject
	{
		public abstract void Apply(AFKS.Features.Stage.StageEventSystem system);
	}
}


