using UnityEngine;

namespace AFKS.Features.Stage.Conditions
{
	[CreateAssetMenu(fileName = "ClickCountCondition", menuName = "AFKS/Stage/Conditions/Click Count")] 
	public sealed class ClickCountCondition : StageCondition
	{
		[SerializeField] private string objectId = "";
		[SerializeField] private int requiredCount = 1;
		[SerializeField] private bool exactMatch = false; // true면 ==, false면 >=

		public override bool Evaluate(AFKS.Features.Stage.StageEventSystem system)
		{
			if (system == null || string.IsNullOrEmpty(objectId)) return false;
			int count = system.GetClickCount(objectId);
			return exactMatch ? (count == requiredCount) : (count >= requiredCount);
		}
	}
}


