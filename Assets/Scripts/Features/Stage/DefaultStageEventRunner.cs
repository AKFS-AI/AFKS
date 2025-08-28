using AFKS.Core.Utils;
using AFKS.Features.Stage.Events;

namespace AFKS.Features.Stage
{
	public sealed class DefaultStageEventRunner : IStageEventRunner
	{
		public void Prepare(Events.StageEvent stageEvent, StageInteractionSystem interactionManager)
		{
			// 기본 구현: 상호작용 오브젝트 설정
			if (interactionManager != null && stageEvent.InteractableObjects != null)
			{
				interactionManager.SetInteractableObjects(stageEvent.InteractableObjects);
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
