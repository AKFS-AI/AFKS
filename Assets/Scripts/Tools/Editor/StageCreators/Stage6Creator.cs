using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

namespace AFKS.Tools.Editor.StageCreators
{
    /// <summary>
    /// Stage6 (최종 스테이지) 생성을 담당하는 Creator 클래스입니다.
    /// 기획서: 최종 스테이지 완료 → 엔딩 → 메뉴로 복귀
    /// </summary>
    public class Stage6Creator : BaseStageCreator
    {
        public Stage6Creator() : base("Stage6", "Menu", true)
        {
        }

        public override void CreateStage(string path)
        {
            Debug.Log($"[Stage6Creator] Stage6 생성 시작: {path}");
            
            // 씬 생성 및 기본 설정
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            
            // 카메라 설정
            var camera = CreateStageCamera(stageName, new Vector3(0, 0, -10), 5f);
            
            // 공통 월드 구조 생성
            var (world, interaction, services) = CreateCommonWorldStructure();
            
            // 스테이지별 배경 생성 (Final Stage 배경 사용)
            CreateStageBackground(world, "Images/Backgrounds/Stage6/Stage6", new Vector3(16, 9, 1), Vector3.zero);
            
            // 스테이지별 로직 생성
            CreateStageSpecificLogic(world, interaction, services);
            
            // 씬 저장
            SaveSceneWithSafety(scene, path);
            
            Debug.Log($"[Stage6Creator] Stage6 생성 완료: {path}");
        }

        protected override void CreateStageSpecificLogic(GameObject world, GameObject interaction, GameObject services)
        {
            // 최종 아이템 생성
            CreateStoryItem(interaction, "FinalKey", "Images/Placeholders/FinalKey", new Vector3(-3, 2, 0));
            CreateStoryItem(interaction, "EscapeRoute", "Images/Placeholders/EscapeRoute", new Vector3(3, 2, 0));
            
            // 최종 문 생성 (초기에는 잠겨있음)
            CreateLockableDoor(interaction, "FinalDoor", "Menu", new Vector3(0, -3, 0), true);
            
            // 엔딩 트리거 생성 (초기에는 비활성화)
            CreateEndingTrigger(interaction, "EndingTrigger", new Vector3(0, 0, 0), false);
            
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
            
            // 1. 최종 열쇠 수집 이벤트
            var finalKeyEvent = CreateStoryProgressEvent("최종 열쇠 수집", "탈출을 위한 최종 열쇠를 수집했습니다.", new[]{"FinalKey"});
            eventSequenceProp.GetArrayElementAtIndex(0).objectReferenceValue = finalKeyEvent;
            
            // 2. 탈출 경로 발견 이벤트
            var escapeRouteEvent = CreateStoryProgressEvent("탈출 경로 발견", "집을 빠져나갈 수 있는 경로를 발견했습니다.", new[]{"EscapeRoute"});
            eventSequenceProp.GetArrayElementAtIndex(1).objectReferenceValue = escapeRouteEvent;
            
            // 3. 엔딩 트리거 활성화 (모든 아이템 수집 후)
            var endingTriggerEvent = CreateStoryProgressEvent("엔딩 준비", "이제 집을 빠져나갈 수 있습니다...", 
                new[]{"EndingTrigger"}, new[]{"FinalKey", "EscapeRoute"});
            eventSequenceProp.GetArrayElementAtIndex(2).objectReferenceValue = endingTriggerEvent;
            
            // 4. 엔딩 실행
            var endingEvent = CreateStoryProgressEvent("엔딩", "집에서 성공적으로 탈출했습니다!", 
                new[]{"EndingTrigger"}, new[]{"FinalKey", "EscapeRoute"});
            eventSequenceProp.GetArrayElementAtIndex(3).objectReferenceValue = endingEvent;
            
            // 5. 최종 문 잠금 해제 (엔딩 후)
            var unlockFinalDoorEvent = CreateStoryProgressEvent("최종 문 잠금 해제", "집을 나가는 문이 열렸습니다.", 
                new[]{"FinalDoor"}, new[]{"FinalKey", "EscapeRoute"});
            eventSequenceProp.GetArrayElementAtIndex(4).objectReferenceValue = unlockFinalDoorEvent;
            
            // 6. 메뉴로 복귀
            var returnToMenuEvent = CreateStoryProgressEvent("메뉴로 복귀", "모든 스테이지를 완료하고 메뉴로 돌아갑니다.", 
                new[]{"FinalDoor"}, new[]{"FinalKey", "EscapeRoute"});
            eventSequenceProp.GetArrayElementAtIndex(5).objectReferenceValue = returnToMenuEvent;
            
            sesSO.ApplyModifiedPropertiesWithoutUndo();
            
            // StageInteractionSystem 설정
            SetupCommonStageInteractionSystem(services);
            
            // 이벤트 라우팅 설정
            SetupEventRouting(interaction, stageDefinition);
            
            // 상호작용 목록 설정
            SetupInteractionList(services);
            
            // 스토리 핵심 시스템 설정
            SetupStoryCoreSystems(services);
            
            Debug.Log($"[Stage6Creator] Stage6 특정 로직 생성 완료");
        }

        private void CreateEndingTrigger(GameObject parent, string triggerId, Vector3 position, bool isActive)
        {
            var trigger = new GameObject(triggerId);
            trigger.transform.SetParent(parent.transform);
            trigger.transform.localPosition = position;
            
            // 엔딩 트리거 컴포넌트 추가 (임시로 빈 GameObject로 대체)
            Debug.Log($"[Stage6Creator] 엔딩 트리거 생성 완료: {triggerId} (컴포넌트는 나중에 추가 예정)");
            // 엔딩 설정 (임시로 주석 처리)
            // var so = new SerializedObject(endingTrigger);
            // so.FindProperty("endingDuration").floatValue = 8.0f;
            // so.FindProperty("showCredits").boolValue = true;
            // so.FindProperty("volume").floatValue = 1.0f;
            // so.ApplyModifiedPropertiesWithoutUndo();
            
            // 초기 상태 설정
            trigger.SetActive(isActive);
            
            Debug.Log($"[Stage6Creator] 엔딩 트리거 생성 완료: {triggerId}");
        }

        private AFKS.Features.Stage.StageDefinition CreateStageDefinition()
        {
            var definition = ScriptableObject.CreateInstance<AFKS.Features.Stage.StageDefinition>();
            definition.name = $"{stageName}_Definition";
            
            // 에셋으로 저장
            string assetPath = $"Assets/Stages/{stageName}_Definition.asset";
            EnsureDirectory("Assets/Stages");
            AssetDatabase.CreateAsset(definition, assetPath);
            
            Debug.Log($"[Stage6Creator] StageDefinition 생성 완료: {assetPath}");
            return definition;
        }

        private void SetupEventRouting(GameObject interaction, AFKS.Features.Stage.StageDefinition definition)
        {
            // 최종 열쇠 이벤트 라우팅
            var finalKey = interaction.transform.Find("FinalKey");
            if (finalKey != null)
            {
                var router = finalKey.GetComponent<AFKS.Features.Stage.Events.ClickEvent>();
                if (router != null)
                {
                    var so = new SerializedObject(router);
                    so.FindProperty("targetEvent").objectReferenceValue = definition;
                    so.FindProperty("eventIndex").intValue = 0;
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
            }
            
            // 탈출 경로 이벤트 라우팅
            var escapeRoute = interaction.transform.Find("EscapeRoute");
            if (escapeRoute != null)
            {
                var router = escapeRoute.GetComponent<AFKS.Features.Stage.Events.ClickEvent>();
                if (router != null)
                {
                    var so = new SerializedObject(router);
                    so.FindProperty("targetEvent").objectReferenceValue = definition;
                    so.FindProperty("eventIndex").intValue = 1;
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
            }
            
            // 엔딩 트리거 이벤트 라우팅
            var endingTrigger = interaction.transform.Find("EndingTrigger");
            if (endingTrigger != null)
            {
                var router = endingTrigger.GetComponent<AFKS.Features.Stage.Events.ClickEvent>();
                if (router != null)
                {
                    var so = new SerializedObject(router);
                    so.FindProperty("targetEvent").objectReferenceValue = definition;
                    so.FindProperty("eventIndex").intValue = 3; // 엔딩 실행
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
            }
            
            // 최종 문 이벤트 라우팅
            var finalDoor = interaction.transform.Find("FinalDoor");
            if (finalDoor != null)
            {
                var router = finalDoor.GetComponent<AFKS.Features.Stage.Events.ClickEvent>();
                if (router != null)
                {
                    var so = new SerializedObject(router);
                    so.FindProperty("targetEvent").objectReferenceValue = definition;
                    so.FindProperty("eventIndex").intValue = 5; // 메뉴로 복귀
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
            }
            
            Debug.Log($"[Stage6Creator] 이벤트 라우팅 설정 완료");
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
                
                interactableObjectsProp.GetArrayElementAtIndex(0).stringValue = "FinalKey";
                interactableObjectsProp.GetArrayElementAtIndex(1).stringValue = "EscapeRoute";
                interactableObjectsProp.GetArrayElementAtIndex(2).stringValue = "EndingTrigger";
                interactableObjectsProp.GetArrayElementAtIndex(3).stringValue = "FinalDoor";
                
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            
            Debug.Log($"[Stage6Creator] 상호작용 목록 설정 완료");
        }
    }
}
