using System.Collections;
using UnityEngine;
using AFKS.Core.Services;
using AFKS.Core.Services.Audio;

namespace AFKS.Features.Interaction
{
	/// <summary>
	/// Stage1 전용 오케스트레이션:
	/// - 배경 상태(bgFar, bgNear1~bgNear5) 스프라이트 전환
	/// - 문 클릭 → 카메라 줌/페이드 스텝 이동
	/// - 빈 영역 클릭 → 원거리로 이탈(줌 아웃)
	/// - 체인 5회 클릭 → 브레이크/낙하 후 문 클릭으로 Stage2 이동 허용
	/// </summary>
	[AddComponentMenu("AFKS/Stage/Stage1 Flow Controller")]
	public sealed class Stage1FlowController : MonoBehaviour
	{
		[SerializeField] private CameraZoomCloseup cameraZoom;
		[SerializeField] private Transform doorZoomTarget;
		[SerializeField] private SpriteRenderer backgroundRenderer; // 배경 표시
		[SerializeField] private Sprite bgFar;
		[SerializeField] private Sprite bgNear1;
		[SerializeField] private Sprite bgNear2;
		[SerializeField] private Sprite bgNear3;
		[SerializeField] private Sprite bgNear4;
		[SerializeField] private Sprite bgNear5; // 최종 근접(체인 보이는 장면)

		[SerializeField] private Collider2D doorArea;   // 문 클릭 영역(줌 인 시작)
		[SerializeField] private Collider2D emptyArea;  // 근접 상태에서 빈 곳 클릭(줌 아웃)
		[SerializeField] private WorldChainController worldChain; // 체인 컨트롤러
		[SerializeField] private HotspotMoveWorld doorHotspot;   // 문 클릭 → Stage2
		[SerializeField] private float stepDuration = 0.15f; // 배경 컷 전환 속도
		[SerializeField] private AudioClip[] footstepClips;   // 접근/이탈 연출 발걸음
		[SerializeField] private bool debugLog;

		private int nearStep; // 0=far, 1..5
		private bool chainUnlocked;
		private IAudioService audioService;

		private void Awake()
		{
			if (backgroundRenderer != null && bgFar != null) backgroundRenderer.sprite = bgFar;
			if (worldChain != null) worldChain.Unlocked += OnChainUnlocked;
			if (doorHotspot != null) doorHotspot.gameObject.SetActive(false);
			ServiceLocator.TryGet<IAudioService>(out audioService);
		}

		private void OnDestroy()
		{
			if (worldChain != null) worldChain.Unlocked -= OnChainUnlocked;
		}

		private void OnMouseDown()
		{
			// Collider 겹침 환경에서는 OnMouseDown이 중복될 수 있어 각 영역 콜백 사용을 권장
		}

		public void OnDoorClicked()
		{
			if (chainUnlocked)
			{
				// 언락 후 문 클릭은 바로 Stage2 핫스팟이 처리
				return;
			}
			StartCoroutine(ApproachDoorRoutine());
		}

		public void OnEmptyClicked()
		{
			if (nearStep > 0)
			{
				StartCoroutine(LeaveDoorRoutine());
			}
		}

		private IEnumerator ApproachDoorRoutine()
		{
			if (debugLog) Debug.Log("[Stage1Flow] approach -> step");
			if (cameraZoom != null && doorZoomTarget != null) cameraZoom.ZoomIn(doorZoomTarget);
			// 5장 스텝으로 점진 전환 + 발걸음 SFX
			for (int s = Mathf.Max(1, nearStep + 1); s <= 5; s++)
			{
				nearStep = s; ApplyNearBg();
				PlayFootstep();
				yield return new WaitForSeconds(stepDuration);
			}
		}

		private IEnumerator LeaveDoorRoutine()
		{
			if (debugLog) Debug.Log("[Stage1Flow] leave <-");
			if (cameraZoom != null) cameraZoom.ZoomOut();
			for (int s = Mathf.Max(0, nearStep - 1); s >= 0; s--)
			{
				nearStep = s; ApplyNearBg();
				PlayFootstep();
				yield return new WaitForSeconds(stepDuration);
			}
		}

		private void ApplyNearBg()
		{
			if (backgroundRenderer == null) return;
			var s = nearStep switch
			{
				0 => bgFar,
				1 => bgNear1 ?? bgFar,
				2 => bgNear2 ?? bgNear1 ?? bgFar,
				3 => bgNear3 ?? bgNear2 ?? bgNear1 ?? bgFar,
				4 => bgNear4 ?? bgNear3 ?? bgNear2 ?? bgNear1 ?? bgFar,
				_ => bgNear5 ?? bgNear4 ?? bgNear3 ?? bgNear2 ?? bgNear1 ?? bgFar
			};
			backgroundRenderer.sprite = s;
		}

		private void OnChainUnlocked()
		{
			chainUnlocked = true;
			if (doorHotspot != null) doorHotspot.gameObject.SetActive(true);
			if (debugLog) Debug.Log("[Stage1Flow] chain unlocked -> door hotspot active");
		}

		private void PlayFootstep()
		{
			if (audioService == null || footstepClips == null || footstepClips.Length == 0) return;
			var clip = footstepClips[Random.Range(0, footstepClips.Length)];
			audioService.PlaySFX(clip, 0.8f);
		}
	}
}


