using System.Collections;

namespace AFKS.Core.Services.Scene
{
    public interface ISceneService
    {
        IEnumerator FadeOutAsync(float seconds);
        IEnumerator FadeInAsync(float seconds);
        IEnumerator LoadStageAdditiveAsync(string stageId, bool activateOnLoad = false);
        IEnumerator ActivateLoadedStageAsync(string stageId);
        IEnumerator UnloadStageAsync(string stageId);
    }
}


