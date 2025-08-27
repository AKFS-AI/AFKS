using System;
using UnityEngine;
using UnityEngine.EventSystems;
using AFKS.Core.Services;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using AFKS.Core.Events;

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

        [SerializeField]
        [InspectorName("클릭 가능한 레이어")]
        [Tooltip("2D/3D 레이캐스트 시 대상으로 삼을 레이어 마스크입니다.")]
        private LayerMask clickableLayers = ~0; // Everything

        [SerializeField]
        [InspectorName("디버그 로그")] 
        [Tooltip("클릭/레이캐스트 경로를 콘솔에 로그로 출력합니다.")]
        private bool debugLogging = false;

        public bool IsLocked { get; private set; }
        public Vector2 PointerPosition => UnityEngine.Input.mousePosition;

        public event Action Clicked;
        public event Action<GameObject> ObjectClicked;
        #endregion

        #region 유니티 수명주기
        private void Awake()
        {
            ServiceLocator.Register<IInputService>(this, overwriteExisting: true);
            // 초기 카메라 자동 할당(없으면 Camera.main 또는 임의 활성 카메라)
            if (uiCamera == null)
            {
                uiCamera = Camera.main != null ? Camera.main : FindAnyActiveCamera();
            }
        }

        private void OnEnable()
        {
            GameEvents.StageLoaded += OnStageLoaded;
            GameEvents.StageUnloaded += OnStageUnloaded;
        }

        private void OnDisable()
        {
            GameEvents.StageLoaded -= OnStageLoaded;
            GameEvents.StageUnloaded -= OnStageUnloaded;
        }

        // Update 폴링 제거: Input System 또는 StandaloneInputModule 이벤트에서 호출할 수 있는 공개 메서드 제공
        public void NotifyPointerPrimaryDown()
        {
            if (IsLocked) return;
            if (ignoreClicksWhenPointerOverUI && IsPointerOverUI())
            {
                if (debugLogging) Debug.Log("[InputService] Click blocked by UI raycast.");
                return;
            }
            Clicked?.Invoke();
            if (TryRaycast(out var go))
            {
                ObjectClicked?.Invoke(go);
                if (debugLogging) Debug.Log($"[InputService] ObjectClicked: {go.name}");
            }
            else if (debugLogging)
            {
                Debug.Log("[InputService] Raycast miss.");
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
            if (debugLogging && overUI)
            {
                // 상위 1~3개 UI 히트 대상을 경로와 함께 출력
                int count = Mathf.Min(3, results.Count);
                System.Text.StringBuilder sb = new System.Text.StringBuilder(128);
                sb.Append("[InputService] UI Raycast hits(").Append(results.Count).Append(") top:");
                for (int i = 0; i < count; i++)
                {
                    var go = results[i].gameObject;
                    if (go == null) continue;
                    sb.Append("\n  ").Append(i + 1).Append(") ").Append(GetTransformPath(go.transform));
                }
                Debug.Log(sb.ToString());
            }
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
            var hits2D = Physics2D.OverlapPointAll(worldPoint);
            if (hits2D != null && hits2D.Length > 0)
            {
                for (int i = 0; i < hits2D.Length; i++)
                {
                    var col = hits2D[i];
                    if (col == null) continue;
                    if (((1 << col.gameObject.layer) & clickableLayers.value) == 0) continue;
                    clickedObject = col.gameObject;
                    return true;
                }
            }

            var ray = cam.ScreenPointToRay(PointerPosition);
            if (Physics.Raycast(ray, out var hit3D, float.MaxValue, clickableLayers))
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

        #region 내부 카메라 바인딩
        private void OnStageLoaded(string stageId)
        {
            // 방금 로드된 스테이지 씬에서 우선 카메라를 찾아 바인딩
            var scene = SceneManager.GetSceneByName(stageId);
            if (!scene.IsValid() || !scene.isLoaded) return;
            var camInScene = FindCameraInScene(scene);
            if (camInScene != null)
            {
                uiCamera = camInScene;
            }
            else if (uiCamera == null)
            {
                uiCamera = Camera.main != null ? Camera.main : FindAnyActiveCamera();
            }
        }

        private void OnStageUnloaded(string stageId)
        {
            // 언로드된 씬의 카메라를 참조 중이면 폴백
            if (uiCamera != null && uiCamera.gameObject.scene.name == stageId)
            {
                uiCamera = Camera.main != null ? Camera.main : FindAnyActiveCamera();
            }
        }

        private static Camera FindCameraInScene(UnityEngine.SceneManagement.Scene scene)
        {
            var roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                var cams = roots[i].GetComponentsInChildren<Camera>(true);
                for (int j = 0; j < cams.Length; j++)
                {
                    var c = cams[j];
                    if (c != null && c.isActiveAndEnabled)
                    {
                        return c;
                    }
                }
            }
            return null;
        }

        private static Camera FindAnyActiveCamera()
        {
            var all = UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i] != null && all[i].isActiveAndEnabled)
                {
                    return all[i];
                }
            }
            return null;
        }
        #endregion

        #region 내부 경로 유틸
        private static string GetTransformPath(Transform tr)
        {
            if (tr == null) return "<null>";
            System.Text.StringBuilder sb = new System.Text.StringBuilder(64);
            while (tr != null)
            {
                sb.Insert(0, "/" + tr.name);
                tr = tr.parent;
            }
            return sb.ToString();
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


