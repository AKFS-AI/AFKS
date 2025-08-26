using System.Collections;
using UnityEngine;
using AFKS.Core.Services;
using AFKS.Core.Events;

namespace AFKS.Core.Systems
{
	public sealed class JumpscareSystem : MonoBehaviour
	{
		private EventBus _bus;

		public void Initialize(EventBus bus)
		{
			_bus = bus;
			_bus.Subscribe<JumpscareEvent>(OnJumpscare);
		}

		private void OnJumpscare(JumpscareEvent evt)
		{
			// Placeholder: could play sequence via UI overlay; keep minimal for now
			StartCoroutine(Flash());
		}

		private IEnumerator Flash()
		{
			var overlay = new GameObject("JumpscareOverlay");
			overlay.transform.SetParent(transform, false);
			var img = overlay.AddComponent<UnityEngine.UI.Image>();
			img.color = Color.white;
			var rt = overlay.GetComponent<RectTransform>();
			rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
			yield return new WaitForSeconds(0.1f);
			Destroy(overlay);
		}
	}
}


