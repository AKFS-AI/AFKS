using System;

namespace AFKS.Core.Events
{
    /// <summary>
    /// 게임 이벤트 허브. 기능 간 결합을 낮추는 경량 C# 이벤트 모음입니다.
    /// </summary>
    public static class GameEvents
    {
        #region 스테이지 흐름
        public static event Action<string> StageChangeRequested;
        public static event Action<string> StageLoaded;
        public static event Action<string> StageUnloaded;
        #endregion

        #region 인벤토리(확장 예정)
        public static event Action<string> ItemPicked;
        #endregion

        #region 점프스케어(확장 예정)
        public static event Action<string> JumpScareTriggered;
        #endregion

        #region 시스템
        public static event Action<bool> PauseToggled; // true=Paused
        #endregion

        #region 발행 함수
        public static void RaiseStageChangeRequested(string targetStageId)
        {
            StageChangeRequested?.Invoke(targetStageId);
        }

        public static void RaiseStageLoaded(string stageId)
        {
            StageLoaded?.Invoke(stageId);
        }

        public static void RaiseStageUnloaded(string stageId)
        {
            StageUnloaded?.Invoke(stageId);
        }

        public static void RaiseItemPicked(string itemId)
        {
            ItemPicked?.Invoke(itemId);
        }

        public static void RaiseJumpScareTriggered(string clipId)
        {
            JumpScareTriggered?.Invoke(clipId);
        }

        public static void RaisePauseToggled(bool paused)
        {
            PauseToggled?.Invoke(paused);
        }
        #endregion
    }
}


