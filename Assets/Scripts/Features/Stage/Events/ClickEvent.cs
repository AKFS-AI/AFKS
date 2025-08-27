using UnityEngine;
using System.Collections.Generic;
using AFKS.Features.Stage.Animations;
using AFKS.Core.Services.Save;
using AFKS.Features.Stage.Effects;

namespace AFKS.Features.Stage.Events
{
    /// <summary>
    /// 클릭으로 트리거되는 기본 이벤트입니다.
    /// 특정 오브젝트 클릭 시 애니메이션과 상호작용을 처리합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "New Click Event", menuName = "AFKS/Stage/Events/Click Event")]
    public class ClickEvent : StageEvent
    {
        [Header("클릭 설정")]
        [SerializeField] private bool requireSpecificClick = false;
        [SerializeField] private int requiredClickCount = 1;
        [SerializeField] private float clickCooldown = 0.5f;
        
        [Header("클릭 후 동작")]
        [SerializeField] private bool hideTriggerObject = false;
        [SerializeField] private bool showNewObjects = false;
        [SerializeField] private List<string> objectsToShow = new List<string>();

        [Header("클릭 시 피드백")]
        [SerializeField] private List<StageEffect> onClickEffects = new List<StageEffect>();
        [SerializeField] private bool showClickFeedback = true;
        
        [Header("진행 저장(옵션)")]
        [SerializeField] private bool persistClickCount = false;
        [SerializeField] private string clickCountPersistKey = ""; // 예: "Stage1.Chain.Clicks"
        
        private int currentClickCount = 0;
        private float lastClickTime = 0f;
        
        #region StageEvent 구현
        
        public override void Execute(System.Action onComplete)
        {
            Debug.Log($"ClickEvent: {eventName} 실행 시작 (RequireSpecificClick={requireSpecificClick})");
            
            // 애니메이션 실행
            if (animations.Count > 0)
            {
                ExecuteWithDelay(delayBeforeExecute, () => {
                    PlayAnimations(onComplete);
                });
            }
            else
            {
                // 애니메이션이 없으면 즉시 완료
                ExecuteWithDelay(delayBeforeExecute, () => {
                    OnEventComplete(onComplete);
                });
            }
        }
        
        public override void Prepare()
        {
            Debug.Log($"ClickEvent: {eventName} 준비됨 - 트리거 오브젝트: {string.Join(", ", triggerObjectIds)}");
            Debug.Log($"ClickEvent: {eventName} 상호작용 가능한 오브젝트: {string.Join(", ", interactableObjects)}");
            
            // interactableObjects가 비어있을 때만 트리거 오브젝트들을 추가
            if (interactableObjects.Count == 0)
            {
                foreach (var triggerId in triggerObjectIds)
                {
                    interactableObjects.Add(triggerId);
                }
                Debug.Log($"ClickEvent: {eventName} interactableObjects가 비어있어서 triggerObjectIds를 추가했습니다.");
            }
            else
            {
                Debug.Log($"ClickEvent: {eventName} interactableObjects가 이미 설정되어 있어서 추가하지 않았습니다.");
            }

            // 클릭 수 복원
            if (persistClickCount && !string.IsNullOrEmpty(clickCountPersistKey))
            {
                if (AFKS.Core.Services.ServiceLocator.TryGet<ISaveService>(out var save))
                {
                    currentClickCount = Mathf.Max(0, save.GetInt(clickCountPersistKey, 0));
                }
                else
                {
                    currentClickCount = Mathf.Max(0, PlayerPrefs.GetInt(clickCountPersistKey, 0));
                }
            }
        }
        
        public override void Cleanup()
        {
            Debug.Log($"ClickEvent: {eventName} 정리됨");
            currentClickCount = 0;
        }
        
        #endregion
        
        #region 오버라이드 메서드
        
        /// <summary>
        /// 지정된 오브젝트 ID로 이벤트를 트리거할 수 있는지 확인합니다.
        /// ClickEvent는 interactableObjects도 확인합니다.
        /// </summary>
        /// <param name="objectId">확인할 오브젝트 ID</param>
        /// <returns>트리거 가능 여부</returns>
        public override bool CanTriggerWith(string objectId)
        {
            bool inTriggerIds = triggerObjectIds.Contains(objectId);
            bool inInteractableIds = interactableObjects.Contains(objectId);
            bool result = inTriggerIds || inInteractableIds;
            
            Debug.Log($"ClickEvent.CanTriggerWith: {objectId} - triggerIds: {inTriggerIds}, interactableIds: {inInteractableIds}, 결과: {result}");
            Debug.Log($"ClickEvent.CanTriggerWith: triggerObjectIds=[{string.Join(", ", triggerObjectIds)}], interactableObjects=[{string.Join(", ", interactableObjects)}]");
            
            return result;
        }
        
        #endregion
        
        #region 클릭 피드백
        
        /// <summary>
        /// 클릭 시 즉시 실행되는 피드백 효과를 실행합니다.
        /// </summary>
        /// <param name="objectId">클릭된 오브젝트 ID</param>
        /// <param name="system">스테이지 이벤트 시스템</param>
        public void ExecuteClickFeedback(string objectId, StageEventSystem system)
        {
            if (!showClickFeedback || onClickEffects == null || onClickEffects.Count == 0) return;
            
            Debug.Log($"ClickEvent: {objectId} 클릭 피드백 실행 - {onClickEffects.Count}개 효과");
            
            foreach (var effect in onClickEffects)
            {
                if (effect != null)
                {
                    // ObjectClickFeedbackEffect인 경우 체인 세트 피드백 또는 개별 피드백 적용
                    if (effect is ObjectClickFeedbackEffect objectFeedback)
                    {
                        // 체인 클릭 시 체인 세트 피드백, 다른 오브젝트 클릭 시 개별 피드백
                        if (objectId == "ChainTop" || objectId == "ChainBottom")
                        {
                            objectFeedback.ApplyFeedbackToChainSet(objectId, system);
                        }
                        else
                        {
                            objectFeedback.ApplyFeedbackToObject(objectId, system);
                        }
                    }
                    else
                    {
                        // 다른 효과는 기존대로 실행
                        effect.Apply(system);
                    }
                }
            }
        }
        
        #endregion
        
        #region 클릭 처리
        
        /// <summary>
        /// 클릭을 처리합니다.
        /// </summary>
        /// <param name="objectId">클릭된 오브젝트 ID</param>
        /// <returns>이벤트가 트리거되었는지 여부</returns>
        public bool HandleClick(string objectId)
        {
            if (!triggerObjectIds.Contains(objectId)) return false;
            
            // 쿨다운 체크
            if (Time.time - lastClickTime < clickCooldown) return false;
            
            lastClickTime = Time.time;
            currentClickCount++;
            
            // requireSpecificClick은 향후 특정 오브젝트/순서 강제에 사용될 예정이며 현재는 로깅으로 참조만 수행합니다.
            Debug.Log($"ClickEvent: {objectId} 클릭됨 - {currentClickCount}/{requiredClickCount} (RequireSpecificClick={requireSpecificClick})");

            // 클릭 수 저장
            if (persistClickCount && !string.IsNullOrEmpty(clickCountPersistKey))
            {
                if (AFKS.Core.Services.ServiceLocator.TryGet<ISaveService>(out var save))
                {
                    save.SetInt(clickCountPersistKey, currentClickCount);
                }
                else
                {
                    PlayerPrefs.SetInt(clickCountPersistKey, currentClickCount);
                    PlayerPrefs.Save();
                }
            }
            
            // 필요한 클릭 횟수 달성 시 트리거 신호만 반환 (실제 실행은 StageEventSystem이 수행)
            if (currentClickCount >= requiredClickCount)
            {
                return true;
            }
            
            return false;
        }
        
        #endregion
        
        #region 애니메이션 실행
        
        private void PlayAnimations(System.Action onComplete)
        {
            if (animations.Count == 0)
            {
                OnEventComplete(onComplete);
                return;
            }
            
            // 애니메이션 컨트롤러 찾기
            var animationController = FindFirstObjectByType<StageAnimationController>();
            if (animationController != null)
            {
                animationController.PlayAnimationSequence(animations, onComplete);
            }
            else
            {
                Debug.LogWarning("ClickEvent: StageAnimationController를 찾을 수 없습니다.");
                OnEventComplete(onComplete);
            }
        }
        
        #endregion
        
        #region 이벤트 완료 처리
        
        private void OnEventComplete(System.Action onComplete)
        {
            // 트리거 오브젝트 숨기기
            if (hideTriggerObject)
            {
                var sim = AFKS.Features.Stage.StageInteractionManager.Instance;
                foreach (var triggerId in triggerObjectIds)
                {
                    var obj = sim != null ? sim.GetObject(triggerId) : null;
                    if (obj != null)
                    {
                        obj.SetActive(false);
                        Debug.Log($"ClickEvent: {triggerId} 숨김");
                    }
                }
            }
            
            // 새로운 오브젝트 표시
            if (showNewObjects)
            {
                var sim = AFKS.Features.Stage.StageInteractionManager.Instance;
                foreach (var objectId in objectsToShow)
                {
                    var obj = sim != null ? sim.GetObject(objectId) : null;
                    if (obj != null)
                    {
                        obj.SetActive(true);
                        Debug.Log($"ClickEvent: {objectId} 표시");
                    }
                }
            }
            
            onComplete?.Invoke();
        }
        
        #endregion
        
        #region 유틸리티
        
        /// <summary>
        /// 현재 클릭 횟수를 반환합니다.
        /// </summary>
        public int GetCurrentClickCount() => currentClickCount;
        
        /// <summary>
        /// 필요한 클릭 횟수를 반환합니다.
        /// </summary>
        public int GetRequiredClickCount() => requiredClickCount;
        
        /// <summary>
        /// 클릭 진행률을 반환합니다 (0.0 ~ 1.0).
        /// </summary>
        public float GetClickProgress() => (float)currentClickCount / requiredClickCount;
        
        #endregion
        
        #region Stage1 전용 설정 메서드
        
        /// <summary>
        /// 문 클릭 이벤트를 설정합니다.
        /// </summary>
        public void SetupDoorClickEvent()
        {
            eventName = "문 클릭 - 카메라 줌";
            description = "문을 클릭하면 체인 영역으로 카메라가 줌됩니다.";
            triggerObjectIds.Clear();
            triggerObjectIds.Add("Door");
            interactableObjects.Clear();
            interactableObjects.Add("Door");
            autoProceed = true;
            delayBeforeExecute = 0f;
        }
        
        /// <summary>
        /// 체인 클릭 이벤트를 설정합니다.
        /// </summary>
        public void SetupChainClickEvent()
        {
            eventName = "체인 클릭 - 흔들림";
            description = "체인을 5번 클릭하면 흔들리는 애니메이션이 재생됩니다.";
            triggerObjectIds.Clear();
            triggerObjectIds.Add("ChainTop");
            triggerObjectIds.Add("ChainBottom");
            interactableObjects.Clear();
            interactableObjects.Add("ChainTop");
            interactableObjects.Add("ChainBottom");
            requiredClickCount = 5;
            clickCooldown = 0.3f;
            autoProceed = false;
            delayBeforeExecute = 0.1f;
            persistClickCount = true;
            clickCountPersistKey = "Stage1.Chain.Clicks";
        }
        
        /// <summary>
        /// 체인 해금 이벤트를 설정합니다.
        /// </summary>
        public void SetupChainUnlockEvent()
        {
            eventName = "체인 해금 - 끊어짐";
            description = "체인이 끊어지는 애니메이션이 재생됩니다.";
            triggerObjectIds.Clear();
            triggerObjectIds.Add("ChainTop");
            triggerObjectIds.Add("ChainBottom");
            interactableObjects.Clear();
            autoProceed = true;
            delayBeforeExecute = 0.5f;
            hideTriggerObject = true;
        }
        
        /// <summary>
        /// 스테이지 전환 이벤트를 설정합니다.
        /// </summary>
        public void SetupStageTransitionEvent()
        {
            eventName = "스테이지 전환";
            description = "문을 클릭하면 다음 스테이지로 전환됩니다.";
            triggerObjectIds.Clear();
            triggerObjectIds.Add("Door");
            interactableObjects.Clear();
            interactableObjects.Add("Door");
            autoProceed = true;
            delayBeforeExecute = 0.2f;
        }
        
        #endregion
    }
}
