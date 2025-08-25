using UnityEngine;
using AFKS.Core.Services.Scene; // 가정: SceneService 네임스페이스
using AFKS.Core.Events;

namespace AFKS.Features.Interaction
{
	/// <summary>
	/// 월드용 문 핫스팟: 클릭 시 Stage 전환 요청.
	/// </summary>
	[AddComponentMenu("AFKS/Interaction/Hotspot Move (World)")]
	public sealed class HotspotMoveWorld : MonoBehaviour
	{
		[SerializeField] private string targetStageId = "Stage2";
		[SerializeField] private bool debugLog = false;

		private void OnMouseDown()
		{
			if (debugLog) Debug.Log($"[HotspotMoveWorld] request -> {targetStageId}");
			GameEvents.RaiseStageChangeRequested(targetStageId);
		}
	}
}


