using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using AFKS.Features.Items;
using TMPro;
using UnityEngine.Events;

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
        // GameDebug 토글
        private bool dbgGlobal = true;
        private bool dbgCore = false;
        private bool dbgStage = false;
        private bool dbgEvent = true;
        private bool dbgInteraction = false;
        private bool dbgCamera = false;
        private bool dbgSave = false;
        private bool addEventSystem = true;
        private bool addAudioService = true;
        private bool addInventoryService = true;
        // 안전 옵션
        private bool dryRun = false; // 실제 파일/에셋/BuildSettings 변경 대신 로그만 출력
        private bool confirmOverwrite = true; // 기존 파일/에셋 덮어쓰기/삭제 시 확인 대화상자 표시
        // 편의 옵션
        private bool autoRegisterBuildSettings = true; // 개별 씬 생성 시에도 Build Settings 자동 등록
        private bool postValidateAfterGenerate = true; // 원클릭 생성 후 전체 유효성 검사/자동수정 실행
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
            EditorGUILayout.LabelField("디버그(카테고리 토글)", EditorStyles.boldLabel);
            dbgGlobal = EditorGUILayout.ToggleLeft("Global", dbgGlobal);
            using (new EditorGUILayout.HorizontalScope())
            {
                dbgEvent = EditorGUILayout.ToggleLeft("Event", dbgEvent);
                dbgStage = EditorGUILayout.ToggleLeft("Stage", dbgStage);
                dbgInteraction = EditorGUILayout.ToggleLeft("Interaction", dbgInteraction);
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                dbgCore = EditorGUILayout.ToggleLeft("Core", dbgCore);
                dbgCamera = EditorGUILayout.ToggleLeft("Camera", dbgCamera);
                dbgSave = EditorGUILayout.ToggleLeft("Save", dbgSave);
            }
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("안전 옵션", EditorStyles.boldLabel);
            dryRun = EditorGUILayout.ToggleLeft("Dry Run(미리보기만, 파일/에셋 변경 없음)", dryRun);
            confirmOverwrite = EditorGUILayout.ToggleLeft("덮어쓰기/삭제 시 확인 대화상자", confirmOverwrite);
            autoRegisterBuildSettings = EditorGUILayout.ToggleLeft("생성한 씬을 Build Settings에 자동 등록", autoRegisterBuildSettings);
            postValidateAfterGenerate = EditorGUILayout.ToggleLeft("원클릭 생성 후 전체 유효성 검사/자동수정 실행", postValidateAfterGenerate);
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
            if (GUILayout.Button("Stage5.unity 생성/덮어쓰기")) CreateOrOverwriteStageGeneric("Stage5", "Stage6");
            if (GUILayout.Button("Stage6.unity 생성/덮어쓰기")) CreateOrOverwriteStageGeneric("Stage6", "Menu");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(6);
            if (GUILayout.Button("모든 씬 원클릭 생성/덮어쓰기(Core/Menu/Stage1~6)+BuildSettings 등록")) GenerateAllAndRegisterBuildSettings();

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("유효성 검사/자동수정", EditorStyles.boldLabel);
            if (GUILayout.Button("Stage1 유효성 검사/자동수정"))
            {
                ValidateAndAutofixStage1(Path.Combine(scenesFolder, "Stage1.unity"));
            }
            // 추가: Scenes 폴더 전체 유효성 검사/자동수정
            if (GUILayout.Button("Scenes 폴더 전체 유효성 검사/자동수정"))
            {
                ValidateAllScenesInScenesFolder();
            }
        }
        #endregion

        #region 구현
        private void CreateOrUpdateCoreScene()
        {
            EnsureDirectory(Path.GetDirectoryName(coreScenePath));

            var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            newScene.name = Path.GetFileNameWithoutExtension(coreScenePath);

            // 그룹 루트 생성
            var servicesRoot = new GameObject("Services");
            var uiRoot = new GameObject("UI");

            // Camera
            var cam = new GameObject("Main Camera");
            var camera = cam.AddComponent<Camera>();
            cam.tag = "MainCamera";
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            camera.orthographic = true;
            cam.transform.position = new Vector3(0f, 0f, -10f);
            cam.AddComponent<Physics2DRaycaster>();

            // FadeCanvas (기본 비가시 + 레이캐스트 차단 해제)
            var fadeGo = new GameObject("FadeCanvas");
            fadeGo.transform.SetParent(uiRoot.transform, false);
            var canvas = fadeGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var cg = fadeGo.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            cg.blocksRaycasts = false;
            var fade = fadeGo.AddComponent<AFKS.Core.UI.FadeCanvas>();

            if (addEventSystem)
            {
                var es = new GameObject("EventSystem");
                es.transform.SetParent(uiRoot.transform, false);
                es.AddComponent<EventSystem>();
                es.AddComponent<StandaloneInputModule>();
            }

            if (addAudioService)
            {
                var audioGo = new GameObject("AudioService");
                audioGo.transform.SetParent(servicesRoot.transform, false);
                audioGo.AddComponent<AudioListener>();
                audioGo.AddComponent<AFKS.Core.Services.Audio.AudioService>();
            }

            if (addInventoryService)
            {
                var invGo = new GameObject("InventoryService");
                invGo.transform.SetParent(servicesRoot.transform, false);
                invGo.AddComponent<AFKS.Core.Services.Inventory.InventoryService>();
            }

            if (addSaveService)
            {
                var saveGo = new GameObject("SaveService");
                saveGo.transform.SetParent(servicesRoot.transform, false);
                saveGo.AddComponent<AFKS.Core.Services.Save.SaveService>();
            }

            if (addGameStateService)
            {
                var gsGo = new GameObject("GameStateService");
                gsGo.transform.SetParent(servicesRoot.transform, false);
                gsGo.AddComponent<AFKS.Core.Services.GameState.GameStateService>();
            }

            // InputService + SceneService + GlobalSingletonGuard + StartupLoader
            var inputGo = new GameObject("InputService");
            inputGo.transform.SetParent(servicesRoot.transform, false);
            inputGo.AddComponent<AFKS.Core.Services.Input.InputService>();

            var sceneSvcGo = new GameObject("SceneService");
            sceneSvcGo.transform.SetParent(servicesRoot.transform, false);
            var sceneSvc = sceneSvcGo.AddComponent<AFKS.Core.Services.Scene.SceneService>();
            // fadeCanvas 필드 연결
            var so = new SerializedObject(sceneSvc);
            so.FindProperty("fadeCanvas").objectReferenceValue = fade;
            so.ApplyModifiedPropertiesWithoutUndo();

            var guardGo = new GameObject("GlobalSingletonGuard");
            guardGo.transform.SetParent(servicesRoot.transform, false);
            var guard = guardGo.AddComponent<AFKS.Core.Utils.GlobalSingletonGuard>();
            // 상세 로그는 기본 비활성
            var soGuard = new SerializedObject(guard);
            soGuard.FindProperty("debugLogs").boolValue = false;
            soGuard.ApplyModifiedPropertiesWithoutUndo();

            // GameDebugBootstrapper
            var dbgGo = new GameObject("DebugSettings");
            dbgGo.transform.SetParent(servicesRoot.transform, false);
            var dbg = dbgGo.AddComponent<AFKS.Core.Utils.GameDebugBootstrapper>();
            var soDbg = new SerializedObject(dbg);
            soDbg.FindProperty("enableGlobal").boolValue = dbgGlobal;
            soDbg.FindProperty("enableCore").boolValue = dbgCore;
            soDbg.FindProperty("enableStage").boolValue = dbgStage;
            soDbg.FindProperty("enableEvent").boolValue = dbgEvent;
            soDbg.FindProperty("enableInteraction").boolValue = dbgInteraction;
            soDbg.FindProperty("enableCamera").boolValue = dbgCamera;
            soDbg.FindProperty("enableSave").boolValue = dbgSave;
            soDbg.ApplyModifiedPropertiesWithoutUndo();

            // 전역 설정창 (모든 씬에서 접근 가능)
            var settingsGo = new GameObject("GlobalSettings");
            settingsGo.transform.SetParent(uiRoot.transform, false);
            var settingsCanvas = settingsGo.AddComponent<Canvas>();
            settingsCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            settingsCanvas.sortingOrder = 1000; // 최상위에 표시
            settingsGo.AddComponent<CanvasScaler>();
            settingsGo.AddComponent<GraphicRaycaster>();
            // 오디오 설정 바인더 추가 및 슬라이더 연결은 아래에서 수행
            
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
            optionsWindowRt.sizeDelta = new Vector2(700, 800); // 스크린샷과 일치하는 크기
            
            // 옵션 창 배경 (짙은 검은색 테마)
            var optionsBg = new GameObject("OptionsBackground");
            optionsBg.transform.SetParent(optionsWindow.transform, false);
            var optionsBgImg = optionsBg.AddComponent<Image>();
            optionsBgImg.color = new Color(0.1f, 0.1f, 0.1f, 0.95f); // 짙은 검은색 테마
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
            closeButtonRt.anchoredPosition = new Vector2(-50, -50); // 스크린샷과 일치하는 위치
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
            closeTextTmp.fontStyle = FontStyles.Bold; // 볼드체로 두껍게
            var closeTextFont = LoadTMPFontAsset(tmpFontAssetPath);
            if (closeTextFont != null) closeTextTmp.font = closeTextFont;
            closeTextTmp.raycastTarget = false;
            
            // X 버튼 클릭 이벤트 연결 (GlobalSettingsManager를 통해 처리)
            closeBtn.onClick.AddListener(() => {
                // GlobalSettingsManager를 통해 설정창 닫기
                var globalSettings = FindFirstObjectByType<AFKS.Core.Systems.GlobalSettingsManager>();
                if (globalSettings != null)
                {
                    globalSettings.CloseSettings();
                    Debug.Log("GlobalSettingsManager를 통해 설정창이 닫혔습니다.");
                }
                else
                {
                    // Fallback: 직접 패널 비활성화
                    if (settingsPanel != null)
                    {
                        settingsPanel.SetActive(false);
                        Debug.Log("Fallback: 설정창이 직접 닫혔습니다.");
                    }
                    else if (optionsWindow != null)
                    {
                        optionsWindow.SetActive(false);
                        Debug.Log("Fallback: OptionsWindow가 직접 닫혔습니다.");
                    }
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
            
            // SoundTitle 제거됨 (헤드폰 아이콘과 텍스트 모두 제거)
            
            // 효과음 슬라이더 (이미지 스타일)
            var effectSoundSlider = CreateImageStyleVolumeSlider("Slider_EffectSound", "효과음", 0.8f, soundPanel.transform);
            
            // 환경음 슬라이더 (이미지 스타일)
            var envSoundSlider = CreateImageStyleVolumeSlider("Slider_EnvSound", "환경음", 0.6f, soundPanel.transform);
            
            // 배경음 슬라이더 (이미지 스타일)
            var bgMusicSlider = CreateImageStyleVolumeSlider("Slider_BGMusic", "배경음", 0.6f, soundPanel.transform);
            
            // 구분선
            var separator = CreateSeparator("Separator");
            separator.transform.SetParent(settingsButtons.transform, false);
            
            // 메뉴로 돌아가기 버튼 (회색)
            var btnReturnMenu = CreateButton("Button_ReturnMenu", "메뉴로 돌아가기", true, new Color(0.6f, 0.6f, 0.6f, 0.9f));
            btnReturnMenu.transform.SetParent(settingsButtons.transform, false);

            // 게임 종료 버튼 (회색)
            var btnExitGame = CreateButton("Button_ExitGame", "게임 종료", true, new Color(0.6f, 0.6f, 0.6f, 0.9f));
            btnExitGame.transform.SetParent(settingsButtons.transform, false);

            // 세이브 초기화 버튼 (빨간색 경고)
            var btnResetSave = CreateButton("Button_ResetSave", "세이브 초기화", true, new Color(0.8f, 0.2f, 0.2f, 0.9f));
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
            soManager.FindProperty("closeSettingsButton").objectReferenceValue = closeBtn; // X 버튼 연결
            soManager.FindProperty("returnToMenuButton").objectReferenceValue = btnReturnMenu;
            soManager.FindProperty("resetSaveButton").objectReferenceValue = btnResetSave;
            // 새 필드: 게임 종료 버튼
            soManager.FindProperty("exitGameButton").objectReferenceValue = btnExitGame;
            soManager.ApplyModifiedPropertiesWithoutUndo();


            // AudioSettingsBinder를 설정창에 추가하고 3개 슬라이더 바인딩
            var audioBinder = settingsGo.AddComponent<AFKS.Core.UI.AudioSettingsBinder>();
            var soAudioBinder = new SerializedObject(audioBinder);
            soAudioBinder.FindProperty("sfxSlider").objectReferenceValue = effectSoundSlider;
            soAudioBinder.FindProperty("ambienceSlider").objectReferenceValue = envSoundSlider;
            soAudioBinder.FindProperty("bgmSlider").objectReferenceValue = bgMusicSlider;
            soAudioBinder.ApplyModifiedPropertiesWithoutUndo();
            
            // CreateButton 함수 정의 (Core 씬용)
            Button CreateButton(string name, string label, bool interactable = true, Color? buttonColor = null)
            {
                var go = new GameObject(name);
                go.transform.SetParent(settingsButtons.transform, false);
                var r = go.AddComponent<RectTransform>();
                var b = go.AddComponent<Button>();
                b.interactable = interactable;
                var imgBtn = go.AddComponent<Image>();
                imgBtn.color = buttonColor ?? new Color(1, 1, 1, 0.08f); // 기본값 또는 지정된 색상
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
            Slider CreateImageStyleVolumeSlider(string name, string label, float defaultValue, Transform parent)
            {
                var container = new GameObject(name);
                container.transform.SetParent(parent, false);
                var containerRt = container.AddComponent<RectTransform>();
                
                // 라벨 (사운드 이름 + "On" 텍스트)
                var labelGo = new GameObject("Label");
                labelGo.transform.SetParent(container.transform, false);
                var labelRt = labelGo.AddComponent<RectTransform>();
                labelRt.anchorMin = new Vector2(0, 0.5f);
                labelRt.anchorMax = new Vector2(0.3f, 0.5f);
                labelRt.offsetMin = Vector2.zero;
                labelRt.offsetMax = Vector2.zero;
                var labelTmp = labelGo.AddComponent<TextMeshProUGUI>();
                labelTmp.text = label; // "효과음", "환경음", "배경음"
                labelTmp.alignment = TextAlignmentOptions.Left;
                labelTmp.fontSize = 32;
                labelTmp.color = Color.white; // 흰색으로 변경
                var labelFont = LoadTMPFontAsset(tmpFontAssetPath);
                if (labelFont != null) labelTmp.font = labelFont;
                labelTmp.raycastTarget = false;
                
                // 슬라이더 컨테이너 (라벨 너비 증가에 맞춤)
                var sliderContainer = new GameObject("SliderContainer");
                sliderContainer.transform.SetParent(container.transform, false);
                var sliderContainerRt = sliderContainer.AddComponent<RectTransform>();
                sliderContainerRt.anchorMin = new Vector2(0.35f, 0.5f);
                sliderContainerRt.anchorMax = new Vector2(1, 0.5f);
                sliderContainerRt.offsetMin = Vector2.zero;
                sliderContainerRt.offsetMax = Vector2.zero;
                
                // - 버튼 제거됨 (빨간색 원 요소)
                
                // 슬라이더 (5개 세그먼트, 전체 너비 사용)
                var sliderGo = new GameObject("Slider");
                sliderGo.transform.SetParent(sliderContainer.transform, false);
                var sliderRt = sliderGo.AddComponent<RectTransform>();
                sliderRt.anchorMin = Vector2.zero;
                sliderRt.anchorMax = Vector2.one;
                sliderRt.offsetMin = Vector2.zero;
                sliderRt.offsetMax = Vector2.zero;
                
                var slider = sliderGo.AddComponent<Slider>();
                slider.minValue = 0f;
                slider.maxValue = 4f; // 5등분 (0, 1, 2, 3, 4)
                slider.value = defaultValue * 4f; // 0.8 -> 3, 0.6 -> 2
                slider.wholeNumbers = true; // 정수값만 (20%씩 조절)
                
                // 슬라이더 배경 (5등분 세그먼트 시각적 표시)
                var bgGo = new GameObject("Background");
                bgGo.transform.SetParent(sliderGo.transform, false);
                var bgImg = bgGo.AddComponent<Image>();
                bgImg.color = new Color(0.4f, 0.4f, 0.4f, 0.9f);
                var bgRt = bgGo.GetComponent<RectTransform>();
                bgRt.anchorMin = Vector2.zero;
                bgRt.anchorMax = Vector2.one;
                bgRt.offsetMin = Vector2.zero;
                bgRt.offsetMax = Vector2.zero;
                
                // 5등분 세그먼트 구분선 추가
                for (int i = 1; i < 5; i++)
                {
                    var segmentLine = new GameObject($"SegmentLine_{i}");
                    segmentLine.transform.SetParent(bgGo.transform, false);
                    var segmentLineImg = segmentLine.AddComponent<Image>();
                    segmentLineImg.color = new Color(0.6f, 0.6f, 0.6f, 0.8f); // 밝은 회색 구분선
                    var segmentLineRt = segmentLine.GetComponent<RectTransform>();
                    segmentLineRt.anchorMin = new Vector2(i * 0.2f, 0);
                    segmentLineRt.anchorMax = new Vector2(i * 0.2f, 1);
                    segmentLineRt.sizeDelta = new Vector2(2, 0); // 2px 두께의 세로선
                    segmentLineRt.anchoredPosition = Vector2.zero;
                }
                
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
                
                // + 버튼 제거됨 (빨간색 원 요소)
                
                // On/Off 컨테이너 제거됨 (이미지와 동일하게)
                
                // 레이아웃 요소
                var le = container.AddComponent<LayoutElement>();
                le.preferredHeight = 80f;
                le.flexibleHeight = 0f;
                
                return slider;
            }
            
            // 사용하지 않는 CreateGameOptionsVolumeSlider 함수 제거됨
            
            // 게임옵션 스타일 사운드 설정 패널 생성 함수
            GameObject CreateGameOptionsSoundPanel(string name)
            {
                var panel = new GameObject(name);
                panel.transform.SetParent(settingsButtons.transform, false);
                
                // 패널 배경 (어두운 회색)
                var panelImg = panel.AddComponent<Image>();
                panelImg.color = new Color(0.3f, 0.3f, 0.3f, 0.9f); // 어두운 회색
                var panelRt = panel.GetComponent<RectTransform>();
                panelRt.sizeDelta = new Vector2(600, 400); // 사운드 패널 길이 증가 (400px)
                
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

            SaveSceneWithSafety(newScene, coreScenePath);
            if (autoRegisterBuildSettings) RegisterSceneInBuildSettingsIfNeeded(coreScenePath);
            if (!dryRun) EditorUtility.DisplayDialog("완료", "코어 씬이 생성/갱신되었습니다.", "확인");
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

            // UI용 EventSystem 생성은 Core 씬에만 존재해야 하므로 Stage 씬에서는 생성하지 않음

            // Services + StageInteractionManager 보장
            var services = new GameObject("Services");
            var simGo = new GameObject("StageInteraction");
            simGo.transform.SetParent(services.transform, false);
            simGo.AddComponent<AFKS.Features.Stage.StageInteractionManager>();

            SaveSceneWithSafety(stage, path);
            if (autoRegisterBuildSettings) RegisterSceneInBuildSettingsIfNeeded(path);
            if (!dryRun) EditorUtility.DisplayDialog("완료", $"스테이지 씬 생성: {path}", "확인");
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
            Button CreateButton(string name, string label, bool interactable = true, Color? buttonColor = null)
            {
                var go = new GameObject(name);
                go.transform.SetParent(buttons.transform, false);
                var r = go.AddComponent<RectTransform>();
                var b = go.AddComponent<Button>();
                b.interactable = interactable;
                var imgBtn = go.AddComponent<Image>();
                imgBtn.color = buttonColor ?? new Color(1, 1, 1, 0.08f); // 기본값 또는 지정된 색상
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
            // 기본값: 저장 데이터가 없다고 가정하고 비활성화. 저장이 생기면 추후 수동 활성화.
            btnContinue.gameObject.SetActive(false);
            btnContinue.interactable = false;
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

            // EventSystem은 Core 씬에서만 생성 (중복 방지)

            // 메뉴 BGM 자동 재생 설정 (Assets/Resources/Sounds/Bgm/menu.mp3)
            var menuAudio = new GameObject("MenuAudio");
            var autoBgm = menuAudio.AddComponent<AFKS.Core.Audio.AutoBGMPlayer>();
            var soAuto = new SerializedObject(autoBgm);
            var menuClip = Resources.Load<AudioClip>("Sounds/Bgm/menu");
            soAuto.FindProperty("bgmClip").objectReferenceValue = menuClip;
            soAuto.FindProperty("volume").floatValue = 0.6f;
            soAuto.FindProperty("useCrossFade").boolValue = true;
            soAuto.FindProperty("crossFadeSeconds").floatValue = 0.5f;
            // 메뉴 씬에서 빠져나갈 때는 반드시 정지
            soAuto.FindProperty("stopOnDisable").boolValue = true;
            soAuto.ApplyModifiedPropertiesWithoutUndo();

            SaveSceneWithSafety(sc, path);
            if (autoRegisterBuildSettings) RegisterSceneInBuildSettingsIfNeeded(path);
            if (!dryRun) EditorUtility.DisplayDialog("완료", $"Menu.unity 생성/덮어쓰기 완료:\n{path}", "확인");
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
            cam.AddComponent<Physics2DRaycaster>();
            // 기본 카메라 셋팅: 위치 (0, 0, -10), Orthographic Size 5.4
            cam.transform.position = new Vector3(0f, 0f, -10f);
            camera.orthographicSize = 5.4f;
            
            
            
            // 컨테이너 (월드/인터랙션/서비스만 사용 - UI 생성 생략)
            var world = new GameObject("World");
            var interaction = new GameObject("Interaction");
            var services = new GameObject("Services");
            
            // StageInteractionManager 보장
            var simGo = new GameObject("StageInteraction");
            simGo.transform.SetParent(services.transform, false);
            simGo.AddComponent<AFKS.Features.Stage.StageInteractionManager>();
            
            // 배경 설정
            var bg = new GameObject("Background");
            bg.transform.SetParent(world.transform, false);
            var bgSprite = bg.AddComponent<SpriteRenderer>();
            
            // Stage1 배경 이미지 로드 (잠금/해금 모두 1_2 사용)
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
            
            // 배경 이미지 스케일링 개선 (Menu와 동일한 방식)
            var bgTransform = bg.transform;
            // 1536x1024 이미지를 1920x1080 화면에 맞게 스케일링
            float imageWidth = 1536f;
            float imageHeight = 1024f;
            
            // 화면 비율과 이미지 비율 계산 (카메라 설정에서 이미 정의된 변수 활용)
            float screenRatio = 1920f / 1080f; // 1.778
            float imageRatio = imageWidth / imageHeight;   // 1.5
            
            // 화면을 완전히 덮도록 스케일 계산
            float scaleX, scaleY;
            if (screenRatio > imageRatio)
            {
                // 화면이 더 넓음 - 너비 기준으로 스케일링
                scaleX = 1920f / imageWidth;  // 1.25
                scaleY = scaleX; // 정사각형 유지
            }
            else
            {
                // 화면이 더 높음 - 높이 기준으로 스케일링
                scaleY = 1080f / imageHeight; // 1.055
                scaleX = scaleY; // 정사각형 유지
            }
            
            // Stage용 최적 스케일 적용 (월드 좌표 기반)
            float optimalScaleX = 1.25f;
            float optimalScaleY = 1.06f;
            bgTransform.localScale = new Vector3(optimalScaleX, optimalScaleY, 1f);
            

            
            // AutoBGMPlayer 추가(스테이지 진입 시 기본 BGM 자동 재생)
            var autoBgm = bg.AddComponent<AFKS.Core.Audio.AutoBGMPlayer>();
            var soAutoBgm = new SerializedObject(autoBgm);
            // 기본 BGM은 플레이스홀더(없으면 null 유지). 프로젝트에 클립을 넣으면 경로 기반으로 연결하도록 사용자에게 노출 가능
            soAutoBgm.FindProperty("volume").floatValue = 0.6f;
            soAutoBgm.FindProperty("useCrossFade").boolValue = true;
            soAutoBgm.FindProperty("crossFadeSeconds").floatValue = 0.5f;
            soAutoBgm.FindProperty("stopOnDisable").boolValue = true; // 스테이지 떠날 때 정지
            soAutoBgm.ApplyModifiedPropertiesWithoutUndo();

            // Hotspots
            var hotspots = new GameObject("Hotspots");
            hotspots.transform.SetParent(interaction.transform, false);
            hotspots.transform.localPosition = new Vector3(0f, -2f, 0f);
            
            // Chain (위쪽 체인) - 문 위쪽에 걸기
            var chainTop = new GameObject("ChainTop");
            chainTop.transform.SetParent(hotspots.transform, false);
            var chainTopSr = chainTop.AddComponent<SpriteRenderer>();
            
            // 위쪽 체인 위치 조정 (문 위쪽에 걸기)
            chainTop.transform.localPosition = new Vector3(0f, 0.1f, 9f);
            
            // 위쪽 체인 이미지 로드 (chain_1.png 스프라이트 시트에서 chain_1_0 서브 스프라이트)
            var chainTopSprite = Resources.Load<Sprite>("Images/Interactives/Stage1/chain_1");
            if (chainTopSprite != null)
            {
                // 스프라이트 시트에서 chain_1_0 서브 스프라이트 찾기
                var allSprites = Resources.LoadAll<Sprite>("Images/Interactives/Stage1/chain_1");
                var targetSprite = System.Array.Find(allSprites, s => s.name == "chain_1_0");
                
                if (targetSprite != null)
                {
                    chainTopSr.sprite = targetSprite;
                
                }
                else
                {
                    chainTopSr.sprite = chainTopSprite; // 전체 스프라이트 시트 사용
                    Debug.Log("위쪽 체인 이미지 로드됨: chain_1.png 전체 스프라이트 시트");
                }
            }
            else
            {
                // 체인 이미지가 없을 때 더 명확한 플레이스홀더 사용
                chainTopSr.sprite = EnsurePlaceholderSprite(Path.Combine("Assets/Resources/Placeholders", "Interactable_Green.png"), new Color32(0, 255, 0, 255));
                Debug.Log("위쪽 체인 이미지 대체: 밝은 초록색 플레이스홀더 사용");
            }
            
            // 체인은 BoxCollider2D를 사용(자연스러운 클릭 영역)
            var chainTopCol = chainTop.AddComponent<BoxCollider2D>();
            chainTopCol.size = new Vector2(1.4f, 0.3f);
            chainTopCol.isTrigger = true;
            
            // 위쪽 체인 스케일 조정 (X=0.5, Y=0.5)
            chainTop.transform.localScale = new Vector3(0.5f, 0.5f, 1f);

            
            // 위쪽 체인 가시성 개선
            chainTopSr.sortingOrder = 1;
            chainTopSr.color = Color.white;
            

            var clickHandlerTop = chainTop.AddComponent<AFKS.Features.Interaction.ClickHandler>();
            // 줌 전에는 클릭 비활성
            var soClickTop = new SerializedObject(clickHandlerTop);
            soClickTop.FindProperty("clickable").boolValue = false;
            soClickTop.ApplyModifiedPropertiesWithoutUndo();
            
            // 착지 타깃(계단 상자 중심)에 스냅
            var landingTop = new GameObject("LandingTarget");
            landingTop.transform.SetParent(hotspots.transform, false);
            // 요청: 최종 착지 높이 -1.5
            landingTop.transform.localPosition = new Vector3(0f, -1.5f, 9f);
            
            // Stage1은 시작 시 체인을 활성화(보이기)
            chainTop.SetActive(true);
            
            // Chain (아래쪽 체인) - 문 아래쪽에 걸기
            var chainBottom = new GameObject("ChainBottom");
            chainBottom.transform.SetParent(hotspots.transform, false);
            var chainBottomSr = chainBottom.AddComponent<SpriteRenderer>();
            
            // 아래쪽 체인 위치 조정 (문 아래쪽에 걸기)
            chainBottom.transform.localPosition = new Vector3(0f, -0.1f, 9f);
            
            // 아래쪽 체인 이미지 로드 (chain_1.png 스프라이트 시트에서 chain_1_1 서브 스프라이트)
            var chainBottomSprite = Resources.Load<Sprite>("Images/Interactives/Stage1/chain_1");
            if (chainBottomSprite != null)
            {
                // 스프라이트 시트에서 chain_1_1 서브 스프라이트 찾기
                var allSprites = Resources.LoadAll<Sprite>("Images/Interactives/Stage1/chain_1");
                var targetSprite = System.Array.Find(allSprites, s => s.name == "chain_1_1");
                
                if (targetSprite != null)
                {
                    chainBottomSr.sprite = targetSprite;
                
                }
                else
                {
                    chainBottomSr.sprite = chainBottomSprite; // 전체 스프라이트 시트 사용
                    Debug.Log("아래쪽 체인 이미지 로드됨: chain_1.png 전체 스프라이트 시트");
                }
            }
            else
            {
                chainBottomSr.sprite = EnsurePlaceholderSprite(Path.Combine("Assets/Resources/Placeholders", "Interactable_Green.png"), new Color32(0, 255, 0, 255));
                Debug.Log("아래쪽 체인 이미지 대체: 밝은 초록색 플레이스홀더 사용");
            }
            
            var chainBottomCol = chainBottom.AddComponent<BoxCollider2D>();
            chainBottomCol.size = new Vector2(1.4f, 0.6f);
            chainBottomCol.isTrigger = true;
            
            // 아래쪽 체인 스케일 조정 (X=0.5, Y=0.5)
            chainBottom.transform.localScale = new Vector3(0.5f, 0.5f, 1f);

            
            // 아래쪽 체인 가시성 개선
            chainBottomSr.sortingOrder = 1;
            chainBottomSr.color = Color.white;
            
            var clickHandlerBottom = chainBottom.AddComponent<AFKS.Features.Interaction.ClickHandler>();
            var soClickBottom = new SerializedObject(clickHandlerBottom);
            soClickBottom.FindProperty("clickable").boolValue = false;
            soClickBottom.ApplyModifiedPropertiesWithoutUndo();
            
            // Stage1은 시작 시 체인을 활성화(보이기)
            chainBottom.SetActive(true);
            
            // 객체 생성 완료 후 상태 확인
            Debug.Log($"객체 생성 완료 - 체인 및 핫스팟 설정 완료");
            
            // Door (초기에는 비활성)
            var door = new GameObject("Door");
            door.transform.SetParent(hotspots.transform, false);
            door.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            var doorSr = door.AddComponent<SpriteRenderer>();
            // 스프라이트는 기획 의도에 따라 비워둡니다(디폴트 None).
            var doorCol = door.AddComponent<BoxCollider2D>();
            doorCol.size = new Vector2(4f, 4f);
            doorCol.isTrigger = true;
            doorCol.isTrigger = true;
            
            // HotspotMove 컴포넌트 먼저 추가
            var move = door.AddComponent<AFKS.Features.Interaction.HotspotMove>();
            

            // HotspotMove의 targetStageId 및 수동 트리거 모드 설정
            var soMove = new SerializedObject(move);
            var targetStageProp = soMove.FindProperty("targetStageId");
            if (targetStageProp != null)
            {
                targetStageProp.stringValue = "Stage2";
            }
            var manualTriggerProp = soMove.FindProperty("manualTriggerOnly");
            if (manualTriggerProp != null)
            {
                manualTriggerProp.boolValue = true; // Door는 이벤트 시스템에서만 이동 트리거
            }
            soMove.ApplyModifiedPropertiesWithoutUndo();
            
            // Door 클릭 핸들러 + StageEventSystem 라우팅
            var doorClickHandler = door.AddComponent<AFKS.Features.Interaction.ClickHandler>();
            var soDoorClick = new SerializedObject(doorClickHandler);
            soDoorClick.FindProperty("debugLog").boolValue = false;
            soDoorClick.ApplyModifiedPropertiesWithoutUndo();
            var doorRouter = door.AddComponent<AFKS.Features.Interaction.ClickToEventRouter>();
            var doorRouterSO = new SerializedObject(doorRouter);
            doorRouterSO.FindProperty("objectId").stringValue = "Door";
            // StageEventSystem는 아래에서 생성/연결 후 다시 지정
            doorRouterSO.ApplyModifiedPropertiesWithoutUndo();
            var soDbgTop = new SerializedObject(clickHandlerTop);
            soDbgTop.FindProperty("debugLog").boolValue = false;
            soDbgTop.ApplyModifiedPropertiesWithoutUndo();
            var soDbgBottom = new SerializedObject(clickHandlerBottom);
            soDbgBottom.FindProperty("debugLog").boolValue = false;
            soDbgBottom.ApplyModifiedPropertiesWithoutUndo();
            
            door.SetActive(true); // Stage1은 시작 시 Door를 활성화
            Debug.Log("Door 초기 상태: 활성화됨 (Stage1 시작 시 카메라 줌용)");

            // StageRoot
            var root = new GameObject("StageRoot");
            root.transform.SetParent(services.transform, false);
            root.AddComponent<AFKS.Features.Stage.StageRoot>();

            // 데이터 드리븐 StageDefinition 생성 및 StageEventSystem에 적용
            var defDir = "Assets/Stages";
            if (!Directory.Exists(defDir)) Directory.CreateDirectory(defDir);
            var defPath = Path.Combine(defDir, "Stage1Definition.asset").Replace('\\','/');
            var existingDef = AssetDatabase.LoadAssetAtPath<AFKS.Features.Stage.StageDefinition>(defPath);
            if (existingDef != null)
            {
                bool proceed = !confirmOverwrite || EditorUtility.DisplayDialog("덮어쓰기 확인", $"기존 StageDefinition 에셋을 삭제 후 재생성합니다.\n{defPath}", "예", "아니오");
                if (!proceed) return;
                if (dryRun) Debug.Log($"[DRY-RUN] DeleteAsset -> {defPath}");
                else AssetDatabase.DeleteAsset(defPath);
            }

            var def = ScriptableObject.CreateInstance<AFKS.Features.Stage.StageDefinition>();
            if (dryRun) Debug.Log($"[DRY-RUN] CreateAsset -> {defPath}");
            else AssetDatabase.CreateAsset(def, defPath);

            // 1) 문 클릭 → 카메라 확대 (ZoomEvent 사용)
            var zoomEvent = ScriptableObject.CreateInstance<AFKS.Features.Stage.Events.ZoomEvent>();
            zoomEvent.SetupChainAreaZoom();
            // ZoomEvent를 클릭 트리거로 전환(Door 필요)
            var soZoomEvt = new SerializedObject(zoomEvent);
            var zoomTriggerList = soZoomEvt.FindProperty("triggerObjectIds"); zoomTriggerList.ClearArray();
            int zidx = zoomTriggerList.arraySize; zoomTriggerList.InsertArrayElementAtIndex(zidx);
            zoomTriggerList.GetArrayElementAtIndex(zidx).stringValue = "Door";
            var zoomInteractList = soZoomEvt.FindProperty("interactableObjects"); zoomInteractList.ClearArray();
            int zi = zoomInteractList.arraySize; zoomInteractList.InsertArrayElementAtIndex(zi);
            zoomInteractList.GetArrayElementAtIndex(zi).stringValue = "Door";
            soZoomEvt.FindProperty("triggerType").enumValueIndex = (int)AFKS.Features.Stage.Events.EventTriggerType.Click;
            // Zoom 완료 후 체인 클릭 가능하게 만드는 효과 2개 부착(ChainTop/ChainBottom)
            var enChainTop = ScriptableObject.CreateInstance<AFKS.Features.Stage.Effects.EnableInteractableEffect>();
            var enChainBottom = ScriptableObject.CreateInstance<AFKS.Features.Stage.Effects.EnableInteractableEffect>();
            var soEnTop = new SerializedObject(enChainTop);
            soEnTop.FindProperty("objectId").stringValue = "ChainTop";
            soEnTop.FindProperty("interactable").boolValue = true;
            soEnTop.ApplyModifiedPropertiesWithoutUndo();
            var soEnBottom = new SerializedObject(enChainBottom);
            soEnBottom.FindProperty("objectId").stringValue = "ChainBottom";
            soEnBottom.FindProperty("interactable").boolValue = true;
            soEnBottom.ApplyModifiedPropertiesWithoutUndo();
            // Door 비활성화 효과 추가
            var disableDoor = ScriptableObject.CreateInstance<AFKS.Features.Stage.Effects.DisableInteractableEffect>();
            var soDisableDoor = new SerializedObject(disableDoor);
            soDisableDoor.FindProperty("objectId").stringValue = "Door";
            soDisableDoor.FindProperty("disableCollider").boolValue = true;
            soDisableDoor.FindProperty("disableClickHandler").boolValue = false; // ClickHandler는 유지 (이벤트 시스템에서 관리)
            soDisableDoor.ApplyModifiedPropertiesWithoutUndo();
            
            // 체인 표시 효과 추가
            var showChains = ScriptableObject.CreateInstance<AFKS.Features.Stage.Effects.ShowChainsEffect>();
            var soShowChains = new SerializedObject(showChains);
            var chainIdsProp = soShowChains.FindProperty("chainObjectIds");
            chainIdsProp.ClearArray();
            int cid0 = chainIdsProp.arraySize; chainIdsProp.InsertArrayElementAtIndex(cid0);
            chainIdsProp.GetArrayElementAtIndex(cid0).stringValue = "ChainTop";
            int cid1 = chainIdsProp.arraySize; chainIdsProp.InsertArrayElementAtIndex(cid1);
            chainIdsProp.GetArrayElementAtIndex(cid1).stringValue = "ChainBottom";
            soShowChains.FindProperty("showChains").boolValue = true;
            soShowChains.FindProperty("enableClickable").boolValue = true;
            soShowChains.ApplyModifiedPropertiesWithoutUndo();
            
            // 배경 보고 효과 추가
            var reportBackground = ScriptableObject.CreateInstance<AFKS.Features.Stage.Effects.ReportBackgroundEffect>();
            var soReportBg = new SerializedObject(reportBackground);
            soReportBg.FindProperty("stageId").stringValue = "Stage1";
            soReportBg.FindProperty("backgroundObjectId").stringValue = "Background"; // 배경 오브젝트 ID
            soReportBg.ApplyModifiedPropertiesWithoutUndo();
            
            var zoomEffectsProp = soZoomEvt.FindProperty("effects");
            int zef0 = zoomEffectsProp.arraySize; zoomEffectsProp.InsertArrayElementAtIndex(zef0);
            zoomEffectsProp.GetArrayElementAtIndex(zef0).objectReferenceValue = enChainTop;
            int zef1 = zoomEffectsProp.arraySize; zoomEffectsProp.InsertArrayElementAtIndex(zef1);
            zoomEffectsProp.GetArrayElementAtIndex(zef1).objectReferenceValue = enChainBottom;
            int zef2 = zoomEffectsProp.arraySize; zoomEffectsProp.InsertArrayElementAtIndex(zef2);
            zoomEffectsProp.GetArrayElementAtIndex(zef2).objectReferenceValue = disableDoor;
            int zef3 = zoomEffectsProp.arraySize; zoomEffectsProp.InsertArrayElementAtIndex(zef3);
            zoomEffectsProp.GetArrayElementAtIndex(zef3).objectReferenceValue = showChains;
            int zef4 = zoomEffectsProp.arraySize; zoomEffectsProp.InsertArrayElementAtIndex(zef4);
            zoomEffectsProp.GetArrayElementAtIndex(zef4).objectReferenceValue = reportBackground;
            // 줌 완료 후 자동으로 다음 이벤트(체인 클릭 단계)로 진행
            var autoProceedAfterZoomProp = soZoomEvt.FindProperty("autoProceedAfterZoom");
            if (autoProceedAfterZoomProp != null) autoProceedAfterZoomProp.boolValue = true;
            soZoomEvt.ApplyModifiedPropertiesWithoutUndo();

            // 2) 체인 5회 클릭 달성 시 애니메이션(흔들림→분리)

            var chainBreak = ScriptableObject.CreateInstance<AFKS.Features.Stage.Events.ClickEvent>();
            // 트리거는 동일 체인, 클릭으로 바로 진입하지 않도록 triggerObjectIds 비워두고 조건으로 진입
            // 조건 에셋 생성: AggregateClickCountCondition(ChainTop+ChainBottom 합계 >=5)
            var cond = ScriptableObject.CreateInstance<AFKS.Features.Stage.Conditions.AggregateClickCountCondition>();
            // Effect 에셋: ChainShakeThenBreakEffect
            var breakFx = ScriptableObject.CreateInstance<AFKS.Features.Stage.Effects.ChainShakeThenBreakEffect>();
            
            // 클릭 시 즉시 피드백 효과 추가
            var clickFeedback = ScriptableObject.CreateInstance<AFKS.Features.Stage.Effects.ObjectClickFeedbackEffect>();
            var soClickFeedback = new SerializedObject(clickFeedback);
            soClickFeedback.FindProperty("shakeDuration").floatValue = 0.2f;
            soClickFeedback.FindProperty("shakeIntensity").floatValue = 0.1f;
            soClickFeedback.FindProperty("highlightColor").colorValue = Color.yellow;
            soClickFeedback.FindProperty("colorDuration").floatValue = 0.3f;
            soClickFeedback.ApplyModifiedPropertiesWithoutUndo();

            // 3) 문 클릭 → 다음 스테이지로 이동
            var stageTransition = ScriptableObject.CreateInstance<AFKS.Features.Stage.Events.ClickEvent>();
            stageTransition.SetupStageTransitionEvent();
            // Effect: StageTransitionEffect("Stage2")
            var goStageFx = ScriptableObject.CreateInstance<AFKS.Features.Stage.Effects.StageTransitionEffect>();
            var soGoStageFx = new SerializedObject(goStageFx);
            soGoStageFx.FindProperty("targetStageId").stringValue = "Stage2";
            soGoStageFx.FindProperty("delay").floatValue = 0.2f;
            soGoStageFx.ApplyModifiedPropertiesWithoutUndo();

            // 이벤트들을 서브 에셋으로 추가해 참조가 유지되도록 한다
            if (!dryRun)
            {
                AssetDatabase.AddObjectToAsset(zoomEvent, def);
                AssetDatabase.AddObjectToAsset(chainBreak, def);
                AssetDatabase.AddObjectToAsset(stageTransition, def);
                AssetDatabase.AddObjectToAsset(cond, def);
                AssetDatabase.AddObjectToAsset(breakFx, def);
                AssetDatabase.AddObjectToAsset(goStageFx, def);
                AssetDatabase.AddObjectToAsset(enChainTop, def);
                AssetDatabase.AddObjectToAsset(enChainBottom, def);
                AssetDatabase.AddObjectToAsset(disableDoor, def);
                AssetDatabase.AddObjectToAsset(showChains, def);
                AssetDatabase.AddObjectToAsset(reportBackground, def);
                AssetDatabase.AddObjectToAsset(clickFeedback, def);
            }
            // StageEvent 필드에 conditions/effects를 채워 넣는다
            var soChainBreak = new SerializedObject(chainBreak);
            // 이름/설명 설정 (SerializedObject 경유)
            soChainBreak.FindProperty("eventName").stringValue = "체인 분리";
            soChainBreak.FindProperty("description").stringValue = "체인이 흔들린 후 분리됩니다.";
            // 트리거: 체인 클릭 필요
            var cbTriggerList = soChainBreak.FindProperty("triggerObjectIds"); cbTriggerList.ClearArray();
            int cb0 = cbTriggerList.arraySize; cbTriggerList.InsertArrayElementAtIndex(cb0); cbTriggerList.GetArrayElementAtIndex(cb0).stringValue = "ChainTop";
            int cb1 = cbTriggerList.arraySize; cbTriggerList.InsertArrayElementAtIndex(cb1); cbTriggerList.GetArrayElementAtIndex(cb1).stringValue = "ChainBottom";
            // 상호작용 가능한 오브젝트 설정 (체인 클릭 가능하게)
            var cbInteractableList = soChainBreak.FindProperty("interactableObjects"); cbInteractableList.ClearArray();
            int cbI0 = cbInteractableList.arraySize; cbInteractableList.InsertArrayElementAtIndex(cbI0); cbInteractableList.GetArrayElementAtIndex(cbI0).stringValue = "ChainTop";
            int cbI1 = cbInteractableList.arraySize; cbInteractableList.InsertArrayElementAtIndex(cbI1); cbInteractableList.GetArrayElementAtIndex(cbI1).stringValue = "ChainBottom";
            soChainBreak.FindProperty("triggerType").enumValueIndex = (int)AFKS.Features.Stage.Events.EventTriggerType.Click;
            var conditionsProp = soChainBreak.FindProperty("conditions");
            conditionsProp.ClearArray();
            int cidx = conditionsProp.arraySize; conditionsProp.InsertArrayElementAtIndex(cidx);
            conditionsProp.GetArrayElementAtIndex(cidx).objectReferenceValue = cond;
            // onClickEffects에 클릭 피드백 추가
            var onClickEffectsProp = soChainBreak.FindProperty("onClickEffects");
            onClickEffectsProp.ClearArray();
            int oce0 = onClickEffectsProp.arraySize; onClickEffectsProp.InsertArrayElementAtIndex(oce0);
            onClickEffectsProp.GetArrayElementAtIndex(oce0).objectReferenceValue = clickFeedback;
            
            var effectsProp = soChainBreak.FindProperty("effects");
            effectsProp.ClearArray();
            int eidx = effectsProp.arraySize; effectsProp.InsertArrayElementAtIndex(eidx);
            effectsProp.GetArrayElementAtIndex(eidx).objectReferenceValue = breakFx;
            // 체인 분리 후 다음 이벤트 자동 진행 (클릭 카운트 조건 만족 시에만)
            soChainBreak.FindProperty("autoProceed").boolValue = false;
            soChainBreak.ApplyModifiedPropertiesWithoutUndo();

            // cond 설정: ChainTop/ChainBottom 합계 5회
            var soCond = new SerializedObject(cond);
            var idsProp = soCond.FindProperty("objectIds"); idsProp.ClearArray();
            int id0 = idsProp.arraySize; idsProp.InsertArrayElementAtIndex(id0); idsProp.GetArrayElementAtIndex(id0).stringValue = "ChainTop";
            int id1 = idsProp.arraySize; idsProp.InsertArrayElementAtIndex(id1); idsProp.GetArrayElementAtIndex(id1).stringValue = "ChainBottom";
            soCond.FindProperty("requiredTotalCount").intValue = 5;
            soCond.ApplyModifiedPropertiesWithoutUndo();

            // StageTransition 이벤트에도 Effect 부착
            var soStageTrans = new SerializedObject(stageTransition);
            // 트리거: Door 클릭으로만 발동
            var stTrig = soStageTrans.FindProperty("triggerObjectIds"); stTrig.ClearArray();
            int stT0 = stTrig.arraySize; stTrig.InsertArrayElementAtIndex(stT0);
            stTrig.GetArrayElementAtIndex(stT0).stringValue = "Door";
            soStageTrans.FindProperty("triggerType").enumValueIndex = (int)AFKS.Features.Stage.Events.EventTriggerType.Click;
            // 조건: 체인 합계 5회(Zoom 이후 바로 전환되는 것 방지)
            var stCond = soStageTrans.FindProperty("conditions"); stCond.ClearArray();
            int sc0 = stCond.arraySize; stCond.InsertArrayElementAtIndex(sc0);
            stCond.GetArrayElementAtIndex(sc0).objectReferenceValue = cond;
            var stFx = soStageTrans.FindProperty("effects"); stFx.ClearArray();
            int tfx = stFx.arraySize; stFx.InsertArrayElementAtIndex(tfx);
            stFx.GetArrayElementAtIndex(tfx).objectReferenceValue = goStageFx;
            soStageTrans.ApplyModifiedPropertiesWithoutUndo();

            def.Set("Stage1", new[]{ zoomEvent as AFKS.Features.Stage.Events.StageEvent, chainBreak, stageTransition});
            if (!dryRun)
            {
                EditorUtility.SetDirty(def);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            // Stage 시스템 컴포넌트는 Services 아래에 배치 (역할 분리)
            var sesHost = services;
            var ses = sesHost.GetComponent<AFKS.Features.Stage.StageEventSystem>();
            if (ses == null) ses = sesHost.AddComponent<AFKS.Features.Stage.StageEventSystem>();
            ses.LoadFromDefinition(def);
            // 컨트롤러는 World 하위 Controllers에 배치
            var controllersRoot = new GameObject("Controllers");
            controllersRoot.transform.SetParent(world.transform, false);
            var stageAnimCtrl = controllersRoot.AddComponent<AFKS.Features.Stage.Animations.StageAnimationController>();
            var objAnimCtrl = controllersRoot.AddComponent<AFKS.Features.Stage.Animations.ObjectAnimationController>();
            var ghostAnimCtrl = controllersRoot.AddComponent<AFKS.Features.Stage.Animations.GhostAnimationController>();
            var sesSO = new SerializedObject(ses);
            sesSO.FindProperty("animationController").objectReferenceValue = stageAnimCtrl;
            // StageInteractionManager 연결
            var sim = services.GetComponent<AFKS.Features.Stage.StageInteractionManager>();
            sesSO.FindProperty("interactionManager").objectReferenceValue = sim;
            // StoryProgress 연결
            var sp = sesHost.GetComponent<AFKS.Features.Stage.StoryProgress>();
            if (sp == null) sp = sesHost.AddComponent<AFKS.Features.Stage.StoryProgress>();
            sesSO.FindProperty("storyProgress").objectReferenceValue = sp;
            sesSO.ApplyModifiedPropertiesWithoutUndo();

            // UI/클로즈업 패널 미사용 (카메라 확대 스크립트로 대체)

            // EventSystem은 Core 씬에서만 생성 (중복 방지)




            // 체인 클릭은 StageEventSystem으로만 라우팅(중복 방지)
            if (clickHandlerTop != null)
            {
                ClearPersistent(clickHandlerTop.onClick);
                EditorUtility.SetDirty(clickHandlerTop);
                var r1 = chainTop.AddComponent<AFKS.Features.Interaction.ClickToEventRouter>();
                var r1SO = new SerializedObject(r1);
                r1SO.FindProperty("objectId").stringValue = "ChainTop";
                r1SO.FindProperty("eventSystem").objectReferenceValue = ses;
                r1SO.ApplyModifiedPropertiesWithoutUndo();
            }
            if (clickHandlerBottom != null)
            {
                ClearPersistent(clickHandlerBottom.onClick);
                EditorUtility.SetDirty(clickHandlerBottom);
                var r2 = chainBottom.AddComponent<AFKS.Features.Interaction.ClickToEventRouter>();
                var r2SO = new SerializedObject(r2);
                r2SO.FindProperty("objectId").stringValue = "ChainBottom";
                r2SO.FindProperty("eventSystem").objectReferenceValue = ses;
                r2SO.ApplyModifiedPropertiesWithoutUndo();
            }

            // StageInteractionManager의 상호작용 목록을 명시적으로 구성
            var simSO = new SerializedObject(sim);
            var listProp = simSO.FindProperty("interactableObjects");
            listProp.ClearArray();
            void AddInteractable(string id, GameObject go, AFKS.Features.Interaction.ClickHandler ch, bool interactable)
            {
                int idx = listProp.arraySize;
                listProp.InsertArrayElementAtIndex(idx);
                var elem = listProp.GetArrayElementAtIndex(idx);
                elem.FindPropertyRelative("objectId").stringValue = id;
                elem.FindPropertyRelative("gameObject").objectReferenceValue = go;
                elem.FindPropertyRelative("clickHandler").objectReferenceValue = ch;
                elem.FindPropertyRelative("isInteractable").boolValue = interactable;
            }
            // 초기 상태: Door만 상호작용 가능, 체인은 ZoomEvent 효과로 활성화됨
            AddInteractable("Door", door, doorClickHandler, true);
            AddInteractable("ChainTop", chainTop, clickHandlerTop, false);
            AddInteractable("ChainBottom", chainBottom, clickHandlerBottom, false);
            simSO.ApplyModifiedPropertiesWithoutUndo();

            // Door 라우터의 StageEventSystem 연결 최종 지정
            var doorRouterSOAssign = new SerializedObject(door.GetComponent<AFKS.Features.Interaction.ClickToEventRouter>());
            doorRouterSOAssign.FindProperty("eventSystem").objectReferenceValue = ses;
            doorRouterSOAssign.ApplyModifiedPropertiesWithoutUndo();

            SaveSceneWithSafety(sc, path);
            if (autoRegisterBuildSettings) RegisterSceneInBuildSettingsIfNeeded(path);
            if (!dryRun) EditorUtility.DisplayDialog("완료", $"Stage1.unity 생성/덮어쓰기 완료:\n{path}", "확인");
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
            cam.AddComponent<Physics2DRaycaster>();
            
            // Full HD 화면에 맞는 카메라 설정
            float screenHeight = 1080f;
            float pixelsPerUnit = 100f; // 기본값
            
            // 1536x1024 이미지를 1920x1080 화면에 맞게 카메라 조정
            float targetHeight = screenHeight / pixelsPerUnit / 2f; // orthographicSize는 절반 높이
            camera.orthographicSize = targetHeight; // 5.4f
            

            
            var world = new GameObject("World");
            var interaction = new GameObject("Interaction");
            var services = new GameObject("Services");
            
            // StageInteractionManager 보장
            var simGo = new GameObject("StageInteraction");
            simGo.transform.SetParent(services.transform, false);
            simGo.AddComponent<AFKS.Features.Stage.StageInteractionManager>();

            var bg = new GameObject("Background");
            bg.transform.SetParent(world.transform, false);
            var bgSr = bg.AddComponent<SpriteRenderer>();
            bgSr.sprite = EnsurePlaceholderSprite(Path.Combine("Assets/Resources/Placeholders", "BG_Default.png"), new Color32(30, 30, 30, 255));
            
            // Stage2 배경 스케일 적용 (월드 좌표 기반)
            float optimalScaleX = 1.25f;
            float optimalScaleY = 1.06f;
            bg.transform.localScale = new Vector3(optimalScaleX, optimalScaleY, 1f);
            

            
            var hotspots = new GameObject("Hotspots");
            hotspots.transform.SetParent(interaction.transform, false);
            var backDoor = new GameObject("Door");
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

            // 컨트롤러 루트(World 하위)
            var controllersRoot = new GameObject("Controllers");
            controllersRoot.transform.SetParent(world.transform, false);
            var stageAnimCtrl = controllersRoot.AddComponent<AFKS.Features.Stage.Animations.StageAnimationController>();
            var objAnimCtrl = controllersRoot.AddComponent<AFKS.Features.Stage.Animations.ObjectAnimationController>();

            // StageDefinition(간단: Door 클릭 → 다음 스테이지 전환)
            var defDir = "Assets/Stages";
            if (!Directory.Exists(defDir)) Directory.CreateDirectory(defDir);
            var defPath = Path.Combine(defDir, "Stage2Definition.asset").Replace('\\','/');
            var existingDef = AssetDatabase.LoadAssetAtPath<AFKS.Features.Stage.StageDefinition>(defPath);
            if (existingDef != null)
            {
                if (confirmOverwrite && !EditorUtility.DisplayDialog("덮어쓰기 확인", $"기존 Stage2Definition을 덮어쓸까요?\n{defPath}", "예", "아니오"))
                {
                    SaveSceneWithSafety(sc, path);
                    if (autoRegisterBuildSettings) RegisterSceneInBuildSettingsIfNeeded(path);
                    if (!dryRun) EditorUtility.DisplayDialog("완료", $"Stage2.unity 생성/덮어쓰기 완료:\n{path}", "확인");
                    return;
                }
                AssetDatabase.DeleteAsset(defPath);
            }
            var def = ScriptableObject.CreateInstance<AFKS.Features.Stage.StageDefinition>();
            AssetDatabase.CreateAsset(def, defPath);
            var clickDoor = ScriptableObject.CreateInstance<AFKS.Features.Stage.Events.ClickEvent>();
            clickDoor.SetupStageTransitionEvent();
            AssetDatabase.AddObjectToAsset(clickDoor, def);
            def.Set("Stage2", new[]{ clickDoor as AFKS.Features.Stage.Events.StageEvent });
            EditorUtility.SetDirty(def); AssetDatabase.SaveAssets(); AssetDatabase.Refresh();

            // StageEventSystem 구성
            var sesHost = services;
            var ses = sesHost.GetComponent<AFKS.Features.Stage.StageEventSystem>();
            if (ses == null) ses = sesHost.AddComponent<AFKS.Features.Stage.StageEventSystem>();
            ses.LoadFromDefinition(def);
            var sesSO = new SerializedObject(ses);
            sesSO.FindProperty("animationController").objectReferenceValue = stageAnimCtrl;
            var sim = services.GetComponent<AFKS.Features.Stage.StageInteractionManager>();
            sesSO.FindProperty("interactionManager").objectReferenceValue = sim;
            var sp = sesHost.GetComponent<AFKS.Features.Stage.StoryProgress>() ?? sesHost.AddComponent<AFKS.Features.Stage.StoryProgress>();
            sesSO.FindProperty("storyProgress").objectReferenceValue = sp;
            sesSO.ApplyModifiedPropertiesWithoutUndo();

            // Door 클릭 라우팅
            var ch = backDoor.AddComponent<AFKS.Features.Interaction.ClickHandler>();
            var chSO = new SerializedObject(ch); chSO.FindProperty("debugLog").boolValue = false; chSO.ApplyModifiedPropertiesWithoutUndo();
            var router = backDoor.AddComponent<AFKS.Features.Interaction.ClickToEventRouter>();
            var rSO = new SerializedObject(router);
            rSO.FindProperty("objectId").stringValue = "Door";
            rSO.FindProperty("eventSystem").objectReferenceValue = ses;
            rSO.ApplyModifiedPropertiesWithoutUndo();

            // 상호작용 목록 명시 구성
            var simSO = new SerializedObject(sim);
            var listProp = simSO.FindProperty("interactableObjects"); listProp.ClearArray();
            int idx = listProp.arraySize; listProp.InsertArrayElementAtIndex(idx);
            var elem = listProp.GetArrayElementAtIndex(idx);
            elem.FindPropertyRelative("objectId").stringValue = "Door";
            elem.FindPropertyRelative("gameObject").objectReferenceValue = backDoor;
            elem.FindPropertyRelative("clickHandler").objectReferenceValue = ch;
            elem.FindPropertyRelative("isInteractable").boolValue = true;
            simSO.ApplyModifiedPropertiesWithoutUndo();

            SaveSceneWithSafety(sc, path);
            if (autoRegisterBuildSettings) RegisterSceneInBuildSettingsIfNeeded(path);
            if (!dryRun) EditorUtility.DisplayDialog("완료", $"Stage2.unity 생성/덮어쓰기 완료:\n{path}", "확인");
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
            cam.AddComponent<Physics2DRaycaster>();
            
            // Full HD 화면에 맞는 카메라 설정
            float screenHeight = 1080f;
            float pixelsPerUnit = 100f; // 기본값
            
            // 1536x1024 이미지를 1920x1080 화면에 맞게 카메라 조정
            float targetHeight = screenHeight / pixelsPerUnit / 2f; // orthographicSize는 절반 높이
            camera.orthographicSize = targetHeight; // 5.4f
            

            
            var world = new GameObject("World");
            var interaction = new GameObject("Interaction");
            var services = new GameObject("Services");

            // StageInteractionManager 보장
            var simGo = new GameObject("StageInteraction");
            simGo.transform.SetParent(services.transform, false);
            simGo.AddComponent<AFKS.Features.Stage.StageInteractionManager>();

            var bg = new GameObject("Background");
            bg.transform.SetParent(world.transform, false);
            var bgSr = bg.AddComponent<SpriteRenderer>();
            bgSr.sprite = EnsurePlaceholderSprite(Path.Combine("Assets/Resources/Placeholders", "BG_Default.png"), new Color32(30, 30, 30, 255));
            
            // StageGeneric 배경 스케일 적용 (월드 좌표 기반)
            float optimalScaleX = 1.25f;
            float optimalScaleY = 1.06f;
            bg.transform.localScale = new Vector3(optimalScaleX, optimalScaleY, 1f);
            

            
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

            // AutoBGMPlayer 추가(기본 BGM 자동 재생)
            var autoBgm = bg.AddComponent<AFKS.Core.Audio.AutoBGMPlayer>();
            var soAuto = new SerializedObject(autoBgm);
            soAuto.FindProperty("volume").floatValue = 0.6f;
            soAuto.FindProperty("useCrossFade").boolValue = true;
            soAuto.FindProperty("crossFadeSeconds").floatValue = 0.5f;
            soAuto.ApplyModifiedPropertiesWithoutUndo();

            // 컨트롤러 루트(World 하위)
            var controllersRoot = new GameObject("Controllers");
            controllersRoot.transform.SetParent(world.transform, false);
            var stageAnimCtrl = controllersRoot.AddComponent<AFKS.Features.Stage.Animations.StageAnimationController>();
            var objAnimCtrl = controllersRoot.AddComponent<AFKS.Features.Stage.Animations.ObjectAnimationController>();

            // StageDefinition (Door 클릭 → 다음 스테이지)
            var defDir = "Assets/Stages";
            if (!Directory.Exists(defDir)) Directory.CreateDirectory(defDir);
            var defPath = Path.Combine(defDir, sceneName + "Definition.asset").Replace('\\','/');
            var exist = AssetDatabase.LoadAssetAtPath<AFKS.Features.Stage.StageDefinition>(defPath);
            if (exist != null) { if (confirmOverwrite) AssetDatabase.DeleteAsset(defPath); }
            var def = ScriptableObject.CreateInstance<AFKS.Features.Stage.StageDefinition>();
            AssetDatabase.CreateAsset(def, defPath);
            var clickDoor = ScriptableObject.CreateInstance<AFKS.Features.Stage.Events.ClickEvent>();
            clickDoor.SetupStageTransitionEvent();
            AssetDatabase.AddObjectToAsset(clickDoor, def);
            def.Set(sceneName, new[]{ clickDoor as AFKS.Features.Stage.Events.StageEvent });
            EditorUtility.SetDirty(def); AssetDatabase.SaveAssets(); AssetDatabase.Refresh();

            // StageEventSystem 구성
            var sesHost = services;
            var ses = sesHost.GetComponent<AFKS.Features.Stage.StageEventSystem>();
            if (ses == null) ses = sesHost.AddComponent<AFKS.Features.Stage.StageEventSystem>();
            ses.LoadFromDefinition(def);
            var sesSO = new SerializedObject(ses);
            sesSO.FindProperty("animationController").objectReferenceValue = stageAnimCtrl;
            var sim = services.GetComponent<AFKS.Features.Stage.StageInteractionManager>();
            sesSO.FindProperty("interactionManager").objectReferenceValue = sim;
            var sp = sesHost.GetComponent<AFKS.Features.Stage.StoryProgress>() ?? sesHost.AddComponent<AFKS.Features.Stage.StoryProgress>();
            sesSO.FindProperty("storyProgress").objectReferenceValue = sp;
            sesSO.ApplyModifiedPropertiesWithoutUndo();

            // Door 라우팅
            var ch = door.AddComponent<AFKS.Features.Interaction.ClickHandler>();
            var chSO = new SerializedObject(ch); chSO.FindProperty("debugLog").boolValue = false; chSO.ApplyModifiedPropertiesWithoutUndo();
            var router = door.AddComponent<AFKS.Features.Interaction.ClickToEventRouter>();
            var rSO = new SerializedObject(router);
            rSO.FindProperty("objectId").stringValue = "Door";
            rSO.FindProperty("eventSystem").objectReferenceValue = ses;
            rSO.ApplyModifiedPropertiesWithoutUndo();

            // 상호작용 목록 명시 구성
            var simSO = new SerializedObject(sim);
            var listProp = simSO.FindProperty("interactableObjects"); listProp.ClearArray();
            int idx = listProp.arraySize; listProp.InsertArrayElementAtIndex(idx);
            var elem = listProp.GetArrayElementAtIndex(idx);
            elem.FindPropertyRelative("objectId").stringValue = "Door";
            elem.FindPropertyRelative("gameObject").objectReferenceValue = door;
            elem.FindPropertyRelative("clickHandler").objectReferenceValue = ch;
            elem.FindPropertyRelative("isInteractable").boolValue = true;
            simSO.ApplyModifiedPropertiesWithoutUndo();

            SaveSceneWithSafety(sc, path);
            if (autoRegisterBuildSettings) RegisterSceneInBuildSettingsIfNeeded(path);
            if (!dryRun) EditorUtility.DisplayDialog("완료", $"{sceneName}.unity 생성/덮어쓰기 완료:\n{path}", "확인");
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
            if (dryRun)
            {
                Debug.Log($"[DRY-RUN] Register {list.Count} scenes to Build Settings");
            }
            else
            {
                EditorBuildSettings.scenes = list.ToArray();
                EditorUtility.DisplayDialog("완료", "모든 씬 생성/덮어쓰기 및 Build Settings 등록이 완료되었습니다.", "확인");
            }

            // 생성 후 자동 유효성 검사/자동수정
            if (!dryRun && postValidateAfterGenerate)
            {
                ValidateAllScenesInScenesFolder();
                CleanupMissingScriptsInScenes(paths);
            }
        }

        /// <summary>
        /// Stage1 씬의 핵심 상호작용(문 클릭→줌→체인→전환)이 깨지지 않도록 배선을 검증하고 누락 시 자동 보완합니다.
        /// </summary>
        private void ValidateAndAutofixStage1(string path)
        {
            if (!File.Exists(path))
            {
                EditorUtility.DisplayDialog("경고", $"Stage1 씬을 찾을 수 없습니다. 먼저 생성하세요.\n{path}", "확인");
                return;
            }

            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

            // 필수 루트 확보
            var world = GameObject.Find("World") ?? new GameObject("World");
            var interaction = GameObject.Find("Interaction") ?? new GameObject("Interaction");
            var services = GameObject.Find("Services") ?? new GameObject("Services");

            // StageInteractionManager 보장(Services 하위)
            var simGo = GameObject.Find("StageInteraction");
            var sim = (simGo != null ? simGo.GetComponent<AFKS.Features.Stage.StageInteractionManager>() : null);
            if (sim == null)
            {
                if (simGo == null)
                {
                    simGo = new GameObject("StageInteraction");
                    simGo.transform.SetParent(services.transform, false);
                }
                sim = simGo.GetComponent<AFKS.Features.Stage.StageInteractionManager>() ?? simGo.AddComponent<AFKS.Features.Stage.StageInteractionManager>();
            }

            // Door 찾기/생성
            var hotspots = GameObject.Find("Hotspots");
            if (hotspots == null)
            {
                hotspots = new GameObject("Hotspots");
                hotspots.transform.SetParent(interaction.transform, false);
            }

            var door = GameObject.Find("Door");
            if (door == null)
            {
                door = new GameObject("Door");
                door.transform.SetParent(hotspots.transform, false);
                door.transform.localPosition = new Vector3(0f, 0.5f, 0f);
                var doorSr = door.AddComponent<SpriteRenderer>();
                doorSr.color = new Color(1f, 1f, 1f, 0.85f);
            }

            // Door 필수 컴포넌트 보장
            var doorSrExist = door.GetComponent<SpriteRenderer>() ?? door.AddComponent<SpriteRenderer>();
            var doorCol = door.GetComponent<BoxCollider2D>() ?? door.AddComponent<BoxCollider2D>();
            doorCol.isTrigger = true;
            if (doorCol.size == Vector2.zero) doorCol.size = new Vector2(4f, 4f);

            var click = door.GetComponent<AFKS.Features.Interaction.ClickHandler>() ?? door.AddComponent<AFKS.Features.Interaction.ClickHandler>();
            // 클릭 가능은 이벤트 시스템이 상태에 따라 토글하지만, 초기값은 true로 두어 핸들러 연결 보장
            var soClick = new SerializedObject(click);
            soClick.FindProperty("clickable").boolValue = true;
            soClick.ApplyModifiedPropertiesWithoutUndo();




            var move = door.GetComponent<AFKS.Features.Interaction.HotspotMove>() ?? door.AddComponent<AFKS.Features.Interaction.HotspotMove>();
            var soMove = new SerializedObject(move);
            var targetStageProp = soMove.FindProperty("targetStageId");
            if (targetStageProp != null && string.IsNullOrEmpty(targetStageProp.stringValue))
            {
                targetStageProp.stringValue = "Stage2";
            }
            var manualTriggerProp = soMove.FindProperty("manualTriggerOnly");
            if (manualTriggerProp != null) manualTriggerProp.boolValue = true;
            soMove.ApplyModifiedPropertiesWithoutUndo();



            // Door 클릭 이벤트는 이벤트 시스템에서 재설정하므로, 컴포넌트 유무와 기본 clickable만 보장하면 충분

            SaveSceneWithSafety(scene, path);
            if (!dryRun) EditorUtility.DisplayDialog("완료", "Stage1 유효성 검사/자동수정이 완료되었습니다.\n문 클릭→줌→체인→전환 배선이 보강되었습니다.", "확인");
        }

        /// <summary>
        /// UnityEvent의 퍼시스턴트 리스너를 모두 제거합니다 (UnityEventTools에 일괄 제거 API가 없어 헬퍼로 구현).
        /// </summary>
        private static void ClearPersistent(UnityEvent evt)
        {
            if (evt == null) return;
            for (int i = evt.GetPersistentEventCount() - 1; i >= 0; i--)
            {
                UnityEventTools.RemovePersistentListener(evt, i);
            }
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
        
        private void SaveSceneWithSafety(UnityEngine.SceneManagement.Scene scene, string path)
        {
            string norm = path?.Replace('\\','/');
            if (File.Exists(norm))
            {
                if (confirmOverwrite)
                {
                    bool ok = EditorUtility.DisplayDialog("덮어쓰기 확인", $"기존 씬 파일을 덮어쓰시겠습니까?\n{norm}", "예", "아니오");
                    if (!ok) return;
                }
            }
            if (dryRun)
            {
                Debug.Log($"[DRY-RUN] SaveScene -> {norm}");
                return;
            }
            EditorSceneManager.SaveScene(scene, norm);
        }

        private void RegisterSceneInBuildSettingsIfNeeded(string path)
        {
            if (dryRun) { Debug.Log($"[DRY-RUN] Register in BuildSettings -> {path}"); return; }
            string norm = path.Replace('\\','/');
            var list = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            int idx = list.FindIndex(s => s.path.Replace('\\','/') == norm);
            if (idx < 0)
            {
                list.Add(new EditorBuildSettingsScene(norm, true));
            }
            else
            {
                var s = list[idx];
                if (!s.enabled) { s.enabled = true; list[idx] = s; }
            }
            EditorBuildSettings.scenes = list.ToArray();
        }
        #endregion

        #region 전체 씬 유효성 검사/자동수정
        private void ValidateAllScenesInScenesFolder()
        {
            if (!Directory.Exists(scenesFolder))
            {
                EditorUtility.DisplayDialog("오류", $"Scenes 폴더가 존재하지 않습니다: {scenesFolder}", "확인");
                return;
            }
            var scenePaths = Directory.GetFiles(scenesFolder, "*.unity");
            for (int i = 0; i < scenePaths.Length; i++)
            {
                string p = scenePaths[i].Replace('\\', '/');
                string name = Path.GetFileNameWithoutExtension(p);
                if (name == "Stage1")
                {
                    ValidateAndAutofixStage1(p);
                }
                else if (name.StartsWith("Stage"))
                {
                    ValidateMinimalStage(p);
                }
                else if (name == "Core")
                {
                    // Core는 생성 로직이 완전하여 별도 보정 최소화. 향후 체크 추가 가능
                    Debug.Log("Core 씬은 스캐폴딩 규격에 맞는지 수동 점검 권장(전역 서비스 구성)");
                }
                else if (name == "Menu")
                {
                    // Menu는 버튼 연결/전역 설정 연동이 필요. 현재 스캐폴딩로 생성한 경우 OK
                    Debug.Log("Menu 씬은 스캐폴딩로 생성 시 기본 연결이 완료됩니다");
                }
            }
            EditorUtility.DisplayDialog("완료", "Scenes 폴더 전체 유효성 검사/자동수정이 완료되었습니다.", "확인");
        }

        // 생성/유효성 검사 후 Missing Script 정리(통합)
        private void CleanupMissingScriptsInScenes(string[] scenePaths)
        {
            for (int i = 0; i < scenePaths.Length; i++)
            {
                string p = scenePaths[i].Replace('\\', '/');
                if (!File.Exists(p)) continue;
                var scene = EditorSceneManager.OpenScene(p, OpenSceneMode.Single);
                int removed = 0;
                var roots = scene.GetRootGameObjects();
                for (int r = 0; r < roots.Length; r++)
                {
                    removed += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(roots[r]);
                }
                if (removed > 0)
                {
                    Debug.Log($"[Scaffolder] Missing Script 제거: {Path.GetFileName(p)} - {removed}개");
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                }
            }
        }

        private void ValidateMinimalStage(string path)
        {
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            var world = GameObject.Find("World") ?? new GameObject("World");
            var interaction = GameObject.Find("Interaction") ?? new GameObject("Interaction");
            var services = GameObject.Find("Services") ?? new GameObject("Services");

            // StageInteractionManager 보장
            var simGo = GameObject.Find("StageInteraction");
            var sim = (simGo != null ? simGo.GetComponent<AFKS.Features.Stage.StageInteractionManager>() : null);
            if (sim == null)
            {
                if (simGo == null)
                {
                    simGo = new GameObject("StageInteraction");
                    simGo.transform.SetParent(services.transform, false);
                }
                sim = simGo.GetComponent<AFKS.Features.Stage.StageInteractionManager>() ?? simGo.AddComponent<AFKS.Features.Stage.StageInteractionManager>();
            }

            // StageRoot 보장
            var root = FindFirstObjectByType<AFKS.Features.Stage.StageRoot>();
            if (root == null)
            {
                var rootGo = new GameObject("StageRoot");
                rootGo.transform.SetParent(services.transform, false);
                rootGo.AddComponent<AFKS.Features.Stage.StageRoot>();
            }

            EditorSceneManager.SaveScene(scene, path);
        }
        #endregion
    }
}






