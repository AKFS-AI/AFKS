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
    [AddComponentMenu("AFKS/Input/Input Service")]
    public sealed class InputService : MonoBehaviour, IInputService
    {
        #region 필드
        [SerializeField]
        [InspectorName("UI 카메라")]
        [Tooltip("UI 레이캐스트에 사용할 카메라(필요 시). 비워도 동작합니다.")]
        private Camera uiCamera;

        [SerializeField]
        [InspectorName("UI 위 클릭 무시")]
        [Tooltip("포인터가 UI 위에 있을 때 클릭 이벤트를 무시합니다.")]
        private bool ignoreClicksWhenPointerOverUI = true;

        public bool IsLocked { get; private set; }
        public Vector2 PointerPosition => UnityEngine.Input.mousePosition;

        public event Action Clicked;
        #endregion

        #region 유니티 수명주기
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

            // UI 위면 클릭 무시(옵션)
            if (ignoreClicksWhenPointerOverUI && IsPointerOverUI())
            {
                return;
            }

            // 마우스 좌클릭 또는 터치 시작을 클릭으로 처리
            bool mouseClicked = UnityEngine.Input.GetMouseButtonDown(0);
            bool touchClicked = UnityEngine.Input.touchCount > 0 && UnityEngine.Input.GetTouch(0).phase == TouchPhase.Began;
            if (mouseClicked || touchClicked)
            {
                Clicked?.Invoke();
            }
        }
        #endregion

        #region 공개 API
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

            var eventData = new PointerEventData(EventSystem.current);
            eventData.position = PointerPosition;
            var results = ListPool<RaycastResult>.Get();
            EventSystem.current.RaycastAll(eventData, results);
            bool overUI = results.Count > 0;
            ListPool<RaycastResult>.Release(results);
            return overUI;
        }
        #endregion

        #region 유니티 수명주기(종료)
        private void OnDestroy()
        {
            ServiceLocator.Unregister<IInputService>();
        }
        #endregion
    }

    #region 내부 유틸리티
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
    #endregion
}


