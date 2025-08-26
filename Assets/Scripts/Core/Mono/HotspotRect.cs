using UnityEngine;

namespace AFKS.Core.Mono
{
	/// <summary>
	/// 씬에 베이크되는 핫스팟 정의(정규화 rect). 런타임 상호작용 시스템이 직접 읽습니다.
	/// </summary>
	public sealed class HotspotRect : MonoBehaviour
	{
		public string id;
		public Rect rect; // 0..1 화면 비율 좌표(x,y,width,height)
		public string[] requiredItemIds;
		public string[] setFlags;
		public string playSfx;
		public string showZoomId;
		public int goToStageIndex = -1;
	}
}


