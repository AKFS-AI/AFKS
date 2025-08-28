using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace AFKS.Features.Interaction
{
    /// <summary>
    /// 월드 스프라이트의 클릭 이벤트를 처리합니다.
    /// BoxCollider2D와 함께 사용하여 클릭 감지 및 이벤트 발생을 담당합니다.
    /// </summary>
    [AddComponentMenu("AFKS/Interaction/Click Handler")]
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class ClickHandler : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Header("클릭 이벤트")]
        [SerializeField]
        [InspectorName("클릭 이벤트")] public UnityEvent onClick = new UnityEvent();
        
        [SerializeField]
        [InspectorName("마우스 다운 이벤트")] private UnityEvent onPointerDown = new UnityEvent();
        
        [SerializeField]
        [InspectorName("마우스 업 이벤트")] private UnityEvent onPointerUp = new UnityEvent();
        
        [Header("설정")]
        [SerializeField]
        [InspectorName("클릭 가능")] private bool clickable = true;
        
        [SerializeField]
        [InspectorName("디버그 로그")] private bool debugLog = false;

        #region Unity 이벤트

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!clickable) return;
            
            // 애니메이션 중 클릭 방지
            if (IsAnimationPlaying())
            {
                if (debugLog)
                    Debug.Log($"ClickHandler: {gameObject.name} 애니메이션 중 클릭 무시됨");
                return;
            }
            
            if (debugLog)
                Debug.Log($"ClickHandler: {gameObject.name} 클릭됨");
            
            onClick?.Invoke();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!clickable) return;
            
            if (debugLog)
                Debug.Log($"ClickHandler: {gameObject.name} 마우스 다운");
            
            onPointerDown?.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!clickable) return;
            
            if (debugLog)
                Debug.Log($"ClickHandler: {gameObject.name} 마우스 업");
            
            onPointerUp?.Invoke();
        }

        #endregion

        #region 공개 API

        /// <summary>
        /// 클릭 가능 여부를 설정합니다.
        /// </summary>
        public void SetClickable(bool enabled)
        {
            clickable = enabled;
        }

        /// <summary>
        /// 현재 클릭 가능 여부를 반환합니다.
        /// </summary>
        public bool IsClickable => clickable;
        
        /// <summary>
        /// 현재 애니메이션이 재생 중인지 확인합니다.
        /// </summary>
        private bool IsAnimationPlaying()
        {
            // 체인 흔들림 애니메이션 확인
            var shakeEffect = GetComponent<AFKS.Features.Stage.Effects.ChainShakeEffect>();
            if (shakeEffect != null && shakeEffect.IsShaking)
            {
                return true;
            }
            
            // 체인 분리 애니메이션 확인
            var breakEffect = GetComponent<AFKS.Features.Stage.Effects.ChainBreakEffect>();
            if (breakEffect != null && breakEffect.IsBreaking)
            {
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// 클릭 이벤트에 리스너를 추가합니다.
        /// </summary>
        public void AddClickListener(UnityAction action)
        {
            onClick.AddListener(action);
        }

        /// <summary>
        /// 클릭 이벤트에서 리스너를 제거합니다.
        /// </summary>
        public void RemoveClickListener(UnityAction action)
        {
            onClick.RemoveListener(action);
        }

        #endregion

        #region 인스펙터 검증

        private void OnValidate()
        {
            // BoxCollider2D가 있는지 확인
            var collider = GetComponent<BoxCollider2D>();
            if (collider == null)
            {
                Debug.LogError($"ClickHandler: {gameObject.name}에 BoxCollider2D가 필요합니다.");
            }
        }

        #endregion

        #region 디버그

        private void OnDrawGizmosSelected()
        {
            var collider = GetComponent<BoxCollider2D>();
            if (collider != null)
            {
                Gizmos.color = clickable ? Color.green : Color.red;
                Gizmos.DrawWireCube(transform.position, collider.bounds.size);
            }
        }

        #endregion
    }
}
