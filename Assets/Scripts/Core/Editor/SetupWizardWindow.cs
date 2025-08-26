using UnityEditor;
using UnityEngine;
using AFKS.Core.Composition;
using AFKS.Core.Data;

namespace AFKS.Core.Editor
{
	public sealed class SetupWizardWindow : EditorWindow
	{
		private bool _createCoreScene = true;
		private bool _createProjectAssets = true;
		private bool _createFrontGateStage = true;
		private bool _generateAllScenes = true;
		private bool _makeCameraLessSafe = true;
		private bool _attachRuntimeAllScenes = true;

		[MenuItem("AFKS/제작 도구/원클릭 설정 마법사")] 
		public static void Open()
		{
			var w = GetWindow<SetupWizardWindow>(true, "원클릭 설정 마법사");
			w.minSize = new Vector2(460, 320);
		}

		private void OnGUI()
		{
			GUILayout.Label("프로젝트 자동 구성", EditorStyles.boldLabel);
			EditorGUILayout.HelpBox("이미 생성된 항목은 건너뛰고, 누락분만 보충합니다(멱등). 이미지/사운드 연결만 수동으로 해주면 됩니다.", MessageType.Info);

			GUILayout.Space(6);
			GUILayout.Label("생성 항목", EditorStyles.miniBoldLabel);
			_createCoreScene = EditorGUILayout.Toggle("Core 씬 생성 (GameRuntime 자동 배치)", _createCoreScene);
			_createProjectAssets = EditorGUILayout.Toggle("ProjectConfig + AudioDatabase 생성", _createProjectAssets);
			_createFrontGateStage = EditorGUILayout.Toggle("Stage 0 템플릿 생성(정문/체인 시퀀스)", _createFrontGateStage);
			_generateAllScenes = EditorGUILayout.Toggle("모든 씬 프리뷰 생성 (MainCore/MainMenu/Stage1~6)", _generateAllScenes);
			_attachRuntimeAllScenes = EditorGUILayout.Toggle("모든 씬 하이라키 자동 구성(GameRuntime/Services/StageRoot)", _attachRuntimeAllScenes);

			GUILayout.Space(6);
			GUILayout.Label("안전 옵션", EditorStyles.miniBoldLabel);
			_makeCameraLessSafe = EditorGUILayout.Toggle("카메라 없이도 입력 처리(스크린 비율 기반)", _makeCameraLessSafe);

			GUILayout.Space(12);
			if (GUILayout.Button("실행"))
			{
				Run();
			}
			GUILayout.Space(4);
			if (GUILayout.Button("생성 폴더 열기 (Assets/Data/Hospital)"))
			{
				var dir = "Assets/Data/Hospital";
				System.IO.Directory.CreateDirectory(dir);
				EditorUtility.RevealInFinder(dir);
			}
		}

		private void Run()
		{
			if (_createCoreScene) ProjectBootstrapEditor.GenerateCoreScene();
			if (_createProjectAssets) ProjectBootstrapEditor.CreateProjectAssets();
			if (_createFrontGateStage) ProjectBootstrapEditor.CreateStageTemplate();
			if (_generateAllScenes) ProjectBootstrapEditor.GenerateAllScenes();
			if (_attachRuntimeAllScenes) ProjectBootstrapEditor.AttachRuntimeToAllStageScenes();
			else ProjectBootstrapEditor.AttachRuntimeToActiveScene();

			// 카메라리스 모드: InteractionSystem은 이미 화면 비율 좌표를 사용합니다.
			// UI: GameRuntime가 항상 UICanvas/UICamera를 생성합니다.
			EditorUtility.DisplayDialog("원클릭 설정", "구성이 완료되었습니다. 생성된 에셋에 스프라이트/오디오만 연결 후 Play 하세요.", "확인");
		}
	}
}


