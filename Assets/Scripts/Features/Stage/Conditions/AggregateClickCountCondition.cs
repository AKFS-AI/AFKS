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
			
			Debug.Log($"AggregateClickCountCondition.Evaluate: {objectIds.Count}개 오브젝트, {requiredTotalCount}번 클릭 필요");
			
			int sum = 0;
			for (int i = 0; i < objectIds.Count; i++)
			{
				var id = objectIds[i];
				if (string.IsNullOrEmpty(id)) continue;
				int clickCount = system.GetClickCount(id);
				sum += clickCount;
				Debug.Log($"AggregateClickCountCondition: {id} 클릭 수 = {clickCount}");
			}
			
			bool result = sum >= requiredTotalCount;
			Debug.Log($"AggregateClickCountCondition: 총 클릭 수 = {sum}, 조건 만족 = {result}");
			
			return result;
		}
	}
}


