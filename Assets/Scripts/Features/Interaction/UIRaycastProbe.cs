using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AFKS.Features.Interaction
{
	/// <summary>
	/// 클릭 시 UI 레이캐스트 결과 상위 N개를 로그로 출력하는 진단 도구입니다.
	/// 패널에 부착하여 어느 UI가 클릭을 가로채는지 빠르게 확인할 수 있습니다.
	/// </summary>
	[AddComponentMenu("AFKS/Interaction/UI Raycast Probe (Diagnostic)")]
	public sealed class UIRaycastProbe : MonoBehaviour
	{
		[SerializeField, Tooltip("클릭 시 상위 몇 개의 히트를 출력할지")]
		private int topCount = 8;

		[SerializeField, Tooltip("로그 출력 활성/비활성")]
		private bool enabledLogging = true;

		private void Update()
		{
			if (!enabledLogging) return;
			if (!Input.GetMouseButtonDown(0)) return;
			var es = EventSystem.current;
			if (es == null)
			{
				Debug.LogWarning("[UIRaycastProbe] No EventSystem found.");
				return;
			}
			var data = new PointerEventData(es) { position = Input.mousePosition };
			var results = new List<RaycastResult>(16);
			es.RaycastAll(data, results);
			var sb = new StringBuilder(256);
			sb.Append("[UIRaycastProbe] hits(").Append(results.Count).Append("):");
			int count = Mathf.Min(topCount, results.Count);
			for (int i = 0; i < count; i++)
			{
				var go = results[i].gameObject;
				if (go == null) continue;
				sb.Append("\n  ").Append(i + 1).Append(") ").Append(GetPath(go.transform));
				var img = go.GetComponent<Image>();
				if (img != null) sb.Append("  [Image.raycastTarget=").Append(img.raycastTarget).Append("]");
			}
			Debug.Log(sb.ToString());
		}

		private static string GetPath(Transform tr)
		{
			if (tr == null) return "<null>";
			var sb = new StringBuilder(64);
			while (tr != null)
			{
				sb.Insert(0, "/" + tr.name);
				tr = tr.parent;
			}
			return sb.ToString();
		}
	}
}


