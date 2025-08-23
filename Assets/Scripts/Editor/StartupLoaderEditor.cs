#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

namespace AFKS.Core.Systems.Editor
{
	/// <summary>
	/// StartupLoader를 위한 커스텀 인스펙터. 빌드 세팅에 등록된 씬을 드롭다운으로 선택할 수 있게 한다.
	/// </summary>
	[CustomEditor(typeof(StartupLoader))]
	public sealed class StartupLoaderEditor : UnityEditor.Editor
	{
		private const string ScenesFolder = "Assets/Scenes";

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			// 속성 참조
			SerializedProperty initialStageIdProp = serializedObject.FindProperty("initialStageId");
			SerializedProperty autoLoadOnStartProp = serializedObject.FindProperty("autoLoadOnStart");

			// Scenes 폴더의 씬을 스캔
			string[] guids = AssetDatabase.FindAssets("t:Scene", new[] { ScenesFolder });
			string[] scenePaths = guids.Select(AssetDatabase.GUIDToAssetPath).ToArray();
			string[] sceneNames = scenePaths
				.Select(p => System.IO.Path.GetFileNameWithoutExtension(p))
				.ToArray();

			EditorGUILayout.LabelField("초기 스테이지 선택", EditorStyles.boldLabel);
			if (sceneNames.Length == 0)
			{
				EditorGUILayout.HelpBox($"{ScenesFolder} 폴더에서 씬을 찾지 못했습니다. 폴더에 씬을 두거나 경로를 조정하세요.", MessageType.Info);
				// 폴백: 문자열 직접 입력 또는 빌드 씬 드롭다운
				string[] buildSceneNames = EditorBuildSettings.scenes.Where(s => s.enabled)
					.Select(s => System.IO.Path.GetFileNameWithoutExtension(s.path)).ToArray();
				if (buildSceneNames.Length > 0)
				{
					int cur = Mathf.Max(0, System.Array.IndexOf(buildSceneNames, initialStageIdProp.stringValue));
					int nxt = EditorGUILayout.Popup(new GUIContent("초기 스테이지(빌드 씬)"), cur, buildSceneNames);
					if (nxt != cur) initialStageIdProp.stringValue = buildSceneNames[nxt];
				}
				else
				{
					EditorGUILayout.PropertyField(initialStageIdProp, new GUIContent("초기 스테이지 ID"));
				}
			}
			else
			{
				int currentIndex = Mathf.Max(0, System.Array.IndexOf(sceneNames, initialStageIdProp.stringValue));
				int nextIndex = EditorGUILayout.Popup(new GUIContent($"초기 스테이지({ScenesFolder})"), currentIndex, sceneNames);
				if (nextIndex != currentIndex)
				{
					initialStageIdProp.stringValue = sceneNames[nextIndex];
				}

				// 선택한 씬을 Ping/보기 버튼
				using (new EditorGUILayout.HorizontalScope())
				{
					if (GUILayout.Button("씬 핑", GUILayout.Width(60)))
					{
						string selPath = scenePaths[nextIndex];
						var obj = AssetDatabase.LoadAssetAtPath<Object>(selPath);
						EditorGUIUtility.PingObject(obj);
					}
				}
			}

			EditorGUILayout.Space(6);
			EditorGUILayout.PropertyField(autoLoadOnStartProp, new GUIContent("시작 시 자동 로드"));

			serializedObject.ApplyModifiedProperties();
		}
	}
}
#endif


