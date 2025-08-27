using UnityEngine;
using System.Collections.Generic;

namespace AFKS.Features.Stage.Conditions
{
	[CreateAssetMenu(fileName = "ClickSequenceCondition", menuName = "AFKS/Stage/Conditions/Click Sequence")] 
	public sealed class ClickSequenceCondition : StageCondition
	{
		[SerializeField] private List<string> sequence = new List<string>();
		[SerializeField] private bool allowExtra = false; // true면 초과 입력 허용(순서만 맞으면 됨)

		public override bool Evaluate(AFKS.Features.Stage.StageEventSystem system)
		{
			if (system == null || sequence == null || sequence.Count == 0) return false;
			return system.DoesClickHistoryMatch(sequence, allowExtra);
		}
	}
}


