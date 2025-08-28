using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEditor.Events;

namespace AFKS.Tools.Editor.StageCreators
{
    /// <summary>
    /// Stage1의 복잡한 로직(체인 상호작용, 줌 효과 등)을 담당하는 Creator입니다.
    /// 기존 SceneScaffolderWindow의 CreateOrOverwriteStage1() 메서드와 동일한 기능을 제공합니다.
    /// </summary>
    public class Stage1Creator : BaseStageCreator
    {
        public Stage1Creator() : base("Stage1", "Stage2", true)
        {
            // Stage1은 복잡한 게임플레이를 가집니다
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
            
            // 2. 스테이지별 배경 설정 (Stage1_2_0 이미지 사용)
            var bg = CreateStageBackground(world, "Images/Backgrounds/Stage1/Stage1_2", 
                new Vector3(1.25f, 1.06f, 1f), new Vector3(0, 0, 10), -1);

            // 4. Stage1 전용 로직 생성
            CreateStageSpecificLogic(world, interaction, services);

            // 5. StageRoot 생성
            var root = new GameObject("StageRoot");
            root.transform.SetParent(services.transform, false);
            root.AddComponent<AFKS.Features.Stage.StageRoot>();

            // 6. 씬 저장 및 Build Settings 등록
            SaveSceneWithSafety(sc, path);
            RegisterSceneInBuildSettings(path);
            
            if (!EditorUtility.DisplayDialog("완료", $"Stage1.unity 생성/덮어쓰기 완료:\n{path}", "확인"))
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
            
            // Chain (위쪽 체인) - 문 위쪽에 걸기
            var chainTop = CreateChainTop(hotspots);
            
            // Chain (아래쪽 체인) - 문 아래쪽에 걸기
            var chainBottom = CreateChainBottom(hotspots);
            
            // Door (초기에는 비활성)
            var door = CreateDoor(hotspots);
            
            // StageDefinition 생성 및 이벤트 설정
            var def = CreateStageDefinition();
            
            // StageEventSystem 설정
            var ses = SetupCommonStageEventSystem(services, def);
            
            // 스토리 핵심 시스템 설정
            SetupStoryCoreSystems(services);
            
            // 체인 클릭 라우팅 설정
            SetupChainClickRouting(chainTop, chainBottom, ses);
            
            // Door 라우터 설정
            SetupDoorRouter(door, ses);
            
            // StageInteractionSystem 상호작용 목록 구성
            SetupInteractionList(services, door, chainTop, chainBottom);
        }

        private GameObject CreateChainTop(GameObject hotspots)
        {
            var chainTop = new GameObject("ChainTop");
            chainTop.transform.SetParent(hotspots.transform, false);
            var chainTopSr = chainTop.AddComponent<SpriteRenderer>();
            
            // 위쪽 체인 위치 조정 (문 위쪽에 걸기)
            chainTop.transform.localPosition = new Vector3(0f, 0.1f, 9f);
            
            // 위쪽 체인 이미지 로드
            var chainTopSprite = Resources.Load<Sprite>("Images/Interactables/Stage1/chain_1");
            if (chainTopSprite != null)
            {
                var allSprites = Resources.LoadAll<Sprite>("Images/Interactables/Stage1/chain_1");
                var targetSprite = System.Array.Find(allSprites, s => s.name == "chain_1_0");
                
                if (targetSprite != null)
                {
                    chainTopSr.sprite = targetSprite;
                }
                else
                {
                    chainTopSr.sprite = chainTopSprite;
                    Debug.Log("위쪽 체인 이미지 로드됨: chain_1.png 전체 스프라이트 시트");
                }
            }
            else
            {
                chainTopSr.sprite = EnsurePlaceholderSprite(Path.Combine("Assets/Resources/Placeholders", "Interactable_Green.png"), new Color32(0, 255, 0, 255));
                Debug.Log("위쪽 체인 이미지 대체: 밝은 초록색 플레이스홀더 사용");
            }
            
            // 체인은 BoxCollider2D를 사용
            var chainTopCol = chainTop.AddComponent<BoxCollider2D>();
            chainTopCol.size = new Vector2(1.4f, 0.3f);
            chainTopCol.isTrigger = true;
            
            // 위쪽 체인 스케일 조정
            chainTop.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
            
            // 위쪽 체인 가시성 개선
            chainTopSr.sortingOrder = 1;
            chainTopSr.color = Color.white;
            
            var clickHandlerTop = chainTop.AddComponent<AFKS.Features.Interaction.ClickHandler>();
            var soClickTop = new SerializedObject(clickHandlerTop);
            soClickTop.FindProperty("clickable").boolValue = false;
            
            // ClickHandler 추가 설정
            soClickTop.FindProperty("clickCooldown").floatValue = 0.1f;
            soClickTop.FindProperty("maxClicksPerSecond").intValue = 10;
            soClickTop.FindProperty("enableClickFeedback").boolValue = true;
            soClickTop.FindProperty("clickFeedbackDuration").floatValue = 0.2f;
            soClickTop.ApplyModifiedPropertiesWithoutUndo();
            
            // ChainBreakEffect 컴포넌트 추가
            var chainBreakEffectTop = chainTop.AddComponent<AFKS.Features.Stage.Effects.ChainBreakEffect>();
            var soChainBreakTop = new SerializedObject(chainBreakEffectTop);
            
            // 체인 분리 효과 설정
            soChainBreakTop.FindProperty("separationDistance").floatValue = 0.6f;
            soChainBreakTop.FindProperty("fallDuration").floatValue = 1.8f;
            soChainBreakTop.FindProperty("fallDistance").floatValue = 2.2f;
            soChainBreakTop.FindProperty("bounceHeight").floatValue = 0.4f;
            soChainBreakTop.FindProperty("bounceCount").intValue = 3;
            soChainBreakTop.FindProperty("gravity").floatValue = 18f;
            soChainBreakTop.FindProperty("bounceDecay").floatValue = 0.65f;
            soChainBreakTop.ApplyModifiedPropertiesWithoutUndo();
            
            // ChainShakeEffect 컴포넌트 추가 (효과음 포함)
            var chainShakeEffectTop = chainTop.AddComponent<AFKS.Features.Stage.Effects.ChainShakeEffect>();
            var soChainShakeTop = new SerializedObject(chainShakeEffectTop);
            
            // 체인 흔들림 효과음 자동 설정
            var chainShakeSound = Resources.Load<AudioClip>("Sounds/Sfx/ChainShake");
            if (chainShakeSound != null)
            {
                soChainShakeTop.FindProperty("chainShakeSound").objectReferenceValue = chainShakeSound;
                Debug.Log("[Stage1Creator] ChainTop에 ChainShake 효과음 자동 설정됨");
            }
            
            // 체인 흔들림 설정
            soChainShakeTop.FindProperty("shakeIntensity").floatValue = 0.15f;
            soChainShakeTop.FindProperty("shakeSpeed").floatValue = 12f;
            soChainShakeTop.FindProperty("soundVolume").floatValue = 0.8f;
            soChainShakeTop.ApplyModifiedPropertiesWithoutUndo();
            
            // 착지 타깃 생성
            var landingTop = new GameObject("LandingTarget");
            landingTop.transform.SetParent(hotspots.transform, false);
            landingTop.transform.localPosition = new Vector3(0f, -1.5f, 9f);
            
            // Stage1은 시작 시 체인을 활성화
            chainTop.SetActive(true);
            
            return chainTop;
        }

        private GameObject CreateChainBottom(GameObject hotspots)
        {
            var chainBottom = new GameObject("ChainBottom");
            chainBottom.transform.SetParent(hotspots.transform, false);
            var chainBottomSr = chainBottom.AddComponent<SpriteRenderer>();
            
            // 아래쪽 체인 위치 조정
            chainBottom.transform.localPosition = new Vector3(0f, -0.1f, 9f);
            
            // 아래쪽 체인 이미지 로드
            var chainBottomSprite = Resources.Load<Sprite>("Images/Interactables/Stage1/chain_1");
            if (chainBottomSprite != null)
            {
                var allSprites = Resources.LoadAll<Sprite>("Images/Interactables/Stage1/chain_1");
                var targetSprite = System.Array.Find(allSprites, s => s.name == "chain_1_1");
                
                if (targetSprite != null)
                {
                    chainBottomSr.sprite = targetSprite;
                }
                else
                {
                    chainBottomSr.sprite = chainBottomSprite;
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
            
            // 아래쪽 체인 스케일 조정
            chainBottom.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
            
            // 아래쪽 체인 가시성 개선
            chainBottomSr.sortingOrder = 1;
            chainBottomSr.color = Color.white;
            
            var clickHandlerBottom = chainBottom.AddComponent<AFKS.Features.Interaction.ClickHandler>();
            var soClickBottom = new SerializedObject(clickHandlerBottom);
            soClickBottom.FindProperty("clickable").boolValue = false;
            
            // ClickHandler 추가 설정
            soClickBottom.FindProperty("clickCooldown").floatValue = 0.1f;
            soClickBottom.FindProperty("maxClicksPerSecond").intValue = 10;
            soClickBottom.FindProperty("enableClickFeedback").boolValue = true;
            soClickBottom.FindProperty("clickFeedbackDuration").floatValue = 0.2f;
            soClickBottom.ApplyModifiedPropertiesWithoutUndo();
            
            // ChainBreakEffect 컴포넌트 추가
            var chainBreakEffectBottom = chainBottom.AddComponent<AFKS.Features.Stage.Effects.ChainBreakEffect>();
            var soChainBreakBottom = new SerializedObject(chainBreakEffectBottom);
            
            // 체인 분리 효과 설정
            soChainBreakBottom.FindProperty("separationDistance").floatValue = 0.6f;
            soChainBreakBottom.FindProperty("fallDuration").floatValue = 1.8f;
            soChainBreakBottom.FindProperty("fallDistance").floatValue = 2.2f;
            soChainBreakBottom.FindProperty("bounceHeight").floatValue = 0.4f;
            soChainBreakBottom.FindProperty("bounceCount").intValue = 3;
            soChainBreakBottom.FindProperty("gravity").floatValue = 18f;
            soChainBreakBottom.FindProperty("bounceDecay").floatValue = 0.65f;
            soChainBreakBottom.ApplyModifiedPropertiesWithoutUndo();
            
            // ChainShakeEffect 컴포넌트 추가 (효과음 포함)
            var chainShakeEffectBottom = chainBottom.AddComponent<AFKS.Features.Stage.Effects.ChainShakeEffect>();
            var soChainShakeBottom = new SerializedObject(chainShakeEffectBottom);
            
            // 체인 흔들림 효과음 자동 설정
            var chainShakeSound = Resources.Load<AudioClip>("Sounds/Sfx/ChainShake");
            if (chainShakeSound != null)
            {
                soChainShakeBottom.FindProperty("chainShakeSound").objectReferenceValue = chainShakeSound;
                Debug.Log("[Stage1Creator] ChainBottom에 ChainShake 효과음 자동 설정됨");
            }
            
            // 체인 흔들림 설정
            soChainShakeBottom.FindProperty("shakeIntensity").floatValue = 0.15f;
            soChainShakeBottom.FindProperty("shakeSpeed").floatValue = 12f;
            soChainShakeBottom.FindProperty("soundVolume").floatValue = 0.8f;
            soChainShakeBottom.ApplyModifiedPropertiesWithoutUndo();
            
            // Stage1은 시작 시 체인을 활성화
            chainBottom.SetActive(true);
            
            return chainBottom;
        }

        private GameObject CreateDoor(GameObject hotspots)
        {
            var door = new GameObject("Door");
            door.transform.SetParent(hotspots.transform, false);
            door.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            var doorSr = door.AddComponent<SpriteRenderer>();
            var doorCol = door.AddComponent<BoxCollider2D>();
            doorCol.size = new Vector2(4f, 4f);
            doorCol.isTrigger = true;
            
            // HotspotMove 컴포넌트 추가
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
            
            // Door 클릭 핸들러 + StageEventSystem 라우팅
            var doorClickHandler = door.AddComponent<AFKS.Features.Interaction.ClickHandler>();
            var soDoorClick = new SerializedObject(doorClickHandler);
            soDoorClick.FindProperty("debugLog").boolValue = false;
            soDoorClick.ApplyModifiedPropertiesWithoutUndo();
            
            var doorRouter = door.AddComponent<AFKS.Features.Interaction.ClickToEventRouter>();
            var doorRouterSO = new SerializedObject(doorRouter);
            doorRouterSO.FindProperty("objectId").stringValue = "Door";
            doorRouterSO.ApplyModifiedPropertiesWithoutUndo();
            
            door.SetActive(true);
            Debug.Log("Door 초기 상태: 활성화됨 (Stage1 시작 시 카메라 줌용)");
            
            return door;
        }

        private AFKS.Features.Stage.StageDefinition CreateStageDefinition()
        {
            // 데이터 드리븐 StageDefinition 생성
            var defDir = "Assets/Stages";
            if (!Directory.Exists(defDir)) Directory.CreateDirectory(defDir);
            var defPath = Path.Combine(defDir, "Stage1Definition.asset").Replace('\\','/');
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

            // 1) 문 클릭 → 카메라 확대 (ZoomEvent 사용)
            var zoomEvent = ScriptableObject.CreateInstance<AFKS.Features.Stage.Events.ZoomEvent>();
            zoomEvent.SetupChainAreaZoom();
            var soZoomEvt = new SerializedObject(zoomEvent);
            var zoomTriggerList = soZoomEvt.FindProperty("triggerObjectIds"); 
            zoomTriggerList.ClearArray();
            int zidx = zoomTriggerList.arraySize; 
            zoomTriggerList.InsertArrayElementAtIndex(zidx);
            zoomTriggerList.GetArrayElementAtIndex(zidx).stringValue = "Door";
            var zoomInteractList = soZoomEvt.FindProperty("interactableObjects"); 
            zoomInteractList.ClearArray();
            int zi = zoomInteractList.arraySize; 
            zoomInteractList.InsertArrayElementAtIndex(zi);
            zoomInteractList.GetArrayElementAtIndex(zi).stringValue = "Door";
            soZoomEvt.FindProperty("triggerType").enumValueIndex = (int)AFKS.Features.Stage.Events.EventTriggerType.Click;
            
            // Door 비활성화 효과 추가
            var disableDoor = ScriptableObject.CreateInstance<AFKS.Features.Stage.Effects.DisableInteractableEffect>();
            var soDisableDoor = new SerializedObject(disableDoor);
            soDisableDoor.FindProperty("objectId").stringValue = "Door";
            soDisableDoor.FindProperty("disableCollider").boolValue = true;
            soDisableDoor.FindProperty("disableClickHandler").boolValue = false;
            soDisableDoor.ApplyModifiedPropertiesWithoutUndo();
            
            // 체인 표시 효과 추가
            var showChains = ScriptableObject.CreateInstance<AFKS.Features.Stage.Effects.ShowChainsEffect>();
            var soShowChains = new SerializedObject(showChains);
            var chainIdsProp = soShowChains.FindProperty("chainObjectIds");
            chainIdsProp.ClearArray();
            int cid0 = chainIdsProp.arraySize; 
            chainIdsProp.InsertArrayElementAtIndex(cid0);
            chainIdsProp.GetArrayElementAtIndex(cid0).stringValue = "ChainTop";
            int cid1 = chainIdsProp.arraySize; 
            chainIdsProp.InsertArrayElementAtIndex(cid1);
            chainIdsProp.GetArrayElementAtIndex(cid1).stringValue = "ChainBottom";
            soShowChains.FindProperty("showChains").boolValue = true;
            soShowChains.FindProperty("enableClickable").boolValue = true;
            soShowChains.ApplyModifiedPropertiesWithoutUndo();
            
            var zoomEffectsProp = soZoomEvt.FindProperty("effects");
            int zef0 = zoomEffectsProp.arraySize; 
            zoomEffectsProp.InsertArrayElementAtIndex(zef0);
            zoomEffectsProp.GetArrayElementAtIndex(zef0).objectReferenceValue = disableDoor;
            int zef1 = zoomEffectsProp.arraySize; 
            zoomEffectsProp.InsertArrayElementAtIndex(zef1);
            zoomEffectsProp.GetArrayElementAtIndex(zef1).objectReferenceValue = showChains;

            // 줌 완료 후 자동으로 다음 이벤트로 진행
            var autoProceedAfterZoomProp = soZoomEvt.FindProperty("autoProceedAfterZoom");
            if (autoProceedAfterZoomProp != null) autoProceedAfterZoomProp.boolValue = true;
            soZoomEvt.ApplyModifiedPropertiesWithoutUndo();

            // 2) 체인 5회 클릭 달성 시 애니메이션
            var chainBreak = ScriptableObject.CreateInstance<AFKS.Features.Stage.Events.ClickEvent>();
            var cond = ScriptableObject.CreateInstance<AFKS.Features.Stage.Conditions.AggregateClickCountCondition>();
            var breakFx = ScriptableObject.CreateInstance<AFKS.Features.Stage.Effects.ChainShakeThenBreakEffect>();
            
            // 클릭 시 즉시 피드백 효과 추가
            var clickFeedback = ScriptableObject.CreateInstance<AFKS.Features.Stage.Effects.ObjectClickFeedbackEffect>();
            var soClickFeedback = new SerializedObject(clickFeedback);
            soClickFeedback.FindProperty("shakeDuration").floatValue = 0.2f;
            soClickFeedback.ApplyModifiedPropertiesWithoutUndo();

            // 3) 문 클릭 → 다음 스테이지로 이동
            var stageTransition = ScriptableObject.CreateInstance<AFKS.Features.Stage.Events.ClickEvent>();
            stageTransition.SetupStageTransitionEvent();

            // 이벤트들을 서브 에셋으로 추가
            AssetDatabase.AddObjectToAsset(zoomEvent, def);
            AssetDatabase.AddObjectToAsset(chainBreak, def);
            AssetDatabase.AddObjectToAsset(stageTransition, def);
            AssetDatabase.AddObjectToAsset(cond, def);
            AssetDatabase.AddObjectToAsset(breakFx, def);
            AssetDatabase.AddObjectToAsset(disableDoor, def);
            AssetDatabase.AddObjectToAsset(showChains, def);
            AssetDatabase.AddObjectToAsset(clickFeedback, def);

            // StageEvent 필드에 conditions/effects를 채워 넣는다
            var soChainBreak = new SerializedObject(chainBreak);
            soChainBreak.FindProperty("eventName").stringValue = "체인 분리";
            soChainBreak.FindProperty("description").stringValue = "체인이 흔들린 후 분리됩니다.";
            
            var cbTriggerList = soChainBreak.FindProperty("triggerObjectIds"); 
            cbTriggerList.ClearArray();
            int cb0 = cbTriggerList.arraySize; 
            cbTriggerList.InsertArrayElementAtIndex(cb0); 
            cbTriggerList.GetArrayElementAtIndex(cb0).stringValue = "ChainTop";
            int cb1 = cbTriggerList.arraySize; 
            cbTriggerList.InsertArrayElementAtIndex(cb1); 
            cbTriggerList.GetArrayElementAtIndex(cb1).stringValue = "ChainBottom";
            
            var cbInteractableList = soChainBreak.FindProperty("interactableObjects"); 
            cbInteractableList.ClearArray();
            int cbI0 = cbInteractableList.arraySize; 
            cbInteractableList.InsertArrayElementAtIndex(cbI0); 
            cbInteractableList.GetArrayElementAtIndex(cbI0).stringValue = "ChainTop";
            int cbI1 = cbInteractableList.arraySize; 
            cbInteractableList.InsertArrayElementAtIndex(cbI1); 
            cbInteractableList.GetArrayElementAtIndex(cbI1).stringValue = "ChainBottom";
            
            soChainBreak.FindProperty("triggerType").enumValueIndex = (int)AFKS.Features.Stage.Events.EventTriggerType.Click;
            var conditionsProp = soChainBreak.FindProperty("conditions");
            conditionsProp.ClearArray();
            int cidx = conditionsProp.arraySize; 
            conditionsProp.InsertArrayElementAtIndex(cidx);
            conditionsProp.GetArrayElementAtIndex(cidx).objectReferenceValue = cond;
            
            var onClickEffectsProp = soChainBreak.FindProperty("onClickEffects");
            onClickEffectsProp.ClearArray();
            int oce0 = onClickEffectsProp.arraySize; 
            onClickEffectsProp.InsertArrayElementAtIndex(oce0);
            onClickEffectsProp.GetArrayElementAtIndex(oce0).objectReferenceValue = clickFeedback;
            
            var effectsProp = soChainBreak.FindProperty("effects");
            effectsProp.ClearArray();
            int eidx = effectsProp.arraySize; 
            effectsProp.InsertArrayElementAtIndex(eidx);
            effectsProp.GetArrayElementAtIndex(eidx).objectReferenceValue = breakFx;
            
            soChainBreak.FindProperty("autoProceed").boolValue = false;
            soChainBreak.ApplyModifiedPropertiesWithoutUndo();

            // cond 설정: ChainTop/ChainBottom 합계 5회
            var soCond = new SerializedObject(cond);
            var idsProp = soCond.FindProperty("objectIds"); 
            idsProp.ClearArray();
            int id0 = idsProp.arraySize; 
            idsProp.InsertArrayElementAtIndex(id0); 
            idsProp.GetArrayElementAtIndex(id0).stringValue = "ChainTop";
            int id1 = idsProp.arraySize; 
            idsProp.InsertArrayElementAtIndex(id1); 
            idsProp.GetArrayElementAtIndex(id1).stringValue = "ChainBottom";
            soCond.FindProperty("requiredTotalCount").intValue = 5;
            soCond.ApplyModifiedPropertiesWithoutUndo();

            // StageTransition 이벤트에도 Effect 부착
            var soStageTrans = new SerializedObject(stageTransition);
            var stTrig = soStageTrans.FindProperty("triggerObjectIds"); 
            stTrig.ClearArray();
            int stT0 = stTrig.arraySize; 
            stTrig.InsertArrayElementAtIndex(stT0);
            stTrig.GetArrayElementAtIndex(stT0).stringValue = "Door";
            soStageTrans.FindProperty("triggerType").enumValueIndex = (int)AFKS.Features.Stage.Events.EventTriggerType.Click;
            
            var stCond = soStageTrans.FindProperty("conditions"); 
            stCond.ClearArray();
            int sc0 = stCond.arraySize; 
            stCond.InsertArrayElementAtIndex(sc0);
            stCond.GetArrayElementAtIndex(sc0).objectReferenceValue = cond;
            
            soStageTrans.ApplyModifiedPropertiesWithoutUndo();

            def.Set("Stage1", new[]{ zoomEvent as AFKS.Features.Stage.Events.StageEvent, chainBreak, stageTransition });
            EditorUtility.SetDirty(def);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            return def;
        }

        private void SetupChainClickRouting(GameObject chainTop, GameObject chainBottom, AFKS.Features.Stage.StageEventSystem ses)
        {
            var clickHandlerTop = chainTop.GetComponent<AFKS.Features.Interaction.ClickHandler>();
            var clickHandlerBottom = chainBottom.GetComponent<AFKS.Features.Interaction.ClickHandler>();
            
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
        }

        private void SetupDoorRouter(GameObject door, AFKS.Features.Stage.StageEventSystem ses)
        {
            var doorRouter = door.GetComponent<AFKS.Features.Interaction.ClickToEventRouter>();
            if (doorRouter != null)
            {
                var doorRouterSO = new SerializedObject(doorRouter);
                doorRouterSO.FindProperty("eventSystem").objectReferenceValue = ses;
                doorRouterSO.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private void SetupInteractionList(GameObject services, GameObject door, GameObject chainTop, GameObject chainBottom)
        {
            var sim = services.GetComponent<AFKS.Features.Stage.StageInteractionSystem>();
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
            
            var doorClickHandler = door.GetComponent<AFKS.Features.Interaction.ClickHandler>();
            var clickHandlerTop = chainTop.GetComponent<AFKS.Features.Interaction.ClickHandler>();
            var clickHandlerBottom = chainBottom.GetComponent<AFKS.Features.Interaction.ClickHandler>();
            
            // 초기 상태: Door만 상호작용 가능, 체인은 ZoomEvent 효과로 활성화됨
            AddInteractable("Door", door, doorClickHandler, true);
            AddInteractable("ChainTop", chainTop, clickHandlerTop, false);
            AddInteractable("ChainBottom", chainBottom, clickHandlerBottom, false);
            
            simSO.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>
        /// UnityEvent의 퍼시스턴트 리스너를 모두 제거합니다.
        /// </summary>
        private static void ClearPersistent(UnityEngine.Events.UnityEvent evt)
        {
            if (evt == null) return;
            for (int i = evt.GetPersistentEventCount() - 1; i >= 0; i--)
            {
                UnityEditor.Events.UnityEventTools.RemovePersistentListener(evt, i);
            }
        }
    }
}
