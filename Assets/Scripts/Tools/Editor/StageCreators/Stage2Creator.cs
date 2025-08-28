using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEditor.Events;

namespace AFKS.Tools.Editor.StageCreators
{
    /// <summary>
    /// Stage2(병원 로비)의 복잡한 로직을 담당하는 Creator입니다.
    /// 아이템 수집, 문 잠금 해제, 귀신 등장 등을 포함합니다.
    /// </summary>
    public class Stage2Creator : BaseStageCreator
    {
        public Stage2Creator() : base("Stage2", "Stage3", true)
        {
            // Stage2는 복잡한 게임플레이를 가집니다
        }

        public override void CreateStage(string path)
        {
            EnsureDirectory(Path.GetDirectoryName(path));
            var sc = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            sc.name = stageName;

            // 1. 기본 구조 생성 (BaseStageCreator 메서드 사용)
            var cam = CreateStageCamera(stageName, new Vector3(0f, 0f, -10f), 5.4f);
            var (world, interaction, services) = CreateCommonWorldStructure();
            var sim = SetupCommonStageInteractionSystem(services);
            
            // 2. 스테이지별 배경 설정 (HospitalLobby 배경 사용)
            var bg = CreateStageBackground(world, "Images/Backgrounds/Stage2/Stage2", 
                new Vector3(1f, 1f, 1f), new Vector3(0, 0, 10), -1);

            // 4. Stage2 전용 로직 생성
            CreateStageSpecificLogic(world, interaction, services);

            // 5. StageRoot 생성
            var root = new GameObject("StageRoot");
            root.transform.SetParent(services.transform, false);
            root.AddComponent<AFKS.Features.Stage.StageRoot>();

            // 6. 씬 저장 및 Build Settings 등록
            SaveSceneWithSafety(sc, path);
            RegisterSceneInBuildSettings(path);
            
            if (!EditorUtility.DisplayDialog("완료", $"Stage2.unity 생성/덮어쓰기 완료:\n{path}", "확인"))
            {
                // 사용자가 취소한 경우
            }
        }

        protected override void CreateStageSpecificLogic(GameObject world, GameObject interaction, GameObject services)
        {
            // Hotspots
            var hotspots = new GameObject("Hotspots");
            hotspots.transform.SetParent(interaction.transform, false);
            hotspots.transform.localPosition = new Vector3(0f, -2f, 0f);
            
            // 아이템들 생성
            var cross = CreateStoryItem(hotspots, "Cross", "Images/Items/Stage2/Cross", new Vector3(-2f, 0f, 9f));
            var permit = CreateStoryItem(hotspots, "Permit", "Images/Items/Stage2/Permit", new Vector3(2f, 0f, 9f));
            var letter = CreateStoryItem(hotspots, "Letter", "Images/Items/Stage2/Letter", new Vector3(0f, -1f, 9f));
            
            // 문들 생성 (초기에는 잠금 상태)
            var managementDoor = CreateLockableDoor(hotspots, "ManagementDoor", "Stage3", new Vector3(-3f, 0.5f, 0f), true);
            var deliveryDoor = CreateLockableDoor(hotspots, "DeliveryDoor", "Stage4", new Vector3(3f, 0.5f, 0f), true);
            
            // 귀신 생성 (초기에는 비활성)
            var ghost = CreateGhostEncounter(hotspots, "HospitalGhost", "Images/Ghosts/Stage2/HospitalGhost", new Vector3(0f, 1f, 9f), false);
            
            // StageDefinition 생성 및 이벤트 설정
            var def = CreateStageDefinition();
            
            // StageEventSystem 설정
            var ses = SetupCommonStageEventSystem(services, def);
            
            // 스토리 핵심 시스템 설정
            SetupStoryCoreSystems(services);
            
            // 모든 오브젝트를 이벤트 시스템과 연결
            SetupEventRouting(hotspots, ses);
            
            // StageInteractionSystem 상호작용 목록 구성
            SetupInteractionList(services, hotspots);
        }

        private AFKS.Features.Stage.StageDefinition CreateStageDefinition()
        {
            // 데이터 드리븐 StageDefinition 생성
            var defDir = "Assets/Stages";
            if (!Directory.Exists(defDir)) Directory.CreateDirectory(defDir);
            var defPath = Path.Combine(defDir, "Stage2Definition.asset").Replace('\\','/');
            var existingDef = AssetDatabase.LoadAssetAtPath<AFKS.Features.Stage.StageDefinition>(defPath);
            if (existingDef != null)
            {
                bool proceed = EditorUtility.DisplayDialog("덮어쓰기 확인", 
                    $"기존 StageDefinition 에셋을 삭제 후 재생성합니다.\n{defPath}", "예", "아니오");
                if (!proceed) return existingDef;
                AssetDatabase.DeleteAsset(defPath);
            }

            var def = ScriptableObject.CreateInstance<AFKS.Features.Stage.StageDefinition>();
            AssetDatabase.CreateAsset(def, defPath);

            // 1) 십자가 수집 이벤트
            var crossEvent = CreateStoryProgressEvent("십자가 수집", "십자가를 수집했습니다.", new[]{"Cross"});
            
            // 2) 허가증 수집 이벤트
            var permitEvent = CreateStoryProgressEvent("허가증 수집", "병원 개설 허가증을 수집했습니다.", new[]{"Permit"});
            
            // 3) 사고 편지 수집 이벤트
            var letterEvent = CreateStoryProgressEvent("사고 편지 수집", "사고 편지를 수집했습니다.", new[]{"Letter"});
            
            // 4) 관리실 문 잠금 해제 이벤트 (십자가 + 허가증 필요)
            var unlockManagementEvent = CreateStoryProgressEvent("관리실 문 잠금 해제", "관리실 문이 열렸습니다.", 
                new[]{"ManagementDoor"}, new[]{"Cross", "Permit"});
            
            // 5) 분만실 문 잠금 해제 이벤트 (모든 아이템 필요)
            var unlockDeliveryEvent = CreateStoryProgressEvent("분만실 문 잠금 해제", "분만실 문이 열렸습니다.", 
                new[]{"DeliveryDoor"}, new[]{"Cross", "Permit", "Letter"});
            
            // 6) 귀신 등장 이벤트 (모든 아이템 수집 후)
            var ghostEvent = CreateStoryProgressEvent("귀신 등장", "병원 카운터 직원 귀신이 나타났습니다!", 
                new[]{"HospitalGhost"}, new[]{"Cross", "Permit", "Letter"});
            
            // 이벤트들을 서브 에셋으로 추가
            AssetDatabase.AddObjectToAsset(crossEvent, def);
            AssetDatabase.AddObjectToAsset(permitEvent, def);
            AssetDatabase.AddObjectToAsset(letterEvent, def);
            AssetDatabase.AddObjectToAsset(unlockManagementEvent, def);
            AssetDatabase.AddObjectToAsset(unlockDeliveryEvent, def);
            AssetDatabase.AddObjectToAsset(ghostEvent, def);

            def.Set("Stage2", new[]{ crossEvent, permitEvent, letterEvent, unlockManagementEvent, unlockDeliveryEvent, ghostEvent });
            EditorUtility.SetDirty(def);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            return def;
        }

        private void SetupEventRouting(GameObject hotspots, AFKS.Features.Stage.StageEventSystem ses)
        {
            // 모든 오브젝트의 이벤트 시스템 연결
            var allObjects = new[] { "Cross", "Permit", "Letter", "ManagementDoor", "DeliveryDoor", "HospitalGhost" };
            
            foreach (var objId in allObjects)
            {
                var obj = hotspots.transform.Find(objId)?.gameObject;
                if (obj != null)
                {
                    var router = obj.GetComponent<AFKS.Features.Interaction.ClickToEventRouter>();
                    if (router != null)
                    {
                        var routerSO = new SerializedObject(router);
                        routerSO.FindProperty("eventSystem").objectReferenceValue = ses;
                        routerSO.ApplyModifiedPropertiesWithoutUndo();
                    }
                }
            }
        }

        private void SetupInteractionList(GameObject services, GameObject hotspots)
        {
            var sim = services.GetComponent<AFKS.Features.Stage.StageInteractionSystem>();
            var simSO = new SerializedObject(sim);
            var listProp = simSO.FindProperty("interactableObjects");
            listProp.ClearArray();
            
            // 모든 오브젝트를 상호작용 목록에 추가
            var allObjects = new[] { "Cross", "Permit", "Letter", "ManagementDoor", "DeliveryDoor", "HospitalGhost" };
            
            foreach (var objId in allObjects)
            {
                var obj = hotspots.transform.Find(objId)?.gameObject;
                if (obj != null)
                {
                    var clickHandler = obj.GetComponent<AFKS.Features.Interaction.ClickHandler>();
                    
                    int idx = listProp.arraySize;
                    listProp.InsertArrayElementAtIndex(idx);
                    var elem = listProp.GetArrayElementAtIndex(idx);
                    elem.FindPropertyRelative("objectId").stringValue = objId;
                    elem.FindPropertyRelative("gameObject").objectReferenceValue = obj;
                    elem.FindPropertyRelative("clickHandler").objectReferenceValue = clickHandler;
                    elem.FindPropertyRelative("isInteractable").boolValue = true;
                }
            }
            
            simSO.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
