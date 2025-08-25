using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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
		[SerializeField]
		[Tooltip("자동 와이어링 진행 상황을 로그로 출력합니다.")]
		private bool debugLog = false;

		private void OnValidate()
		{
			TryAutoWire();
		}

		private void Reset()
		{
			TryAutoWire();
		}

		private void Awake()
		{
			// 런타임 인스턴스화 시에도 안전하게 와이어링/차단막을 보장
			TryAutoWire();
			EnsureRaycastBlocker();
			EnsureEventSystemStrict();
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

			// 최상위 레이어에서 레이캐스트/정렬을 보장하기 위해 중첩 Canvas를 구성
			var nestedCanvas = GetComponent<Canvas>();
			if (nestedCanvas == null) nestedCanvas = gameObject.AddComponent<Canvas>();
			nestedCanvas.overrideSorting = true;
			// 충분히 높은 순서로 올려 상위 UI(MainMenu 등)를 덮도록 함
			nestedCanvas.sortingOrder = 32760; // short.MaxValue 근처 안전값
			if (GetComponent<GraphicRaycaster>() == null)
			{
				gameObject.AddComponent<GraphicRaycaster>();
			}

			// 필수 노드 탐색
			var closeupImageTr = transform.Find("CloseupImage") as RectTransform;
			var closeupImage = closeupImageTr != null ? closeupImageTr.GetComponent<Image>() : null;
			var chainRoot = transform.Find("ChainRoot") != null ? transform.Find("ChainRoot").gameObject : null;
			var chainHotspot = transform.Find("ChainHotspot") != null ? transform.Find("ChainHotspot").gameObject : null;
			var doorHotspot = transform.Find("DoorHotspot") != null ? transform.Find("DoorHotspot").gameObject : null;

			// 배경 스프라이트 자동 바인딩(이름 규칙)
			Sprite autoLocked = null, autoUnlocked = null;
			if (closeupImage != null && closeupImage.sprite != null)
			{
				// CloseupImage 현재 스프라이트를 잠금 기본값으로 가정
				autoLocked = closeupImage.sprite;
			}
			// 리소스/동일 폴더 기준으로 이름 규칙 탐색: *_Locked, *_Unlocked 또는 _L/_U
			// 간단 구현: 하위 자식 이미지들에서 이름 키워드로 스프라이트 수집
			var images = GetComponentsInChildren<Image>(true);
			for (int i = 0; i < images.Length; i++)
			{
				var sp = images[i].sprite;
				if (sp == null) continue;
				var n = sp.name.ToLowerInvariant();
				if (n.Contains("locked") || n.EndsWith("_l") || n.EndsWith("_lock")) autoLocked = sp;
				if (n.Contains("unlocked") || n.EndsWith("_u") || n.EndsWith("_unlock")) autoUnlocked = sp;
			}

			// 컨트롤러 보장 및 주입
			var controller = GetComponent<ChainCloseupController>();
			if (controller == null) controller = gameObject.AddComponent<ChainCloseupController>();
			// 필드 연결(존재할 때만) - 에디터 API 없이 직접 주입
			if (closeupImage != null)
			{
				controller.SetCloseupImage(closeupImage);
				controller.SetBackgroundSprites(autoLocked, autoUnlocked);
				// 배경 이미지는 클릭을 가로채지 않도록 RaycastTarget 해제
				if (closeupImage.raycastTarget)
				{
					closeupImage.raycastTarget = false;
				}
				if (debugLog)
				{
					Debug.Log($"[CloseupPanelAutoWire] CloseupImage wired. BG locked={(autoLocked!=null?autoLocked.name:"null")} unlocked={(autoUnlocked!=null?autoUnlocked.name:"null")} ");
				}
			}
			if (chainRoot != null)
			{
				var anim = chainRoot.GetComponent<Animator>();
				if (anim == null) anim = chainRoot.AddComponent<Animator>();
				controller.SetChain(chainRoot, anim);
				// 체인 그래픽은 클릭을 가로채지 않도록 레이캐스트 비활성화
				var chainImages = chainRoot.GetComponentsInChildren<Image>(true);
				for (int i = 0; i < chainImages.Length; i++)
				{
					if (chainImages[i] != null) chainImages[i].raycastTarget = false;
				}
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
				// 반드시 레이캐스트 가능한 투명 이미지 부착
				var img = chainHotspot.GetComponent<Image>();
				if (img == null) img = chainHotspot.AddComponent<Image>();
				img.color = new Color(0f, 0f, 0f, 0f);
				img.raycastTarget = true;
				// 형제 순서: 차단막보다 뒤, 문핫스팟보다 앞
				var rt = chainHotspot.transform as RectTransform;
				if (rt != null) rt.SetSiblingIndex(transform.childCount - 2);
				if (debugLog) Debug.Log("[CloseupPanelAutoWire] ChainHotspotUI wired.");
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
				// 형제 순서: RaycastBlocker(0) < CloseupImage < ChainRoot < ChainHotspot < DoorHotspot
				// 보이는 UI가 핫스팟을 가리지 않도록 핫스팟들을 뒤쪽 시블링으로 이동
				var rt = doorHotspot.transform as RectTransform;
				if (rt != null) rt.SetSiblingIndex(transform.childCount - 1);
				if (debugLog) Debug.Log("[CloseupPanelAutoWire] DoorHotspot wired.");
			}

			// 패널 활성 시 월드 클릭 차단용 입력락 컴포넌트 보장
			var locker = GetComponent<InputLockWhileActive>();
			if (locker == null) gameObject.AddComponent<InputLockWhileActive>();

			// CloseupImage 스트레치 보정
			if (closeupImageTr != null)
			{
				closeupImageTr.anchorMin = Vector2.zero;
				closeupImageTr.anchorMax = Vector2.one;
				closeupImageTr.offsetMin = Vector2.zero;
				closeupImageTr.offsetMax = Vector2.zero;
			}
		}

		/// <summary>
		/// 패널 전체에 투명 레이캐스트 차단막을 추가해 월드 오브젝트 클릭을 확실히 차단합니다.
		/// 자식 UI가 있는 영역에서는 자식이 우선 처리되고, 빈 영역에서는 차단막이 처리합니다.
		/// </summary>
		private void EnsureRaycastBlocker()
		{
			var rootRt = transform as RectTransform;
			if (rootRt == null) return;

			var blocker = transform.Find("RaycastBlocker") as RectTransform;
			if (blocker == null)
			{
				var go = new GameObject("RaycastBlocker");
				go.transform.SetParent(transform, false);
				blocker = go.AddComponent<RectTransform>();
				var img = go.AddComponent<Image>();
				img.color = new Color(0f, 0f, 0f, 0f);
				img.raycastTarget = true;
				// 뒤쪽에 배치(자식 UI가 우선 클릭되도록 가장 아래 시블링)
				blocker.SetSiblingIndex(0);
			}

			blocker.anchorMin = Vector2.zero;
			blocker.anchorMax = Vector2.one;
			blocker.offsetMin = Vector2.zero;
			blocker.offsetMax = Vector2.zero;
		}

		private static void EnsureEventSystemStrict()
		{
			// 절대 자동 생성 금지(싱글톤 정책). 존재 여부만 확인하고 없으면 경고만 출력.
			var all = Object.FindObjectsByType<EventSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
			if (all != null && all.Length > 0)
			{
				// 하나 이상 있으면 추가 작업 불필요. (중복은 Unity가 경고하고, 제작자가 정리하도록 유도)
				return;
			}
			// 생성하지 않음: 편집/런타임 모두 경고만
			Debug.LogWarning("[CloseupPanelAutoWire] EventSystem not found in scene. Please add one to your UI root.");
		}
	}
}


