using UnityEngine;

namespace AFKS.Core.Utils
{
	/// <summary>
	/// 표준화된 로그 유틸리티. 추후 상관ID/원격 수집으로 확장 가능.
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


