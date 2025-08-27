using UnityEngine;
using System.Collections.Generic;

namespace AFKS.Features.Stage.Conditions
{
	[CreateAssetMenu(fileName = "AggregateClickCountCondition", menuName = "AFKS/Stage/Conditions/Aggregate Click Count")] 
	public sealed class AggregateClickCountCondition : StageCondition
	{
		[SerializeField] private List<string> objectIds = new List<string>();
		[SerializeField] private int requiredTotalCount = 1;

		public override bool Evaluate(AFKS.Features.Stage.StageEventSystem system)
		{
			if (system == null || objectIds == null || objectIds.Count == 0) return false;
			int sum = 0;
			for (int i = 0; i < objectIds.Count; i++)
			{
				var id = objectIds[i];
				if (string.IsNullOrEmpty(id)) continue;
				sum += system.GetClickCount(id);
			}
			return sum >= requiredTotalCount;
		}
	}
}


