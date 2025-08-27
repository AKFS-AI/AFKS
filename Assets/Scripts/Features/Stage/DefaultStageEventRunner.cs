using AFKS.Core.Utils;

namespace AFKS.Features.Stage
{
	public sealed class DefaultStageEventRunner : IStageEventRunner
	{
		public void Prepare(Events.StageEvent stageEvent, StageInteractionManager interactionManager)
		{
			if (stageEvent == null) return;
			stageEvent.Prepare();
			if (interactionManager != null)
			{
				interactionManager.SetInteractableObjects(stageEvent.GetInteractableObjects());
				var list = string.Join(", ", interactionManager.GetCurrentInteractableObjects());
				GameDebug.Info(GameDebug.Category.Event, $"Interactable after prepare: {list}");
			}
		}

		public void Execute(Events.StageEvent stageEvent, System.Action onComplete)
		{
			if (stageEvent == null)
			{
				onComplete?.Invoke();
				return;
			}
			stageEvent.Execute(onComplete);
		}

		public bool ShouldAutoProceed(Events.StageEvent stageEvent)
		{
			return stageEvent != null && stageEvent.AutoProceed;
		}
	}
}
