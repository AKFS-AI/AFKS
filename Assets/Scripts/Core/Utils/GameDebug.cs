using UnityEngine;

namespace AFKS.Core.Utils
{
	/// <summary>
	/// 카테고리별 토글 가능한 중앙 디버그 유틸리티.
	/// 필요할 때만 켜서 로그 소음을 줄입니다.
	/// </summary>
	public static class GameDebug
	{
		public enum Category
		{
			Core,
			Stage,
			Event,
			Interaction,
			Camera,
			Save
		}

		private static bool globalEnabled = true;
		private static bool coreEnabled = false;
		private static bool stageEnabled = false;
		private static bool eventEnabled = true; // 기본값: 이벤트 흐름만 표시
		private static bool interactionEnabled = false;
		private static bool cameraEnabled = false;
		private static bool saveEnabled = false;

		public static void SetGlobal(bool enabled) => globalEnabled = enabled;
		public static void Set(Category category, bool enabled)
		{
			switch (category)
			{
				case Category.Core: coreEnabled = enabled; break;
				case Category.Stage: stageEnabled = enabled; break;
				case Category.Event: eventEnabled = enabled; break;
				case Category.Interaction: interactionEnabled = enabled; break;
				case Category.Camera: cameraEnabled = enabled; break;
				case Category.Save: saveEnabled = enabled; break;
			}
		}

		private static bool IsEnabled(Category category)
		{
			if (!globalEnabled) return false;
			switch (category)
			{
				case Category.Core: return coreEnabled;
				case Category.Stage: return stageEnabled;
				case Category.Event: return eventEnabled;
				case Category.Interaction: return interactionEnabled;
				case Category.Camera: return cameraEnabled;
				case Category.Save: return saveEnabled;
				default: return false;
			}
		}

		public static void Info(Category category, string message)
		{
			if (!IsEnabled(category)) return;
			Debug.Log(message);
		}

		public static void Warn(Category category, string message)
		{
			if (!IsEnabled(category)) return;
			Debug.LogWarning(message);
		}

		public static void Error(Category category, string message)
		{
			if (!IsEnabled(category)) return;
			Debug.LogError(message);
		}

		public static void Progress(Category category, string label, int current, int required)
		{
			if (!IsEnabled(category)) return;
			Debug.Log($"[Progress] {label}: {current}/{required}");
		}
	}
}


