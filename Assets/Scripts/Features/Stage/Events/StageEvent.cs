using UnityEngine;
using System.Collections.Generic;
using AFKS.Features.Stage.Animations;
using AFKS.Features.Stage.Conditions;
using AFKS.Features.Stage.Effects;

namespace AFKS.Features.Stage.Events
{
    /// <summary>
    /// 스테이지의 개별 이벤트를 정의하는 기본 클래스입니다.
    /// 각 이벤트는 특정 조건과 애니메이션을 가집니다.
    /// </summary>
    [System.Serializable]
    public abstract class StageEvent : ScriptableObject
    {
        [Header("이벤트 기본 정보")]
        [SerializeField] protected string eventName = "New Event";
        [SerializeField] protected string description = "이벤트 설명";
        [SerializeField] protected bool autoProceed = false;
        [SerializeField] protected float delayBeforeExecute = 0f;
        
        [Header("트리거 조건")]
        [SerializeField] protected List<string> triggerObjectIds = new List<string>();
        [SerializeField] protected EventTriggerType triggerType = EventTriggerType.Click;
        
        [Header("애니메이션")]
        [SerializeField] protected List<StageAnimation> animations = new List<StageAnimation>();
        
        [Header("상호작용")]
        [SerializeField] protected List<string> interactableObjects = new List<string>();
        [SerializeField] protected List<string> hiddenObjects = new List<string>();

        [Header("조건/효과")]
        [SerializeField] protected List<StageCondition> conditions = new List<StageCondition>();
        [SerializeField] protected List<StageEffect> effects = new List<StageEffect>();
        
        #region 공개 프로퍼티
        
        /// <summary>
        /// 이벤트 이름을 반환합니다.
        /// </summary>
        public string EventName => eventName;
        
        /// <summary>
        /// 이벤트 설명을 반환합니다.
        /// </summary>
        public string Description => description;
        
        /// <summary>
        /// 자동 진행 여부를 반환합니다.
        /// </summary>
        public bool AutoProceed => autoProceed;
        
        /// <summary>
        /// 자동 진행 여부를 설정합니다.
        /// </summary>
        /// <param name="value">설정할 값</param>
        public virtual void SetAutoProceed(bool value)
        {
            autoProceed = value;
        }

        /// <summary>
        /// 트리거 타입을 반환합니다.
        /// </summary>
        public EventTriggerType TriggerType => triggerType;

        /// <summary>
        /// 클릭 트리거 실행에 필요한 트리거 오브젝트가 존재하는지 여부를 반환합니다.
        /// </summary>
        public bool HasTriggerObjects => triggerObjectIds != null && triggerObjectIds.Count > 0;
        
        #endregion
        
        #region 추상 메서드
        
        /// <summary>
        /// 이벤트를 실행합니다.
        /// </summary>
        /// <param name="onComplete">이벤트 완료 시 호출될 콜백</param>
        public abstract void Execute(System.Action onComplete);
        
        /// <summary>
        /// 이벤트를 준비합니다.
        /// </summary>
        public abstract void Prepare();
        
        /// <summary>
        /// 이벤트를 정리합니다.
        /// </summary>
        public abstract void Cleanup();
        
        #endregion
        
        #region 공통 메서드
        
        /// <summary>
        /// 지정된 오브젝트 ID로 이벤트를 트리거할 수 있는지 확인합니다.
        /// </summary>
        /// <param name="objectId">확인할 오브젝트 ID</param>
        /// <returns>트리거 가능 여부</returns>
        public virtual bool CanTriggerWith(string objectId)
        {
            return triggerObjectIds.Contains(objectId);
        }
        
        /// <summary>
        /// 상호작용 가능한 오브젝트 목록을 반환합니다.
        /// </summary>
        /// <returns>상호작용 가능한 오브젝트 ID 목록</returns>
        public virtual List<string> GetInteractableObjects()
        {
            return new List<string>(interactableObjects);
        }
        
        /// <summary>
        /// 숨겨질 오브젝트 목록을 반환합니다.
        /// </summary>
        /// <returns>숨겨질 오브젝트 ID 목록</returns>
        public virtual List<string> GetHiddenObjects()
        {
            return new List<string>(hiddenObjects);
        }
        
        /// <summary>
        /// 애니메이션 목록을 반환합니다.
        /// </summary>
        /// <returns>애니메이션 목록</returns>
        public virtual List<StageAnimation> GetAnimations()
        {
            return new List<StageAnimation>(animations);
        }

        /// <summary>
        /// 이 이벤트가 실행 가능하려면 모든 조건을 만족해야 합니다.
        /// 조건이 비어있으면 항상 참으로 간주합니다.
        /// </summary>
        public virtual bool AreConditionsSatisfied(AFKS.Features.Stage.StageEventSystem system)
        {
            if (conditions == null || conditions.Count == 0) return true;
            for (int i = 0; i < conditions.Count; i++)
            {
                var c = conditions[i];
                if (c == null) continue;
                if (!c.Evaluate(system)) return false;
            }
            return true;
        }

        /// <summary>
        /// 이벤트 완료 시 정의된 효과를 적용합니다.
        /// </summary>
        public virtual void ApplyEffects(AFKS.Features.Stage.StageEventSystem system)
        {
            if (effects == null) return;
            for (int i = 0; i < effects.Count; i++)
            {
                var e = effects[i];
                if (e == null) continue;
                e.Apply(system);
            }
        }
        
        #endregion
        
        #region 유틸리티 메서드
        
        /// <summary>
        /// 지연 후 콜백을 실행합니다.
        /// </summary>
        /// <param name="delay">지연 시간 (초)</param>
        /// <param name="callback">실행할 콜백</param>
        protected void ExecuteWithDelay(float delay, System.Action callback)
        {
            if (delay <= 0f)
            {
                callback?.Invoke();
            }
            else
            {
                // 공용 코루틴 러너 사용
                AFKS.Core.Utils.CoroutineRunner.InvokeAfterSeconds(delay, callback);
            }
        }
        
        private System.Collections.IEnumerator DelayedExecute(float delay, System.Action callback)
        {
            yield return new WaitForSeconds(delay);
            callback?.Invoke();
        }
        
        #endregion
    }
    
    /// <summary>
    /// 이벤트 트리거 타입을 정의합니다.
    /// </summary>
    public enum EventTriggerType
    {
        Click,      // 클릭
        Hover,      // 호버
        Proximity,  // 근접
        Timer,      // 타이머
        Condition   // 조건
    }
}
