using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

namespace AFKS.Tools.Editor.StageCreators
{
    /// <summary>
    /// Stage3 (학교 교실) 생성을 담당하는 Creator 클래스입니다.
    /// 기획서: 교실에서 아이템 수집 → 귀신 등장 → 다음 스테이지 진행
    /// </summary>
    public class Stage3Creator : BaseStageCreator
    {
        public Stage3Creator() : base("Stage3", "Stage4", true)
        {
        }

        public override void CreateStage(string path)
        {
            Debug.Log($"[Stage3Creator] Stage3 생성 시작: {path}");
            
            // 씬 생성 및 기본 설정
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            
            // 카메라 설정
            var camera = CreateStageCamera(stageName, new Vector3(0, 0, -10), 5f);
            
            // 공통 월드 구조 생성
            var (world, interaction, services) = CreateCommonWorldStructure();
            
            // 스테이지별 배경 생성 (School Classroom 배경 사용)
            CreateStageBackground(world, "Images/Backgrounds/Stage3/Stage3", new Vector3(16, 9, 1), Vector3.zero);
            
            // 스테이지별 로직 생성
            CreateStageSpecificLogic(world, interaction, services);
            
            // 씬 저장
            SaveSceneWithSafety(scene, path);
            
            Debug.Log($"[Stage3Creator] Stage3 생성 완료: {path}");
        }

        protected override void CreateStageSpecificLogic(GameObject world, GameObject interaction, GameObject services)
        {
            // 아이템 생성
            CreateStoryItem(interaction, "Textbook", "Images/Placeholders/Textbook", new Vector3(-3, 1, 0));
            CreateStoryItem(interaction, "Diary", "Images/Placeholders/Diary", new Vector3(3, 1, 0));
            CreateStoryItem(interaction, "Key", "Images/Placeholders/Key", new Vector3(0, -2, 0));
            
            // 문 생성 (초기에는 잠겨있음)
            CreateLockableDoor(interaction, "ClassroomDoor", "Stage4", new Vector3(0, -3, 0), true);
            
            // 귀신 생성 (초기에는 비활성화)
            CreateGhostEncounter(interaction, "SchoolGhost", "Images/Placeholders/Ghost", new Vector3(0, 0, 0), false);
            
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
            eventSequenceProp.arraySize = 6;
            
            // 1. 교과서 수집 이벤트
            var textbookEvent = CreateStoryProgressEvent("교과서 수집", "교과서를 수집했습니다.", new[]{"Textbook"});
            eventSequenceProp.GetArrayElementAtIndex(0).objectReferenceValue = textbookEvent;
            
            // 2. 일기장 수집 이벤트
            var diaryEvent = CreateStoryProgressEvent("일기장 수집", "일기장을 수집했습니다.", new[]{"Diary"});
            eventSequenceProp.GetArrayElementAtIndex(1).objectReferenceValue = diaryEvent;
            
            // 3. 열쇠 수집 이벤트
            var keyEvent = CreateStoryProgressEvent("열쇠 수집", "교실 열쇠를 수집했습니다.", new[]{"Key"});
            eventSequenceProp.GetArrayElementAtIndex(2).objectReferenceValue = keyEvent;
            
            // 4. 교실 문 잠금 해제 (모든 아이템 수집 후)
            var unlockDoorEvent = CreateStoryProgressEvent("교실 문 잠금 해제", "교실 문이 열렸습니다.", 
                new[]{"ClassroomDoor"}, new[]{"Textbook", "Diary", "Key"});
            eventSequenceProp.GetArrayElementAtIndex(3).objectReferenceValue = unlockDoorEvent;
            
            // 5. 귀신 등장 (모든 아이템 수집 후)
            var ghostEvent = CreateStoryProgressEvent("귀신 등장", "교실에 귀신이 나타났습니다!", 
                new[]{"SchoolGhost"}, new[]{"Textbook", "Diary", "Key"});
            eventSequenceProp.GetArrayElementAtIndex(4).objectReferenceValue = ghostEvent;
            
            // 6. 다음 스테이지 진행
            var nextStageEvent = CreateStoryProgressEvent("다음 스테이지", "교실을 나가 다음 스테이지로 진행합니다.", 
                new[]{"ClassroomDoor"}, new[]{"Textbook", "Diary", "Key"});
            eventSequenceProp.GetArrayElementAtIndex(5).objectReferenceValue = nextStageEvent;
            
            sesSO.ApplyModifiedPropertiesWithoutUndo();
            
            // StageInteractionSystem 설정
            SetupCommonStageInteractionSystem(services);
            
            // 이벤트 라우팅 설정
            SetupEventRouting(interaction, stageDefinition);
            
            // 상호작용 목록 설정
            SetupInteractionList(services);
            
            // 스토리 핵심 시스템 설정
            SetupStoryCoreSystems(services);
            
            Debug.Log($"[Stage3Creator] Stage3 특정 로직 생성 완료");
        }

        private AFKS.Features.Stage.StageDefinition CreateStageDefinition()
        {
            var definition = ScriptableObject.CreateInstance<AFKS.Features.Stage.StageDefinition>();
            definition.name = $"{stageName}_Definition";
            
            // 에셋으로 저장
            string assetPath = $"Assets/Stages/{stageName}_Definition.asset";
            EnsureDirectory("Assets/Stages");
            AssetDatabase.CreateAsset(definition, assetPath);
            
            Debug.Log($"[Stage3Creator] StageDefinition 생성 완료: {assetPath}");
            return definition;
        }

        private void SetupEventRouting(GameObject interaction, AFKS.Features.Stage.StageDefinition definition)
        {
            // 교과서 이벤트 라우팅
            var textbook = interaction.transform.Find("Textbook");
            if (textbook != null)
            {
                var router = textbook.GetComponent<AFKS.Features.Stage.Events.ClickEvent>();
                if (router != null)
                {
                    var so = new SerializedObject(router);
                    so.FindProperty("targetEvent").objectReferenceValue = definition;
                    so.FindProperty("eventIndex").intValue = 0;
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
            }
            
            // 일기장 이벤트 라우팅
            var diary = interaction.transform.Find("Diary");
            if (diary != null)
            {
                var router = diary.GetComponent<AFKS.Features.Stage.Events.ClickEvent>();
                if (router != null)
                {
                    var so = new SerializedObject(router);
                    so.FindProperty("targetEvent").objectReferenceValue = definition;
                    so.FindProperty("eventIndex").intValue = 1;
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
            }
            
            // 열쇠 이벤트 라우팅
            var key = interaction.transform.Find("Key");
            if (key != null)
            {
                var router = key.GetComponent<AFKS.Features.Stage.Events.ClickEvent>();
                if (router != null)
                {
                    var so = new SerializedObject(router);
                    so.FindProperty("targetEvent").objectReferenceValue = definition;
                    so.FindProperty("eventIndex").intValue = 2;
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
            }
            
            // 교실 문 이벤트 라우팅
            var door = interaction.transform.Find("ClassroomDoor");
            if (door != null)
            {
                var router = door.GetComponent<AFKS.Features.Stage.Events.ClickEvent>();
                if (router != null)
                {
                    var so = new SerializedObject(router);
                    so.FindProperty("targetEvent").objectReferenceValue = definition;
                    so.FindProperty("eventIndex").intValue = 5; // 다음 스테이지 진행
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
            }
            
            Debug.Log($"[Stage3Creator] 이벤트 라우팅 설정 완료");
        }

        private void SetupInteractionList(GameObject services)
        {
            var sis = services.GetComponent<AFKS.Features.Stage.StageInteractionSystem>();
            if (sis != null)
            {
                var so = new SerializedObject(sis);
                var interactableObjectsProp = so.FindProperty("interactableObjects");
                
                interactableObjectsProp.ClearArray();
                interactableObjectsProp.arraySize = 4;
                
                interactableObjectsProp.GetArrayElementAtIndex(0).stringValue = "Textbook";
                interactableObjectsProp.GetArrayElementAtIndex(1).stringValue = "Diary";
                interactableObjectsProp.GetArrayElementAtIndex(2).stringValue = "Key";
                interactableObjectsProp.GetArrayElementAtIndex(3).stringValue = "ClassroomDoor";
                
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            
            Debug.Log($"[Stage3Creator] 상호작용 목록 설정 완료");
        }
    }
}
