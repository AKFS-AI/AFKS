using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

namespace AFKS.Tools.Editor.StageCreators
{
    /// <summary>
    /// Menu 씬 생성을 담당하는 Creator 클래스입니다.
    /// 메뉴 UI와 스토리 진행 상태를 포함합니다.
    /// </summary>
    public class MenuCreator : BaseStageCreator
    {
        public MenuCreator() : base("Menu", "Stage1", false)
        {
        }

        public override void CreateStage(string path)
        {
            Debug.Log($"[MenuCreator] Menu 생성 시작: {path}");
            
            // 씬 생성 및 기본 설정
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            
            // 카메라 설정
            var camera = CreateStageCamera(stageName, new Vector3(0, 0, -10), 5f);
            
            // 공통 월드 구조 생성
            var (world, interaction, services) = CreateCommonWorldStructure();
            
            // 메뉴는 별도 배경 시스템 사용 (CreateStageSpecificBackground 제거)
            // CreateStageSpecificBackground(world, stageName, new Vector3(16, 9, 1), Vector3.zero);
            
            // 스테이지별 로직 생성
            CreateStageSpecificLogic(world, interaction, services);
            
            // 씬 저장
            SaveSceneWithSafety(scene, path);
            
            Debug.Log($"[MenuCreator] Menu 생성 완료: {path}");
        }

        protected override void CreateStageSpecificLogic(GameObject world, GameObject interaction, GameObject services)
        {
            // 메뉴 배경 패널 생성
            CreateMenuBackgroundPanel(world, services);
            
            // 메뉴 UI 생성
            CreateMenuUI(interaction);
            
            // 스토리 진행 상태 관리
            var storyProgress = services.AddComponent<AFKS.Features.Stage.StoryProgress>();
            
            // 메뉴 시스템 설정
            var mainMenuUI = services.AddComponent<AFKS.Features.Menu.MainMenuUI>();
            var mainMenuUISO = new SerializedObject(mainMenuUI);
            
            // 메뉴 설정
            mainMenuUISO.FindProperty("storyProgress").objectReferenceValue = storyProgress;
            mainMenuUISO.FindProperty("startButton").objectReferenceValue = interaction.transform.Find("StartButton")?.gameObject;
            mainMenuUISO.FindProperty("continueButton").objectReferenceValue = interaction.transform.Find("ContinueButton")?.gameObject;
            mainMenuUISO.FindProperty("settingsButton").objectReferenceValue = interaction.transform.Find("SettingsButton")?.gameObject;
            mainMenuUISO.FindProperty("quitButton").objectReferenceValue = interaction.transform.Find("QuitButton")?.gameObject;
            
            mainMenuUISO.ApplyModifiedPropertiesWithoutUndo();
            
            // 스토리 핵심 시스템 설정
            SetupStoryCoreSystems(services);
            
            Debug.Log($"[MenuCreator] Menu 특정 로직 생성 완료");
        }

        private void CreateMenuUI(GameObject parent)
        {
            // 시작 버튼
            var startButton = CreateMenuButton(parent, "StartButton", "새 게임 시작", new Vector3(0, 1, 0));
            
            // 계속하기 버튼
            var continueButton = CreateMenuButton(parent, "ContinueButton", "게임 계속", new Vector3(0, 0, 0));
            
            // 설정 버튼
            var settingsButton = CreateMenuButton(parent, "SettingsButton", "설정", new Vector3(0, -1, 0));
            
            // 종료 버튼
            var quitButton = CreateMenuButton(parent, "QuitButton", "게임 종료", new Vector3(0, -2, 0));
            
            Debug.Log($"[MenuCreator] 메뉴 UI 생성 완료");
        }

        private GameObject CreateMenuButton(GameObject parent, string buttonName, string buttonText, Vector3 position)
        {
            var button = new GameObject(buttonName);
            button.transform.SetParent(parent.transform);
            button.transform.localPosition = position;
            
            // 버튼 컴포넌트 추가
            var buttonComponent = button.AddComponent<UnityEngine.UI.Button>();
            var image = button.AddComponent<UnityEngine.UI.Image>();
            
            // 버튼 텍스트 추가
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(button.transform);
            textObj.transform.localPosition = Vector3.zero;
            
            var textComponent = textObj.AddComponent<UnityEngine.UI.Text>();
            textComponent.text = buttonText;
            textComponent.font = Resources.Load<Font>("Fonts/The_Jamsil_OTF_2024/The_Jamsil_OTF_2024");
            textComponent.fontSize = 24;
            textComponent.color = Color.white;
            textComponent.alignment = TextAnchor.MiddleCenter;
            
            // 텍스트 크기 조정
            var textRectTransform = textComponent.GetComponent<RectTransform>();
            textRectTransform.sizeDelta = new Vector2(200, 50);
            
            // 버튼 크기 조정
            var buttonRectTransform = button.GetComponent<RectTransform>();
            buttonRectTransform.sizeDelta = new Vector2(200, 50);
            
            Debug.Log($"[MenuCreator] 메뉴 버튼 생성 완료: {buttonName}");
            return button;
        }

        private void CreateMenuBackgroundPanel(GameObject world, GameObject services)
        {
            // 메뉴 배경 패널 생성 (UI 시스템용)
            var bgPanel = new GameObject("Panel_BG");
            // UI를 world의 자식으로 설정하여 Hierarchy에서 보이도록 함
            bgPanel.transform.SetParent(world.transform, false);
            
            // UI 컴포넌트 추가
            var canvas = bgPanel.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera; // 카메라 기반 렌더링
            canvas.sortingOrder = -1; // 배경이 가장 뒤에 오도록
            canvas.worldCamera = Camera.main; // 메인 카메라 연결
            canvas.planeDistance = 1f; // 카메라로부터의 거리
            
            var canvasScaler = bgPanel.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasScaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920, 1080);
            
            var graphicRaycaster = bgPanel.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            
            // 배경 이미지 추가
            var bgImage = bgPanel.AddComponent<UnityEngine.UI.Image>();
            
            // 배경 이미지 즉시 로드 및 할당
            var bgSprite = Resources.Load<Sprite>("Images/Backgrounds/Stage1/Stage1_1"); // 초기 배경
            if (bgSprite != null)
            {
                bgImage.sprite = bgSprite;
                Debug.Log($"[MenuCreator] 메뉴 초기 배경 이미지 로드됨: Stage1_1");
                
                // 즉시 이미지 적용 확인
                bgImage.color = Color.white;
                bgImage.raycastTarget = false; // 클릭 이벤트 방지
                
                // SerializedObject로 강제 적용
                var bgImageSO = new SerializedObject(bgImage);
                bgImageSO.FindProperty("m_Sprite").objectReferenceValue = bgSprite;
                bgImageSO.ApplyModifiedPropertiesWithoutUndo();
                
                Debug.Log($"[MenuCreator] 배경 이미지 SerializedObject로 강제 적용 완료");
            }
            else
            {
                Debug.LogWarning($"[MenuCreator] 메뉴 배경 이미지를 찾을 수 없음: Stage1_1");
                // 폴백: 기본 색상으로 설정
                bgImage.color = new Color(0.2f, 0.2f, 0.2f, 1f); // 어두운 회색
            }
            
            // RectTransform 설정 (전체 화면 커버)
            var rectTransform = bgPanel.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            
            // 동적 배경 전환 시스템 추가
            var dynamicBackground = bgPanel.AddComponent<AFKS.Features.Menu.DynamicMenuBackground>();
            var dynamicBgSO = new SerializedObject(dynamicBackground);
            
            // 배경 전환 설정 (즉시 적용)
            var bgImageProp = dynamicBgSO.FindProperty("backgroundImage");
            if (bgImageProp != null)
            {
                bgImageProp.objectReferenceValue = bgImage;
                Debug.Log($"[MenuCreator] DynamicMenuBackground에 배경 이미지 연결됨");
            }
            else
            {
                Debug.LogError($"[MenuCreator] DynamicMenuBackground의 backgroundImage 속성을 찾을 수 없음!");
            }
            
            var storyProgressProp = dynamicBgSO.FindProperty("storyProgress");
            if (storyProgressProp != null)
            {
                storyProgressProp.objectReferenceValue = services.GetComponent<AFKS.Features.Stage.StoryProgress>();
                Debug.Log($"[MenuCreator] DynamicMenuBackground에 StoryProgress 연결됨");
            }
            else
            {
                Debug.LogError($"[MenuCreator] DynamicMenuBackground의 storyProgress 속성을 찾을 수 없음!");
            }
            
            // 전환 설정
            var transitionDurationProp = dynamicBgSO.FindProperty("transitionDuration");
            if (transitionDurationProp != null) transitionDurationProp.floatValue = 2.0f;
            
            var backgroundChangeIntervalProp = dynamicBgSO.FindProperty("backgroundChangeInterval");
            if (backgroundChangeIntervalProp != null) backgroundChangeIntervalProp.floatValue = 15.0f;
            
            var enableCameraMovementProp = dynamicBgSO.FindProperty("enableCameraMovement");
            if (enableCameraMovementProp != null) enableCameraMovementProp.boolValue = true;
            
            var cameraMovementSpeedProp = dynamicBgSO.FindProperty("cameraMovementSpeed");
            if (cameraMovementSpeedProp != null) cameraMovementSpeedProp.floatValue = 0.5f;
            
            var cameraMovementRangeProp = dynamicBgSO.FindProperty("cameraMovementRange");
            if (cameraMovementRangeProp != null) cameraMovementRangeProp.floatValue = 0.1f;
            
            dynamicBgSO.ApplyModifiedPropertiesWithoutUndo();
            Debug.Log($"[MenuCreator] DynamicMenuBackground 설정 완료 - 모든 속성 적용됨");
            
            Debug.Log($"[MenuCreator] 메뉴 배경 패널 생성 완료: Panel_BG + 동적 배경 시스템");
        }
    }
}
