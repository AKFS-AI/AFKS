using UnityEngine;

namespace AFKS.Features.Interaction
{
	/// <summary>
	/// Animator(ChainRoot)에 붙어 애니메이션 이벤트를 상위의 ChainCloseupController로 릴레이합니다.
	/// </summary>
	[AddComponentMenu("AFKS/Interaction/Animation Event Relay (Chain)")]
	public sealed class AnimationEventRelay : MonoBehaviour
	{
		[SerializeField] private ChainCloseupController controller;

		private void Awake()
		{
			if (controller == null)
			{
				controller = GetComponentInParent<ChainCloseupController>(true);
			}
		}

		// 애니메이션 클립 이벤트에서 호출: ChainRoot_Chain_Break 마지막 프레임 등
		public void OnBreakAnimationCompleted()
		{
			if (controller != null)
			{
				controller.OnBreakAnimationCompleted();
			}
		}
	}
}


