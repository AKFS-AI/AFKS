using UnityEditor;
using UnityEngine;
using AFKS.Core.Composition;
using AFKS.Core.Data;

namespace AFKS.Core.Editor
{
	public sealed class ProjectBootstrapEditor : UnityEditor.EditorWindow
	{
		[MenuItem("AFKS/Auto Setup/Generate Core Scene")] 
		public static void GenerateCoreScene()
		{
			var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene);
			var root = new GameObject("GameRuntime");
			root.AddComponent<GameRuntime>();
			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
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

		[MenuItem("AFKS/Auto Setup/Create Stage Template (FrontGate)")] 
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

		[MenuItem("AFKS/Auto Setup/Generate All Scenes (Core/Menu/Stage1-6)")]
		public static void GenerateAllScenes()
		{
			// Core scene
			GenerateCoreScene();
			var coreScene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
			UnityEditor.SceneManagement.EditorSceneManager.SaveScene(coreScene, "Assets/Scenes/MainCore.unity");

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

			AssetDatabase.Refresh();
		}
	}
}


