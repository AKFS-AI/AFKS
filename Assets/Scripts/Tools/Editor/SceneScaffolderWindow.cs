using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using AFKS.Features.Items;
using TMPro;

namespace AFKS.Tools.Editor
{
    /// <summary>
    /// 스테이지 기반 공포/방탈출 프로젝트를 위한 씬 스캐폴딩 도구.
    /// - Core 씬 생성: FadeCanvas, AudioService, InventoryService, EventSystem 등 기본 구성
    /// - Stage 씬 생성: StageRoot 배치, 선택 항목 자동 와이어링
    /// - 간단 전환 테스트: Core + Stage를 편집기에서 Additive 로드
    /// 인스펙터 라벨은 한국어로 표기합니다.
    /// </summary>
    public sealed class SceneScaffolderWindow : EditorWindow
    {
        #region 메뉴
        [MenuItem("AFKS/도구/씬 스캐폴더 열기")] private static void Open() => GetWindow<SceneScaffolderWindow>(true, "씬 스캐폴더");
        #endregion

        #region 직렬화 상태
        private string coreScenePath = "Assets/Scenes/Core.unity"; // 기본 경로 교정
        private string stageName = "Stage_HospitalLobby";
        private string stageFolder = "Assets/Scenes";
        private string scenesFolder = "Assets/Scenes"; // 빠른 생성 대상 폴더
        private bool addSaveService = true;
        private bool addGameStateService = true;
        private string tmpFontAssetPath = "Assets/Fonts/The_Jamsil_OTF_2024/The Jamsil OTF 1 Thin SDF.asset"; // TMP 기본 폰트
        private bool addEventSystem = true;
        private bool addAudioService = true;
        private bool addInventoryService = true;
        #endregion

        #region GUI
        private void OnGUI()
        {
            EditorGUILayout.LabelField("코어 씬", EditorStyles.boldLabel);
            coreScenePath = EditorGUILayout.TextField("저장 경로", coreScenePath);
            addEventSystem = EditorGUILayout.ToggleLeft("EventSystem 추가", addEventSystem);
            addAudioService = EditorGUILayout.ToggleLeft("Audio Service 추가", addAudioService);
            addInventoryService = EditorGUILayout.ToggleLeft("Inventory Service 추가", addInventoryService);
            addSaveService = EditorGUILayout.ToggleLeft("Save Service 추가", addSaveService);
            addGameStateService = EditorGUILayout.ToggleLeft("Game State Service 추가", addGameStateService);
            tmpFontAssetPath = EditorGUILayout.TextField("TMP 폰트 경로", tmpFontAssetPath);
            if (GUILayout.Button("코어 씬 생성/갱신")) CreateOrUpdateCoreScene();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("스테이지 씬", EditorStyles.boldLabel);
            stageFolder = EditorGUILayout.TextField("폴더", stageFolder);
            stageName = EditorGUILayout.TextField("스테이지 이름", stageName);
            if (GUILayout.Button("스테이지 씬 생성")) CreateStageScene();

            EditorGUILayout.Space(10);
            if (GUILayout.Button("전환 테스트 로드(Core+Stage)")) LoadCoreAndStageForTest();

            EditorGUILayout.Space(12);
            EditorGUILayout.LabelField("빠른 생성/덮어쓰기 (명명 규칙 고정)", EditorStyles.boldLabel);
            scenesFolder = EditorGUILayout.TextField("대상 폴더", scenesFolder);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Core.unity 생성/덮어쓰기")) CreateOrUpdateCoreSceneAt(Path.Combine(scenesFolder, "Core.unity"));
            if (GUILayout.Button("Menu.unity 생성/덮어쓰기")) CreateOrOverwriteMenuScene();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Stage1.unity 생성/덮어쓰기")) CreateOrOverwriteStage1();
            if (GUILayout.Button("Stage2.unity 생성/덮어쓰기")) CreateOrOverwriteStage2();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Stage3.unity 생성/덮어쓰기")) CreateOrOverwriteStageGeneric("Stage3", nextStageId: "Stage4");
            if (GUILayout.Button("Stage4.unity 생성/덮어쓰기")) CreateOrOverwriteStageGeneric("Stage4", nextStageId: "Stage5");
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Stage5.unity 생성/덮어쓰기")) CreateOrOverwriteStageGeneric("Stage5", nextStageId: "Stage6");
            if (GUILayout.Button("Stage6.unity 생성/덮어쓰기")) CreateOrOverwriteStageGeneric("Stage6", nextStageId: "Menu");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(6);
            if (GUILayout.Button("모든 씬 원클릭 생성/덮어쓰기(Core/Menu/Stage1~6)+BuildSettings 등록")) GenerateAllAndRegisterBuildSettings();
        }
        #endregion

        #region 구현
        private void CreateOrUpdateCoreScene()
        {
            EnsureDirectory(Path.GetDirectoryName(coreScenePath));

            var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            newScene.name = Path.GetFileNameWithoutExtension(coreScenePath);

            // Camera
            var cam = new GameObject("Main Camera");
            var camera = cam.AddComponent<Camera>();
            cam.tag = "MainCamera";
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;

            // FadeCanvas
            var fadeGo = new GameObject("FadeCanvas");
            var canvas = fadeGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            fadeGo.AddComponent<CanvasGroup>();
            var fade = fadeGo.AddComponent<AFKS.Core.UI.FadeCanvas>();

            if (addEventSystem)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<StandaloneInputModule>();
            }

            if (addAudioService)
            {
                var audioGo = new GameObject("AudioService");
                audioGo.AddComponent<AudioListener>();
                audioGo.AddComponent<AFKS.Core.Services.Audio.AudioService>();
            }

            if (addInventoryService)
            {
                var invGo = new GameObject("InventoryService");
                invGo.AddComponent<AFKS.Core.Services.Inventory.InventoryService>();
            }

            if (addSaveService)
            {
                var saveGo = new GameObject("SaveService");
                saveGo.AddComponent<AFKS.Core.Services.Save.SaveService>();
            }

            if (addGameStateService)
            {
                var gsGo = new GameObject("GameStateService");
                gsGo.AddComponent<AFKS.Core.Services.GameState.GameStateService>();
            }

            // InputService + SceneService + GlobalSingletonGuard + StartupLoader
            var inputGo = new GameObject("InputService");
            inputGo.AddComponent<AFKS.Core.Services.Input.InputService>();

            var sceneSvcGo = new GameObject("SceneService");
            var sceneSvc = sceneSvcGo.AddComponent<AFKS.Core.Services.Scene.SceneService>();
            // fadeCanvas 필드 연결
            var so = new SerializedObject(sceneSvc);
            so.FindProperty("fadeCanvas").objectReferenceValue = fade;
            so.ApplyModifiedPropertiesWithoutUndo();

            var guardGo = new GameObject("GlobalSingletonGuard");
            guardGo.AddComponent<AFKS.Core.Utils.GlobalSingletonGuard>();

            // 전역 설정창 (모든 씬에서 접근 가능)
            var settingsGo = new GameObject("GlobalSettings");
            var settingsCanvas = settingsGo.AddComponent<Canvas>();
            settingsCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            settingsCanvas.sortingOrder = 1000; // 최상위에 표시
            settingsGo.AddComponent<CanvasScaler>();
            settingsGo.AddComponent<GraphicRaycaster>();
            
            // 설정 패널 (전체 화면 덮기)
            var settingsPanel = new GameObject("SettingsPanel");
            settingsPanel.transform.SetParent(settingsGo.transform, false);
            var settingsPanelRt = settingsPanel.AddComponent<RectTransform>();
            settingsPanelRt.anchorMin = Vector2.zero;
            settingsPanelRt.anchorMax = Vector2.one;
            settingsPanelRt.offsetMin = Vector2.zero;
            settingsPanelRt.offsetMax = Vector2.zero;
            
            // 설정 패널 반투명 어두운 배경 (전체 화면)
            var settingsBg = new GameObject("SettingsBackground");
            settingsBg.transform.SetParent(settingsPanel.transform, false);
            var settingsBgImg = settingsBg.AddComponent<Image>();
            settingsBgImg.color = new Color(0, 0, 0, 0.7f); // 반투명 검은색
            var settingsBgRt = settingsBg.GetComponent<RectTransform>();
            settingsBgRt.anchorMin = Vector2.zero;
            settingsBgRt.anchorMax = Vector2.one;
            settingsBgRt.offsetMin = Vector2.zero;
            settingsBgRt.offsetMax = Vector2.zero;
            
            // 옵션 창 컨테이너 (중앙에 떠있는 창)
            var optionsWindow = new GameObject("OptionsWindow");
            optionsWindow.transform.SetParent(settingsPanel.transform, false);
            var optionsWindowRt = optionsWindow.AddComponent<RectTransform>();
            optionsWindowRt.anchorMin = new Vector2(0.5f, 0.5f);
            optionsWindowRt.anchorMax = new Vector2(0.5f, 0.5f);
            optionsWindowRt.anchoredPosition = Vector2.zero;
            optionsWindowRt.sizeDelta = new Vector2(700, 800);
            
            // 옵션 창 배경 (갈색 테마)
            var optionsBg = new GameObject("OptionsBackground");
            optionsBg.transform.SetParent(optionsWindow.transform, false);
            var optionsBgImg = optionsBg.AddComponent<Image>();
            optionsBgImg.color = new Color(0.6f, 0.4f, 0.2f, 0.95f); // 갈색 테마
            var optionsBgRt = optionsBg.GetComponent<RectTransform>();
            optionsBgRt.anchorMin = Vector2.zero;
            optionsBgRt.anchorMax = Vector2.one;
            optionsBgRt.offsetMin = Vector2.zero;
            optionsBgRt.offsetMax = Vector2.zero;
            
            // 오른쪽 위 X 버튼 (창 닫기) - 크기 증가 및 클릭 문제 해결
            var closeButton = new GameObject("CloseButton");
            closeButton.transform.SetParent(optionsWindow.transform, false);
            var closeButtonRt = closeButton.AddComponent<RectTransform>();
            closeButtonRt.anchorMin = new Vector2(1, 1);
            closeButtonRt.anchorMax = new Vector2(1, 1);
            closeButtonRt.anchoredPosition = new Vector2(-25, -25);
            closeButtonRt.sizeDelta = new Vector2(60, 60); // 크기 증가
            
            var closeBtn = closeButton.AddComponent<Button>();
            var closeBtnImg = closeButton.AddComponent<Image>();
            closeBtnImg.color = new Color(1, 1, 1, 0.1f); // 약간 보이는 배경 (클릭 영역 확보)
            
            // X 텍스트
            var closeTextGo = new GameObject("CloseText");
            closeTextGo.transform.SetParent(closeButton.transform, false);
            var closeTextRt = closeTextGo.AddComponent<RectTransform>();
            closeTextRt.anchorMin = Vector2.zero;
            closeTextRt.anchorMax = Vector2.one;
            closeTextRt.offsetMin = Vector2.zero;
            closeTextRt.offsetMax = Vector2.zero;
            var closeTextTmp = closeTextGo.AddComponent<TextMeshProUGUI>();
            closeTextTmp.text = "X";
            closeTextTmp.alignment = TextAlignmentOptions.Center;
            closeTextTmp.fontSize = 40; // 폰트 크기 증가
            closeTextTmp.color = Color.white;
            var closeTextFont = LoadTMPFontAsset(tmpFontAssetPath);
            if (closeTextFont != null) closeTextTmp.font = closeTextFont;
            closeTextTmp.raycastTarget = false;
            
            // X 버튼 클릭 이벤트 연결
            closeBtn.onClick.AddListener(() => {
                if (settingsPanel != null)
                {
                    settingsPanel.SetActive(false);
                }
            });
            
            // 옵션 창 내부 버튼 컨테이너
            var settingsButtons = new GameObject("SettingsButtons");
            settingsButtons.transform.SetParent(optionsWindow.transform, false); // 옵션 창의 자식으로 변경
            var settingsButtonsRt = settingsButtons.AddComponent<RectTransform>();
            settingsButtonsRt.anchorMin = new Vector2(0.5f, 0.5f);
            settingsButtonsRt.anchorMax = new Vector2(0.5f, 0.5f);
            settingsButtonsRt.anchoredPosition = Vector2.zero;
            settingsButtonsRt.sizeDelta = new Vector2(600, 700); // 옵션 창 안에 맞춤
            var settingsVlg = settingsButtons.AddComponent<VerticalLayoutGroup>();
            settingsVlg.padding = new RectOffset(40, 40, 40, 40);
            settingsVlg.spacing = 25f;
            settingsVlg.childAlignment = TextAnchor.MiddleCenter;
            settingsVlg.childControlWidth = true;
            settingsVlg.childControlHeight = true;
            settingsVlg.childForceExpandWidth = true;
            settingsVlg.childForceExpandHeight = false;
            
            // 현대적인 게임 설정창 UI 구성
            
            // 제목
            var titleGo = new GameObject("Title");
            titleGo.transform.SetParent(settingsButtons.transform, false);
            var titleRt = titleGo.AddComponent<RectTransform>();
            var titleTmp = titleGo.AddComponent<TextMeshProUGUI>();
            titleTmp.text = "설정";
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleTmp.fontSize = 48;
            titleTmp.color = Color.white;
            var titleFont = LoadTMPFontAsset(tmpFontAssetPath);
            if (titleFont != null) titleTmp.font = titleFont;
            titleTmp.raycastTarget = false;
            var titleLe = titleGo.AddComponent<LayoutElement>();
            titleLe.preferredHeight = 80f;
            titleLe.flexibleHeight = 0f;
            
            // 사운드 설정 섹션 패널 (게임옵션 스타일)
            var soundPanel = CreateGameOptionsSoundPanel("SoundPanel");
            soundPanel.transform.SetParent(settingsButtons.transform, false);
            
            // 사운드 설정 제목 (헤드폰 아이콘 + 텍스트)
            var soundTitleGo = new GameObject("SoundTitle");
            soundTitleGo.transform.SetParent(soundPanel.transform, false);
            var soundTitleRt = soundTitleGo.AddComponent<RectTransform>();
            soundTitleRt.sizeDelta = new Vector2(600, 60);
            
            // 헤드폰 아이콘 (텍스트로 대체)
            var headphoneIconGo = new GameObject("HeadphoneIcon");
            headphoneIconGo.transform.SetParent(soundTitleGo.transform, false);
            var headphoneIconRt = headphoneIconGo.AddComponent<RectTransform>();
            headphoneIconRt.anchorMin = new Vector2(0, 0.5f);
            headphoneIconRt.anchorMax = new Vector2(0.1f, 0.5f);
            headphoneIconRt.sizeDelta = new Vector2(40, 40);
            headphoneIconRt.anchoredPosition = Vector2.zero;
            var headphoneIconTmp = headphoneIconGo.AddComponent<TextMeshProUGUI>();
            headphoneIconTmp.text = "🎧"; // 헤드폰 이모지
            headphoneIconTmp.alignment = TextAlignmentOptions.Center;
            headphoneIconTmp.fontSize = 32;
            headphoneIconTmp.color = new Color(0.9f, 0.8f, 0.6f);
            headphoneIconTmp.raycastTarget = false;
            
            // 사운드 설정 텍스트
            var soundTitleTextGo = new GameObject("SoundTitleText");
            soundTitleTextGo.transform.SetParent(soundTitleGo.transform, false);
            var soundTitleTextRt = soundTitleTextGo.AddComponent<RectTransform>();
            soundTitleTextRt.anchorMin = new Vector2(0.15f, 0.5f);
            soundTitleTextRt.anchorMax = new Vector2(1, 0.5f);
            soundTitleTextRt.offsetMin = Vector2.zero;
            soundTitleTextRt.offsetMax = Vector2.zero;
            var soundTitleTextTmp = soundTitleTextGo.AddComponent<TextMeshProUGUI>();
            soundTitleTextTmp.text = "사운드 설정";
            soundTitleTextTmp.alignment = TextAlignmentOptions.Left;
            soundTitleTextTmp.fontSize = 36;
            soundTitleTextTmp.color = new Color(0.9f, 0.8f, 0.6f);
            var soundTitleFont = LoadTMPFontAsset(tmpFontAssetPath);
            if (soundTitleFont != null) soundTitleTextTmp.font = soundTitleFont;
            soundTitleTextTmp.raycastTarget = false;
            
            // 효과음 슬라이더 (이미지 스타일)
            var effectSoundSlider = CreateImageStyleVolumeSlider("Slider_EffectSound", "효과음", 0.8f);
            effectSoundSlider.transform.SetParent(soundPanel.transform, false);
            
            // 환경음 슬라이더 (이미지 스타일)
            var envSoundSlider = CreateImageStyleVolumeSlider("Slider_EnvSound", "환경음", 0.6f);
            envSoundSlider.transform.SetParent(soundPanel.transform, false);
            
            // 배경음 슬라이더 (이미지 스타일)
            var bgMusicSlider = CreateImageStyleVolumeSlider("Slider_BGMusic", "배경음", 0.6f);
            bgMusicSlider.transform.SetParent(soundPanel.transform, false);
            
            // 구분선
            var separator = CreateSeparator("Separator");
            separator.transform.SetParent(settingsButtons.transform, false);
            
            // 메뉴로 돌아가기 버튼
            var btnReturnMenu = CreateButton("Button_ReturnMenu", "메뉴로 돌아가기");
            btnReturnMenu.transform.SetParent(settingsButtons.transform, false);
            
            // 세이브초기화 버튼
            var btnResetSave = CreateButton("Button_ResetSave", "세이브초기화");
            btnResetSave.transform.SetParent(settingsButtons.transform, false);
            
            // 설정창 닫기 버튼 제거 (ESC 키로만 닫기)
            // var btnCloseSettings = CreateButton("Button_CloseSettings", "설정창 닫기");
            // btnCloseSettings.transform.SetParent(settingsButtons.transform, false);
            
            // 설정 패널 초기 상태 (비활성화)
            settingsPanel.SetActive(false);
            
            // GlobalSettingsManager 추가
            var settingsManager = settingsGo.AddComponent<AFKS.Core.Systems.GlobalSettingsManager>();
            var soManager = new SerializedObject(settingsManager);
            soManager.FindProperty("settingsPanel").objectReferenceValue = settingsPanel;
            // soManager.FindProperty("closeSettingsButton").objectReferenceValue = btnCloseSettings; // 닫기 버튼 제거됨
            soManager.FindProperty("returnToMenuButton").objectReferenceValue = btnReturnMenu;
            soManager.FindProperty("resetSaveButton").objectReferenceValue = btnResetSave;
            soManager.ApplyModifiedPropertiesWithoutUndo();
            
            // CreateButton 함수 정의 (Core 씬용)
            Button CreateButton(string name, string label, bool interactable = true)
            {
                var go = new GameObject(name);
                go.transform.SetParent(settingsButtons.transform, false);
                var r = go.AddComponent<RectTransform>();
                var b = go.AddComponent<Button>();
                b.interactable = interactable;
                var imgBtn = go.AddComponent<Image>();
                imgBtn.color = new Color(1, 1, 1, 0.08f);
                var labelGo = new GameObject("Label");
                labelGo.transform.SetParent(go.transform, false);
                var tr = labelGo.AddComponent<RectTransform>();
                tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one; tr.offsetMin = Vector2.zero; tr.offsetMax = Vector2.zero;
                var tmp = labelGo.AddComponent<TextMeshProUGUI>();
                tmp.text = label; tmp.alignment = TextAlignmentOptions.Center; tmp.enableAutoSizing = true; tmp.fontSizeMin = 24; tmp.fontSizeMax = 40;
                var font = LoadTMPFontAsset(tmpFontAssetPath);
                if (font != null) tmp.font = font;
                tmp.raycastTarget = false;
                var le = go.AddComponent<LayoutElement>(); le.preferredHeight = 80f; le.flexibleHeight = 0f;
                return b;
            }
            
            // 이미지와 유사한 사운드 설정 슬라이더 생성 함수 (5개 세그먼트, +/- 버튼)
            Slider CreateImageStyleVolumeSlider(string name, string label, float defaultValue)
            {
                var container = new GameObject(name);
                container.transform.SetParent(settingsButtons.transform, false);
                var containerRt = container.AddComponent<RectTransform>();
                
                // 라벨
                var labelGo = new GameObject("Label");
                labelGo.transform.SetParent(container.transform, false);
                var labelRt = labelGo.AddComponent<RectTransform>();
                labelRt.anchorMin = new Vector2(0, 0.5f);
                labelRt.anchorMax = new Vector2(0.25f, 0.5f);
                labelRt.offsetMin = Vector2.zero;
                labelRt.offsetMax = Vector2.zero;
                var labelTmp = labelGo.AddComponent<TextMeshProUGUI>();
                labelTmp.text = label;
                labelTmp.alignment = TextAlignmentOptions.Left;
                labelTmp.fontSize = 32;
                labelTmp.color = new Color(0.9f, 0.8f, 0.6f); // 밝은 갈색
                var labelFont = LoadTMPFontAsset(tmpFontAssetPath);
                if (labelFont != null) labelTmp.font = labelFont;
                labelTmp.raycastTarget = false;
                
                // 슬라이더 컨테이너
                var sliderContainer = new GameObject("SliderContainer");
                sliderContainer.transform.SetParent(container.transform, false);
                var sliderContainerRt = sliderContainer.AddComponent<RectTransform>();
                sliderContainerRt.anchorMin = new Vector2(0.3f, 0.5f);
                sliderContainerRt.anchorMax = new Vector2(0.7f, 0.5f);
                sliderContainerRt.offsetMin = Vector2.zero;
                sliderContainerRt.offsetMax = Vector2.zero;
                
                // - 버튼
                var minusBtn = new GameObject("MinusButton");
                minusBtn.transform.SetParent(sliderContainer.transform, false);
                var minusBtnRt = minusBtn.AddComponent<RectTransform>();
                minusBtnRt.anchorMin = new Vector2(0, 0.5f);
                minusBtnRt.anchorMax = new Vector2(0.1f, 0.5f);
                minusBtnRt.sizeDelta = new Vector2(30, 30);
                minusBtnRt.anchoredPosition = Vector2.zero;
                var minusButton = minusBtn.AddComponent<Button>();
                var minusBtnImg = minusBtn.AddComponent<Image>();
                minusBtnImg.color = new Color(0.8f, 0.6f, 0.4f, 0.9f);
                var minusTextGo = new GameObject("MinusText");
                minusTextGo.transform.SetParent(minusBtn.transform, false);
                var minusTextRt = minusTextGo.AddComponent<RectTransform>();
                minusTextRt.anchorMin = Vector2.zero;
                minusTextRt.anchorMax = Vector2.one;
                minusTextRt.offsetMin = Vector2.zero;
                minusTextRt.offsetMax = Vector2.zero;
                var minusTextTmp = minusTextGo.AddComponent<TextMeshProUGUI>();
                minusTextTmp.text = "-";
                minusTextTmp.alignment = TextAlignmentOptions.Center;
                minusTextTmp.fontSize = 24;
                minusTextTmp.color = Color.white;
                minusTextTmp.raycastTarget = false;
                
                // 슬라이더 (5개 세그먼트)
                var sliderGo = new GameObject("Slider");
                sliderGo.transform.SetParent(sliderContainer.transform, false);
                var sliderRt = sliderGo.AddComponent<RectTransform>();
                sliderRt.anchorMin = new Vector2(0.15f, 0.5f);
                sliderRt.anchorMax = new Vector2(0.85f, 0.5f);
                sliderRt.offsetMin = Vector2.zero;
                sliderRt.offsetMax = Vector2.zero;
                
                var slider = sliderGo.AddComponent<Slider>();
                slider.minValue = 0f;
                slider.maxValue = 5f; // 5개 세그먼트
                slider.value = defaultValue * 5f; // 0.8 -> 4, 0.6 -> 3
                slider.wholeNumbers = true; // 정수값만
                
                // 슬라이더 배경 (5개 세그먼트)
                var bgGo = new GameObject("Background");
                bgGo.transform.SetParent(sliderGo.transform, false);
                var bgImg = bgGo.AddComponent<Image>();
                bgImg.color = new Color(0.4f, 0.4f, 0.4f, 0.9f);
                var bgRt = bgGo.GetComponent<RectTransform>();
                bgRt.anchorMin = Vector2.zero;
                bgRt.anchorMax = Vector2.one;
                bgRt.offsetMin = Vector2.zero;
                bgRt.offsetMax = Vector2.zero;
                
                // 슬라이더 채움 (세그먼트별)
                var fillGo = new GameObject("Fill");
                fillGo.transform.SetParent(sliderGo.transform, false);
                var fillImg = fillGo.AddComponent<Image>();
                fillImg.color = new Color(0.2f, 0.8f, 0.2f, 0.9f); // 밝은 초록색
                var fillRt = fillGo.GetComponent<RectTransform>();
                fillRt.anchorMin = Vector2.zero;
                fillRt.anchorMax = Vector2.one;
                fillRt.offsetMin = Vector2.zero;
                fillRt.offsetMax = Vector2.zero;
                
                // 슬라이더 핸들
                var handleGo = new GameObject("Handle");
                handleGo.transform.SetParent(sliderGo.transform, false);
                var handleImg = handleGo.AddComponent<Image>();
                handleImg.color = new Color(0.2f, 0.8f, 0.2f, 1f); // 밝은 초록색
                var handleRt = handleGo.GetComponent<RectTransform>();
                handleRt.anchorMin = new Vector2(0.5f, 0.5f);
                handleRt.anchorMax = new Vector2(0.5f, 0.5f);
                handleRt.sizeDelta = new Vector2(20, 20);
                handleRt.anchoredPosition = Vector2.zero;
                
                // 슬라이더 컴포넌트 연결
                slider.targetGraphic = handleImg;
                slider.fillRect = fillRt;
                slider.handleRect = handleRt;
                
                // + 버튼
                var plusBtn = new GameObject("PlusButton");
                plusBtn.transform.SetParent(sliderContainer.transform, false);
                var plusBtnRt = plusBtn.AddComponent<RectTransform>();
                plusBtnRt.anchorMin = new Vector2(0.9f, 0.5f);
                plusBtnRt.anchorMax = new Vector2(1, 0.5f);
                plusBtnRt.sizeDelta = new Vector2(30, 30);
                plusBtnRt.anchoredPosition = Vector2.zero;
                var plusButton = plusBtn.AddComponent<Button>();
                var plusBtnImg = plusBtn.AddComponent<Image>();
                plusBtnImg.color = new Color(0.8f, 0.6f, 0.4f, 0.9f);
                var plusTextGo = new GameObject("PlusText");
                plusTextGo.transform.SetParent(plusBtn.transform, false);
                var plusTextRt = plusTextGo.AddComponent<RectTransform>();
                plusTextRt.anchorMin = Vector2.zero;
                plusTextRt.anchorMax = Vector2.one;
                plusTextRt.offsetMin = Vector2.zero;
                plusTextRt.offsetMax = Vector2.zero;
                var plusTextTmp = plusTextGo.AddComponent<TextMeshProUGUI>();
                plusTextTmp.text = "+";
                plusTextTmp.alignment = TextAlignmentOptions.Center;
                plusTextTmp.fontSize = 24;
                plusTextTmp.color = Color.white;
                plusTextTmp.raycastTarget = false;
                
                // On/Off 컨테이너
                var onOffContainer = new GameObject("OnOffContainer");
                onOffContainer.transform.SetParent(container.transform, false);
                var onOffContainerRt = onOffContainer.AddComponent<RectTransform>();
                onOffContainerRt.anchorMin = new Vector2(0.75f, 0.5f);
                onOffContainerRt.anchorMax = new Vector2(1, 0.5f);
                onOffContainerRt.offsetMin = Vector2.zero;
                onOffContainerRt.offsetMax = Vector2.zero;
                
                // On 라벨
                var onLabelGo = new GameObject("OnLabel");
                onLabelGo.transform.SetParent(onOffContainer.transform, false);
                var onLabelRt = onLabelGo.AddComponent<RectTransform>();
                onLabelRt.anchorMin = new Vector2(0, 0.5f);
                onLabelRt.anchorMax = new Vector2(0.6f, 0.5f);
                onLabelRt.offsetMin = Vector2.zero;
                onLabelRt.offsetMax = Vector2.zero;
                var onLabelTmp = onLabelGo.AddComponent<TextMeshProUGUI>();
                onLabelTmp.text = "On";
                onLabelTmp.alignment = TextAlignmentOptions.Right;
                onLabelTmp.fontSize = 28;
                onLabelTmp.color = new Color(0.9f, 0.8f, 0.6f);
                if (labelFont != null) onLabelTmp.font = labelFont;
                onLabelTmp.raycastTarget = false;
                
                // 체크박스
                var checkboxGo = new GameObject("Checkbox");
                checkboxGo.transform.SetParent(onOffContainer.transform, false);
                var checkboxRt = checkboxGo.AddComponent<RectTransform>();
                checkboxRt.anchorMin = new Vector2(0.7f, 0.5f);
                checkboxRt.anchorMax = new Vector2(1, 0.5f);
                checkboxRt.sizeDelta = new Vector2(20, 20);
                checkboxRt.anchoredPosition = Vector2.zero;
                var checkbox = checkboxGo.AddComponent<Toggle>();
                checkbox.isOn = false; // 기본값 Off (이미지와 동일)
                var checkboxImg = checkboxGo.AddComponent<Image>();
                checkboxImg.color = Color.white;
                
                // 레이아웃 요소
                var le = container.AddComponent<LayoutElement>();
                le.preferredHeight = 80f;
                le.flexibleHeight = 0f;
                
                return slider;
            }
            
            // 기존 게임옵션 스타일 슬라이더 (호환성 유지)
            Slider CreateGameOptionsVolumeSlider(string name, string label, float defaultValue)
            {
                var container = new GameObject(name);
                container.transform.SetParent(settingsButtons.transform, false);
                var containerRt = container.AddComponent<RectTransform>();
                
                // 라벨과 On/Off 체크박스
                var labelContainer = new GameObject("LabelContainer");
                labelContainer.transform.SetParent(container.transform, false);
                var labelContainerRt = labelContainer.AddComponent<RectTransform>();
                labelContainerRt.anchorMin = new Vector2(0, 0.5f);
                labelContainerRt.anchorMax = new Vector2(0.4f, 0.5f);
                labelContainerRt.offsetMin = Vector2.zero;
                labelContainerRt.offsetMax = Vector2.zero;
                
                // 라벨
                var labelGo = new GameObject("Label");
                labelGo.transform.SetParent(labelContainer.transform, false);
                var labelRt = labelGo.AddComponent<RectTransform>();
                labelRt.anchorMin = new Vector2(0, 0.5f);
                labelRt.anchorMax = new Vector2(0.7f, 0.5f);
                labelRt.offsetMin = Vector2.zero;
                labelRt.offsetMax = Vector2.zero;
                var labelTmp = labelGo.AddComponent<TextMeshProUGUI>();
                labelTmp.text = label;
                labelTmp.alignment = TextAlignmentOptions.Left;
                labelTmp.fontSize = 32;
                labelTmp.color = Color.white;
                var labelFont = LoadTMPFontAsset(tmpFontAssetPath);
                if (labelFont != null) labelTmp.font = labelFont;
                labelTmp.raycastTarget = false;
                
                // On 라벨
                var onLabelGo = new GameObject("OnLabel");
                onLabelGo.transform.SetParent(labelContainer.transform, false);
                var onLabelRt = onLabelGo.AddComponent<RectTransform>();
                onLabelRt.anchorMin = new Vector2(0.75f, 0.5f);
                onLabelRt.anchorMax = new Vector2(1, 0.5f);
                onLabelRt.offsetMin = Vector2.zero;
                onLabelRt.offsetMax = Vector2.zero;
                var onLabelTmp = onLabelGo.AddComponent<TextMeshProUGUI>();
                onLabelTmp.text = "On";
                onLabelTmp.alignment = TextAlignmentOptions.Right;
                onLabelTmp.fontSize = 28;
                labelTmp.color = Color.white;
                if (labelFont != null) onLabelTmp.font = labelFont;
                onLabelTmp.raycastTarget = false;
                
                // 체크박스
                var checkboxGo = new GameObject("Checkbox");
                checkboxGo.transform.SetParent(labelContainer.transform, false);
                var checkboxRt = checkboxGo.AddComponent<RectTransform>();
                checkboxRt.anchorMin = new Vector2(0.8f, 0.5f);
                checkboxRt.anchorMax = new Vector2(0.9f, 0.5f);
                checkboxRt.sizeDelta = new Vector2(20, 20);
                checkboxRt.anchoredPosition = Vector2.zero;
                var checkbox = checkboxGo.AddComponent<Toggle>();
                checkbox.isOn = true; // 기본값 On
                var checkboxImg = checkboxGo.AddComponent<Image>();
                checkboxImg.color = Color.white;
                
                // 슬라이더 (10개 세그먼트)
                var sliderGo = new GameObject("Slider");
                sliderGo.transform.SetParent(container.transform, false);
                var sliderRt = sliderGo.AddComponent<RectTransform>();
                sliderRt.anchorMin = new Vector2(0.45f, 0.5f);
                sliderRt.anchorMax = new Vector2(1, 0.5f);
                sliderRt.offsetMin = Vector2.zero;
                sliderRt.offsetMax = Vector2.zero;
                
                var slider = sliderGo.AddComponent<Slider>();
                slider.minValue = 0f;
                slider.maxValue = 10f; // 10개 세그먼트
                slider.value = defaultValue * 10f; // 0.8 -> 8, 0.6 -> 6
                slider.wholeNumbers = true; // 정수값만
                
                // 슬라이더 배경 (10개 세그먼트)
                var bgGo = new GameObject("Background");
                bgGo.transform.SetParent(sliderGo.transform, false);
                var bgImg = bgGo.AddComponent<Image>();
                bgImg.color = new Color(0.3f, 0.3f, 0.3f, 0.9f);
                var bgRt = bgGo.GetComponent<RectTransform>();
                bgRt.anchorMin = Vector2.zero;
                bgRt.anchorMax = Vector2.one;
                bgRt.offsetMin = Vector2.zero;
                bgRt.offsetMax = Vector2.zero;
                
                // 슬라이더 채움 (세그먼트별)
                var fillGo = new GameObject("Fill");
                fillGo.transform.SetParent(sliderGo.transform, false);
                var fillImg = fillGo.AddComponent<Image>();
                fillImg.color = new Color(0.2f, 0.8f, 0.2f, 0.9f); // 초록색
                var fillRt = fillGo.GetComponent<RectTransform>();
                fillRt.anchorMin = Vector2.zero;
                fillRt.anchorMax = Vector2.one;
                fillRt.offsetMin = Vector2.zero;
                fillRt.offsetMax = Vector2.zero;
                
                // 슬라이더 핸들
                var handleGo = new GameObject("Handle");
                handleGo.transform.SetParent(sliderGo.transform, false);
                var handleImg = handleGo.AddComponent<Image>();
                handleImg.color = Color.white;
                var handleRt = handleGo.GetComponent<RectTransform>();
                handleRt.anchorMin = new Vector2(0.5f, 0.5f);
                handleRt.anchorMax = new Vector2(0.5f, 0.5f);
                handleRt.sizeDelta = new Vector2(25, 25);
                handleRt.anchoredPosition = Vector2.zero;
                
                // 슬라이더 컴포넌트 연결
                slider.targetGraphic = handleImg;
                slider.fillRect = fillRt;
                slider.handleRect = handleRt;
                
                // 레이아웃 요소
                var le = container.AddComponent<LayoutElement>();
                le.preferredHeight = 80f;
                le.preferredHeight = 0f;
                
                return slider;
            }
            
            // 기존 볼륨 슬라이더 생성 함수는 제거됨 (CreateGameOptionsVolumeSlider로 대체)
            
            // 게임옵션 스타일 사운드 설정 패널 생성 함수
            GameObject CreateGameOptionsSoundPanel(string name)
            {
                var panel = new GameObject(name);
                panel.transform.SetParent(settingsButtons.transform, false);
                
                // 패널 배경 (게임옵션 스타일)
                var panelImg = panel.AddComponent<Image>();
                panelImg.color = new Color(0.7f, 0.5f, 0.3f, 0.9f); // 더 밝은 갈색
                var panelRt = panel.GetComponent<RectTransform>();
                panelRt.sizeDelta = new Vector2(600, 250); // 옵션 창 안에 맞춤
                
                // 패널 내부 레이아웃
                var panelVlg = panel.AddComponent<VerticalLayoutGroup>();
                panelVlg.padding = new RectOffset(30, 30, 30, 30);
                panelVlg.spacing = 20f;
                panelVlg.childAlignment = TextAnchor.MiddleCenter;
                panelVlg.childControlWidth = true;
                panelVlg.childControlHeight = true;
                panelVlg.childForceExpandWidth = true;
                panelVlg.childForceExpandHeight = false;
                
                // 레이아웃 요소
                var panelLe = panel.AddComponent<LayoutElement>();
                panelLe.preferredHeight = 250f;
                panelLe.flexibleHeight = 0f;
                
                return panel;
            }
            
            // 구분선 생성 함수
            GameObject CreateSeparator(string name)
            {
                var separator = new GameObject(name);
                separator.transform.SetParent(settingsButtons.transform, false);
                var separatorImg = separator.AddComponent<Image>();
                separatorImg.color = new Color(1, 1, 1, 0.3f);
                var separatorRt = separator.GetComponent<RectTransform>();
                separatorRt.sizeDelta = new Vector2(400, 2);
                var separatorLe = separator.AddComponent<LayoutElement>();
                separatorLe.preferredHeight = 20f;
                separatorLe.flexibleHeight = 0f;
                return separator;
            }

            var bootGo = new GameObject("StartupLoader");
            var boot = bootGo.AddComponent<AFKS.Core.Systems.StartupLoader>();
            var bootSO = new SerializedObject(boot);
            bootSO.FindProperty("initialStageId").stringValue = "Menu";
            bootSO.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(newScene, coreScenePath);
            EditorUtility.DisplayDialog("완료", "코어 씬이 생성/갱신되었습니다.", "확인");
        }

        private void CreateOrUpdateCoreSceneAt(string path)
        {
            coreScenePath = path;
            CreateOrUpdateCoreScene();
        }

        private void CreateStageScene()
        {
            EnsureDirectory(stageFolder);
            string path = Path.Combine(stageFolder, stageName + ".unity");

            var stage = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            stage.name = stageName;

            var root = new GameObject("StageRoot");
            root.AddComponent<AFKS.Features.Stage.StageRoot>();

            // UI용 EventSystem(중복 시 Core에서 정리됨)
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();

            EditorSceneManager.SaveScene(stage, path);
            EditorUtility.DisplayDialog("완료", $"스테이지 씬 생성: {path}", "확인");
        }

        private void CreateOrOverwriteMenuScene()
        {
            string path = Path.Combine(scenesFolder, "Menu.unity");
            EnsureDirectory(Path.GetDirectoryName(path));
            var sc = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            sc.name = "Menu";

            // 카메라(단독 실행 시 Game 뷰 경고 방지)
            var camGo = new GameObject("Menu Camera");
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;

            // Canvas
            var canvasRoot = new GameObject("Canvas");
            var rt = canvasRoot.AddComponent<RectTransform>();
            var canvas = canvasRoot.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasRoot.AddComponent<CanvasScaler>();
            canvasRoot.AddComponent<GraphicRaycaster>();

            // UI/MainMenu 루트
            var ui = new GameObject("UI");
            ui.transform.SetParent(canvasRoot.transform, false);
            ui.AddComponent<RectTransform>();
            var menu = new GameObject("MainMenu");
            menu.transform.SetParent(ui.transform, false);
            menu.AddComponent<RectTransform>();
            var menuUI = menu.AddComponent<AFKS.Features.Menu.MainMenuUI>();

            // 배경 패널(메뉴 기본 배경) - Canvas의 직접 자식으로 설정, 맨 뒤로 보내기
            var bg = new GameObject("Panel_BG");
            bg.transform.SetParent(canvas.transform, false); // UI가 아닌 Canvas의 직접 자식
            bg.transform.SetAsFirstSibling(); // 맨 뒤로 보내서 버튼 뒤에 위치
            var img = bg.AddComponent<Image>();
            
            // Stage1 배경 이미지를 기본 메뉴 배경으로 사용 (쇠사슬이 있는 1_1)
            var stage1BgSprite = Resources.Load<Sprite>("Images/Backgrounds/Stage1/StageBG_1_1");
            if (stage1BgSprite != null)
            {
                img.sprite = stage1BgSprite;
                img.color = Color.white;
                Debug.Log($"Stage1 배경 이미지 로드됨: {stage1BgSprite.name}");
            }
            else
            {
                // 대체 이미지 시도
                var menuBgSprite = Resources.Load<Sprite>("Placeholders/MenuBG_Default");
                if (menuBgSprite != null)
                {
                    img.sprite = menuBgSprite;
                    img.color = Color.white;
                    Debug.Log($"대체 배경 이미지 로드됨: {menuBgSprite.name}");
                }
                else
                {
                    img.color = new Color(0, 0, 0, 0.65f);
                    Debug.LogWarning("배경 이미지를 찾을 수 없어 검은색 사용");
                }
            }
            
            // 해상도 문제 해결: 1536x1024 이미지를 Full HD에 단순하게 맞춤
            var bgImg = bg.GetComponent<Image>();
            bgImg.type = Image.Type.Simple; // Simple 타입으로 설정
            bgImg.preserveAspect = false; // 비율 유지 해제 (스케일링 충돌 방지)
            bgImg.raycastTarget = false; // 배경은 클릭 이벤트 방지
            
            // 이미지가 화면을 완전히 덮도록 강제 설정
            bgImg.color = Color.white; // 색상 흰색으로 설정
            bgImg.material = null; // 머티리얼 제거
            
            // 강제로 화면에 맞게 스케일 조정
            var bgRt = bg.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero; // 좌상단 앵커
            bgRt.anchorMax = Vector2.one;   // 우하단 앵커
            bgRt.offsetMin = Vector2.zero;  // 좌상단 오프셋
            bgRt.offsetMax = Vector2.zero;  // 우하단 오프셋
            
            // 실제로 잘 맞는 스케일 값 사용 (Inspector에서 확인된 값)
            // X=1.19, Y=1.0이 가장 자연스럽게 보임
            float optimalScaleX = 1.19f; // 실제로 잘 맞는 X 스케일
            float optimalScaleY = 1.0f;  // 실제로 잘 맞는 Y 스케일
            
            // 실제 테스트로 확인된 최적 스케일 적용
            bgRt.localScale = new Vector3(optimalScaleX, optimalScaleY, 1f);
            
            Debug.Log($"배경 이미지 최적 스케일링 적용: X={optimalScaleX:F2}, Y={optimalScaleY:F2} (실제 테스트로 확인된 값)");
            var bgMirror = bg.AddComponent<AFKS.Features.Menu.MainMenuBGMirror>();
            
            // MainMenuBGMirror에 Stage1 배경을 기본 배경으로 설정
            var soBgMirror = new SerializedObject(bgMirror);
            soBgMirror.FindProperty("defaultMenuBackground").objectReferenceValue = stage1BgSprite;
            soBgMirror.ApplyModifiedPropertiesWithoutUndo();

            // 버튼 컨테이너
            var buttons = new GameObject("Buttons");
            buttons.transform.SetParent(menu.transform, false);
            var brt = buttons.AddComponent<RectTransform>();
            brt.anchorMin = new Vector2(0.5f, 0.5f); // 중앙 앵커
            brt.anchorMax = new Vector2(0.5f, 0.5f);
            brt.anchoredPosition = Vector2.zero; // 중앙 위치
            brt.sizeDelta = new Vector2(400, 500); // 크기
            
            // MainMenu 객체 자체의 위치 조정 (이미지와 동일하게)
            var menuRt = menu.GetComponent<RectTransform>();
            menuRt.anchoredPosition = new Vector2(-400, -100); // 이미지와 동일한 위치
            menuRt.sizeDelta = new Vector2(400, 500); // 버튼 컨테이너와 동일한 크기
            var vlg = buttons.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(24, 24, 24, 24);
            vlg.spacing = 20f; vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true; vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true; vlg.childForceExpandHeight = false;

            // 생성 함수
            Button CreateButton(string name, string label, bool interactable = true)
            {
                var go = new GameObject(name);
                go.transform.SetParent(buttons.transform, false);
                var r = go.AddComponent<RectTransform>();
                var b = go.AddComponent<Button>();
                b.interactable = interactable;
                var imgBtn = go.AddComponent<Image>();
                imgBtn.color = new Color(1, 1, 1, 0.08f);
                var labelGo = new GameObject("Label");
                labelGo.transform.SetParent(go.transform, false);
                var tr = labelGo.AddComponent<RectTransform>();
                tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one; tr.offsetMin = Vector2.zero; tr.offsetMax = Vector2.zero;
                var tmp = labelGo.AddComponent<TextMeshProUGUI>();
                tmp.text = label; tmp.alignment = TextAlignmentOptions.Center; tmp.enableAutoSizing = true; tmp.fontSizeMin = 24; tmp.fontSizeMax = 40;
                var font = LoadTMPFontAsset(tmpFontAssetPath);
                if (font != null) tmp.font = font;
                tmp.raycastTarget = false;
                var le = go.AddComponent<LayoutElement>(); le.preferredHeight = 80f; le.flexibleHeight = 0f;
                return b;
            }

            // 버튼 생성 및 MainMenuUI 연결
            var btnContinue = CreateButton("Button_Continue", "계속하기");
            var btnNew = CreateButton("Button_New", "새 게임");
            var btnSettings = CreateButton("Button_Settings", "설정");
            var btnQuit = CreateButton("Button_Quit", "종료");

            // 설정창은 이제 Core 씬의 전역 설정창을 사용합니다.
            // Menu 씬에서는 설정 버튼만 남겨두고, 클릭 시 전역 설정창을 엽니다.

            var soMenu = new SerializedObject(menuUI);
            soMenu.FindProperty("newGameButton").objectReferenceValue = btnNew;
            soMenu.FindProperty("continueButton").objectReferenceValue = btnContinue;
            soMenu.FindProperty("settingsButton").objectReferenceValue = btnSettings;
            soMenu.FindProperty("quitButton").objectReferenceValue = btnQuit;
            soMenu.ApplyModifiedPropertiesWithoutUndo();

            // EventSystem
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();

            EditorSceneManager.SaveScene(sc, path);
            EditorUtility.DisplayDialog("완료", $"Menu.unity 생성/덮어쓰기 완료:\n{path}", "확인");
        }

        private void CreateOrOverwriteStage1()
        {
            string path = Path.Combine(scenesFolder, "Stage1.unity");
            EnsureDirectory(Path.GetDirectoryName(path));
            var sc = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            sc.name = "Stage1";

            // 카메라 추가 (Stage1 단독 실행 시 필요)
            var cam = new GameObject("Stage1 Camera");
            var camera = cam.AddComponent<Camera>();
            cam.tag = "MainCamera";
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            camera.orthographic = true;
            camera.orthographicSize = 5f;

            // 컨테이너
            var world = new GameObject("World");
            var interaction = new GameObject("Interaction");
            var services = new GameObject("Services");
            var ui = new GameObject("UI");
            
            // 배경 설정
            var bg = new GameObject("BG");
            bg.transform.SetParent(world.transform, false);
            var bgSprite = bg.AddComponent<SpriteRenderer>();
            
            // Stage1 배경 이미지 로드 (쇠사슬이 없는 1_2)
            var stage1BgSprite = Resources.Load<Sprite>("Images/Backgrounds/Stage1/StageBG_1_2");
            if (stage1BgSprite != null)
            {
                bgSprite.sprite = stage1BgSprite;
                Debug.Log($"Stage1 배경 이미지 설정됨: {stage1BgSprite.name}");
            }
            else
            {
                Debug.LogWarning("Stage1 배경 이미지를 찾을 수 없습니다");
                bgSprite.color = Color.gray; // 임시 색상
            }
            
            // 배경을 카메라 뒤로 보내기
            bg.transform.position = new Vector3(0, 0, 10);
            bgSprite.sortingOrder = -1;

            // Stage1Controller 추가
            var stage1Controller = bg.AddComponent<AFKS.Features.Stage.Stage1Controller>();
            
            // Stage1Controller에 배경 설정
            var soController = new SerializedObject(stage1Controller);
            soController.FindProperty("defaultBackground").objectReferenceValue = stage1BgSprite;
            
            // 해금 배경 (1_1 - 쇠사슬이 있는 상태)도 설정
            var stage1Bg1Sprite = Resources.Load<Sprite>("Images/Backgrounds/Stage1/StageBG_1_1");
            if (stage1Bg1Sprite != null)
            {
                soController.FindProperty("unlockedBackground").objectReferenceValue = stage1Bg1Sprite;
                Debug.Log($"Stage1 해금 배경 이미지 설정됨: {stage1Bg1Sprite.name}");
            }
            
            soController.ApplyModifiedPropertiesWithoutUndo();

            // Hotspots
            var hotspots = new GameObject("Hotspots");
            hotspots.transform.SetParent(interaction.transform, false);
            
            // Chain (체인 해금용)
            var chain = new GameObject("Chain");
            chain.transform.SetParent(hotspots.transform, false);
            var chainSr = chain.AddComponent<SpriteRenderer>();
            
            // 체인 이미지 로드
            var chainSprite = Resources.Load<Sprite>("Placeholders/Chain_Default");
            if (chainSprite != null)
            {
                chainSr.sprite = chainSprite;
            }
            else
            {
                chainSr.sprite = EnsurePlaceholderSprite(Path.Combine("Assets/Resources/Placeholders", "Interactable_Green.png"), new Color32(0, 200, 80, 255));
            }
            
            var chainCol = chain.AddComponent<BoxCollider2D>();
            chainCol.size = new Vector2(2f, 1f);
            chainCol.isTrigger = true;
            var chainController = chain.AddComponent<AFKS.Features.Interaction.ChainController>();
            var clickHandler = chain.AddComponent<AFKS.Features.Interaction.ClickHandler>();
            
            // Door (초기에는 비활성)
            var door = new GameObject("Door");
            door.transform.SetParent(hotspots.transform, false);
            var doorSr = door.AddComponent<SpriteRenderer>();
            doorSr.sprite = EnsurePlaceholderSprite(Path.Combine("Assets/Resources/Placeholders", "Interactable_Green.png"), new Color32(0, 200, 80, 255));
            var doorCol = door.AddComponent<BoxCollider2D>();
            doorCol.size = new Vector2(3f, 2f);
            doorCol.isTrigger = true;
            var move = door.AddComponent<AFKS.Features.Interaction.HotspotMove>();
            // HotspotMove의 targetStageId 필드에 직접 설정
            var soMove = new SerializedObject(move);
            var targetStageProp = soMove.FindProperty("targetStageId");
            if (targetStageProp != null)
            {
                targetStageProp.stringValue = "Stage2";
                soMove.ApplyModifiedPropertiesWithoutUndo();
            }
            door.SetActive(false); // 초기에는 비활성

            // StageRoot
            var root = new GameObject("StageRoot");
            root.transform.SetParent(services.transform, false);
            root.AddComponent<AFKS.Features.Stage.StageRoot>();

            // UI
            var mainCanvas = new GameObject("MainCanvas");
            mainCanvas.transform.SetParent(ui.transform, false);
            var canvas = mainCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;
            mainCanvas.AddComponent<CanvasScaler>();
            mainCanvas.AddComponent<GraphicRaycaster>();
            
            // CloseupPanel
            var closeupPanel = new GameObject("CloseupPanel");
            closeupPanel.transform.SetParent(mainCanvas.transform, false);
            var closeupImg = closeupPanel.AddComponent<Image>();
            closeupImg.color = Color.white;
            closeupImg.preserveAspect = true;
            closeupImg.raycastTarget = false;
            closeupPanel.AddComponent<AFKS.Core.UI.CloseupViewer>();

            // EventSystem
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();

            // Stage1Controller 참조 설정
            var soStage1Controller = new SerializedObject(stage1Controller);
            soStage1Controller.FindProperty("defaultBackground").objectReferenceValue = stage1BgSprite;
            soStage1Controller.FindProperty("chainObject").objectReferenceValue = chain;
            soStage1Controller.FindProperty("doorHotspot").objectReferenceValue = door;
            soStage1Controller.ApplyModifiedPropertiesWithoutUndo();

            // ChainController 이벤트 연결
            var soChain = new SerializedObject(chainController);
            var onChainUnlockedProp = soChain.FindProperty("onChainUnlocked");
            if (onChainUnlockedProp != null && onChainUnlockedProp.isArray)
            {
                // UnityEvent 배열이 비어있으면 크기를 1로 설정
                if (onChainUnlockedProp.arraySize == 0)
                {
                    onChainUnlockedProp.arraySize = 1;
                }
                
                var element = onChainUnlockedProp.GetArrayElementAtIndex(0);
                if (element != null)
                {
                    element.objectReferenceValue = stage1Controller;
                    var functionNameProp = element.FindPropertyRelative("m_FunctionName");
                    if (functionNameProp != null)
                    {
                        functionNameProp.stringValue = "UnlockStage";
                    }
                }
            }
            soChain.ApplyModifiedPropertiesWithoutUndo();
            
            // ClickHandler 이벤트 연결
            var soClick = new SerializedObject(clickHandler);
            var onClickProp = soClick.FindProperty("onClick");
            if (onClickProp != null && onClickProp.isArray)
            {
                // UnityEvent 배열이 비어있으면 크기를 1로 설정
                if (onClickProp.arraySize == 0)
                {
                    onClickProp.arraySize = 1;
                }
                
                var element = onClickProp.GetArrayElementAtIndex(0);
                if (element != null)
                {
                    element.objectReferenceValue = chainController;
                    var functionNameProp = element.FindPropertyRelative("m_FunctionName");
                    if (functionNameProp != null)
                    {
                        functionNameProp.stringValue = "OnChainClicked";
                    }
                }
            }
            soClick.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(sc, path);
            EditorUtility.DisplayDialog("완료", $"Stage1.unity 생성/덮어쓰기 완료:\n{path}", "확인");
        }

        private void CreateOrOverwriteStage2()
        {
            string path = Path.Combine(scenesFolder, "Stage2.unity");
            EnsureDirectory(Path.GetDirectoryName(path));
            var sc = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            sc.name = "Stage2";

            // 카메라 추가 (Stage2 단독 실행 시 필요)
            var cam = new GameObject("Stage2 Camera");
            var camera = cam.AddComponent<Camera>();
            cam.tag = "MainCamera";
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            camera.orthographic = true;
            camera.orthographicSize = 5f;

            var world = new GameObject("World");
            var interaction = new GameObject("Interaction");
            var services = new GameObject("Services");

            var bg = new GameObject("BG");
            bg.transform.SetParent(world.transform, false);
            var bgSr = bg.AddComponent<SpriteRenderer>();
            bgSr.sprite = EnsurePlaceholderSprite(Path.Combine("Assets/Resources/Placeholders", "BG_Default.png"), new Color32(30, 30, 30, 255));

            var hotspots = new GameObject("Hotspots");
            hotspots.transform.SetParent(interaction.transform, false);
            var backDoor = new GameObject("BackDoor");
            backDoor.transform.SetParent(hotspots.transform, false);
            var doorSr = backDoor.AddComponent<SpriteRenderer>();
            doorSr.sprite = EnsurePlaceholderSprite(Path.Combine("Assets/Resources/Placeholders", "Interactable_Green.png"), new Color32(0, 200, 80, 255));
            var col = backDoor.AddComponent<BoxCollider2D>();
            col.size = new Vector2(2f, 2f);
            var backDoorMove = backDoor.AddComponent<AFKS.Features.Interaction.HotspotMove>();
            
            // HotspotMove의 targetStageId 필드에 직접 설정
            var soBackDoorMove = new SerializedObject(backDoorMove);
            var backDoorTargetStageProp = soBackDoorMove.FindProperty("targetStageId");
            if (backDoorTargetStageProp != null)
            {
                backDoorTargetStageProp.stringValue = "Stage3";
                soBackDoorMove.ApplyModifiedPropertiesWithoutUndo();
            }

            // 예시: 비상호작용 오브젝트(빨강)
            var prop = new GameObject("Prop_Static");
            prop.transform.SetParent(world.transform, false);
            var propSr = prop.AddComponent<SpriteRenderer>();
            propSr.sprite = EnsurePlaceholderSprite(Path.Combine("Assets/Resources/Placeholders", "Static_Red.png"), new Color32(200, 40, 40, 255));

            var root = new GameObject("StageRoot");
            root.transform.SetParent(services.transform, false);
            root.AddComponent<AFKS.Features.Stage.StageRoot>();

            EditorSceneManager.SaveScene(sc, path);
            EditorUtility.DisplayDialog("완료", $"Stage2.unity 생성/덮어쓰기 완료:\n{path}", "확인");
        }

        private void CreateOrOverwriteStageGeneric(string sceneName, string nextStageId)
        {
            string path = Path.Combine(scenesFolder, sceneName + ".unity");
            EnsureDirectory(Path.GetDirectoryName(path));
            var sc = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            sc.name = sceneName;

            // 카메라 추가 (스테이지 단독 실행 시 필요)
            var cam = new GameObject($"{sceneName} Camera");
            var camera = cam.AddComponent<Camera>();
            cam.tag = "MainCamera";
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            camera.orthographic = true;
            camera.orthographicSize = 5f;

            var world = new GameObject("World");
            var interaction = new GameObject("Interaction");
            var services = new GameObject("Services");

            var bg = new GameObject("BG");
            bg.transform.SetParent(world.transform, false);
            var bgSr = bg.AddComponent<SpriteRenderer>();
            bgSr.sprite = EnsurePlaceholderSprite(Path.Combine("Assets/Resources/Placeholders", "BG_Default.png"), new Color32(30, 30, 30, 255));

            var hotspots = new GameObject("Hotspots");
            hotspots.transform.SetParent(interaction.transform, false);
            var door = new GameObject("Door");
            door.transform.SetParent(hotspots.transform, false);
            var doorSr = door.AddComponent<SpriteRenderer>();
            doorSr.sprite = EnsurePlaceholderSprite(Path.Combine("Assets/Resources/Placeholders", "Interactable_Green.png"), new Color32(0, 200, 80, 255));
            var col = door.AddComponent<BoxCollider2D>();
            col.size = new Vector2(2f, 2f);
            var move = door.AddComponent<AFKS.Features.Interaction.HotspotMove>();
            
            // HotspotMove의 targetStageId 필드에 직접 설정
            var soMove = new SerializedObject(move);
            var targetStageProp = soMove.FindProperty("targetStageId");
            if (targetStageProp != null)
            {
                targetStageProp.stringValue = nextStageId;
                soMove.ApplyModifiedPropertiesWithoutUndo();
            }

            var prop = new GameObject("Prop_Static");
            prop.transform.SetParent(world.transform, false);
            var propSr = prop.AddComponent<SpriteRenderer>();
            propSr.sprite = EnsurePlaceholderSprite(Path.Combine("Assets/Resources/Placeholders", "Static_Red.png"), new Color32(200, 40, 40, 255));

            var root = new GameObject("StageRoot");
            root.transform.SetParent(services.transform, false);
            root.AddComponent<AFKS.Features.Stage.StageRoot>();

            EditorSceneManager.SaveScene(sc, path);
            EditorUtility.DisplayDialog("완료", $"{sceneName}.unity 생성/덮어쓰기 완료:\n{path}", "확인");
        }

        private void GenerateAllAndRegisterBuildSettings()
        {
            CreateOrUpdateCoreSceneAt(Path.Combine(scenesFolder, "Core.unity"));
            CreateOrOverwriteMenuScene();
            CreateOrOverwriteStage1();
            CreateOrOverwriteStage2();
            CreateOrOverwriteStageGeneric("Stage3", "Stage4");
            CreateOrOverwriteStageGeneric("Stage4", "Stage5");
            CreateOrOverwriteStageGeneric("Stage5", "Stage6");
            CreateOrOverwriteStageGeneric("Stage6", "Menu");

            // Build Settings 등록
            var paths = new[]
            {
                Path.Combine(scenesFolder, "Core.unity"),
                Path.Combine(scenesFolder, "Menu.unity"),
                Path.Combine(scenesFolder, "Stage1.unity"),
                Path.Combine(scenesFolder, "Stage2.unity"),
                Path.Combine(scenesFolder, "Stage3.unity"),
                Path.Combine(scenesFolder, "Stage4.unity"),
                Path.Combine(scenesFolder, "Stage5.unity"),
                Path.Combine(scenesFolder, "Stage6.unity"),
            };
            var list = new System.Collections.Generic.List<EditorBuildSettingsScene>();
            for (int i = 0; i < paths.Length; i++)
            {
                string p = paths[i].Replace('\\', '/');
                if (File.Exists(p))
                {
                    list.Add(new EditorBuildSettingsScene(p, true));
                }
            }
            EditorBuildSettings.scenes = list.ToArray();
            EditorUtility.DisplayDialog("완료", "모든 씬 생성/덮어쓰기 및 Build Settings 등록이 완료되었습니다.", "확인");
        }

        /// <summary>
        /// 경로에 단색 PNG 스프라이트가 없으면 생성하고, Sprite로 로드해 반환합니다.
        /// </summary>
        private static Sprite EnsurePlaceholderSprite(string pngPath, Color color)
        {
            string dir = Path.GetDirectoryName(pngPath)?.Replace('\\', '/');
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            if (!File.Exists(pngPath))
            {
                var tex = new Texture2D(32, 32, TextureFormat.RGBA32, false);
                var pixels = new Color32[32 * 32];
                var c32 = (Color32)color;
                for (int i = 0; i < pixels.Length; i++) pixels[i] = c32;
                tex.SetPixels32(pixels);
                tex.Apply();
                var bytes = tex.EncodeToPNG();
                Object.DestroyImmediate(tex);
                File.WriteAllBytes(pngPath, bytes);
                AssetDatabase.ImportAsset(pngPath);
                var importer = AssetImporter.GetAtPath(pngPath) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spritePixelsPerUnit = 100;
                    importer.mipmapEnabled = false;
                    importer.alphaIsTransparency = true;
                    importer.SaveAndReimport();
                }
            }
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(pngPath);
            if (sprite == null)
            {
                // 만약 Sprite가 null이면 Texture2D를 불러 Sprite.Create로 구성
                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(pngPath);
                if (tex != null)
                {
                    sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
                }
            }
            return sprite;
        }

        private static TMP_FontAsset LoadTMPFontAsset(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath)) return null;
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
            return font;
        }

        private void LoadCoreAndStageForTest()
        {
            if (!File.Exists(coreScenePath))
            {
                EditorUtility.DisplayDialog("경고", "먼저 코어 씬을 생성/저장하세요.", "확인");
                return;
            }
            string stagePath = Path.Combine(stageFolder, stageName + ".unity");
            if (!File.Exists(stagePath))
            {
                EditorUtility.DisplayDialog("경고", "스테이지 씬을 먼저 생성하세요.", "확인");
                return;
            }

            EditorSceneManager.OpenScene(coreScenePath, OpenSceneMode.Single);
            EditorSceneManager.OpenScene(stagePath, OpenSceneMode.Additive);
            var scene = SceneManager.GetSceneByPath(stagePath);
            if (scene.IsValid()) SceneManager.SetActiveScene(scene);
        }

        private static void EnsureDirectory(string dir)
        {
            if (string.IsNullOrEmpty(dir)) return;
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        }
        #endregion
    }
}


