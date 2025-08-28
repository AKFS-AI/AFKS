using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;

namespace AFKS.Tools.Editor.StageCreators
{
    /// <summary>
    /// 모든 스테이지의 공통 설계를 담당하는 기본 클래스입니다.
    /// 기존 SceneScaffolderWindow의 기능을 그대로 유지하면서 코드를 분리합니다.
    /// </summary>
    public abstract class BaseStageCreator
    {
        #region 공통 속성
        protected string stageName;
        protected string nextStageId;
        protected bool hasComplexGameplay;
        protected string scenesFolder = "Assets/Scenes";
        protected bool confirmOverwrite = true;
        protected bool autoRegisterBuildSettings = true;
        #endregion

        #region 생성자
        protected BaseStageCreator(string stageName, string nextStageId, bool hasComplexGameplay = false)
        {
            this.stageName = stageName;
            this.nextStageId = nextStageId;
            this.hasComplexGameplay = hasComplexGameplay;
        }
        #endregion

        #region 공통 메서드들 (기존 SceneScaffolderWindow에서 이동)
        
        /// <summary>
        /// 스테이지용 카메라를 생성합니다.
        /// </summary>
        protected GameObject CreateStageCamera(string stageName, Vector3 position, float orthographicSize)
        {
            var cam = new GameObject($"{stageName} Camera");
            var camera = cam.AddComponent<Camera>();
            cam.tag = "MainCamera";
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            camera.orthographic = true;
            camera.orthographicSize = orthographicSize;
            cam.AddComponent<UnityEngine.EventSystems.Physics2DRaycaster>();
            cam.transform.position = position;
            
            return cam;
        }
        
        /// <summary>
        /// 공통 월드 구조를 생성합니다.
        /// </summary>
        protected (GameObject world, GameObject interaction, GameObject services) CreateCommonWorldStructure()
        {
            var world = new GameObject("World");
            var interaction = new GameObject("Interaction");
            var services = new GameObject("Services");
            
            return (world, interaction, services);
        }
        
        /// <summary>
        /// 스테이지 배경을 생성합니다.
        /// </summary>
        protected GameObject CreateStageBackground(GameObject world, string spritePath, Vector3 scale, Vector3 position, int sortingOrder = -1)
        {
            var bg = new GameObject("Background");
            bg.transform.SetParent(world.transform, false);
            var bgSprite = bg.AddComponent<SpriteRenderer>();
            
            // 스프라이트 로드
            var sprite = Resources.Load<Sprite>(spritePath);
            if (sprite != null)
            {
                bgSprite.sprite = sprite;
                Debug.Log($"배경 이미지 설정됨: {sprite.name}");
            }
            else
            {
                Debug.LogWarning($"배경 이미지를 찾을 수 없습니다: {spritePath}");
                bgSprite.color = Color.gray;
            }
            
            // 위치 및 스케일 설정
            bg.transform.position = position;
            bg.transform.localScale = scale;
            bgSprite.sortingOrder = sortingOrder;
            
            return bg;
        }
        
        /// <summary>
        /// 공통 StageEventSystem을 설정합니다.
        /// </summary>
        protected AFKS.Features.Stage.StageEventSystem SetupCommonStageEventSystem(GameObject services, AFKS.Features.Stage.StageDefinition definition)
        {
            var ses = services.GetComponent<AFKS.Features.Stage.StageEventSystem>();
            if (ses == null)
            {
                ses = services.AddComponent<AFKS.Features.Stage.StageEventSystem>();
            }
            
            ses.LoadFromDefinition(definition);
            
            var sesSO = new SerializedObject(ses);
            
            // StageInteractionSystem 연결
            var sim = services.GetComponent<AFKS.Features.Stage.StageInteractionSystem>();
            sesSO.FindProperty("interactionManager").objectReferenceValue = sim;
            
            // StoryProgress 연결
            var sp = services.GetComponent<AFKS.Features.Stage.StoryProgress>();
            if (sp == null)
            {
                sp = services.AddComponent<AFKS.Features.Stage.StoryProgress>();
            }
            sesSO.FindProperty("storyProgress").objectReferenceValue = sp;
            
            sesSO.ApplyModifiedPropertiesWithoutUndo();
            
            return ses;
        }
        
        /// <summary>
        /// 공통 StageInteractionSystem을 설정합니다.
        /// </summary>
        protected AFKS.Features.Stage.StageInteractionSystem SetupCommonStageInteractionSystem(GameObject services)
        {
            var simGo = new GameObject("StageInteraction");
            simGo.transform.SetParent(services.transform, false);
            var sim = simGo.AddComponent<AFKS.Features.Stage.StageInteractionSystem>();
            
            return sim;
        }
        
        /// <summary>
        /// 경로에 단색 PNG 스프라이트가 없으면 생성하고, Sprite로 로드해 반환합니다.
        /// </summary>
        protected Sprite EnsurePlaceholderSprite(string pngPath, Color color)
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
                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(pngPath);
                if (tex != null)
                {
                    sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
                }
            }
            return sprite;
        }
        
        /// <summary>
        /// 디렉토리가 존재하지 않으면 생성합니다.
        /// </summary>
        protected void EnsureDirectory(string dir)
        {
            if (string.IsNullOrEmpty(dir)) return;
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        }
        
        /// <summary>
        /// 씬을 안전하게 저장합니다.
        /// </summary>
        protected void SaveSceneWithSafety(Scene scene, string path)
        {
            string norm = path?.Replace('\\', '/');
            if (File.Exists(norm) && confirmOverwrite)
            {
                bool ok = EditorUtility.DisplayDialog("덮어쓰기 확인", 
                    $"기존 씬 파일을 덮어쓰시겠습니까?\n{norm}", "예", "아니오");
                if (!ok) return;
            }
            
            EditorSceneManager.SaveScene(scene, norm);
        }
        
        /// <summary>
        /// 씬을 Build Settings에 등록합니다.
        /// </summary>
        protected void RegisterSceneInBuildSettings(string path)
        {
            if (!autoRegisterBuildSettings) return;
            
            string norm = path.Replace('\\', '/');
            var list = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            int idx = list.FindIndex(s => s.path.Replace('\\', '/') == norm);
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

        #region 스토리 진행 공통 시스템 (새로 추가)
        
        /// <summary>
        /// 아이템을 생성하고 스토리 이벤트와 연결합니다.
        /// </summary>
        protected GameObject CreateStoryItem(GameObject parent, string itemId, string spritePath, Vector3 position, bool isCollectible = true)
        {
            var item = new GameObject(itemId);
            item.transform.SetParent(parent.transform, false);
            item.transform.localPosition = position;
            
            var itemSr = item.AddComponent<SpriteRenderer>();
            var itemCol = item.AddComponent<BoxCollider2D>();
            itemCol.size = new Vector2(1f, 1f);
            itemCol.isTrigger = true;
            
            // 아이템 이미지 로드
            var sprite = Resources.Load<Sprite>(spritePath);
            if (sprite != null)
            {
                itemSr.sprite = sprite;
                Debug.Log($"아이템 이미지 설정됨: {itemId} - {sprite.name}");
            }
            else
            {
                itemSr.sprite = EnsurePlaceholderSprite(Path.Combine("Assets/Resources/Placeholders", "Item_Blue.png"), new Color32(0, 150, 255, 255));
                Debug.Log($"아이템 이미지 대체: {itemId} - 파란색 플레이스홀더 사용");
            }
            
            // 아이템 가시성 설정
            itemSr.sortingOrder = 2;
            itemSr.color = Color.white;
            
            // 클릭 핸들러 추가
            var clickHandler = item.AddComponent<AFKS.Features.Interaction.ClickHandler>();
            var soClick = new SerializedObject(clickHandler);
            soClick.FindProperty("clickable").boolValue = isCollectible;
            soClick.ApplyModifiedPropertiesWithoutUndo();
            
            // 아이템 라우터 추가
            var itemRouter = item.AddComponent<AFKS.Features.Interaction.ClickToEventRouter>();
            var itemRouterSO = new SerializedObject(itemRouter);
            itemRouterSO.FindProperty("objectId").stringValue = itemId;
            itemRouterSO.ApplyModifiedPropertiesWithoutUndo();
            
            item.SetActive(true);
            Debug.Log($"스토리 아이템 생성됨: {itemId}");
            
            return item;
        }
        
        /// <summary>
        /// 귀신/깜놀 포인트를 생성하고 이벤트와 연결합니다.
        /// </summary>
        protected GameObject CreateGhostEncounter(GameObject parent, string ghostId, string spritePath, Vector3 position, bool isActive = false)
        {
            var ghost = new GameObject(ghostId);
            ghost.transform.SetParent(parent.transform, false);
            ghost.transform.localPosition = position;
            
            var ghostSr = ghost.AddComponent<SpriteRenderer>();
            var ghostCol = ghost.AddComponent<BoxCollider2D>();
            ghostCol.size = new Vector2(2f, 2f);
            ghostCol.isTrigger = true;
            
            // 귀신 이미지 로드
            var sprite = Resources.Load<Sprite>(spritePath);
            if (sprite != null)
            {
                ghostSr.sprite = sprite;
                Debug.Log($"귀신 이미지 설정됨: {ghostId} - {sprite.name}");
            }
            else
            {
                ghostSr.sprite = EnsurePlaceholderSprite(Path.Combine("Assets/Resources/Placeholders", "Ghost_Red.png"), new Color32(255, 0, 0, 200));
                Debug.Log($"귀신 이미지 대체: {ghostId} - 빨간색 플레이스홀더 사용");
            }
            
            // 귀신 가시성 설정 (반투명)
            ghostSr.sortingOrder = 3;
            ghostSr.color = new Color(1f, 1f, 1f, 0.8f);
            
            // 클릭 핸들러 추가
            var clickHandler = ghost.AddComponent<AFKS.Features.Interaction.ClickHandler>();
            var soClick = new SerializedObject(clickHandler);
            soClick.FindProperty("clickable").boolValue = true;
            soClick.ApplyModifiedPropertiesWithoutUndo();
            
            // 귀신 라우터 추가
            var ghostRouter = ghost.AddComponent<AFKS.Features.Interaction.ClickToEventRouter>();
            var ghostRouterSO = new SerializedObject(ghostRouter);
            ghostRouterSO.FindProperty("objectId").stringValue = ghostId;
            ghostRouterSO.ApplyModifiedPropertiesWithoutUndo();
            
            ghost.SetActive(isActive);
            Debug.Log($"귀신 생성됨: {ghostId} (활성화: {isActive})");
            
            return ghost;
        }
        
        /// <summary>
        /// 문을 생성하고 잠금/해제 시스템과 연결합니다.
        /// </summary>
        protected GameObject CreateLockableDoor(GameObject parent, string doorId, string nextStageId, Vector3 position, bool isLocked = true)
        {
            var door = new GameObject(doorId);
            door.transform.SetParent(parent.transform, false);
            door.transform.localPosition = position;
            
            var doorSr = door.AddComponent<SpriteRenderer>();
            var doorCol = door.AddComponent<BoxCollider2D>();
            doorCol.size = new Vector2(2f, 3f);
            doorCol.isTrigger = true;
            
            // 문 이미지 (잠금 상태에 따라)
            var spritePath = isLocked ? "Placeholders/Door_Locked.png" : "Placeholders/Door_Unlocked.png";
            var sprite = Resources.Load<Sprite>(spritePath);
            if (sprite != null)
            {
                doorSr.sprite = sprite;
            }
            else
            {
                doorSr.sprite = EnsurePlaceholderSprite(Path.Combine("Assets/Resources/Placeholders", isLocked ? "Door_Locked.png" : "Door_Unlocked.png"), 
                    isLocked ? new Color32(100, 100, 100, 255) : new Color32(150, 150, 150, 255));
            }
            
            // 문 가시성 설정
            doorSr.sortingOrder = 1;
            doorSr.color = Color.white;
            
            // HotspotMove 컴포넌트 추가 (잠금 해제된 경우만)
            if (!isLocked)
            {
                var move = door.AddComponent<AFKS.Features.Interaction.HotspotMove>();
                var soMove = new SerializedObject(move);
                var targetStageProp = soMove.FindProperty("targetStageId");
                if (targetStageProp != null)
                {
                    targetStageProp.stringValue = nextStageId;
                }
                var manualTriggerProp = soMove.FindProperty("manualTriggerOnly");
                if (manualTriggerProp != null)
                {
                    manualTriggerProp.boolValue = true;
                }
                soMove.ApplyModifiedPropertiesWithoutUndo();
            }
            
            // 클릭 핸들러 추가
            var doorClickHandler = door.AddComponent<AFKS.Features.Interaction.ClickHandler>();
            var soDoorClick = new SerializedObject(doorClickHandler);
            soDoorClick.FindProperty("debugLog").boolValue = false;
            soDoorClick.ApplyModifiedPropertiesWithoutUndo();
            
            // 문 라우터 추가
            var doorRouter = door.AddComponent<AFKS.Features.Interaction.ClickToEventRouter>();
            var doorRouterSO = new SerializedObject(doorRouter);
            doorRouterSO.FindProperty("objectId").stringValue = doorId;
            doorRouterSO.ApplyModifiedPropertiesWithoutUndo();
            
            door.SetActive(true);
            Debug.Log($"문 생성됨: {doorId} (잠금 상태: {isLocked})");
            
            return door;
        }
        
        /// <summary>
        /// 스토리 진행 조건을 체크하는 이벤트를 생성합니다.
        /// </summary>
        protected AFKS.Features.Stage.Events.StageEvent CreateStoryProgressEvent(string eventName, string description, string[] triggerObjectIds, string[] requiredItems = null)
        {
            var storyEvent = ScriptableObject.CreateInstance<AFKS.Features.Stage.Events.ClickEvent>();
            var soEvent = new SerializedObject(storyEvent);
            
            // 이벤트 기본 정보 설정
            soEvent.FindProperty("eventName").stringValue = eventName;
            soEvent.FindProperty("description").stringValue = description;
            soEvent.FindProperty("triggerType").enumValueIndex = (int)AFKS.Features.Stage.Events.EventTriggerType.Click;
            
            // 트리거 오브젝트 설정
            var triggerList = soEvent.FindProperty("triggerObjectIds");
            triggerList.ClearArray();
            for (int i = 0; i < triggerObjectIds.Length; i++)
            {
                int idx = triggerList.arraySize;
                triggerList.InsertArrayElementAtIndex(idx);
                triggerList.GetArrayElementAtIndex(idx).stringValue = triggerObjectIds[i];
            }
            
            // 상호작용 가능한 오브젝트 설정
            var interactList = soEvent.FindProperty("interactableObjects");
            interactList.ClearArray();
            for (int i = 0; i < triggerObjectIds.Length; i++)
            {
                int idx = interactList.arraySize;
                interactList.InsertArrayElementAtIndex(idx);
                interactList.GetArrayElementAtIndex(idx).stringValue = triggerObjectIds[i];
            }
            
            // 아이템 수집 조건 설정 (있는 경우)
            if (requiredItems != null && requiredItems.Length > 0)
            {
                var itemCondition = ScriptableObject.CreateInstance<AFKS.Features.Stage.Conditions.ItemCollectionCondition>();
                var soCondition = new SerializedObject(itemCondition);
                
                var itemsProp = soCondition.FindProperty("requiredItems");
                itemsProp.ClearArray();
                for (int i = 0; i < requiredItems.Length; i++)
                {
                    int idx = itemsProp.arraySize;
                    itemsProp.InsertArrayElementAtIndex(idx);
                    itemsProp.GetArrayElementAtIndex(idx).stringValue = requiredItems[i];
                }
                
                soCondition.ApplyModifiedPropertiesWithoutUndo();
                
                // 조건을 이벤트에 연결
                var conditionsProp = soEvent.FindProperty("conditions");
                conditionsProp.ClearArray();
                int cidx = conditionsProp.arraySize;
                conditionsProp.InsertArrayElementAtIndex(cidx);
                conditionsProp.GetArrayElementAtIndex(cidx).objectReferenceValue = itemCondition;
                
                // 조건을 서브 에셋으로 추가
                AssetDatabase.AddObjectToAsset(itemCondition, storyEvent);
            }
            
            soEvent.ApplyModifiedPropertiesWithoutUndo();
            
            return storyEvent;
        }
        
        /// <summary>
        /// 스토리 진행을 위한 핵심 시스템을 설정합니다.
        /// </summary>
        protected void SetupStoryCoreSystems(GameObject services)
        {
            // StoryProgress 컴포넌트 추가 (스토리 진행 상태 관리)
            var storyProgress = services.GetComponent<AFKS.Features.Stage.StoryProgress>();
            if (storyProgress == null)
            {
                storyProgress = services.AddComponent<AFKS.Features.Stage.StoryProgress>();
            }
            
            // StageEventSystem에 StoryProgress 연결
            var ses = services.GetComponent<AFKS.Features.Stage.StageEventSystem>();
            if (ses != null)
            {
                var sesSO = new SerializedObject(ses);
                var storyProgressProp = sesSO.FindProperty("storyProgress");
                if (storyProgressProp != null)
                {
                    storyProgressProp.objectReferenceValue = storyProgress;
                    sesSO.ApplyModifiedPropertiesWithoutUndo();
                }
            }
            
            Debug.Log($"스토리 핵심 시스템 설정 완료: {stageName}");
        }
        
        /// <summary>
        /// 스테이지별 배경을 설정합니다. (스마트한 배경 경로 자동 감지)
        /// </summary>
        protected GameObject CreateStageSpecificBackground(GameObject world, string stageName, Vector3 scale, Vector3 position, int sortingOrder = -1)
        {
            // 스테이지별 최적 배경 경로 자동 감지
            string backgroundPath = GetOptimalBackgroundPath(stageName);
            
            var bg = CreateStageBackground(world, backgroundPath, scale, position, sortingOrder);
            
            // 배경에 AutoBGMPlayer 추가
            var autoBgm = bg.AddComponent<AFKS.Core.Audio.AutoBGMPlayer>();
            var soAutoBgm = new SerializedObject(autoBgm);
            soAutoBgm.FindProperty("volume").floatValue = 0.6f;
            soAutoBgm.FindProperty("useCrossFade").boolValue = true;
            soAutoBgm.FindProperty("crossFadeSeconds").floatValue = 0.5f;
            soAutoBgm.FindProperty("stopOnDisable").boolValue = true;
            soAutoBgm.ApplyModifiedPropertiesWithoutUndo();
            
            Debug.Log($"스테이지별 배경 설정 완료: {stageName} - {backgroundPath}");
            
            return bg;
        }
        
        /// <summary>
        /// 스테이지별 최적 배경 경로를 자동으로 결정합니다.
        /// </summary>
        private string GetOptimalBackgroundPath(string stageName)
        {
            // 스테이지별 특수 케이스 처리
            switch (stageName)
            {
                case "Stage1":
                    return "Images/Backgrounds/Stage1/Stage1_2"; // 체인 줌용 배경
                case "Stage5":
                    return "Images/Backgrounds/Stage5/Stage5_1"; // 옥상 전용 배경
                case "Menu":
                    return "Images/Backgrounds/Stage1/Stage1_1"; // 메뉴용 배경
                default:
                    // 일반적인 경우: Images/Backgrounds/StageX/StageX
                    return $"Images/Backgrounds/{stageName}/{stageName}";
            }
        }
        
        #endregion

        #region 추상 메서드
        
        /// <summary>
        /// 스테이지를 생성합니다. (기존 CreateOrOverwriteStage 메서드와 동일한 기능)
        /// </summary>
        public abstract void CreateStage(string path);
        
        /// <summary>
        /// 스테이지별 특화 로직을 구현합니다.
        /// </summary>
        protected abstract void CreateStageSpecificLogic(GameObject world, GameObject interaction, GameObject services);
        
        #endregion
    }
}
