using UnityEditor;
using UnityEngine;
using AFKS.Core.Data;

namespace AFKS.Core.Editor
{
	[CustomEditor(typeof(StageDefinition))]
	public sealed class StageDefinitionEditor : UnityEditor.Editor
	{
		private SerializedProperty _stageIndex;
		private SerializedProperty _backgroundSprite;
		private SerializedProperty _ambientSfx;
		private SerializedProperty _overlays;
		private SerializedProperty _hotspots;
		private SerializedProperty _zooms;
		private SerializedProperty _gimmicks;

		private void OnEnable()
		{
			_stageIndex = serializedObject.FindProperty("stageIndex");
			_backgroundSprite = serializedObject.FindProperty("backgroundSprite");
			_ambientSfx = serializedObject.FindProperty("ambientSfx");
			_overlays = serializedObject.FindProperty("overlays");
			_hotspots = serializedObject.FindProperty("hotspots");
			_zooms = serializedObject.FindProperty("zooms");
			_gimmicks = serializedObject.FindProperty("gimmicks");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField(_stageIndex);
			EditorGUILayout.PropertyField(_backgroundSprite);
			EditorGUILayout.PropertyField(_ambientSfx);

			DrawOverlays();
			DrawHotspots();
			DrawZooms();
			DrawGimmicks();

			serializedObject.ApplyModifiedProperties();
		}

		private void DrawOverlays()
		{
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Overlays", EditorStyles.boldLabel);
			for (int i = 0; i < _overlays.arraySize; i++)
			{
				var elem = _overlays.GetArrayElementAtIndex(i);
				EditorGUILayout.BeginVertical("box");
				EditorGUILayout.PropertyField(elem.FindPropertyRelative("id"));
				EditorGUILayout.PropertyField(elem.FindPropertyRelative("sprite"));
				EditorGUILayout.PropertyField(elem.FindPropertyRelative("rect"));
				EditorGUILayout.PropertyField(elem.FindPropertyRelative("initiallyVisible"));
				EditorGUILayout.PropertyField(elem.FindPropertyRelative("sortingOrder"));
				EditorGUILayout.BeginHorizontal();
				if (GUILayout.Button("▲", GUILayout.Width(28)) && i > 0) _overlays.MoveArrayElement(i, i - 1);
				if (GUILayout.Button("▼", GUILayout.Width(28)) && i < _overlays.arraySize - 1) _overlays.MoveArrayElement(i, i + 1);
				if (GUILayout.Button("-")) _overlays.DeleteArrayElementAtIndex(i);
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.EndVertical();
			}
			if (GUILayout.Button("+ Add Overlay")) _overlays.InsertArrayElementAtIndex(_overlays.arraySize);
		}

		private void DrawHotspots()
		{
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Hotspots", EditorStyles.boldLabel);
			for (int i = 0; i < _hotspots.arraySize; i++)
			{
				var elem = _hotspots.GetArrayElementAtIndex(i);
				EditorGUILayout.BeginVertical("box");
				EditorGUILayout.PropertyField(elem.FindPropertyRelative("id"));
				EditorGUILayout.PropertyField(elem.FindPropertyRelative("rect"));
				EditorGUILayout.PropertyField(elem.FindPropertyRelative("requiredItemIds"), true);
				EditorGUILayout.PropertyField(elem.FindPropertyRelative("setFlags"), true);
				EditorGUILayout.PropertyField(elem.FindPropertyRelative("playSfx"));
				EditorGUILayout.PropertyField(elem.FindPropertyRelative("showZoomId"));
				EditorGUILayout.PropertyField(elem.FindPropertyRelative("goToStageIndex"));
				// Sequence
				var seq = elem.FindPropertyRelative("sequence");
				EditorGUILayout.PropertyField(seq, new UnityEngine.GUIContent("Sequence"), false);
				if (seq.isExpanded)
				{
					var steps = seq.FindPropertyRelative("steps");
					for (int s = 0; s < steps.arraySize; s++)
					{
						var st = steps.GetArrayElementAtIndex(s);
						EditorGUILayout.BeginVertical("box");
						EditorGUILayout.PropertyField(st.FindPropertyRelative("onClickCount"));
						EditorGUILayout.PropertyField(st.FindPropertyRelative("conditions"), true);
						EditorGUILayout.PropertyField(st.FindPropertyRelative("actions"), true);
						EditorGUILayout.BeginHorizontal();
						if (GUILayout.Button("▲", GUILayout.Width(28)) && s > 0) steps.MoveArrayElement(s, s - 1);
						if (GUILayout.Button("▼", GUILayout.Width(28)) && s < steps.arraySize - 1) steps.MoveArrayElement(s, s + 1);
						if (GUILayout.Button("-")) steps.DeleteArrayElementAtIndex(s);
						EditorGUILayout.EndHorizontal();
						EditorGUILayout.EndVertical();
					}
					if (GUILayout.Button("+ Add Step")) steps.InsertArrayElementAtIndex(steps.arraySize);
				}
				EditorGUILayout.BeginHorizontal();
				if (GUILayout.Button("▲", GUILayout.Width(28)) && i > 0) _hotspots.MoveArrayElement(i, i - 1);
				if (GUILayout.Button("▼", GUILayout.Width(28)) && i < _hotspots.arraySize - 1) _hotspots.MoveArrayElement(i, i + 1);
				if (GUILayout.Button("-")) _hotspots.DeleteArrayElementAtIndex(i);
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.EndVertical();
			}
			if (GUILayout.Button("+ Add Hotspot")) _hotspots.InsertArrayElementAtIndex(_hotspots.arraySize);
		}

		private void DrawZooms()
		{
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Zooms", EditorStyles.boldLabel);
			for (int i = 0; i < _zooms.arraySize; i++)
			{
				var elem = _zooms.GetArrayElementAtIndex(i);
				EditorGUILayout.BeginVertical("box");
				EditorGUILayout.PropertyField(elem.FindPropertyRelative("id"));
				EditorGUILayout.PropertyField(elem.FindPropertyRelative("sprite"));
				EditorGUILayout.PropertyField(elem.FindPropertyRelative("caption"));
				EditorGUILayout.PropertyField(elem.FindPropertyRelative("canPickupItemId"));
				EditorGUILayout.BeginHorizontal();
				if (GUILayout.Button("▲", GUILayout.Width(28)) && i > 0) _zooms.MoveArrayElement(i, i - 1);
				if (GUILayout.Button("▼", GUILayout.Width(28)) && i < _zooms.arraySize - 1) _zooms.MoveArrayElement(i, i + 1);
				if (GUILayout.Button("-")) _zooms.DeleteArrayElementAtIndex(i);
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.EndVertical();
			}
			if (GUILayout.Button("+ Add Zoom")) _zooms.InsertArrayElementAtIndex(_zooms.arraySize);
		}

		private void DrawGimmicks()
		{
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Gimmicks", EditorStyles.boldLabel);
			for (int i = 0; i < _gimmicks.arraySize; i++)
			{
				var elem = _gimmicks.GetArrayElementAtIndex(i);
				EditorGUILayout.BeginVertical("box");
				EditorGUILayout.PropertyField(elem.FindPropertyRelative("id"));
				EditorGUILayout.PropertyField(elem.FindPropertyRelative("type"));
				EditorGUILayout.PropertyField(elem.FindPropertyRelative("parameters"), true);
				EditorGUILayout.BeginHorizontal();
				if (GUILayout.Button("▲", GUILayout.Width(28)) && i > 0) _gimmicks.MoveArrayElement(i, i - 1);
				if (GUILayout.Button("▼", GUILayout.Width(28)) && i < _gimmicks.arraySize - 1) _gimmicks.MoveArrayElement(i, i + 1);
				if (GUILayout.Button("-")) _gimmicks.DeleteArrayElementAtIndex(i);
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.EndVertical();
			}
			if (GUILayout.Button("+ Add Gimmick")) _gimmicks.InsertArrayElementAtIndex(_gimmicks.arraySize);
		}
	}
}


