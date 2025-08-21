using UnityEngine;

namespace AFKS.Core.Utils
{
	/// <summary>
	/// 프로젝트 전역 로깅 유틸리티. Debug.Log 직접 호출 금지, 본 유틸만 사용.
	/// </summary>
	public static class Log
	{
		public static void Info(string message)
		{
			Debug.Log(message);
		}

		public static void Warn(string message)
		{
			Debug.LogWarning(message);
		}

		public static void Error(string message)
		{
			Debug.LogError(message);
		}
	}
}
