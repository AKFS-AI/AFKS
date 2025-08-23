using System.Collections;

namespace AFKS.Core.Services.Scene
{
    /// <summary>
    /// 스테이지 전환(페이드/로드/언로드/활성화)을 비동기로 수행하는 씬 서비스 인터페이스입니다.
    /// </summary>
    public interface ISceneService
    {
        #region 페이드
        /// <summary>화면을 어둡게 페이드아웃합니다.</summary>
        IEnumerator FadeOutAsync(float seconds);
        /// <summary>화면을 밝게 페이드인합니다.</summary>
        IEnumerator FadeInAsync(float seconds);
        #endregion

        #region 로드/언로드
        /// <summary>스테이지 씬을 Additive로 로드합니다.</summary>
        IEnumerator LoadStageAdditiveAsync(string stageId, bool activateOnLoad = false);
        /// <summary>이미 로드된 스테이지 씬을 활성화합니다.</summary>
        IEnumerator ActivateLoadedStageAsync(string stageId);
        /// <summary>스테이지 씬을 언로드합니다.</summary>
        IEnumerator UnloadStageAsync(string stageId);
        #endregion
    }
}


