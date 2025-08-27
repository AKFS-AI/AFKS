using UnityEngine;

namespace AFKS.Features.Stage.Effects
{
	[CreateAssetMenu(fileName = "StageTransitionEffect", menuName = "AFKS/Stage/Effects/Stage Transition")] 
	public sealed class StageTransitionEffect : StageEffect
	{
		[SerializeField] private string targetStageId = "";
		[SerializeField] private float delay = 0.2f;

		public override void Apply(AFKS.Features.Stage.StageEventSystem system)
		{
			if (string.IsNullOrEmpty(targetStageId)) return;
			system.StartCoroutine(DoTransition(system, delay));
		}

		private System.Collections.IEnumerator DoTransition(AFKS.Features.Stage.StageEventSystem system, float d)
		{
			if (d > 0f) yield return new WaitForSeconds(d);
			if (AFKS.Core.Services.ServiceLocator.TryGet<AFKS.Core.Services.Scene.ISceneService>(out var sceneSvc))
			{
				AFKS.Core.Events.GameEvents.RaiseStageChangeRequested(targetStageId);
			}
		}
	}
}


