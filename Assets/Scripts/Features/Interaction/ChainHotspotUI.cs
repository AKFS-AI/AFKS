using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AFKS.Features.Interaction
{
	/// <summary>
	/// CloseupImage 위에서 체인 클릭을 처리하는 UI 핫스팟.
	/// - 정규화(0~1) 앵커 입력으로 해상도 무관 정확 위치를 보장합니다.
	/// - 클릭 시 지정 타깃 GameObject를 활성화하거나 UnityEvent를 호출하는 대신
	///   기존 월드 오브젝트(예: DoorMove)의 SetActive를 직접 트리거할 수 있습니다.
	/// </summary>
	[AddComponentMenu("AFKS/Interaction/Chain Hotspot UI")]
	[RequireComponent(typeof(RectTransform))]
	public sealed class ChainHotspotUI : MonoBehaviour, IPointerClickHandler
	{
		#region 필드
		[SerializeField] private RectTransform targetRect; // 기준 이미지(RectTransform). 비워두면 부모를 사용
		[SerializeField] private Vector2 anchorMin01 = new Vector2(0.45f, 0.42f);
		[SerializeField] private Vector2 anchorMax01 = new Vector2(0.55f, 0.55f);
		[SerializeField] private GameObject targetToEnable; // DoorMove 등
		[SerializeField] private ChainCloseupController controller; // 자동 연결 지원
		#endregion

		private RectTransform self;

		private void Awake()
		{
			self = GetComponent<RectTransform>();
			if (controller == null) controller = GetComponentInParent<ChainCloseupController>(true);
			if (targetRect == null)
			{
				// 컨트롤러 기준 CloseupImage를 우선 탐색
				Transform root = controller != null ? controller.transform : transform.parent;
				if (root != null)
				{
					var t = root.Find("CloseupImage");
					if (t is RectTransform rtr) targetRect = rtr;
				}
				if (targetRect == null) targetRect = transform.parent as RectTransform;
			}
			ApplyNormalizedAnchors();
			// 기본적으로 투명 클릭 영역: Raycast Target 필요 시 부모 이미지 대신 이 오브젝트의 Image 사용
			var img = GetComponent<Image>();
			if (img != null)
			{
				img.color = new Color(0f, 0f, 0f, 0f);
				img.raycastTarget = true;
			}
		}

		public void ConfigureNormalizedArea(Rect rectPixels, Vector2 textureSize)
		{
			if (textureSize.x <= 0f || textureSize.y <= 0f) return;
			var nx = rectPixels.x / textureSize.x;
			var ny = rectPixels.y / textureSize.y;
			var nw = rectPixels.width / textureSize.x;
			var nh = rectPixels.height / textureSize.y;
			anchorMin01 = new Vector2(nx, ny);
			anchorMax01 = new Vector2(nx + nw, ny + nh);
			ApplyNormalizedAnchors();
		}

		private void ApplyNormalizedAnchors()
		{
			if (self == null) self = GetComponent<RectTransform>();
			self.anchorMin = anchorMin01;
			self.anchorMax = anchorMax01;
			self.offsetMin = Vector2.zero;
			self.offsetMax = Vector2.zero;
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			if (controller == null)
			{
				Debug.LogWarning("[ChainHotspotUI] Controller is null. Click ignored.");
				return;
			}
			if (!gameObject.activeInHierarchy || !enabled)
			{
				Debug.LogWarning("[ChainHotspotUI] Hotspot inactive/disabled. Click ignored.");
				return;
			}
			var img = GetComponent<Image>();
			Debug.Log($"[ChainHotspotUI] Click pos={eventData.position} rtAnchors=({self.anchorMin}->{self.anchorMax}) raycast={(img!=null?img.raycastTarget:false)}");
			controller.OnChainClicked();
			if (targetToEnable != null)
			{
				targetToEnable.SetActive(true);
			}
		}

		public void SetController(ChainCloseupController c)
		{
			controller = c;
			if (targetRect == null && controller != null)
			{
				var t = controller.transform.Find("CloseupImage");
				if (t is RectTransform rtr)
				{
					targetRect = rtr;
					ApplyNormalizedAnchors();
				}
			}
		}
	}
}


