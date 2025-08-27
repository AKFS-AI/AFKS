using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using AFKS.Features.Stage;

namespace AFKS.Features.Interaction
{
	/// <summary>
	/// UI 버튼(또는 투명 핫스팟)의 클릭을 StageEventSystem으로 라우팅합니다.
	/// 씬 전환은 StageEventSystem의 이벤트가 담당합니다.
	/// </summary>
	[AddComponentMenu("AFKS/Interaction/Hotspot Move UI")]
	[RequireComponent(typeof(RectTransform))]
	public sealed class HotspotMoveUI : MonoBehaviour, IPointerClickHandler
	{
		[SerializeField] private string objectId = "Door"; // StageEventSystem에 전달할 오브젝트 ID
		[SerializeField] private StageEventSystem eventSystem; // 명시 연결 권장

		public void OnPointerClick(PointerEventData eventData)
		{
			if (eventSystem == null) eventSystem = FindFirstObjectByType<StageEventSystem>();
			if (eventSystem == null)
			{
				Debug.LogWarning("[HotspotMoveUI] StageEventSystem not found.");
				return;
			}
			if (string.IsNullOrEmpty(objectId)) objectId = gameObject.name;
			eventSystem.OnObjectClicked(objectId);
		}
	}
}


