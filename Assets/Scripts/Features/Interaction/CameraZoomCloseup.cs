using System.Collections;
using UnityEngine;

namespace AFKS.Features.Interaction
{
	/// <summary>
	/// 카메라 오쏘사이즈/포지션을 트윈하여 클로즈업 연출을 제공합니다.
	/// - 2D 오쏘 카메라 전제
	/// - 입력 락 없이 자체 가드(isZooming)로 중복 호출 방지
	/// </summary>
	[AddComponentMenu("AFKS/Interaction/Camera Zoom Closeup")]
	public sealed class CameraZoomCloseup : MonoBehaviour
	{
		#region Fields
		[SerializeField] private Camera targetCamera; // 비워두면 Camera.main
		[SerializeField] private float zoomOrthoSize = 2.8f;
		[SerializeField] private float zoomDuration = 0.5f;
		[SerializeField] private AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);
		[SerializeField] private bool debugLog = false;

		private Vector3 originalPosition;
		private float originalSize;
		private bool isZoomed;
		private bool isZooming;
		#endregion

		private void Awake()
		{
			if (targetCamera == null) targetCamera = Camera.main;
		}

		/// <summary>
		/// 지정한 타깃으로 카메라 줌-인.
		/// </summary>
		public void ZoomIn(Transform focus)
		{
			if (isZooming || isZoomed || targetCamera == null || focus == null) return;
			if (!targetCamera.orthographic)
			{
				if (debugLog) Debug.Log("[CameraZoom] 대상 카메라가 오쏘가 아닙니다.");
				return;
			}
			StopAllCoroutines();
			StartCoroutine(ZoomRoutine(focus.position, zoomOrthoSize, true));
		}

		/// <summary>
		/// 원래 상태로 줌-아웃.
		/// </summary>
		public void ZoomOut()
		{
			if (isZooming || !isZoomed || targetCamera == null) return;
			StopAllCoroutines();
			StartCoroutine(ZoomRoutine(originalPosition, originalSize, false));
		}

		private IEnumerator ZoomRoutine(Vector3 targetPos, float targetSize, bool toZoom)
		{
			isZooming = true;
			if (toZoom)
			{
				originalPosition = targetCamera.transform.position;
				originalSize = targetCamera.orthographicSize;
			}
			Vector3 startPos = targetCamera.transform.position;
			float startSize = targetCamera.orthographicSize;
			float t = 0f;
			while (t < 1f)
			{
				t += Mathf.Clamp01(Time.unscaledDeltaTime / Mathf.Max(0.001f, zoomDuration));
				float k = ease != null ? ease.Evaluate(t) : t;
				// Z는 유지
				var pos = Vector3.Lerp(startPos, new Vector3(targetPos.x, targetPos.y, startPos.z), k);
				var size = Mathf.Lerp(startSize, targetSize, k);
				targetCamera.transform.position = pos;
				targetCamera.orthographicSize = size;
				yield return null;
			}
			targetCamera.transform.position = new Vector3(targetPos.x, targetPos.y, targetCamera.transform.position.z);
			targetCamera.orthographicSize = targetSize;
			isZooming = false;
			isZoomed = toZoom;
			if (debugLog) Debug.Log(toZoom ? "[CameraZoom] ZoomIn 완료" : "[CameraZoom] ZoomOut 완료");
		}
	}
}


