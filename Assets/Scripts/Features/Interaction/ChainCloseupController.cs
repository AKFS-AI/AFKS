using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace AFKS.Features.Interaction
{
	/// <summary>
	/// CloseupCanvas 프리팹 안에서 체인 해금 흐름을 제어합니다.
	/// - BG 잠금/해제 스프라이트 교체
	/// - 체인 클릭 누적/흔들림/파괴 애니메이션
	/// - 파괴 후 문(HotspotMoveUI) 활성화
	/// - 상태는 PlayerPrefs 키(stateKey)로 저장/복원
	/// </summary>
	[AddComponentMenu("AFKS/Interaction/Chain Closeup Controller")]
	public sealed class ChainCloseupController : MonoBehaviour
	{
		#region 필드
		[Header("배경 이미지")]
		[SerializeField] private Image closeupImage;
		[SerializeField] private Sprite bgLockedSprite;   // BG1_1
		[SerializeField] private Sprite bgUnlockedSprite; // BG1_2

		[Header("체인 오브젝트/애니메이션")]
		[SerializeField] private GameObject chainRoot;    // 체인 UI 루트(보이기/숨기기)
		[SerializeField] private Animator chainAnimator;  // 체인 애니메이터
		[SerializeField] private string shakeTrigger = "Shake";
		[SerializeField] private string breakTrigger = "Break";
		[SerializeField] private int requiredClicks = 3;
		[SerializeField] private float breakAnimSeconds = 0.8f; // 애니메이션 이벤트가 없을 때 폴백 대기시간
		[SerializeField] private float shakeLockSeconds = 0.2f; // 흔들림 연출 중 중복 클릭 잠금 시간

		[Header("체인 낙하 연출(폴백)")]
		[SerializeField] private RectTransform chainTopRect;    // 선택: 상단 조각
		[SerializeField] private RectTransform chainBottomRect; // 선택: 하단 조각
		[SerializeField] private float dropSeconds = 0.6f;
		[SerializeField] private float floorNormalizedY = -0.45f; // 부모 높이 기준 -0.45 지점(화면 하단 근처)
		[SerializeField] private float horizontalJitter = 40f;
		[SerializeField] private float rotateDegrees = 35f;
		[SerializeField] private AnimationCurve dropCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

		[Header("목표 지점(Image 좌표계, 0~1)")]
		[SerializeField] private bool useImageSpaceTargets = true;
		[SerializeField] private Vector2 topTargetN01 = new Vector2(0.48f, 0.12f);
		[SerializeField] private Vector2 bottomTargetN01 = new Vector2(0.52f, 0.12f);

		[Header("문 핫스팟")]
		[SerializeField] private GameObject doorHotspot;  // HotspotMoveUI가 붙은 투명 버튼
		[SerializeField] private GameObject closeupRootToDestroy; // 전환 전 제거할 루트(프리팹)

		[Header("체인 핫스팟(UI 클릭 영역)")]
		[SerializeField] private GameObject chainHotspot; // 해제 후 비활성화

		[Header("상태 저장")]
		[SerializeField] private string stateKey = "Stage1_ChainUnlocked";
		[SerializeField]
		[Tooltip("에디터에서 Play 시작 시 저장 상태를 무시하고 항상 잠금 상태로 시작합니다.")]
		private bool startLockedInEditor = true;

		private int clickCount;
		private bool unlocked;
		public bool IsUnlocked => unlocked;
		// 동시 실행 방지 가드
		private bool breakStarted;
		private bool breakCompleted;
		private bool isAnimating; // 흔들림 또는 브레이크 연출 중
		#endregion

		private void Awake()
		{
			// 문 핫스팟은 초기 비활성화
			if (doorHotspot != null) doorHotspot.SetActive(false);
			// 문 핫스팟에 closeupRootToDestroy 주입(있다면)
			var moveUI = doorHotspot != null ? doorHotspot.GetComponent<HotspotMoveUI>() : null;
			if (moveUI != null && closeupRootToDestroy == null)
			{
				// 자동으로 루트 연결 시도
				moveUI.SendMessage("Awake", SendMessageOptions.DontRequireReceiver);
			}
			// 에디터에서는 기본적으로 항상 잠금 상태로 시작(테스트 안정화)
			#if UNITY_EDITOR
			if (startLockedInEditor)
			{
				ApplyLocked();
				unlocked = false;
				return;
			}
			#endif
			LoadStateAndApply();
		}

		public void OnChainClicked()
		{
			if (unlocked) return;
			// 연출 중에는 추가 클릭 무시(흔들림/브레이크/낙하 포함)
			if (isAnimating || breakStarted) return;
			clickCount++;
			if (chainAnimator != null) chainAnimator.SetTrigger(shakeTrigger);
			// 흔들림 연출 동안 입력 잠금
			if (shakeLockSeconds > 0f) StartCoroutine(ShakeLockRoutine(shakeLockSeconds));
			if (clickCount >= Mathf.Max(1, requiredClicks))
			{
				if (!breakStarted)
				{
					StartCoroutine(BreakRoutine());
				}
			}
		}

		private IEnumerator BreakRoutine()
		{
			breakStarted = true;
			isAnimating = true;
			if (chainAnimator != null) chainAnimator.SetTrigger(breakTrigger);
			// 애니 이벤트가 없으면 폴백으로 대기
			yield return new WaitForSecondsRealtime(Mathf.Max(0f, breakAnimSeconds));
			// 애니 이벤트가 먼저 완료되었으면 종료
			if (breakCompleted) yield break;
			// 낙하 연출(체인 조각 RectTransform이 지정된 경우)
			bool hasPieces = chainRoot != null && (chainTopRect != null || chainBottomRect != null);
			if (hasPieces)
			{
				yield return StartCoroutine(DropPiecesRoutine());
			}
			ApplyUnlocked();
			SaveState();
			breakCompleted = true;
			isAnimating = false;
		}

		public void OnBreakAnimationCompleted() // 애니메이션 이벤트로 직접 호출 가능
		{
			if (breakCompleted) return;
			breakStarted = true;
			isAnimating = true;
			// 애니메이션 이벤트가 오면 즉시 낙하 연출을 시도
			if (chainRoot != null && (chainTopRect != null || chainBottomRect != null))
			{
				StartCoroutine(DropPiecesThenUnlock());
			}
			else
			{
				ApplyUnlocked();
				SaveState();
				breakCompleted = true;
				isAnimating = false;
			}
		}

		private void ApplyUnlocked()
		{
			unlocked = true;
			if (closeupImage != null && bgUnlockedSprite != null)
			{
				closeupImage.sprite = bgUnlockedSprite;
			}
			if (chainRoot != null) chainRoot.SetActive(false);
			if (chainHotspot != null) chainHotspot.SetActive(false); // 해제 후 더 이상 클릭되지 않도록
			if (doorHotspot != null) doorHotspot.SetActive(true);
		}

		private void LoadStateAndApply()
		{
			if (AFKS.Core.Services.ServiceLocator.TryGet<AFKS.Core.Services.Save.ISaveService>(out var save))
			{
				unlocked = save.GetBool(stateKey, false);
			}
			else
			{
				unlocked = PlayerPrefs.GetInt(stateKey, 0) == 1;
			}
			if (unlocked)
			{
				// 해제 상태 즉시 반영
				if (closeupImage != null && bgUnlockedSprite != null)
				{
					closeupImage.sprite = bgUnlockedSprite;
				}
				if (chainRoot != null) chainRoot.SetActive(false);
				if (doorHotspot != null) doorHotspot.SetActive(true);
			}
			else
			{
				ApplyLocked();
			}
		}

		private void ApplyLocked()
		{
			if (closeupImage != null && bgLockedSprite != null)
			{
				closeupImage.sprite = bgLockedSprite;
			}
			if (chainRoot != null) chainRoot.SetActive(true);
			if (doorHotspot != null) doorHotspot.SetActive(false);
		}

		private void SaveState()
		{
			if (AFKS.Core.Services.ServiceLocator.TryGet<AFKS.Core.Services.Save.ISaveService>(out var save))
			{
				save.SetBool(stateKey, unlocked);
			}
			else
			{
				PlayerPrefs.SetInt(stateKey, unlocked ? 1 : 0);
				PlayerPrefs.Save();
			}
		}

		#region 에디터/런타임 와이어링 지원 API
		public void SetCloseupImage(Image image)
		{
			closeupImage = image;
		}

		public void SetChain(GameObject root, Animator animator)
		{
			chainRoot = root;
			chainAnimator = animator;
		}

		public void SetChainParts(RectTransform top, RectTransform bottom)
		{
			chainTopRect = top;
			chainBottomRect = bottom;
		}

		public void SetDoor(GameObject door, GameObject rootToDestroy)
		{
			doorHotspot = door;
			closeupRootToDestroy = rootToDestroy;
			var move = doorHotspot != null ? doorHotspot.GetComponent<HotspotMoveUI>() : null;
			if (move != null && move != null && rootToDestroy != null)
			{
				// Awake 전에 호출될 수 있으니 직접 필드도 반영
				var field = typeof(HotspotMoveUI).GetField("closeupRootToDestroy", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
				if (field != null) field.SetValue(move, rootToDestroy);
			}
		}

		public void SetChainHotspot(GameObject hotspot)
		{
			chainHotspot = hotspot;
		}

		public void SetBackgroundSprites(Sprite locked, Sprite unlocked)
		{
			if (locked != null) bgLockedSprite = locked;
			if (unlocked != null) bgUnlockedSprite = unlocked;
		}

		private IEnumerator DropPiecesThenUnlock()
		{
			yield return StartCoroutine(DropPiecesRoutine());
			ApplyUnlocked();
			SaveState();
			breakCompleted = true;
			isAnimating = false;
		}

		private IEnumerator DropPiecesRoutine()
		{
			if (chainRoot == null) yield break;
			var parent = chainRoot.transform as RectTransform;
			if (parent == null) yield break;
			float floorY = Mathf.Clamp(floorNormalizedY, -0.49f, -0.2f) * parent.rect.height; // 로컬 기준 타깃 Y(폴백)
			// 목표 지점을 CloseupImage의 실제 표시 영역 기준으로 변환(선택)
			Vector2? topLocal = null;
			Vector2? bottomLocal = null;
			if (useImageSpaceTargets && closeupImage != null)
			{
				topLocal = ImageN01ToLocal(topTargetN01, parent, closeupImage.rectTransform);
				bottomLocal = ImageN01ToLocal(bottomTargetN01, parent, closeupImage.rectTransform);
			}
			// 병렬 낙하
			bool topDone = chainTopRect == null;
			bool bottomDone = chainBottomRect == null;
			if (!topDone)
			{
				var t = topLocal ?? new Vector2(-horizontalJitter, floorY);
				if (!topLocal.HasValue) t.x = -horizontalJitter; // 폴백 좌우 분리
				StartCoroutine(DropSingle(chainTopRect, t, rotateDegrees));
			}
			if (!bottomDone)
			{
				var t = bottomLocal ?? new Vector2(horizontalJitter, floorY);
				if (!bottomLocal.HasValue) t.x = horizontalJitter; // 폴백 좌우 분리
				StartCoroutine(DropSingle(chainBottomRect, t, -rotateDegrees));
			}
			// 완료 대기
			float elapsed = 0f;
			while (!(topDone && bottomDone))
			{
				elapsed += Time.unscaledDeltaTime;
				if (!topDone && chainTopRect == null) topDone = true;
				if (!bottomDone && chainBottomRect == null) bottomDone = true;
				if (elapsed > dropSeconds + 0.25f) break; // 안전 타임아웃
				yield return null;
			}
		}

		private IEnumerator DropSingle(RectTransform piece, Vector2 targetLocal, float rotate)
		{
			if (piece == null) yield break;
			var startPos = piece.anchoredPosition;
			var startRot = piece.localEulerAngles.z;
			float t = 0f;
			while (t < 1f)
			{
				t += Mathf.Clamp01(Time.unscaledDeltaTime / Mathf.Max(0.001f, dropSeconds));
				float k = dropCurve != null ? dropCurve.Evaluate(t) : t;
				var y = Mathf.Lerp(startPos.y, targetLocal.y, k);
				var x = Mathf.Lerp(startPos.x, targetLocal.x, k);
				piece.anchoredPosition = new Vector2(x, y);
				piece.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(startRot, startRot + rotate, k));
				yield return null;
			}
			piece.anchoredPosition = targetLocal;
			piece.localRotation = Quaternion.Euler(0f, 0f, startRot + rotate);
		}

		// CloseupImage의 표시 영역에서 정규화 좌표(0~1)를 chainRoot(부모) 로컬 좌표로 변환
		private static Vector2 ImageN01ToLocal(Vector2 n01, RectTransform parent, RectTransform image)
		{
			var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(parent, image);
			var min = (Vector2)bounds.min;
			var size = (Vector2)bounds.size;
			return new Vector2(min.x + n01.x * size.x, min.y + n01.y * size.y);
		}

		private IEnumerator ShakeLockRoutine(float seconds)
		{
			isAnimating = true;
			yield return new WaitForSecondsRealtime(Mathf.Max(0.01f, seconds));
			// 브레이크가 시작되었다면 해제는 브레이크 완료 후에 수행됨
			if (!breakStarted) isAnimating = false;
		}
		#endregion
	}
}


