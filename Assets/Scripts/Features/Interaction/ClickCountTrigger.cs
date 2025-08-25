using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace AFKS.Features.Interaction
{
	/// <summary>
	/// 월드 오브젝트(콜라이더 필요)를 N회 클릭하면 한 번만 연출을 발동하는 간단 트리거.
	/// - Animator 트리거, 대상 오브젝트 활성화, UnityEvent 호출 지원
	/// - PlayerPrefs 키로 일회성 저장 가능
	/// </summary>
	[AddComponentMenu("AFKS/Interaction/Click Count Trigger (World)")]
	public sealed class ClickCountTrigger : MonoBehaviour
	{
		[SerializeField] private int requiredClicks = 3;
		[SerializeField] private float clickCooldownSeconds = 0.05f;
		[SerializeField] private bool triggerOnlyOnce = true;
		[SerializeField] private string stateKey = "Trigger_Once_Default";
		[SerializeField] private bool persistWithPlayerPrefs = true;

		[Header("Actions")]
		[SerializeField] private Animator targetAnimator;
		[SerializeField] private string animatorTriggerName = "Appear";
		[SerializeField] private GameObject targetToEnable;
		[SerializeField] private UnityEvent onTriggered;

		[Header("Debug")] 
		[SerializeField] private bool debugLog;

		private int currentClicks;
		private bool isCoolingDown;
		private bool hasTriggered;

		private void Awake()
		{
			if (persistWithPlayerPrefs && triggerOnlyOnce)
			{
				hasTriggered = PlayerPrefs.GetInt(stateKey, 0) == 1;
			}
		}

		private void OnMouseDown()
		{
			if (hasTriggered || isCoolingDown) return;
			currentClicks++;
			if (debugLog) Debug.Log($"[ClickCountTrigger] {name} click {currentClicks}/{Mathf.Max(1, requiredClicks)}");
			if (clickCooldownSeconds > 0f) StartCoroutine(Cooldown());
			if (currentClicks >= Mathf.Max(1, requiredClicks))
			{
				Trigger();
			}
		}

		private IEnumerator Cooldown()
		{
			isCoolingDown = true;
			yield return new WaitForSeconds(clickCooldownSeconds);
			isCoolingDown = false;
		}

		private void Trigger()
		{
			if (hasTriggered) return;
			if (targetToEnable != null) targetToEnable.SetActive(true);
			if (targetAnimator != null && !string.IsNullOrEmpty(animatorTriggerName))
			{
				targetAnimator.SetTrigger(animatorTriggerName);
			}
			onTriggered?.Invoke();
			hasTriggered = true;
			if (persistWithPlayerPrefs && triggerOnlyOnce)
			{
				PlayerPrefs.SetInt(stateKey, 1);
				PlayerPrefs.Save();
			}
			if (debugLog) Debug.Log("[ClickCountTrigger] Triggered");
		}
	}
}


