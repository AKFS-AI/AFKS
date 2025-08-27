using UnityEngine;
using UnityEngine.UI;

namespace AFKS.Core.UI
{
	/// <summary>
	/// 슬라이더 뒤에 5단계(기본) 가이드 마커를 생성하고 현재 단계를 강조 표시합니다.
	/// - 슬라이더는 wholeNumbers=true, maxValue=steps-1 구성을 권장합니다.
	/// </summary>
	[AddComponentMenu("AFKS/UI/Slider Step Indicator")]
	public sealed class SliderStepIndicator : MonoBehaviour
	{
		[SerializeField] private Slider targetSlider;
		[SerializeField] private int steps = 5;
		[SerializeField] private Color inactiveColor = new Color(1f, 1f, 1f, 0.18f);
		[SerializeField] private Color activeColor = new Color(0.2f, 0.9f, 0.2f, 0.9f);
		[SerializeField] private Vector2 markerSize = new Vector2(14f, 14f);

		private Image[] markerImages;

		private void Awake()
		{
			BuildMarkers();
			Bind();
			UpdateMarkers();
		}

		private void OnEnable()
		{
			Bind();
			UpdateMarkers();
		}

		private void OnDisable()
		{
			if (targetSlider != null)
				targetSlider.onValueChanged.RemoveListener(OnSliderChanged);
		}

		private void Bind()
		{
			if (targetSlider == null) targetSlider = GetComponentInParent<Slider>();
			if (targetSlider != null)
			{
				targetSlider.wholeNumbers = true;
				targetSlider.minValue = 0;
				targetSlider.maxValue = Mathf.Max(1, steps - 1);
				targetSlider.onValueChanged.AddListener(OnSliderChanged);
			}
		}

		private void OnSliderChanged(float _)
		{
			UpdateMarkers();
		}

		private void BuildMarkers()
		{
			// 이미 생성됐으면 스킵
			if (markerImages != null && markerImages.Length == steps) return;

			// 자식 정리
			foreach (Transform child in transform)
			{
				DestroyImmediate(child.gameObject);
			}

			markerImages = new Image[steps];
			for (int i = 0; i < steps; i++)
			{
				var go = new GameObject($"Step_{i}");
				go.transform.SetParent(transform, false);
				var rt = go.AddComponent<RectTransform>();
				rt.anchorMin = new Vector2(0f, 0.5f);
				rt.anchorMax = new Vector2(0f, 0.5f);
				rt.sizeDelta = markerSize;
				float t = steps > 1 ? (float)i / (steps - 1) : 0f;
				rt.anchoredPosition = new Vector2(t * ((transform as RectTransform).rect.width), 0f);
				var img = go.AddComponent<Image>();
				img.color = inactiveColor;
				img.raycastTarget = false;
				markerImages[i] = img;
			}
		}

		private void UpdateMarkers()
		{
			if (markerImages == null || targetSlider == null) return;
			int current = Mathf.RoundToInt(targetSlider.value);
			for (int i = 0; i < markerImages.Length; i++)
			{
				markerImages[i].color = (i == current) ? activeColor : inactiveColor;
			}
		}
	}
}


