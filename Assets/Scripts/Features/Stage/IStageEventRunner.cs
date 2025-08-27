namespace AFKS.Features.Stage
{
	public interface IStageEventRunner
	{
		void Prepare(Stage.Events.StageEvent stageEvent, StageInteractionManager interactionManager);
		void Execute(Stage.Events.StageEvent stageEvent, System.Action onComplete);
		bool ShouldAutoProceed(Stage.Events.StageEvent stageEvent);
	}
}
