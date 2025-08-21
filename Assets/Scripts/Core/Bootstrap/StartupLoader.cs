using AFKS.Core.Events;
using AFKS.Features.Stage;
using UnityEngine;

namespace AFKS.Core.Bootstrap
{
	/// <summary>
	/// Core 시작 시 초기 스테이지를 요청하는 로더. 기본은 Stage_Menu.
	/// </summary>
	public sealed class StartupLoader : MonoBehaviour
	{
		[SerializeField] private string initialStageId = StageIds.Stage_Menu;
		[SerializeField] private bool autoLoadOnStart = true;

		private void Start()
		{
			if (autoLoadOnStart && !string.IsNullOrEmpty(initialStageId))
			{
				GameEvents.RaiseStageChangeRequested(initialStageId);
			}
		}
	}
}
