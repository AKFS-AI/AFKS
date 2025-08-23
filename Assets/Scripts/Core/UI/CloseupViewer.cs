using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using AFKS.Features.Items;

namespace AFKS.Core.UI
{
	/// <summary>
	/// 아이템 이미지/텍스트를 전체 화면으로 보여주는 클로즈업 뷰어.
	/// </summary>
	[RequireComponent(typeof(CanvasGroup))]
	[AddComponentMenu("AFKS/UI/Closeup Viewer")]
	public sealed class CloseupViewer : MonoBehaviour
	{
		#region 필드
		[SerializeField] private CanvasGroup canvasGroup;
		[SerializeField] private Image image;
		[SerializeField] private Text text;
		[SerializeField] private float fadeSeconds = 0.15f;
		private Coroutine current;
		#endregion

		#region 유니티 수명주기
		private void Reset()
		{
			canvasGroup = GetComponent<CanvasGroup>();
		}

		private void Awake()
		{
			if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
			HideImmediate();
		}
		#endregion

		#region 공개 API
		public void Show(ItemData item)
		{
			if (item != null)
			{
				if (image != null) image.sprite = item.Image;
				if (text != null) text.text = string.IsNullOrEmpty(item.DisplayName) ? item.ItemId : item.DisplayName;
			}
			Show();
		}

		public void Show(Sprite sprite, string title = null)
		{
			if (image != null) image.sprite = sprite;
			if (text != null) text.text = title ?? string.Empty;
			Show();
		}

		public void Show()
		{
			if (current != null) StopCoroutine(current);
			current = StartCoroutine(FadeTo(1f, true));
		}

		public void Hide()
		{
			if (current != null) StopCoroutine(current);
			current = StartCoroutine(FadeTo(0f, false));
		}

		public void HideImmediate()
		{
			if (current != null) { StopCoroutine(current); current = null; }
			canvasGroup.alpha = 0f;
			canvasGroup.blocksRaycasts = false;
			canvasGroup.interactable = false;
		}
		#endregion

		#region 내부 메서드
		private IEnumerator FadeTo(float target, bool block)
		{
			float start = canvasGroup.alpha;
			float t = 0f;
			canvasGroup.blocksRaycasts = block;
			canvasGroup.interactable = block;
			while (t < fadeSeconds)
			{
				t += Time.unscaledDeltaTime;
				float p = fadeSeconds > 0f ? Mathf.Clamp01(t / fadeSeconds) : 1f;
				canvasGroup.alpha = Mathf.Lerp(start, target, p);
				yield return null;
			}
			canvasGroup.alpha = target;
			current = null;
		}
		#endregion
	}
}


