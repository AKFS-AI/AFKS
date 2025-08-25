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
		[SerializeField] private GameObject closeupRootToDestroy; // 전환 시 제거할 프리팹 루트(지연 제거)
		[SerializeField] private bool destroyOnStageLoaded = true; // StageLoaded 후 파괴(페이드아웃이 가린 뒤)
		[SerializeField] private bool disableInteractionImmediately = true; // 클릭 직후 상호작용 차단

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
			// 체인이 잠겨 있으면 문 클릭을 무시
			var controller = GetComponentInParent<ChainCloseupController>(true);
			if (controller != null && !controller.IsUnlocked)
			{
				Debug.Log("[HotspotMoveUI] Door click ignored: chain not unlocked yet.");
				return;
			}
			// 즉시 상호작용 차단(시각은 유지)
			if (disableInteractionImmediately && closeupRootToDestroy != null)
			{
				var cg = closeupRootToDestroy.GetComponent<CanvasGroup>();
				if (cg == null) cg = closeupRootToDestroy.AddComponent<CanvasGroup>();
				cg.interactable = false;
				cg.blocksRaycasts = false;
			}
			GameEvents.RaiseStageChangeRequested(targetStageId);
			// 페이드아웃이 시작된 뒤 검은 화면에서 씬이 교체되도록 지연 파괴
			if (destroyOnStageLoaded && closeupRootToDestroy != null)
			{
				GameEvents.StageLoaded += OnAnyStageLoaded;
			}
			if (!ServiceLocator.TryGet<ISceneService>(out _))
			{
				SceneManager.LoadScene(targetStageId, LoadSceneMode.Single);
			}
		}

		private void OnAnyStageLoaded(string _)
		{
			GameEvents.StageLoaded -= OnAnyStageLoaded;
			if (closeupRootToDestroy != null)
			{
				Destroy(closeupRootToDestroy);
			}
		}
	}
}


