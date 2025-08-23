using System;
using UnityEngine;
using UnityEngine.EventSystems;
using AFKS.Core.Services;
using UnityEngine.UI;

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
        public event Action<GameObject> ObjectClicked;
        #endregion

        #region 유니티 수명주기
        private void Awake()
        {
            ServiceLocator.Register<IInputService>(this, overwriteExisting: true);
        }

        private void Update()
        {
            if (IsLocked) return;

            // 클릭 에지 검출 후 UI 위 체크로 할당/비용 최소화
            bool mouseClicked = UnityEngine.Input.GetMouseButtonDown(0);
            bool touchClicked = UnityEngine.Input.touchCount > 0 && UnityEngine.Input.GetTouch(0).phase == TouchPhase.Began;
            if (!(mouseClicked || touchClicked)) return;

            if (ignoreClicksWhenPointerOverUI && IsPointerOverUI()) return;

            Clicked?.Invoke();

            if (TryRaycast(out var go))
            {
                ObjectClicked?.Invoke(go);
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

        public bool TryRaycast(out GameObject clickedObject)
        {
            // 2D 물리 우선(사진형 2D 씬 기준), 실패 시 3D 물리 폴백
            var cam = uiCamera != null ? uiCamera : Camera.main;
            clickedObject = null;
            if (cam == null) return false;

            var worldPoint = cam.ScreenToWorldPoint(new Vector3(PointerPosition.x, PointerPosition.y, Mathf.Abs(cam.transform.position.z)));
            var hit2D = Physics2D.OverlapPoint(worldPoint);
            if (hit2D != null)
            {
                clickedObject = hit2D.gameObject;
                return true;
            }

            var ray = cam.ScreenPointToRay(PointerPosition);
            if (Physics.Raycast(ray, out var hit3D))
            {
                clickedObject = hit3D.collider.gameObject;
                return true;
            }

            return false;
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


