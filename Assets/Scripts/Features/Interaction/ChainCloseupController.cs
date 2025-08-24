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

		[Header("문 핫스팟")]
		[SerializeField] private GameObject doorHotspot;  // HotspotMoveUI가 붙은 투명 버튼
		[SerializeField] private GameObject closeupRootToDestroy; // 전환 전 제거할 루트(프리팹)

		[Header("상태 저장")]
		[SerializeField] private string stateKey = "Stage1_ChainUnlocked";

		private int clickCount;
		private bool unlocked;
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
			LoadStateAndApply();
		}

		public void OnChainClicked()
		{
			if (unlocked) return;
			clickCount++;
			if (chainAnimator != null) chainAnimator.SetTrigger(shakeTrigger);
			if (clickCount >= Mathf.Max(1, requiredClicks))
			{
				StartCoroutine(BreakRoutine());
			}
		}

		private IEnumerator BreakRoutine()
		{
			if (chainAnimator != null) chainAnimator.SetTrigger(breakTrigger);
			// 애니 이벤트가 없으면 폴백으로 대기
			yield return new WaitForSecondsRealtime(Mathf.Max(0f, breakAnimSeconds));
			ApplyUnlocked();
			SaveState();
		}

		public void OnBreakAnimationCompleted() // 애니메이션 이벤트로 직접 호출 가능
		{
			ApplyUnlocked();
			SaveState();
		}

		private void ApplyUnlocked()
		{
			unlocked = true;
			if (closeupImage != null && bgUnlockedSprite != null)
			{
				closeupImage.sprite = bgUnlockedSprite;
			}
			if (chainRoot != null) chainRoot.SetActive(false);
			if (doorHotspot != null) doorHotspot.SetActive(true);
		}

		private void LoadStateAndApply()
		{
			unlocked = PlayerPrefs.GetInt(stateKey, 0) == 1;
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
				if (closeupImage != null && bgLockedSprite != null)
				{
					closeupImage.sprite = bgLockedSprite;
				}
				if (chainRoot != null) chainRoot.SetActive(true);
				if (doorHotspot != null) doorHotspot.SetActive(false);
			}
		}

		private void SaveState()
		{
			PlayerPrefs.SetInt(stateKey, unlocked ? 1 : 0);
			PlayerPrefs.Save();
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
		#endregion
	}
}


