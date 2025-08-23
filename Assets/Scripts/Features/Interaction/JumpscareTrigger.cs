using UnityEngine;
using AFKS.Core.Services;
using AFKS.Core.Services.Input;
using AFKS.Core.Services.Audio;
using AFKS.Core.Events;

namespace AFKS.Features.Interaction
{
	/// <summary>
	/// 클릭 또는 자동 트리거 시 점프스케어 연출(사운드/이벤트)을 실행.
	/// </summary>
	[AddComponentMenu("AFKS/Interaction/Jumpscare Trigger")]
	public sealed class JumpscareTrigger : MonoBehaviour
	{
		#region 필드
		[SerializeField] private bool triggerOnClick = true;
		[SerializeField] private AudioClip sfx;
		[SerializeField] private string jumpscareId;
		[SerializeField] private GameObject visual;
		private IInputService inputService;
		private IAudioService audioService;
		private bool fired;
		#endregion

		#region 유니티 수명주기
		private void Awake()
		{
			ServiceLocator.TryGet<IInputService>(out inputService);
			ServiceLocator.TryGet<IAudioService>(out audioService);
		}

		private void OnEnable()
		{
			if (triggerOnClick && inputService != null) inputService.ObjectClicked += OnObjectClicked;
		}

		private void OnDisable()
		{
			if (triggerOnClick && inputService != null) inputService.ObjectClicked -= OnObjectClicked;
		}
		#endregion

		#region 공개 API
		public void Fire()
		{
			if (fired) return;
			fired = true;
			if (visual != null) visual.SetActive(true);
			if (audioService != null && sfx != null) audioService.PlaySFX(sfx, 1f);
			if (!string.IsNullOrEmpty(jumpscareId)) GameEvents.RaiseJumpScareTriggered(jumpscareId);
		}
		#endregion

		#region 이벤트 핸들러
		private void OnObjectClicked(GameObject clicked)
		{
			if (clicked == gameObject) Fire();
		}
		#endregion
	}
}


