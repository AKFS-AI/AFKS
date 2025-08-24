using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using AFKS.Core.Events;
using AFKS.Core.Services;
using AFKS.Core.Services.Scene;
using UnityEngine.SceneManagement;

namespace AFKS.Features.Interaction
{
	/// <summary>
	/// UI 버튼(또는 투명 핫스팟)에서 직접 스테이지 전환을 수행합니다.
	/// 확대 프리팹 위에서 문 클릭 → Stage2 전환용.
	/// </summary>
	[AddComponentMenu("AFKS/Interaction/Hotspot Move UI")]
	[RequireComponent(typeof(RectTransform))]
	public sealed class HotspotMoveUI : MonoBehaviour, IPointerClickHandler
	{
		[SerializeField] private string targetStageId = "Stage2";
		[SerializeField] private GameObject closeupRootToDestroy; // 전환 직전 닫을 프리팹 루트

		private void Awake()
		{
			// 루트 자동 연결(없을 때만): ChainCloseupController가 있는 가장 가까운 루트를 파괴 대상으로 지정
			if (closeupRootToDestroy == null)
			{
				var controller = GetComponentInParent<ChainCloseupController>(true);
				if (controller != null) closeupRootToDestroy = controller.gameObject;
			}
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			if (string.IsNullOrEmpty(targetStageId)) return;
			GameEvents.RaiseStageChangeRequested(targetStageId);
			if (closeupRootToDestroy != null) Destroy(closeupRootToDestroy);
			if (!ServiceLocator.TryGet<ISceneService>(out _))
			{
				SceneManager.LoadScene(targetStageId, LoadSceneMode.Single);
			}
		}
	}
}


