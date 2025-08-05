using UnityEngine;
using System;
using System.Collections.Generic;

namespace AFKS.Shared.Events
{
    /// <summary>
    /// 게임 이벤트 시스템
    /// 런타임 이벤트용 일반 클래스로 변경 (ScriptableObject 오류 방지)
    /// </summary>
    public class GameEvent
    {
        /// <summary>
        /// 이벤트 리스너들
        /// </summary>
        private readonly List<GameEventListener> eventListeners = new List<GameEventListener>();
        
        /// <summary>
        /// 런타임 액션 리스너들
        /// </summary>
        private event Action runtimeListeners;
        
        /// <summary>
        /// 이벤트 발생
        /// </summary>
        public void Raise()
        {
            // GameEventListener 호출
            for (int i = eventListeners.Count - 1; i >= 0; i--)
            {
                if (eventListeners[i] != null)
                    eventListeners[i].OnEventRaised();
                else
                    eventListeners.RemoveAt(i);
            }
            
            // 런타임 리스너 호출
            runtimeListeners?.Invoke();
        }
        
        /// <summary>
        /// 리스너 등록
        /// </summary>
        public void RegisterListener(GameEventListener listener)
        {
            if (!eventListeners.Contains(listener))
                eventListeners.Add(listener);
        }
        
        /// <summary>
        /// 리스너 해제
        /// </summary>
        public void UnregisterListener(GameEventListener listener)
        {
            if (eventListeners.Contains(listener))
                eventListeners.Remove(listener);
        }
        
        /// <summary>
        /// 런타임 리스너 등록
        /// </summary>
        public void AddListener(Action listener)
        {
            runtimeListeners += listener;
        }
        
        /// <summary>
        /// 런타임 리스너 해제
        /// </summary>
        public void RemoveListener(Action listener)
        {
            runtimeListeners -= listener;
        }
        
        /// <summary>
        /// 모든 런타임 리스너 해제
        /// </summary>
        public void RemoveAllListeners()
        {
            runtimeListeners = null;
        }
    }
    
    /// <summary>
    /// 매개변수가 있는 게임 이벤트
    /// </summary>
    /// <typeparam name="T">매개변수 타입</typeparam>
    [System.Serializable]
    public class GameEvent<T>
    {
        private event Action<T> listeners;
        
        public void Raise(T value)
        {
            listeners?.Invoke(value);
        }
        
        public void AddListener(Action<T> listener)
        {
            listeners += listener;
        }
        
        public void RemoveListener(Action<T> listener)
        {
            listeners -= listener;
        }
        
        public void RemoveAllListeners()
        {
            listeners = null;
        }
    }
}