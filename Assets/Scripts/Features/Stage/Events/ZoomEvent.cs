using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;
using AFKS.Features.Stage.Animations;

namespace AFKS.Features.Stage.Events
{
    /// <summary>
    /// 카메라 줌을 처리하는 이벤트입니다.
    /// 특정 위치로 카메라를 이동시키고 줌 효과를 제공합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "New Zoom Event", menuName = "AFKS/Stage/Events/Zoom Event")]
    public class ZoomEvent : StageEvent
    {
        [Header("줌 설정")]
        [SerializeField] private Vector3 targetPosition = Vector3.zero;
        [SerializeField] private float targetOrthographicSize = 2.5f;
        [SerializeField] private float zoomDuration = 1.5f;
        [SerializeField] private AnimationCurve zoomCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        
        [Header("줌 후 동작")]
        [SerializeField] private bool showObjectsAfterZoom = false;
        [SerializeField] private List<string> objectsToShow = new List<string>();
        [SerializeField] private bool hideObjectsAfterZoom = false;
        [SerializeField] private List<string> objectsToHide = new List<string>();
        
        [Header("줌 완료 후")]
        [SerializeField] private bool autoProceedAfterZoom = false; // 향후 자동 진행 옵션 연결 예정(현재 로깅으로 참조)
        [SerializeField] private float delayAfterZoom = 0.5f;
        
        #region StageEvent 구현
        
        public override void Execute(System.Action onComplete)
        {
            Debug.Log($"ZoomEvent: {eventName} 실행 시작 - 위치: {targetPosition}, 크기: {targetOrthographicSize}, AutoProceedAfterZoom={autoProceedAfterZoom}");

            // autoProceedAfterZoom 설정을 autoProceed에 반영
            if (autoProceedAfterZoom)
            {
                SetAutoProceed(true);
            }

            // 내장 줌 기능 사용 - 스테이지의 카메라를 우선적으로 찾기
            var targetCamera = FindStageCamera();
            if (targetCamera != null)
            {
                // StageEventSystem에서 코루틴 실행
                var eventSystem = FindFirstObjectByType<StageEventSystem>();
                if (eventSystem != null)
                {
                    eventSystem.StartCoroutine(ZoomCoroutine(targetCamera, onComplete));
                }
                else
                {
                    Debug.LogError("ZoomEvent: StageEventSystem을 찾을 수 없습니다.");
                    onComplete?.Invoke();
                }
            }
            else
            {
                Debug.LogError("ZoomEvent: 스테이지 카메라를 찾을 수 없습니다.");
                onComplete?.Invoke();
            }
        }
        
        public override void Prepare()
        {
            Debug.Log($"ZoomEvent: {eventName} 준비됨 - 줌 위치: {targetPosition}");
            
            // 줌 후 표시할 오브젝트들을 숨김 상태로 설정
            if (showObjectsAfterZoom)
            {
                foreach (var objectId in objectsToShow)
                {
                    var obj = GameObject.Find(objectId);
                    if (obj != null)
                    {
                        obj.SetActive(false);
                        Debug.Log($"ZoomEvent: {objectId} 줌 전 숨김");
                    }
                }
            }
        }
        
        public override void Cleanup()
        {
            Debug.Log($"ZoomEvent: {eventName} 정리됨");
        }
        
        #endregion
        
        #region 카메라 찾기
        
        /// <summary>
        /// 스테이지의 카메라를 찾습니다. 스테이지 씬의 카메라를 우선적으로 찾고, 없으면 메인 카메라를 사용합니다.
        /// </summary>
        private Camera FindStageCamera()
        {
            // 1. 현재 활성 씬에서 "Stage"로 시작하는 이름의 카메라 찾기
            var stageCameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
            foreach (var cam in stageCameras)
            {
                if (cam.name.Contains("Stage") || cam.name.Contains("Camera"))
                {
                    // 스테이지 씬에 속한 카메라인지 확인
                    if (cam.gameObject.scene.name.StartsWith("Stage"))
                    {
                        Debug.Log($"ZoomEvent: 스테이지 카메라 발견 - {cam.name} (씬: {cam.gameObject.scene.name})");
                        return cam;
                    }
                }
            }
            
            // 2. 현재 활성 씬의 메인 카메라 찾기
            var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            var activeSceneCameras = activeScene.GetRootGameObjects()
                .SelectMany(go => go.GetComponentsInChildren<Camera>())
                .Where(cam => cam != null)
                .ToArray();
            
            if (activeSceneCameras.Length > 0)
            {
                var mainCam = activeSceneCameras.FirstOrDefault(cam => cam.CompareTag("MainCamera")) ?? activeSceneCameras[0];
                Debug.Log($"ZoomEvent: 활성 씬 카메라 사용 - {mainCam.name} (씬: {mainCam.gameObject.scene.name})");
                return mainCam;
            }
            
            // 3. 마지막 수단으로 전체 씬의 메인 카메라 사용
            var fallbackCamera = Camera.main;
            if (fallbackCamera != null)
            {
                Debug.Log($"ZoomEvent: 메인 카메라 사용 - {fallbackCamera.name} (씬: {fallbackCamera.gameObject.scene.name})");
                return fallbackCamera;
            }
            
            Debug.LogWarning("ZoomEvent: 사용 가능한 카메라를 찾을 수 없습니다.");
            return null;
        }
        
        #endregion
        
        #region 줌 완료 처리
        
        private void OnZoomComplete(System.Action onComplete)
        {
            Debug.Log($"ZoomEvent: {eventName} 줌 완료");
            
            // 줌 후 오브젝트 표시
            if (showObjectsAfterZoom)
            {
                var sim = AFKS.Features.Stage.StageInteractionManager.Instance;
                foreach (var objectId in objectsToShow)
                {
                    var obj = sim != null ? sim.GetObject(objectId) : null;
                    if (obj != null)
                    {
                        obj.SetActive(true);
                        Debug.Log($"ZoomEvent: {objectId} 줌 후 표시");
                    }
                }
            }
            
            // 줌 후 오브젝트 숨김
            if (hideObjectsAfterZoom)
            {
                var sim = AFKS.Features.Stage.StageInteractionManager.Instance;
                foreach (var objectId in objectsToHide)
                {
                    var obj = sim != null ? sim.GetObject(objectId) : null;
                    if (obj != null)
                    {
                        obj.SetActive(false);
                        Debug.Log($"ZoomEvent: {objectId} 줌 후 숨김");
                    }
                }
            }
            
            // 지연 후 완료 콜백 호출
            ExecuteWithDelay(delayAfterZoom, () => {
                if (autoProceedAfterZoom)
                {
                    // 자동 진행 옵션이 켜져 있으면 StageEventSystem에 다음 이벤트 진행을 요청
                    var eventSystem = FindFirstObjectByType<AFKS.Features.Stage.StageEventSystem>();
                    eventSystem?.ProceedToNextEvent();
                }
                onComplete?.Invoke();
            });
        }
        
        #endregion
        
        #region 내장 줌 기능
        
        /// <summary>
        /// 카메라 줌을 수행하는 코루틴입니다.
        /// </summary>
        private System.Collections.IEnumerator ZoomCoroutine(Camera targetCamera, System.Action onComplete)
        {
            var originalPosition = targetCamera.transform.position;
            var originalSize = targetCamera.orthographicSize;
            var startTime = Time.time;
            
            while (Time.time - startTime < zoomDuration)
            {
                var progress = (Time.time - startTime) / zoomDuration;
                var curveValue = zoomCurve.Evaluate(progress);
                
                // 위치 보간
                targetCamera.transform.position = Vector3.Lerp(originalPosition, targetPosition, curveValue);
                
                // 크기 보간
                targetCamera.orthographicSize = Mathf.Lerp(originalSize, targetOrthographicSize, curveValue);
                
                yield return null;
            }
            
            // 최종 상태 설정
            targetCamera.transform.position = targetPosition;
            targetCamera.orthographicSize = targetOrthographicSize;
            
            // 줌 완료 처리
            OnZoomComplete(onComplete);
        }
        
        #endregion
        
        #region 설정 메서드
        
        /// <summary>
        /// 줌 타겟 위치를 설정합니다.
        /// </summary>
        /// <param name="position">타겟 위치</param>
        public void SetTargetPosition(Vector3 position)
        {
            targetPosition = position;
        }
        
        /// <summary>
        /// 줌 타겟 크기를 설정합니다.
        /// </summary>
        /// <param name="size">타겟 크기</param>
        public void SetTargetSize(float size)
        {
            targetOrthographicSize = size;
        }
        
        /// <summary>
        /// 줌 지속 시간을 설정합니다.
        /// </summary>
        /// <param name="duration">지속 시간</param>
        public void SetZoomDuration(float duration)
        {
            zoomDuration = duration;
        }
        
        #endregion
        
        #region 유틸리티
        
        /// <summary>
        /// 현재 줌 타겟 위치를 반환합니다.
        /// </summary>
        public Vector3 GetTargetPosition() => targetPosition;
        
        /// <summary>
        /// 현재 줌 타겟 크기를 반환합니다.
        /// </summary>
        public float GetTargetSize() => targetOrthographicSize;
        
        /// <summary>
        /// 줌 지속 시간을 반환합니다.
        /// </summary>
        public float GetZoomDuration() => zoomDuration;
        
        #endregion
        
        #region Stage1 전용 설정 메서드
        
        /// <summary>
        /// 체인 영역으로 줌하는 설정을 적용합니다.
        /// </summary>
        public void SetupChainAreaZoom()
        {
            eventName = "카메라 줌 - 체인 영역";
            description = "체인 영역으로 카메라가 줌됩니다.";
            // Stage1 카메라(Orthographic) 기준 값: Pos(0, -1, -10), Size 3.3
            targetPosition = new Vector3(0f, -1f, -10f);
            targetOrthographicSize = 3.3f;
            zoomDuration = 1.5f;
            // 체인 가시성은 Stage1Controller가 전담하므로 이 이벤트에서는 건드리지 않음
            showObjectsAfterZoom = false;
            objectsToShow.Clear();
            hideObjectsAfterZoom = false;
            objectsToHide.Clear();
            // 줌 완료 후의 진행은 Stage1Controller에서 관리
            autoProceedAfterZoom = false;
            delayAfterZoom = 0.5f;
        }
        
        #endregion
    }
}
