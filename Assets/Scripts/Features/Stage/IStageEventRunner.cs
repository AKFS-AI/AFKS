using AFKS.Features.Stage.Events;

namespace AFKS.Features.Stage
{
	public interface IStageEventRunner
	{
		void Prepare(Stage.Events.StageEvent stageEvent, StageInteractionSystem interactionManager);
		void Execute(Stage.Events.StageEvent stageEvent, System.Action onComplete);
		bool ShouldAutoProceed(Stage.Events.StageEvent stageEvent);
	}
}
