using System;

namespace AFKS.Core.Events
{
	/// <summary>
	/// 전역 이벤트 허브. 씬 교체에도 일관되게 브로드캐스트한다.
	/// </summary>
	public static class GameEvents
	{
		public static event Action<string> StageChangeRequested;
		public static event Action<string> StageLoaded;
		public static event Action<string> StageUnloaded;

		public static void RaiseStageChangeRequested(string stageId)
		{
			StageChangeRequested?.Invoke(stageId);
		}

		public static void RaiseStageLoaded(string stageId)
		{
			StageLoaded?.Invoke(stageId);
		}

		public static void RaiseStageUnloaded(string stageId)
		{
			StageUnloaded?.Invoke(stageId);
		}
	}
}
