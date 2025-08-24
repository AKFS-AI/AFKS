using UnityEngine;
using UnityEngine.UI;

namespace AFKS.Features.Interaction
{
	/// <summary>
	/// CloseupPanel.prefab 안의 필수 컴포넌트들을 자동으로 찾아 연결합니다.
	/// - 루트(이 스크립트 부착)에 ChainCloseupController가 없으면 추가/필드 자동 주입
	/// - ChainHotspot에 ChainHotspotUI 보장 및 컨트롤러 주입

	/// - DoorHotspot에 HotspotMoveUI 보장 및 closeupRootToDestroy=루트 지정
	/// - DoorHotspot은 기본 비활성화
	/// - 루트/CloseupImage RectTransform 스트레치 보정
	/// 주의: bgLockedSprite/bgUnlockedSprite는 에셋 의존이므로 수동 지정 필요
	/// </summary>
	[ExecuteAlways]
	[AddComponentMenu("AFKS/Interaction/Closeup Panel AutoWire")]
	public sealed class CloseupPanelAutoWire : MonoBehaviour
	{
		private void OnValidate()
		{
			TryAutoWire();
		}

		private void Reset()
		{
			TryAutoWire();
		}

		private void TryAutoWire()
		{
			var root = transform as RectTransform;
			if (root != null)
			{
				root.anchorMin = Vector2.zero;
				root.anchorMax = Vector2.one;
				root.offsetMin = Vector2.zero;
				root.offsetMax = Vector2.zero;
			}

			// 필수 노드 탐색
			var closeupImageTr = transform.Find("CloseupImage") as RectTransform;
			var closeupImage = closeupImageTr != null ? closeupImageTr.GetComponent<Image>() : null;
			var chainRoot = transform.Find("ChainRoot") != null ? transform.Find("ChainRoot").gameObject : null;
			var chainHotspot = transform.Find("ChainHotspot") != null ? transform.Find("ChainHotspot").gameObject : null;
			var doorHotspot = transform.Find("DoorHotspot") != null ? transform.Find("DoorHotspot").gameObject : null;

			// 컨트롤러 보장 및 주입
			var controller = GetComponent<ChainCloseupController>();
			if (controller == null) controller = gameObject.AddComponent<ChainCloseupController>();
			// 필드 연결(존재할 때만) - 에디터 API 없이 직접 주입
			if (closeupImage != null)
			{
				controller.SetCloseupImage(closeupImage);
			}
			if (chainRoot != null)
			{
				var anim = chainRoot.GetComponent<Animator>();
				if (anim == null) anim = chainRoot.AddComponent<Animator>();
				controller.SetChain(chainRoot, anim);
			}
			if (doorHotspot != null)
			{
				controller.SetDoor(doorHotspot, gameObject);
			}

			// 체인 핫스팟 보장/연결
			if (chainHotspot != null)
			{
				var ui = chainHotspot.GetComponent<ChainHotspotUI>();
				if (ui == null) ui = chainHotspot.AddComponent<ChainHotspotUI>();
				ui.SetController(controller);
			}

			// 문 핫스팟 보장/연결
			if (doorHotspot != null)
			{
				var img = doorHotspot.GetComponent<Image>();
				if (img == null) img = doorHotspot.AddComponent<Image>();
				img.color = new Color(1f, 1f, 1f, 0f);
				img.raycastTarget = true;
				var move = doorHotspot.GetComponent<HotspotMoveUI>();
				if (move == null) move = doorHotspot.AddComponent<HotspotMoveUI>();
				// 기본 비활성
				if (Application.isEditor && !Application.isPlaying)
				{
					if (doorHotspot.activeSelf) doorHotspot.SetActive(false);
				}
			}

			// CloseupImage 스트레치 보정
			if (closeupImageTr != null)
			{
				closeupImageTr.anchorMin = Vector2.zero;
				closeupImageTr.anchorMax = Vector2.one;
				closeupImageTr.offsetMin = Vector2.zero;
				closeupImageTr.offsetMax = Vector2.zero;
			}
		}
	}
}


