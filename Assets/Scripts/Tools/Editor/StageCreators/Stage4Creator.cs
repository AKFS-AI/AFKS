using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

namespace AFKS.Tools.Editor.StageCreators
{
    /// <summary>
    /// Stage4 (지하실) 생성을 담당하는 Creator 클래스입니다.
    /// 기획서: 지하실에서 아이템 수집 → 점프스케어 → 다음 스테이지 진행
    /// </summary>
    public class Stage4Creator : BaseStageCreator
    {
        public Stage4Creator() : base("Stage4", "Stage5", true)
        {
        }

        public override void CreateStage(string path)
        {
            Debug.Log($"[Stage4Creator] Stage4 생성 시작: {path}");
            
            // 씬 생성 및 기본 설정
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            
            // 카메라 설정
            var camera = CreateStageCamera(stageName, new Vector3(0, 0, -10), 5f);
            
            // 공통 월드 구조 생성
            var (world, interaction, services) = CreateCommonWorldStructure();
            
            // 스테이지별 배경 생성 (Basement 배경 사용)
            CreateStageBackground(world, "Images/Backgrounds/Stage4/Stage4", new Vector3(16, 9, 1), Vector3.zero);
            
            // 스테이지별 로직 생성
            CreateStageSpecificLogic(world, interaction, services);
            
            // 씬 저장
            SaveSceneWithSafety(scene, path);
            
            Debug.Log($"[Stage4Creator] Stage4 생성 완료: {path}");
        }

        protected override void CreateStageSpecificLogic(GameObject world, GameObject interaction, GameObject services)
        {
            // 아이템 생성
            CreateStoryItem(interaction, "Flashlight", "Images/Placeholders/Flashlight", new Vector3(-4, 2, 0));
            CreateStoryItem(interaction, "Battery", "Images/Placeholders/Battery", new Vector3(4, 2, 0));
            CreateStoryItem(interaction, "Map", "Images/Placeholders/Map", new Vector3(0, 3, 0));
            
            // 문 생성 (초기에는 잠겨있음)
            CreateLockableDoor(interaction, "BasementDoor", "Stage5", new Vector3(0, -3, 0), true);
            
            // 점프스케어 트리거 생성 (초기에는 비활성화)
            CreateJumpScareTrigger(interaction, "JumpScareTrigger", new Vector3(0, 0, 0), false);
            
            // StageDefinition 생성
            var stageDefinition = CreateStageDefinition();
            
            // StageEventSystem 설정
            var ses = services.AddComponent<AFKS.Features.Stage.StageEventSystem>();
            var sesSO = new SerializedObject(ses);
            
            // 기본 속성 설정
            sesSO.FindProperty("stageId").stringValue = stageName;
            sesSO.FindProperty("stageType").enumValueIndex = 1; // Story
            sesSO.FindProperty("eventSequence").ClearArray();
            
            // 이벤트 시퀀스 설정
            var eventSequenceProp = sesSO.FindProperty("eventSequence");
            eventSequenceProp.arraySize = 7;
            
            // 1. 손전등 수집 이벤트
            var flashlightEvent = CreateStoryProgressEvent("손전등 수집", "손전등을 수집했습니다.", new[]{"Flashlight"});
            eventSequenceProp.GetArrayElementAtIndex(0).objectReferenceValue = flashlightEvent;
            
            // 2. 배터리 수집 이벤트
            var batteryEvent = CreateStoryProgressEvent("배터리 수집", "배터리를 수집했습니다.", new[]{"Battery"});
            eventSequenceProp.GetArrayElementAtIndex(1).objectReferenceValue = batteryEvent;
            
            // 3. 지도 수집 이벤트
            var mapEvent = CreateStoryProgressEvent("지도 수집", "지하실 지도를 수집했습니다.", new[]{"Map"});
            eventSequenceProp.GetArrayElementAtIndex(2).objectReferenceValue = mapEvent;
            
            // 4. 점프스케어 트리거 활성화 (모든 아이템 수집 후)
            var jumpScareTriggerEvent = CreateStoryProgressEvent("점프스케어 준비", "지하실에 무언가 숨어있습니다...", 
                new[]{"JumpScareTrigger"}, new[]{"Flashlight", "Battery", "Map"});
            eventSequenceProp.GetArrayElementAtIndex(3).objectReferenceValue = jumpScareTriggerEvent;
            
            // 5. 점프스케어 실행
            var jumpScareEvent = CreateStoryProgressEvent("점프스케어!", "갑자기 귀신이 나타났습니다!", 
                new[]{"JumpScareTrigger"}, new[]{"Flashlight", "Battery", "Map"});
            eventSequenceProp.GetArrayElementAtIndex(4).objectReferenceValue = jumpScareEvent;
            
            // 6. 지하실 문 잠금 해제 (점프스케어 후)
            var unlockDoorEvent = CreateStoryProgressEvent("지하실 문 잠금 해제", "지하실 문이 열렸습니다.", 
                new[]{"BasementDoor"}, new[]{"Flashlight", "Battery", "Map"});
            eventSequenceProp.GetArrayElementAtIndex(5).objectReferenceValue = unlockDoorEvent;
            
            // 7. 다음 스테이지 진행
            var nextStageEvent = CreateStoryProgressEvent("다음 스테이지", "지하실을 나가 다음 스테이지로 진행합니다.", 
                new[]{"BasementDoor"}, new[]{"Flashlight", "Battery", "Map"});
            eventSequenceProp.GetArrayElementAtIndex(6).objectReferenceValue = nextStageEvent;
            
            sesSO.ApplyModifiedPropertiesWithoutUndo();
            
            // StageInteractionSystem 설정
            SetupCommonStageInteractionSystem(services);
            
            // 이벤트 라우팅 설정
            SetupEventRouting(interaction, stageDefinition);
            
            // 상호작용 목록 설정
            SetupInteractionList(services);
            
            // 스토리 핵심 시스템 설정
            SetupStoryCoreSystems(services);
            
            Debug.Log($"[Stage4Creator] Stage4 특정 로직 생성 완료");
        }

        private void CreateJumpScareTrigger(GameObject parent, string triggerId, Vector3 position, bool isActive)
        {
            var trigger = new GameObject(triggerId);
            trigger.transform.SetParent(parent.transform);
            trigger.transform.localPosition = position;
            
            // 점프스케어 트리거 컴포넌트 추가 (임시로 빈 GameObject로 대체)
            Debug.Log($"[Stage4Creator] 점프스케어 트리거 생성 완료: {triggerId} (컴포넌트는 나중에 추가 예정)");
            // 점프스케어 설정 (임시로 주석 처리)
            // var so = new SerializedObject(jumpScareTrigger);
            // so.FindProperty("jumpscareSprite").objectReferenceValue = Resources.Load<Sprite>("Images/Placeholders/Jumpscare");
            // so.FindProperty("duration").floatValue = 2.0f;
            // so.FindProperty("volume").floatValue = 0.8f;
            // so.ApplyModifiedPropertiesWithoutUndo();
            
            // 초기 상태 설정
            trigger.SetActive(isActive);
            
            Debug.Log($"[Stage4Creator] 점프스케어 트리거 생성 완료: {triggerId}");
        }

        private AFKS.Features.Stage.StageDefinition CreateStageDefinition()
        {
            var definition = ScriptableObject.CreateInstance<AFKS.Features.Stage.StageDefinition>();
            definition.name = $"{stageName}_Definition";
            
            // 에셋으로 저장
            string assetPath = $"Assets/Stages/{stageName}_Definition.asset";
            EnsureDirectory("Assets/Stages");
            AssetDatabase.CreateAsset(definition, assetPath);
            
            Debug.Log($"[Stage4Creator] StageDefinition 생성 완료: {assetPath}");
            return definition;
        }

        private void SetupEventRouting(GameObject interaction, AFKS.Features.Stage.StageDefinition definition)
        {
            // 손전등 이벤트 라우팅
            var flashlight = interaction.transform.Find("Flashlight");
            if (flashlight != null)
            {
                var router = flashlight.GetComponent<AFKS.Features.Stage.Events.ClickEvent>();
                if (router != null)
                {
                    var so = new SerializedObject(router);
                    so.FindProperty("targetEvent").objectReferenceValue = definition;
                    so.FindProperty("eventIndex").intValue = 0;
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
            }
            
            // 배터리 이벤트 라우팅
            var battery = interaction.transform.Find("Battery");
            if (battery != null)
            {
                var router = battery.GetComponent<AFKS.Features.Stage.Events.ClickEvent>();
                if (router != null)
                {
                    var so = new SerializedObject(router);
                    so.FindProperty("targetEvent").objectReferenceValue = definition;
                    so.FindProperty("eventIndex").intValue = 1;
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
            }
            
            // 지도 이벤트 라우팅
            var map = interaction.transform.Find("Map");
            if (map != null)
            {
                var router = map.GetComponent<AFKS.Features.Stage.Events.ClickEvent>();
                if (router != null)
                {
                    var so = new SerializedObject(router);
                    so.FindProperty("targetEvent").objectReferenceValue = definition;
                    so.FindProperty("eventIndex").intValue = 2;
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
            }
            
            // 점프스케어 트리거 이벤트 라우팅
            var jumpScareTrigger = interaction.transform.Find("JumpScareTrigger");
            if (jumpScareTrigger != null)
            {
                var router = jumpScareTrigger.GetComponent<AFKS.Features.Stage.Events.ClickEvent>();
                if (router != null)
                {
                    var so = new SerializedObject(router);
                    so.FindProperty("targetEvent").objectReferenceValue = definition;
                    so.FindProperty("eventIndex").intValue = 4; // 점프스케어 실행
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
            }
            
            // 지하실 문 이벤트 라우팅
            var door = interaction.transform.Find("BasementDoor");
            if (door != null)
            {
                var router = door.GetComponent<AFKS.Features.Stage.Events.ClickEvent>();
                if (router != null)
                {
                    var so = new SerializedObject(router);
                    so.FindProperty("targetEvent").objectReferenceValue = definition;
                    so.FindProperty("eventIndex").intValue = 6; // 다음 스테이지 진행
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
            }
            
            Debug.Log($"[Stage4Creator] 이벤트 라우팅 설정 완료");
        }

        private void SetupInteractionList(GameObject services)
        {
            var sis = services.GetComponent<AFKS.Features.Stage.StageInteractionSystem>();
            if (sis != null)
            {
                var so = new SerializedObject(sis);
                var interactableObjectsProp = so.FindProperty("interactableObjects");
                
                interactableObjectsProp.ClearArray();
                interactableObjectsProp.arraySize = 5;
                
                interactableObjectsProp.GetArrayElementAtIndex(0).stringValue = "Flashlight";
                interactableObjectsProp.GetArrayElementAtIndex(1).stringValue = "Battery";
                interactableObjectsProp.GetArrayElementAtIndex(2).stringValue = "Map";
                interactableObjectsProp.GetArrayElementAtIndex(3).stringValue = "JumpScareTrigger";
                interactableObjectsProp.GetArrayElementAtIndex(4).stringValue = "BasementDoor";
                
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            
            Debug.Log($"[Stage4Creator] 상호작용 목록 설정 완료");
        }
    }
}
