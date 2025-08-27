using UnityEngine;

namespace AFKS.Features.Stage.Conditions
{
	/// <summary>
	/// StageEvent가 실행 가능하기 위한 단일 조건의 추상 베이스.
	/// </summary>
	public abstract class StageCondition : ScriptableObject
	{
		/// <summary>
		/// 현재 StageEventSystem 상태에서 조건이 충족되는지 평가합니다.
		/// </summary>
		public abstract bool Evaluate(AFKS.Features.Stage.StageEventSystem system);
	}
}


