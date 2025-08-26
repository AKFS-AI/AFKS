using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AFKS.Core.Composition
{
	/// <summary>
	/// Central composition root that creates and wires global services and systems.
	/// Lives across scenes via DontDestroyOnLoad.
	/// </summary>
	public sealed class GameRuntime : MonoBehaviour
	{
		public static GameRuntime Instance { get; private set; }

		// Services (pure C#)
		public Services.EventBus EventBus { get; private set; }
		public Services.InventoryService Inventory { get; private set; }
		public Services.ProgressService Progress { get; private set; }
		public Services.SaveService Save { get; private set; }

		// Systems (MonoBehaviours)
		public Systems.StageController StageController { get; private set; }
		public Systems.InteractionSystem InteractionSystem { get; private set; }
		public Systems.ZoomView ZoomView { get; private set; }
		public Systems.JumpscareSystem Jumpscare { get; private set; }
		public Systems.AudioSystem AudioSystem { get; private set; }
		public Systems.SequenceSystem SequenceSystem { get; private set; }

		[SerializeField] private Camera _uiCamera;
		[SerializeField] private Canvas _uiCanvas;
		[SerializeField] private RectTransform _uiRoot;
		[SerializeField] private Data.ProjectConfig _projectConfig;
		[SerializeField] private Data.AudioDatabase _audioDatabase;

		private void Awake()
		{
			if (Instance != null)
			{
				Destroy(gameObject);
				return;
			}
			Instance = this;
			DontDestroyOnLoad(gameObject);

			// Instantiate services
			EventBus = new Services.EventBus();
			Inventory = new Services.InventoryService(EventBus);
			Progress = new Services.ProgressService(EventBus);
			Save = new Services.SaveService(EventBus, Inventory, Progress);

			// Build basic UI (Canvas + Camera) if not assigned
			EnsureUiHierarchy();

			// Attach systems
			StageController = gameObject.AddComponent<Systems.StageController>();
			InteractionSystem = gameObject.AddComponent<Systems.InteractionSystem>();
			Jumpscare = gameObject.AddComponent<Systems.JumpscareSystem>();
			SequenceSystem = gameObject.AddComponent<Systems.SequenceSystem>();
			AudioSystem = gameObject.AddComponent<Systems.AudioSystem>();

			// Zoom view lives under UI canvas
			var zoomGo = new GameObject("ZoomView", typeof(RectTransform));
			zoomGo.transform.SetParent(_uiRoot, false);
			ZoomView = zoomGo.AddComponent<Systems.ZoomView>();
			ZoomView.InitializeUi();
			ZoomView.Inject(EventBus, Inventory);

			// Initialize systems with services
			StageController.Initialize(EventBus, Inventory, Progress, Save, ZoomView, Jumpscare, AudioSystem, _uiRoot);
			InteractionSystem.Initialize(EventBus, StageController, Inventory, Progress, ZoomView);
			Jumpscare.Initialize(EventBus);
			AudioSystem.Initialize(EventBus);
			SequenceSystem.Initialize(EventBus, StageController, Progress);
			// Provide Audio DB to audio system if assigned
			if (_audioDatabase != null)
			{
				AudioSystem.SetDatabase(_audioDatabase);
			}

			// Load first stage from project config if present
			if (_projectConfig != null && _projectConfig.stages != null && _projectConfig.stages.Length > 0)
			{
				StageController.SetProject(_projectConfig);
				StageController.LoadStage(_projectConfig.stages[0]);
			}
		}

		private void EnsureUiHierarchy()
		{
			if (_uiCanvas != null && _uiRoot != null)
				return;

			var camGo = new GameObject("UICamera");
			camGo.transform.SetParent(transform, false);
			_uiCamera = camGo.AddComponent<Camera>();
			_uiCamera.clearFlags = CameraClearFlags.Depth;
			_uiCamera.orthographic = true;
			_uiCamera.depth = 100;

			var canvasGo = new GameObject("UICanvas", typeof(RectTransform));
			canvasGo.transform.SetParent(transform, false);
			_uiCanvas = canvasGo.AddComponent<Canvas>();
			_uiCanvas.renderMode = RenderMode.ScreenSpaceCamera;
			_uiCanvas.worldCamera = _uiCamera;
			_uiCanvas.sortingOrder = 1000;
			canvasGo.AddComponent<CanvasScaler>();
			canvasGo.AddComponent<GraphicRaycaster>();

			_uiRoot = canvasGo.GetComponent<RectTransform>();
			_uiRoot.anchorMin = Vector2.zero;
			_uiRoot.anchorMax = Vector2.one;
			_uiRoot.offsetMin = Vector2.zero;
			_uiRoot.offsetMax = Vector2.zero;
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		private static void AutoBootstrap()
		{
			if (Instance != null) return;
			var rootGo = new GameObject("GameRuntime");
			rootGo.AddComponent<GameRuntime>();
		}
	}
}


