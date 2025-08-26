using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AFKS.Core.Events;

namespace AFKS.Core.Systems
{
	public sealed class ZoomView : MonoBehaviour
	{
		private RectTransform _root;
		private Image _image;
		private Text _caption;
		private Button _close;
		private Button _pickup;

		private readonly Dictionary<string, Data.ZoomDefinition> _zoomById = new Dictionary<string, Data.ZoomDefinition>();
		private AFKS.Core.Services.EventBus _bus;
		private AFKS.Core.Services.InventoryService _inventory;

		public void Inject(AFKS.Core.Services.EventBus bus, AFKS.Core.Services.InventoryService inventory)
		{
			_bus = bus; _inventory = inventory;
			_bus.Subscribe<AFKS.Core.Systems.SequenceSystem.ShowZoomRequest>(e => Show(e.zoomId));
		}

		public void InitializeUi()
		{
			_root = gameObject.GetComponent<RectTransform>();
			_root.anchorMin = Vector2.zero; _root.anchorMax = Vector2.one; _root.offsetMin = Vector2.zero; _root.offsetMax = Vector2.zero;

			var bg = new GameObject("Background");
			bg.transform.SetParent(transform, false);
			var bgImg = bg.AddComponent<Image>();
			bgImg.color = new Color(0, 0, 0, 0.6f);
			var bgRt = bg.GetComponent<RectTransform>();
			bgRt.anchorMin = Vector2.zero; bgRt.anchorMax = Vector2.one; bgRt.offsetMin = Vector2.zero; bgRt.offsetMax = Vector2.zero;

			var panel = new GameObject("Panel");
			panel.transform.SetParent(transform, false);
			var prt = panel.AddComponent<RectTransform>();
			prt.sizeDelta = new Vector2(900, 600);
			_image = panel.AddComponent<Image>();

			var capGo = new GameObject("Caption");
			capGo.transform.SetParent(panel.transform, false);
			_caption = capGo.AddComponent<Text>();
			_caption.alignment = TextAnchor.UpperCenter;
			_caption.raycastTarget = false;
			_caption.color = Color.white;
			var crt = capGo.GetComponent<RectTransform>();
			crt.anchorMin = new Vector2(0, 0); crt.anchorMax = new Vector2(1, 0);
			crt.pivot = new Vector2(0.5f, 0);
			crt.sizeDelta = new Vector2(0, 80);
			crt.anchoredPosition = new Vector2(0, -20);

			var closeGo = new GameObject("Close");
			closeGo.transform.SetParent(panel.transform, false);
			_close = closeGo.AddComponent<Button>();
			var closeImg = closeGo.AddComponent<Image>();
			closeImg.color = new Color(0, 0, 0, 0.8f);
			var brt = closeGo.GetComponent<RectTransform>();
			brt.anchorMin = new Vector2(1, 1); brt.anchorMax = new Vector2(1, 1); brt.pivot = new Vector2(1, 1);
			brt.sizeDelta = new Vector2(80, 40);
			brt.anchoredPosition = new Vector2(-10, -10);
			_close.onClick.AddListener(Hide);

			var pickGo = new GameObject("Pickup");
			pickGo.transform.SetParent(panel.transform, false);
			_pickup = pickGo.AddComponent<Button>();
			var pickImg = pickGo.AddComponent<Image>();
			pickImg.color = new Color(0, 0, 0, 0.8f);
			var prtBtn = pickGo.GetComponent<RectTransform>();
			prtBtn.anchorMin = new Vector2(0, 0); prtBtn.anchorMax = new Vector2(0, 0); prtBtn.pivot = new Vector2(0, 0);
			prtBtn.sizeDelta = new Vector2(140, 40);
			prtBtn.anchoredPosition = new Vector2(10, 10);

			Hide();
		}

		public void RegisterZooms(IEnumerable<Data.ZoomDefinition> zooms)
		{
			_zoomById.Clear();
			foreach (var z in zooms) _zoomById[z.id] = z;
		}

		public void Show(string zoomId)
		{
			if (!_zoomById.TryGetValue(zoomId, out var def)) return;
			_image.sprite = def.sprite;
			_caption.text = def.caption;
			_pickup.onClick.RemoveAllListeners();
			_pickup.gameObject.SetActive(!string.IsNullOrEmpty(def.canPickupItemId));
			if (!string.IsNullOrEmpty(def.canPickupItemId))
			{
				_pickup.onClick.AddListener(() => {
					if (_inventory != null)
					{
						_inventory.TryAdd(new AFKS.Core.Services.ItemId(def.canPickupItemId));
					}
					Hide();
				});
			}
			gameObject.SetActive(true);
		}

		public void Hide()
		{
			gameObject.SetActive(false);
		}
	}
}


