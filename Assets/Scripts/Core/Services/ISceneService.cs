using System.Threading.Tasks;

namespace AFKS.Core.Services
{
	public interface ISceneService
	{
		Task LoadStageAdditiveAsync(string stageId, bool preload = false);
		Task ActivateLoadedStageAsync(string stageId);
		Task UnloadStageAsync(string stageId);
		Task FadeOutAsync(float seconds);
		Task FadeInAsync(float seconds);
	}
}
