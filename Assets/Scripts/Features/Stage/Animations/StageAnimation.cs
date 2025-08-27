using UnityEngine;

namespace AFKS.Features.Stage.Animations
{
    /// <summary>
    /// 모든 스테이지 애니메이션의 기본이 되는 추상 클래스입니다.
    /// 각 애니메이션은 이 클래스를 상속받아 구현됩니다.
    /// </summary>
    public abstract class StageAnimation : ScriptableObject
    {
        [Header("애니메이션 기본 정보")]
        [SerializeField] protected string animationName = "New Animation";
        [SerializeField] protected string description = "애니메이션 설명";
        [SerializeField] protected float duration = 1.0f;
        [SerializeField] protected bool isLooping = false;
        
        [Header("트리거 조건")]
        [SerializeField] protected AnimationTriggerType triggerType = AnimationTriggerType.Manual;
        [SerializeField] protected string triggerObjectId = "";
        
        [Header("설정")]
        [SerializeField] protected bool autoStart = false;
        [SerializeField] protected float delayBeforeStart = 0f;
        
        #region 공개 프로퍼티
        
        /// <summary>
        /// 애니메이션 이름을 반환합니다.
        /// </summary>
        public string AnimationName => animationName;
        
        /// <summary>
        /// 애니메이션 설명을 반환합니다.
        /// </summary>
        public string Description => description;
        
        /// <summary>
        /// 애니메이션 지속 시간을 반환합니다.
        /// </summary>
        public float Duration => duration;
        
        /// <summary>
        /// 루프 여부를 반환합니다.
        /// </summary>
        public bool IsLooping => isLooping;
        
        #endregion
        
        #region 추상 메서드
        
        /// <summary>
        /// 애니메이션을 실행합니다.
        /// </summary>
        /// <param name="onComplete">애니메이션 완료 시 호출될 콜백</param>
        public abstract void Execute(System.Action onComplete);
        
        /// <summary>
        /// 애니메이션을 중지합니다.
        /// </summary>
        public abstract void Stop();
        
        /// <summary>
        /// 애니메이션을 일시정지합니다.
        /// </summary>
        public abstract void Pause();
        
        /// <summary>
        /// 애니메이션을 재개합니다.
        /// </summary>
        public abstract void Resume();
        
        #endregion
        
        #region 공통 메서드
        
        /// <summary>
        /// 애니메이션을 준비합니다.
        /// </summary>
        public virtual void Prepare()
        {
            Debug.Log($"StageAnimation: {animationName} 준비됨");
        }
        
        /// <summary>
        /// 애니메이션을 정리합니다.
        /// </summary>
        public virtual void Cleanup()
        {
            Debug.Log($"StageAnimation: {animationName} 정리됨");
        }
        
        /// <summary>
        /// 애니메이션이 실행 중인지 확인합니다.
        /// </summary>
        /// <returns>실행 중 여부</returns>
        public virtual bool IsPlaying()
        {
            return false; // 하위 클래스에서 구현
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
                // 지연 실행을 위한 코루틴 시작
                var coroutineRunner = FindFirstObjectByType<MonoBehaviour>();
                if (coroutineRunner != null)
                {
                    coroutineRunner.StartCoroutine(DelayedExecute(delay, callback));
                }
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
    /// 애니메이션 트리거 타입을 정의합니다.
    /// </summary>
    public enum AnimationTriggerType
    {
        Manual,     // 수동 실행
        Auto,       // 자동 실행
        OnClick,    // 클릭 시
        OnHover,    // 호버 시
        OnProximity // 근접 시
    }
}
