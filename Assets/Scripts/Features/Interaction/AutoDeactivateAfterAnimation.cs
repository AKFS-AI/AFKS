using UnityEngine;

namespace AFKS.Features.Interaction
{
	/// <summary>
	/// 애니메이션이 끝나면 자동으로 비활성화(또는 파괴)합니다.
	/// - 애니메이션 이벤트로 OnAnimationEnd 호출하거나, 수동 호출도 가능
	/// </summary>
	[AddComponentMenu("AFKS/Interaction/Auto Deactivate After Animation")]
	public sealed class AutoDeactivateAfterAnimation : MonoBehaviour
	{
		[SerializeField] private bool destroyInstead;
		[SerializeField] private float delaySeconds;

		public void OnAnimationEnd()
		{
			if (destroyInstead)
			{
				Destroy(gameObject, Mathf.Max(0f, delaySeconds));
			}
			else
			{
				Invoke(nameof(Deactivate), Mathf.Max(0f, delaySeconds));
			}
		}

		private void Deactivate()
		{
			gameObject.SetActive(false);
		}
	}
}


