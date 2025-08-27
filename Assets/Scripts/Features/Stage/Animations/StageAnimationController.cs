using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AFKS.Features.Stage.Animations;

namespace AFKS.Features.Stage.Animations
{
    /// <summary>
    /// 스테이지의 모든 애니메이션을 통합 관리하는 컨트롤러입니다.
    /// 줌, 체인 애니메이션, 귀신 등장 등 다양한 애니메이션을 처리합니다.
    /// </summary>
    [AddComponentMenu("AFKS/Stage/Animations/Stage Animation Controller")]
    public class StageAnimationController : MonoBehaviour
    {
        [Header("애니메이션 타입별 컨트롤러")]
        [SerializeField] private GhostAnimationController ghostAnimationController;
        [SerializeField] private ObjectAnimationController objectAnimationController;
        
        [Header("애니메이션 상태")]
        [SerializeField] private bool isAnimationPlaying = false;
        [SerializeField] private List<StageAnimation> queuedAnimations = new List<StageAnimation>();
        
        [Header("설정")]
        [SerializeField] private bool playAnimationsSequentially = true;
        [SerializeField] private float animationDelay = 0.1f;
        
        #region Unity 수명주기
        
        private void Awake()
        {
            InitializeControllers();
        }
        
        #endregion
        
        #region 초기화
        
        private void InitializeControllers()
        {
            // 체인 애니메이션은 ChainShakeThenBreakEffect에서 처리합니다.
            
            // 귀신 애니메이션 컨트롤러 자동 생성
            if (ghostAnimationController == null)
            {
                ghostAnimationController = gameObject.AddComponent<GhostAnimationController>();
            }
            
            // 오브젝트 애니메이션 컨트롤러 자동 생성
            if (objectAnimationController == null)
            {
                objectAnimationController = gameObject.AddComponent<ObjectAnimationController>();
            }
        }
        
        #endregion
        
        #region 공개 API
        
        // 체인 애니메이션은 ChainShakeThenBreakEffect에서 처리합니다.
        
        /// <summary>
        /// 귀신 등장 애니메이션을 실행합니다.
        /// </summary>
        /// <param name="ghostId">귀신 ID</param>
        /// <param name="onComplete">완료 시 콜백</param>
        public void PlayGhostAnimation(string ghostId, System.Action onComplete = null)
        {
            if (ghostAnimationController != null)
            {
                ghostAnimationController.PlayGhostAppearance(ghostId, onComplete);
            }
        }
        
        /// <summary>
        /// 오브젝트 애니메이션을 실행합니다.
        /// </summary>
        /// <param name="objectId">오브젝트 ID</param>
        /// <param name="animationType">애니메이션 타입</param>
        /// <param name="onComplete">완료 시 콜백</param>
        public void PlayObjectAnimation(string objectId, ObjectAnimationType animationType, System.Action onComplete = null)
        {
            if (objectAnimationController != null)
            {
                objectAnimationController.PlayAnimation(objectId, animationType, onComplete);
            }
        }
        
        /// <summary>
        /// 애니메이션 시퀀스를 실행합니다.
        /// </summary>
        /// <param name="animations">실행할 애니메이션 목록</param>
        /// <param name="onComplete">완료 시 콜백</param>
        public void PlayAnimationSequence(List<StageAnimation> animations, System.Action onComplete = null)
        {
            if (animations == null || animations.Count == 0)
            {
                onComplete?.Invoke();
                return;
            }
            
            queuedAnimations.Clear();
            queuedAnimations.AddRange(animations);
            
            if (playAnimationsSequentially)
            {
                StartCoroutine(PlaySequentialAnimations(onComplete));
            }
            else
            {
                StartCoroutine(PlayParallelAnimations(onComplete));
            }
        }
        
        /// <summary>
        /// 현재 애니메이션이 진행 중인지 확인합니다.
        /// </summary>
        public bool IsAnimationPlaying => isAnimationPlaying;
        
        #endregion
        
        #region 애니메이션 시퀀스
        
        private IEnumerator PlaySequentialAnimations(System.Action onComplete)
        {
            isAnimationPlaying = true;
            
            foreach (var animation in queuedAnimations)
            {
                if (animation != null)
                {
                    yield return StartCoroutine(ExecuteAnimation(animation));
                    
                    if (animationDelay > 0f)
                    {
                        yield return new WaitForSeconds(animationDelay);
                    }
                }
            }
            
            isAnimationPlaying = false;
            onComplete?.Invoke();
        }
        
        private IEnumerator PlayParallelAnimations(System.Action onComplete)
        {
            isAnimationPlaying = true;
            
            var runningAnimations = new List<Coroutine>();
            
            foreach (var animation in queuedAnimations)
            {
                if (animation != null)
                {
                    var coroutine = StartCoroutine(ExecuteAnimation(animation));
                    runningAnimations.Add(coroutine);
                }
            }
            
            // 모든 애니메이션 완료 대기
            foreach (var coroutine in runningAnimations)
            {
                yield return coroutine;
            }
            
            isAnimationPlaying = false;
            onComplete?.Invoke();
        }
        
        private IEnumerator ExecuteAnimation(StageAnimation animation)
        {
            bool completed = false;
            
            animation.Execute(() => {
                completed = true;
            });
            
            // 애니메이션 완료 대기
            while (!completed)
            {
                yield return null;
            }
        }
        
        #endregion
        
        #region 유틸리티
        
        /// <summary>
        /// 모든 애니메이션을 중지합니다.
        /// </summary>
        public void StopAllAnimations()
        {
            StopAllCoroutines();
            isAnimationPlaying = false;
            queuedAnimations.Clear();
        }
        
        /// <summary>
        /// 특정 타입의 애니메이션을 중지합니다.
        /// </summary>
        /// <param name="animationType">중지할 애니메이션 타입</param>
        public void StopAnimation(AnimationType animationType)
        {
            switch (animationType)
            {
                case AnimationType.Ghost:
                    if (ghostAnimationController != null) ghostAnimationController.StopAnimation();
                    break;
                case AnimationType.Object:
                    if (objectAnimationController != null) objectAnimationController.StopAnimation();
                    break;
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// 오브젝트 애니메이션 타입을 정의합니다.
    /// </summary>
    public enum ObjectAnimationType
    {
        FadeIn,     // 페이드 인
        FadeOut,    // 페이드 아웃
        Scale,      // 크기 변화
        Move,       // 이동
        Rotate      // 회전
    }
    
    /// <summary>
    /// 애니메이션 타입을 정의합니다.
    /// </summary>
    public enum AnimationType
    {
        Ghost,  // 귀신
        Object  // 오브젝트
    }
}
