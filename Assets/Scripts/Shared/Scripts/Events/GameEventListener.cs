using UnityEngine;
using UnityEngine.Events;

namespace AFKS.Shared.Events
{
    /// <summary>
    /// 게임 이벤트 리스너
    /// </summary>
    public class GameEventListener : MonoBehaviour
    {
        [Header("Event Settings")]
        [SerializeField] private GameEvent gameEvent;
        
        [Header("Response")]
        [SerializeField] private UnityEvent response;
        
        private void OnEnable()
        {
            if (gameEvent != null)
                gameEvent.RegisterListener(this);
        }
        
        private void OnDisable()
        {
            if (gameEvent != null)
                gameEvent.UnregisterListener(this);
        }
        
        /// <summary>
        /// 이벤트가 발생했을 때 호출
        /// </summary>
        public void OnEventRaised()
        {
            response?.Invoke();
        }
    }
}