using System.Collections;
using UnityEngine;

namespace AFKS.Features.Interaction
{
	/// <summary>
	/// 월드(2D) 공간 체인 해금 컨트롤러.
	/// - 클릭 누적→Shake→Break→조각 낙하
	/// - 조각은 SpriteRenderer + Rigidbody2D(선택) 또는 스크립트 트윈 낙하
	/// - 상태는 PlayerPrefs key로 저장/복원
	/// </summary>
	[AddComponentMenu("AFKS/Interaction/World Chain Controller")]
	public sealed class WorldChainController : MonoBehaviour
	{
		[SerializeField] private Animator chainAnimator;
		[SerializeField] private string shakeTrigger = "Shake";
		[SerializeField] private string breakTrigger = "Break";
		[SerializeField] private int requiredClicks = 3;
		[SerializeField] private float shakeLockSeconds = 0.2f;
		[SerializeField] private float breakAnimSeconds = 0.8f;
		[SerializeField] private Transform topPiece;
		[SerializeField] private Transform bottomPiece;
		[SerializeField] private Transform floorTarget; // 착지 기준점
		[SerializeField] private float dropSeconds = 0.6f;
		[SerializeField] private AnimationCurve dropCurve = AnimationCurve.EaseInOut(0,0,1,1);
		[SerializeField] private string stateKey = "Stage1_ChainUnlocked";
		[SerializeField] private bool debugLog;

		public System.Action Unlocked; // 언락 완료 이벤트

		private int clickCount;
		private bool unlocked;
		private bool isAnimating;
		private bool breakStarted;
		private bool dropStarted;

		private Vector3 topStartPos;
		private Vector3 bottomStartPos;
		private float topStartRot;
		private float bottomStartRot;

		private void Awake()
		{
			unlocked = PlayerPrefs.GetInt(stateKey, 0) == 1;
			CacheStarts();
			if (unlocked) gameObject.SetActive(false);
		}

		private void CacheStarts()
		{
			if (topPiece != null)
			{
				topStartPos = topPiece.position;
				topStartRot = topPiece.eulerAngles.z;
			}
			if (bottomPiece != null)
			{
				bottomStartPos = bottomPiece.position;
				bottomStartRot = bottomPiece.eulerAngles.z;
			}
		}

		private void OnMouseDown()
		{
			OnChainClicked();
		}

		public void OnChainClicked()
		{
			if (unlocked || isAnimating || breakStarted) return;
			if (debugLog) Debug.Log($"[WorldChain] Click {(clickCount+1)}/{Mathf.Max(1, requiredClicks)}");
			clickCount++;
			if (chainAnimator != null) chainAnimator.SetTrigger(shakeTrigger);
			if (shakeLockSeconds > 0f) StartCoroutine(ShakeLock());
			if (clickCount >= Mathf.Max(1, requiredClicks)) StartCoroutine(StartBreakAfterShake());
		}

		private IEnumerator ShakeLock()
		{
			isAnimating = true;
			yield return new WaitForSeconds(shakeLockSeconds);
			if (!breakStarted) isAnimating = false;
		}

		private IEnumerator StartBreakAfterShake()
		{
			if (debugLog) Debug.Log("[WorldChain] wait shake -> break");
			while (isAnimating) yield return null;
			breakStarted = true;
			isAnimating = true;
			if (chainAnimator != null) chainAnimator.SetTrigger(breakTrigger);
			yield return new WaitForSeconds(Mathf.Max(0f, breakAnimSeconds));
			if (!dropStarted) StartCoroutine(DropThenUnlock());
		}

		public void OnBreakAnimationCompleted() // Animation Event
		{
			if (!dropStarted) StartCoroutine(DropThenUnlock());
		}

		private IEnumerator DropThenUnlock()
		{
			dropStarted = true;
			if (debugLog) Debug.Log("[WorldChain] drop start");
			float t = 0f;
			Vector3 topTarget = topPiece != null ? (floorTarget != null ? new Vector3(topPiece.position.x, floorTarget.position.y, topPiece.position.z) : topPiece.position + Vector3.down * 2f) : Vector3.zero;
			Vector3 bottomTarget = bottomPiece != null ? (floorTarget != null ? new Vector3(bottomPiece.position.x, floorTarget.position.y, bottomPiece.position.z) : bottomPiece.position + Vector3.down * 2f) : Vector3.zero;
			while (t < 1f)
			{
				t += Mathf.Clamp01(Time.deltaTime / Mathf.Max(0.001f, dropSeconds));
				float k = dropCurve != null ? dropCurve.Evaluate(t) : t;
				if (topPiece != null) topPiece.position = Vector3.Lerp(topStartPos, topTarget, k);
				if (bottomPiece != null) bottomPiece.position = Vector3.Lerp(bottomStartPos, bottomTarget, k);
				yield return null;
			}
			if (debugLog) Debug.Log("[WorldChain] drop complete -> unlock");
			unlocked = true;
			PlayerPrefs.SetInt(stateKey, 1);
			PlayerPrefs.Save();
			gameObject.SetActive(false);
			isAnimating = false;
			Unlocked?.Invoke();
		}
	}
}


