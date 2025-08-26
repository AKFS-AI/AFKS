using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AFKS.Core.Services;
using AFKS.Core.Events;

namespace AFKS.Core.Systems
{
	public sealed class StageController : MonoBehaviour
	{
		private EventBus _bus;
		private InventoryService _inventory;
		private ProgressService _progress;
		private SaveService _save;
		private ZoomView _zoom;
		private JumpscareSystem _jumpscare;
		private AudioSystem _audio;

		[SerializeField] private Transform _stageRoot;

		private Data.StageDefinition _currentStage;
		private Data.ProjectConfig _project;
		private readonly Dictionary<string, GameObject> _hotspotIdToObject = new Dictionary<string, GameObject>();
		private readonly Dictionary<string, Image> _overlayIdToImage = new Dictionary<string, Image>();
		private RectTransform _uiRoot;
		private Transform _overlayRoot;

		public Data.StageDefinition CurrentDefinition => _currentStage;

		public void Initialize(EventBus bus, InventoryService inventory, ProgressService progress, SaveService save, ZoomView zoom, JumpscareSystem jumpscare, AudioSystem audio, RectTransform uiRoot = null)
		{
			_bus = bus; _inventory = inventory; _progress = progress; _save = save; _zoom = zoom; _jumpscare = jumpscare; _audio = audio; _uiRoot = uiRoot;
			if (_stageRoot == null)
			{
				var go = new GameObject("StageRoot");
				go.transform.SetParent(transform, false);
				_stageRoot = go.transform;
			}
		}

		public void SetProject(Data.ProjectConfig project)
		{
			_project = project;
		}

		public void LoadStage(Data.StageDefinition definition)
		{
			UnloadStage();
			_currentStage = definition;
			_progress.SetStage(definition.stageIndex);

			// Background sprite to full screen quad (SpriteRenderer under StageRoot)
			var bgGo = new GameObject("Background");
			bgGo.transform.SetParent(_stageRoot, false);
			var sr = bgGo.AddComponent<SpriteRenderer>();
			sr.sprite = definition.backgroundSprite;
			sr.sortingOrder = 0;

			// Register zooms for this stage
			_zoom.RegisterZooms(definition.zooms);

			// Build hotspots as invisible UI-free rects using colliderless hit test via InteractionSystem
			foreach (var hs in definition.hotspots)
			{
				var hgo = new GameObject($"Hotspot_{hs.id}");
				hgo.transform.SetParent(_stageRoot, false);
				var hotspot = hgo.AddComponent<StageHotspot>();
				hotspot.Definition = hs;
				_hotspotIdToObject[hs.id] = hgo;
			}

			// Build UI overlays
			_overlayIdToImage.Clear();
			if (_uiRoot != null && definition.overlays != null)
			{
				var overlayRoot = new GameObject("StageOverlay", typeof(RectTransform));
				_overlayRoot = overlayRoot.transform;
				var ort = overlayRoot.GetComponent<RectTransform>();
				overlayRoot.transform.SetParent(_uiRoot, false);
				ort.anchorMin = Vector2.zero; ort.anchorMax = Vector2.one; ort.offsetMin = Vector2.zero; ort.offsetMax = Vector2.zero;
				for (int i = 0; i < definition.overlays.Length; i++)
				{
					var od = definition.overlays[i];
					if (od == null) continue;
					var go = new GameObject($"Overlay_{od.id}", typeof(RectTransform));
					go.transform.SetParent(_overlayRoot, false);
					var img = go.AddComponent<Image>();
					img.sprite = od.sprite;
					img.raycastTarget = false;
					var rt = go.GetComponent<RectTransform>();
					rt.anchorMin = new Vector2(od.rect.xMin, od.rect.yMin);
					rt.anchorMax = new Vector2(od.rect.xMax, od.rect.yMax);
					rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
					go.SetActive(od.initiallyVisible);
					_overlayIdToImage[od.id] = img;
				}
			}

			_audio.PlayAmbient(definition.ambientSfx);
		}

		public void UnloadStage()
		{
			_hotspotIdToObject.Clear();
			for (int i = transform.childCount - 1; i >= 0; i--)
			{
				var child = transform.GetChild(i);
				if (child == null) continue;
				Destroy(child.gameObject);
			}
			if (_overlayRoot != null)
			{
				Destroy(_overlayRoot.gameObject);
				_overlayRoot = null;
				_overlayIdToImage.Clear();
			}
			_currentStage = null;
		}

		public bool TryGetHotspotObject(string hotspotId, out GameObject go) => _hotspotIdToObject.TryGetValue(hotspotId, out go);

		public void SetOverlayVisible(string id, bool visible)
		{
			if (_overlayIdToImage.TryGetValue(id, out var img))
				img.gameObject.SetActive(visible);
		}

		public void SwapOverlaySprite(string id, Sprite s)
		{
			if (_overlayIdToImage.TryGetValue(id, out var img))
				img.sprite = s;
		}

		public void LoadStageByIndex(int index)
		{
			if (_project == null || _project.stages == null) return;
			for (int i = 0; i < _project.stages.Length; i++)
			{
				var sd = _project.stages[i];
				if (sd != null && sd.stageIndex == index)
				{
					LoadStage(sd);
					return;
				}
			}
		}
	}

	public sealed class StageHotspot : MonoBehaviour
	{
		public Data.HotspotDefinition Definition { get; set; }
	}
}


