using UnityEngine;
using UnityEngine.Events;

namespace AFKS.Features.Interaction
{
	/// <summary>
	/// ClickHandler의 onClick을 StageEventSystem으로 라우팅합니다.
	/// objectId와 eventSystem 참조는 에디터에서 명시적으로 설정합니다.
	/// </summary>
	[RequireComponent(typeof(ClickHandler))]
	[AddComponentMenu("AFKS/Interaction/Click To Event Router")]
	public sealed class ClickToEventRouter : MonoBehaviour
	{
		[SerializeField] private string objectId;
		[SerializeField] private AFKS.Features.Stage.StageEventSystem eventSystem;

		private ClickHandler clickHandler;

		private void Awake()
		{
			clickHandler = GetComponent<ClickHandler>();
			if (string.IsNullOrEmpty(objectId)) objectId = gameObject.name;
		}

		private void OnEnable()
		{
			if (clickHandler != null)
			{
				clickHandler.onClick.AddListener(OnClicked);
			}
		}

		private void OnDisable()
		{
			if (clickHandler != null)
			{
				clickHandler.onClick.RemoveListener(OnClicked);
			}
		}

		private void OnClicked()
		{
			if (eventSystem == null) return;
			eventSystem.OnObjectClicked(objectId);
		}

		public void SetObjectId(string id) { objectId = id; }
		public void SetEventSystem(AFKS.Features.Stage.StageEventSystem es) { eventSystem = es; }
	}
}


