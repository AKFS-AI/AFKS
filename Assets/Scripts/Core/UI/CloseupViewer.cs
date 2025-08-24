using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using AFKS.Features.Items;

namespace AFKS.Core.UI
{
	/// <summary>
	/// 아이템 이미지/텍스트를 전체 화면으로 보여주는 클로즈업 뷰어.
	/// </summary>
	[RequireComponent(typeof(CanvasGroup))]
	[AddComponentMenu("AFKS/UI/Closeup Viewer")]
	public sealed class CloseupViewer : MonoBehaviour, IPointerDownHandler
	{
		#region 필드
		[SerializeField] private CanvasGroup canvasGroup;
		[SerializeField] private Image image;
		[SerializeField] private Text text;
		[SerializeField] private float fadeSeconds = 0.15f;
		[SerializeField] private bool closeOnClick = true;
		[SerializeField] private bool closeOnEsc = true;
		[SerializeField] private bool debugLog = false;
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
			// 자식에 연결된 Image를 자동 바인딩(인스펙터 미연결 대비)
			if (image == null) image = GetComponentInChildren<Image>(true);
			if (image == null) EnsureImageExists();
			var parentCanvas = GetComponentInParent<Canvas>();
			if (debugLog)
			{
				Debug.Log($"[CloseupViewer] Awake active={gameObject.activeInHierarchy}, canvas={parentCanvas}, image={(image!=null)}, text={(text!=null)} renderMode={parentCanvas?.renderMode} sortingOrder={parentCanvas?.sortingOrder}");
			}
			EnsureSelfLayout();
			EnsureImageLayout();
			HideImmediate();
		}

		private void Update()
		{
			if (closeOnEsc && canvasGroup != null && canvasGroup.blocksRaycasts)
			{
				if (Input.GetKeyDown(KeyCode.Escape))
				{
					Hide();
				}
			}
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
			EnsureSelfLayout();
			if (image == null) EnsureImageExists();
			if (image != null)
			{
				image.sprite = sprite;
				// 보이는 상태 보장
				if (!image.enabled) image.enabled = true;
				var c = image.color; c.a = 1f; image.color = c;
				EnsureImageLayout();
			}
			if (text != null) text.text = title ?? string.Empty;
			if (debugLog)
			{
				var rt = image != null ? image.rectTransform : null;
				var size = rt != null ? rt.rect.size : Vector2.zero;
				Debug.Log($"[CloseupViewer] Show(sprite) spriteNull={(sprite==null)} hasImage={(image!=null)} imgSize={size} title='{title}' cull={(image!=null ? image.canvasRenderer.cull : (bool?)null)}");
			}
			// 레이아웃 강제 업데이트 및 사이즈 보정
			Canvas.ForceUpdateCanvases();
			if (image != null)
			{
				var rt = image.rectTransform;
				if (rt.rect.width < 1f || rt.rect.height < 1f)
				{
					var parentCanvas = GetComponentInParent<Canvas>();
					if (parentCanvas != null)
					{
						parentCanvas.overrideSorting = true;
						if (parentCanvas.sortingOrder < 200) parentCanvas.sortingOrder = 200;
					}
					// 부모 크기가 0인 비정상 상태 대비 하드 보정
					rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
					rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
					if (debugLog) Debug.Log($"[CloseupViewer] Rect was zero. Forced stretch fill. NewSize={rt.rect.size}");
				}
			}
			Show();
		}

		public void Show()
		{
			if (debugLog)
			{
				var parentCanvas = GetComponentInParent<Canvas>();
				Debug.Log($"[CloseupViewer] Show() canStart={(isActiveAndEnabled)} parentCanvas={parentCanvas} alpha={canvasGroup?.alpha}");
			}
			if (current != null) StopCoroutine(current);
			current = StartCoroutine(FadeTo(1f, true));
		}

		public void Hide()
		{
			if (debugLog) Debug.Log("[CloseupViewer] Hide()");
			if (current != null) StopCoroutine(current);
			current = StartCoroutine(FadeTo(0f, false));
		}

		public void HideImmediate()
		{
			if (current != null) { StopCoroutine(current); current = null; }
			canvasGroup.alpha = 0f;
			canvasGroup.blocksRaycasts = false;
			canvasGroup.interactable = false;
			if (debugLog) Debug.Log("[CloseupViewer] HideImmediate()");
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

		private void EnsureImageLayout()
		{
			if (image == null) return;
			var rt = image.rectTransform;
			rt.anchorMin = Vector2.zero;
			rt.anchorMax = Vector2.one;
			rt.offsetMin = Vector2.zero;
			rt.offsetMax = Vector2.zero;
			image.preserveAspect = true;
		}

		private void EnsureSelfLayout()
		{
			var rt = GetComponent<RectTransform>();
			if (rt == null) return;
			rt.anchorMin = Vector2.zero;
			rt.anchorMax = Vector2.one;
			rt.offsetMin = Vector2.zero;
			rt.offsetMax = Vector2.zero;
			// 스케일/위치 비정상값 보정
			transform.localScale = Vector3.one;
			rt.anchoredPosition = Vector2.zero;
		}

		private void EnsureImageExists()
		{
			var imgGO = new GameObject("Image");
			imgGO.transform.SetParent(transform, false);
			image = imgGO.AddComponent<Image>();
			image.preserveAspect = true;
			EnsureImageLayout();
		}
		#endregion

		#region 이벤트 시스템
		public void OnPointerDown(PointerEventData eventData)
		{
			if (!closeOnClick) return;
			if (canvasGroup != null && canvasGroup.blocksRaycasts)
			{
				Hide();
			}
		}
		#endregion
	}
}


