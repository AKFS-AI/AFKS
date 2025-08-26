using UnityEditor;
using UnityEngine;
using AFKS.Core.Composition;
using AFKS.Core.Data;
using UnityEngine.EventSystems;

namespace AFKS.Core.Editor
{
	public sealed class ProjectBootstrapEditor : UnityEditor.EditorWindow
	{
		// Menu hidden; used internally by Setup Wizard
		public static void GenerateCoreScene()
		{
			var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene);
			var root = new GameObject("GameRuntime");
			root.AddComponent<GameRuntime>();
			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
			System.IO.Directory.CreateDirectory("Assets/Scenes");
			UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainCore.unity");
		}

		[MenuItem("AFKS/Auto Setup/Create ProjectConfig + AudioDB")] 
		public static void CreateProjectAssets()
		{
			var config = ScriptableObject.CreateInstance<ProjectConfig>();
			var audio = ScriptableObject.CreateInstance<AudioDatabase>();
			var dir = "Assets/Data/Hospital";
			System.IO.Directory.CreateDirectory(dir);
			AssetDatabase.CreateAsset(config, dir + "/ProjectConfig.asset");
			AssetDatabase.CreateAsset(audio, dir + "/AudioDatabase.asset");
			AssetDatabase.SaveAssets();
			Selection.activeObject = config;
		}

		// Menu hidden; used internally by Setup Wizard
		public static void CreateStageTemplate()
		{
			var stage = ScriptableObject.CreateInstance<StageDefinition>();
			stage.stageIndex = 0;
			stage.overlays = new OverlayObjectDefinition[2];
			stage.overlays[0] = new OverlayObjectDefinition { id = "chain_full", initiallyVisible = true, rect = new Rect(0,0,1,1) };
			stage.overlays[1] = new OverlayObjectDefinition { id = "chain_broken", initiallyVisible = false, rect = new Rect(0,0,1,1) };
			stage.zooms = new ZoomDefinition[1];
			stage.zooms[0] = new ZoomDefinition { id = "gate_zoom", caption = "녹슨 체인이 문을 묶고 있다." };
			stage.hotspots = new HotspotDefinition[1];
			var hs = new HotspotDefinition();
			hs.id = "front_gate_chain";
			hs.rect = new Rect(0.45f, 0.38f, 0.10f, 0.22f);
			hs.sequence = new InteractionSequenceDefinition
			{
				steps = new SequenceStep[]
				{
					new SequenceStep { onClickCount = 1, actions = new[]{ new SequenceAction{ type = ActionType.PlaySfx, a = "lock_click"}, new SequenceAction{ type = ActionType.ShowZoom, a = "gate_zoom"} } },
					new SequenceStep { onClickCount = 3, actions = new[]{ new SequenceAction{ type = ActionType.PlaySfx, a = "chain_strain"} } },
					new SequenceStep { onClickCount = 4, actions = new[]{ new SequenceAction{ type = ActionType.ToggleOverlay, a = "chain_full", v = false }, new SequenceAction{ type = ActionType.ToggleOverlay, a = "chain_broken", v = true }, new SequenceAction{ type = ActionType.PlaySfx, a = "chain_break"}, new SequenceAction{ type = ActionType.SetFlag, a = "GateChainBroken" } } },
					new SequenceStep { onClickCount = 5, conditions = new[]{ new Condition{ type = ConditionType.Flag, key = "GateChainBroken" } }, actions = new[]{ new SequenceAction{ type = ActionType.GoToStage, i = 1 } } },
				}
			};
			stage.hotspots[0] = hs;

			var dir = "Assets/Data/Hospital/Stages";
			System.IO.Directory.CreateDirectory(dir);
			AssetDatabase.CreateAsset(stage, dir + "/Stage_0_FrontGate.asset");
			AssetDatabase.SaveAssets();
			Selection.activeObject = stage;
		}

		// Menu hidden; used internally by Setup Wizard
		public static void GenerateAllScenes()
		{
			// Core scene
			GenerateCoreScene();

			// Main Menu (minimal stub)
			var menu = UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene);
			new GameObject("MainMenu");
			UnityEditor.SceneManagement.EditorSceneManager.SaveScene(menu, "Assets/Scenes/MainMenu.unity");

			// Stage placeholders as separate empty scenes (optional, for editing previews)
			for (int i = 1; i <= 6; i++)
			{
				var st = UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene);
				new GameObject($"Stage{i}_Preview");
				UnityEditor.SceneManagement.EditorSceneManager.SaveScene(st, $"Assets/Scenes/Stage{i}.unity");
			}

			// Open MainCore at the end
			UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/MainCore.unity");
			AssetDatabase.Refresh();
		}

		// --- New: Attach GameRuntime to current scene (preview) ---
		// Menu hidden; used internally by Setup Wizard
		public static void AttachRuntimeToActiveScene()
		{
			var active = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
			if (!active.IsValid()) return;
			EnsureRuntimeInScene(active);
			EnsureServiceGameObjects();
			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(active);
		}

		// Menu hidden; used internally by Setup Wizard
		public static void AttachRuntimeToAllStageScenes()
		{
			AttachRuntimeToAllScenesInFolder("Assets/Scenes");
		}

		// Generic: attach runtime/hierarchy to every scene in folder (recursively)
		public static void AttachRuntimeToAllScenesInFolder(string folder)
		{
			var guids = AssetDatabase.FindAssets("t:Scene", new[] { folder });
			string reopen = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().path;
			for (int i = 0; i < guids.Length; i++)
			{
				var path = AssetDatabase.GUIDToAssetPath(guids[i]);
				if (string.IsNullOrEmpty(path)) continue;
				var scn = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(path);
				EnsureRuntimeInScene(scn);
				EnsureServiceGameObjects();
				UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scn);
			}
			if (!string.IsNullOrEmpty(reopen))
				UnityEditor.SceneManagement.EditorSceneManager.OpenScene(reopen);
		}

		private static void EnsureRuntimeInScene(UnityEngine.SceneManagement.Scene scn)
		{
			var existing = UnityEngine.Object.FindFirstObjectByType<GameRuntime>();
			if (existing == null)
			{
				var root = new GameObject("GameRuntime");
				existing = root.AddComponent<GameRuntime>();
			}
			// Try linking config/audio automatically if they exist
			var config = AssetDatabase.LoadAssetAtPath<ProjectConfig>("Assets/Data/Hospital/ProjectConfig.asset");
			var audio = AssetDatabase.LoadAssetAtPath<AudioDatabase>("Assets/Data/Hospital/AudioDatabase.asset");
			var so = new SerializedObject(existing);
			so.FindProperty("_projectConfig").objectReferenceValue = config;
			so.FindProperty("_audioDatabase").objectReferenceValue = audio;

			// Ensure UI hierarchy exists in the scene (so 런타임이 중복 생성하지 않도록 미리 주입)
			Camera uiCam = null;
			Canvas uiCanvas = null;
			RectTransform uiRoot = null;
			var existingCanvas = Object.FindFirstObjectByType<Canvas>();
			if (existingCanvas != null)
			{
				uiCanvas = existingCanvas;
				uiRoot = existingCanvas.GetComponent<RectTransform>();
				uiCam = existingCanvas.worldCamera;
			}
			if (uiCanvas == null)
			{
				var camGo = new GameObject("UICamera");
				camGo.transform.SetParent(existing.transform, false);
				uiCam = camGo.AddComponent<Camera>();
				uiCam.clearFlags = CameraClearFlags.Depth;
				uiCam.orthographic = true;
				uiCam.depth = 100;

				var canvasGo = new GameObject("UICanvas", typeof(RectTransform));
				canvasGo.transform.SetParent(existing.transform, false);
				uiCanvas = canvasGo.AddComponent<Canvas>();
				uiCanvas.renderMode = RenderMode.ScreenSpaceCamera;
				uiCanvas.worldCamera = uiCam;
				canvasGo.AddComponent<UnityEngine.UI.CanvasScaler>();
				canvasGo.AddComponent<UnityEngine.UI.GraphicRaycaster>();
				uiRoot = canvasGo.GetComponent<RectTransform>();
				uiRoot.anchorMin = Vector2.zero; uiRoot.anchorMax = Vector2.one; uiRoot.offsetMin = Vector2.zero; uiRoot.offsetMax = Vector2.zero;
			}

			// Inject to GameRuntime serialized fields so Awake에서 중복 생성 방지
			so.FindProperty("_uiCamera").objectReferenceValue = uiCam;
			so.FindProperty("_uiCanvas").objectReferenceValue = uiCanvas;
			so.FindProperty("_uiRoot").objectReferenceValue = uiRoot;
			so.ApplyModifiedPropertiesWithoutUndo();

			// Ensure EventSystem exists for UI
			if (Object.FindFirstObjectByType<EventSystem>() == null)
			{
				var es = new GameObject("EventSystem");
				es.AddComponent<EventSystem>();
				es.AddComponent<StandaloneInputModule>();
			}
		}

		private static void EnsureServiceGameObjects()
		{
			// Create visible scene objects that expose services via components
			if (Object.FindFirstObjectByType<AFKS.Core.Mono.InventoryServiceComponent>() == null)
			{
				var inv = new GameObject("InventoryService");
				inv.AddComponent<AFKS.Core.Mono.InventoryServiceComponent>();
			}
			if (Object.FindFirstObjectByType<AFKS.Core.Mono.ProgressServiceComponent>() == null)
			{
				var prog = new GameObject("ProgressService");
				prog.AddComponent<AFKS.Core.Mono.ProgressServiceComponent>();
			}

			// StageRoot + 레이어 구성 + 예시 핫스팟/오버레이 배치
			var stageRoot = GameObject.Find("StageRoot") ?? new GameObject("StageRoot");
			var background = GameObject.Find("Background") ?? new GameObject("Background");
			background.transform.SetParent(stageRoot.transform, false);
			if (background.GetComponent<SpriteRenderer>() == null)
				background.AddComponent<SpriteRenderer>();

			// 예시 핫스팟 1개(정문 중앙). 필요 시 삭제/이동 가능
			if (Object.FindFirstObjectByType<AFKS.Core.Mono.HotspotRect>() == null)
			{
				var hs = new GameObject("Hotspot_front_gate");
				hs.transform.SetParent(stageRoot.transform, false);
				var comp = hs.AddComponent<AFKS.Core.Mono.HotspotRect>();
				comp.id = "front_gate_chain";
				comp.rect = new Rect(0.45f, 0.38f, 0.10f, 0.22f);
			}
		}
	}
}
