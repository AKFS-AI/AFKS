using System;
using UnityEngine;
using UnityEngine.EventSystems;
using AFKS.Core.Services;

namespace AFKS.Core.Services.Input
{
    /// <summary>
    /// 클릭/포인터 상태를 추상화하고 전역 입력 잠금을 지원하는 최소 입력 서비스입니다.
    /// 게임 코드는 특정 입력 에셋이 아닌 IInputService 인터페이스에 의존해야 합니다.
    /// </summary>
    [AddComponentMenu("AFKS/입력/입력 서비스")]
    public sealed class InputService : MonoBehaviour, IInputService
    {
        [SerializeField]
        [InspectorName("UI 카메라")]
        [Tooltip("UI 레이캐스트에 사용할 카메라(필요 시). 비워도 동작합니다.")]
        private Camera uiCamera;

        public bool IsLocked { get; private set; }
        public Vector2 PointerPosition => UnityEngine.Input.mousePosition;

        public event Action Clicked;

        private void Awake()
        {
            ServiceLocator.Register<IInputService>(this, overwriteExisting: true);
        }

        private void Update()
        {
            if (IsLocked)
            {
                return;
            }

            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                Clicked?.Invoke();
            }
        }

        public void Lock(bool locked)
        {
            IsLocked = locked;
        }

        public bool IsPointerOverUI()
        {
            if (EventSystem.current == null)
            {
                return false;
            }

            var eventData = new PointerEventData(EventSystem.current)
            {
                position = PointerPosition
            };
            var results = ListPool<RaycastResult>.Get();
            EventSystem.current.RaycastAll(eventData, results);
            bool overUI = results.Count > 0;
            ListPool<RaycastResult>.Release(results);
            return overUI;
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<IInputService>();
        }
    }

    internal static class ListPool<T>
    {
        private static readonly System.Collections.Generic.Stack<System.Collections.Generic.List<T>> Pool = new System.Collections.Generic.Stack<System.Collections.Generic.List<T>>();

        public static System.Collections.Generic.List<T> Get()
        {
            return Pool.Count > 0 ? Pool.Pop() : new System.Collections.Generic.List<T>(8);
        }

        public static void Release(System.Collections.Generic.List<T> list)
        {
            list.Clear();
            Pool.Push(list);
        }
    }
}


