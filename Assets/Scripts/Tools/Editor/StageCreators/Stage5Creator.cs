using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

namespace AFKS.Tools.Editor.StageCreators
{
    /// <summary>
    /// Stage5 (옥상) 생성을 담당하는 Creator 클래스입니다.
    /// 기획서: 옥상에서 아이템 수집 → 최종 도전 → 다음 스테이지 진행
    /// </summary>
    public class Stage5Creator : BaseStageCreator
    {
        public Stage5Creator() : base("Stage5", "Stage6", true)
        {
        }

        public override void CreateStage(string path)
        {
            Debug.Log($"[Stage5Creator] Stage5 생성 시작: {path}");
            
            // 씬 생성 및 기본 설정
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            
            // 카메라 설정
            var camera = CreateStageCamera(stageName, new Vector3(0, 0, -10), 5f);
            
            // 공통 월드 구조 생성
            var (world, interaction, services) = CreateCommonWorldStructure();
            
            // 스테이지별 배경 생성 (Rooftop 배경 사용)
            CreateStageBackground(world, "Images/Backgrounds/Stage5/Stage5_1", new Vector3(16, 9, 1), Vector3.zero);
            
            // 스테이지별 로직 생성
            CreateStageSpecificLogic(world, interaction, services);
            
            // 씬 저장
            SaveSceneWithSafety(scene, path);
            
            Debug.Log($"[Stage5Creator] Stage5 생성 완료: {path}");
        }

        protected override void CreateStageSpecificLogic(GameObject world, GameObject interaction, GameObject services)
        {
            // 아이템 생성
            CreateStoryItem(interaction, "Rope", "Images/Placeholders/Rope", new Vector3(-5, 2, 0));
            CreateStoryItem(interaction, "Hook", "Images/Placeholders/Hook", new Vector3(5, 2, 0));
            CreateStoryItem(interaction, "Pulley", "Images/Placeholders/Pulley", new Vector3(0, 3, 0));
            
            // 문 생성 (초기에는 잠겨있음)
            CreateLockableDoor(interaction, "RoofDoor", "Stage6", new Vector3(0, -3, 0), true);
            
            // 최종 도전 트리거 생성 (초기에는 비활성화)
            CreateFinalChallengeTrigger(interaction, "FinalChallenge", new Vector3(0, 0, 0), false);
            
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
            eventSequenceProp.arraySize = 8;
            
            // 1. 밧줄 수집 이벤트
            var ropeEvent = CreateStoryProgressEvent("밧줄 수집", "밧줄을 수집했습니다.", new[]{"Rope"});
            eventSequenceProp.GetArrayElementAtIndex(0).objectReferenceValue = ropeEvent;
            
            // 2. 갈고리 수집 이벤트
            var hookEvent = CreateStoryProgressEvent("갈고리 수집", "갈고리를 수집했습니다.", new[]{"Hook"});
            eventSequenceProp.GetArrayElementAtIndex(1).objectReferenceValue = hookEvent;
            
            // 3. 도르래 수집 이벤트
            var pulleyEvent = CreateStoryProgressEvent("도르래 수집", "도르래를 수집했습니다.", new[]{"Pulley"});
            eventSequenceProp.GetArrayElementAtIndex(2).objectReferenceValue = pulleyEvent;
            
            // 4. 최종 도전 트리거 활성화 (모든 아이템 수집 후)
            var challengeTriggerEvent = CreateStoryProgressEvent("최종 도전 준비", "옥상에서 탈출할 방법을 찾았습니다...", 
                new[]{"FinalChallenge"}, new[]{"Rope", "Hook", "Pulley"});
            eventSequenceProp.GetArrayElementAtIndex(3).objectReferenceValue = challengeTriggerEvent;
            
            // 5. 최종 도전 실행
            var challengeEvent = CreateStoryProgressEvent("최종 도전!", "밧줄을 타고 옥상에서 내려갑니다!", 
                new[]{"FinalChallenge"}, new[]{"Rope", "Hook", "Pulley"});
            eventSequenceProp.GetArrayElementAtIndex(4).objectReferenceValue = challengeEvent;
            
            // 6. 최종 도전 완료
            var challengeCompleteEvent = CreateStoryProgressEvent("도전 완료", "옥상에서 성공적으로 탈출했습니다!", 
                new[]{"FinalChallenge"}, new[]{"Rope", "Hook", "Pulley"});
            eventSequenceProp.GetArrayElementAtIndex(5).objectReferenceValue = challengeCompleteEvent;
            
            // 7. 옥상 문 잠금 해제 (도전 완료 후)
            var unlockDoorEvent = CreateStoryProgressEvent("옥상 문 잠금 해제", "옥상 문이 열렸습니다.", 
                new[]{"RoofDoor"}, new[]{"Rope", "Hook", "Pulley"});
            eventSequenceProp.GetArrayElementAtIndex(6).objectReferenceValue = unlockDoorEvent;
            
            // 8. 다음 스테이지 진행
            var nextStageEvent = CreateStoryProgressEvent("다음 스테이지", "옥상을 나가 다음 스테이지로 진행합니다.", 
                new[]{"RoofDoor"}, new[]{"Rope", "Hook", "Pulley"});
            eventSequenceProp.GetArrayElementAtIndex(7).objectReferenceValue = nextStageEvent;
            
            sesSO.ApplyModifiedPropertiesWithoutUndo();
            
            // StageInteractionSystem 설정
            SetupCommonStageInteractionSystem(services);
            
            // 이벤트 라우팅 설정
            SetupEventRouting(interaction, stageDefinition);
            
            // 상호작용 목록 설정
            SetupInteractionList(services);
            
            // 스토리 핵심 시스템 설정
            SetupStoryCoreSystems(services);
            
            Debug.Log($"[Stage5Creator] Stage5 특정 로직 생성 완료");
        }

        private void CreateFinalChallengeTrigger(GameObject parent, string triggerId, Vector3 position, bool isActive)
        {
            var trigger = new GameObject(triggerId);
            trigger.transform.SetParent(parent.transform);
            trigger.transform.localPosition = position;
            
            // 최종 도전 트리거 컴포넌트 추가 (임시로 빈 GameObject로 대체)
            Debug.Log($"[Stage5Creator] 최종 도전 트리거 생성 완료: {triggerId} (컴포넌트는 나중에 추가 예정)");
            // 도전 설정 (임시로 주석 처리)
            // var so = new SerializedObject(challengeTrigger);
            // so.FindProperty("challengeDuration").floatValue = 5.0f;
            // so.FindProperty("successRate").floatValue = 0.8f;
            // so.FindProperty("volume").floatValue = 0.9f;
            // so.FindProperty("volume").floatValue = 0.9f;
            // so.ApplyModifiedPropertiesWithoutUndo();
            
            // 초기 상태 설정
            trigger.SetActive(isActive);
            
            Debug.Log($"[Stage5Creator] 최종 도전 트리거 생성 완료: {triggerId}");
        }

        private AFKS.Features.Stage.StageDefinition CreateStageDefinition()
        {
            var definition = ScriptableObject.CreateInstance<AFKS.Features.Stage.StageDefinition>();
            definition.name = $"{stageName}_Definition";
            
            // 에셋으로 저장
            string assetPath = $"Assets/Stages/{stageName}_Definition.asset";
            EnsureDirectory("Assets/Stages");
            AssetDatabase.CreateAsset(definition, assetPath);
            
            Debug.Log($"[Stage5Creator] StageDefinition 생성 완료: {assetPath}");
            return definition;
        }

        private void SetupEventRouting(GameObject interaction, AFKS.Features.Stage.StageDefinition definition)
        {
            // 밧줄 이벤트 라우팅
            var rope = interaction.transform.Find("Rope");
            if (rope != null)
            {
                var router = rope.GetComponent<AFKS.Features.Stage.Events.ClickEvent>();
                if (router != null)
                {
                    var so = new SerializedObject(router);
                    so.FindProperty("targetEvent").objectReferenceValue = definition;
                    so.FindProperty("eventIndex").intValue = 0;
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
            }
            
            // 갈고리 이벤트 라우팅
            var hook = interaction.transform.Find("Hook");
            if (hook != null)
            {
                var router = hook.GetComponent<AFKS.Features.Stage.Events.ClickEvent>();
                if (router != null)
                {
                    var so = new SerializedObject(router);
                    so.FindProperty("targetEvent").objectReferenceValue = definition;
                    so.FindProperty("eventIndex").intValue = 1;
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
            }
            
            // 도르래 이벤트 라우팅
            var pulley = interaction.transform.Find("Pulley");
            if (pulley != null)
            {
                var router = pulley.GetComponent<AFKS.Features.Stage.Events.ClickEvent>();
                if (router != null)
                {
                    var so = new SerializedObject(router);
                    so.FindProperty("targetEvent").objectReferenceValue = definition;
                    so.FindProperty("eventIndex").intValue = 2;
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
            }
            
            // 최종 도전 트리거 이벤트 라우팅
            var finalChallenge = interaction.transform.Find("FinalChallenge");
            if (finalChallenge != null)
            {
                var router = finalChallenge.GetComponent<AFKS.Features.Stage.Events.ClickEvent>();
                if (router != null)
                {
                    var so = new SerializedObject(router);
                    so.FindProperty("targetEvent").objectReferenceValue = definition;
                    so.FindProperty("eventIndex").intValue = 4; // 최종 도전 실행
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
            }
            
            // 옥상 문 이벤트 라우팅
            var door = interaction.transform.Find("RoofDoor");
            if (door != null)
            {
                var router = door.GetComponent<AFKS.Features.Stage.Events.ClickEvent>();
                if (router != null)
                {
                    var so = new SerializedObject(router);
                    so.FindProperty("targetEvent").objectReferenceValue = definition;
                    so.FindProperty("eventIndex").intValue = 7; // 다음 스테이지 진행
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
            }
            
            Debug.Log($"[Stage5Creator] 이벤트 라우팅 설정 완료");
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
                
                interactableObjectsProp.GetArrayElementAtIndex(0).stringValue = "Rope";
                interactableObjectsProp.GetArrayElementAtIndex(1).stringValue = "Hook";
                interactableObjectsProp.GetArrayElementAtIndex(2).stringValue = "Pulley";
                interactableObjectsProp.GetArrayElementAtIndex(3).stringValue = "FinalChallenge";
                interactableObjectsProp.GetArrayElementAtIndex(4).stringValue = "RoofDoor";
                
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            
            Debug.Log($"[Stage5Creator] 상호작용 목록 설정 완료");
        }
    }
}
