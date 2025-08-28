using UnityEngine;
using AFKS.Core.Events;
using AFKS.Core.Services;
using AFKS.Core.Services.Input;
using AFKS.Core.Services.Scene;
using UnityEngine.SceneManagement;

namespace AFKS.Features.Interaction
{
    /// <summary>
    /// 클릭 가능한 오브젝트에 부착하여 스테이지 전환을 요청합니다.
    /// </summary>
    [AddComponentMenu("AFKS/Interaction/Hotspot Move")]
    public sealed class HotspotMove : MonoBehaviour
    {
        #region 필드
        [SerializeField]
        [InspectorName("목표 스테이지 ID")]
        [Tooltip("클릭 시 이동할 스테이지 ID (예: Stage_Lobby)")]
        private string targetStageId;

        private IInputService inputService;
        [SerializeField]
        [Tooltip("전환 요청/폴백 경로를 로그로 출력합니다.")]
        private bool debugLog = false;

        [SerializeField]
        [Tooltip("수동 트리거 전용: 클릭/입력 이벤트에 반응하지 않고 MoveToStage() 호출로만 이동합니다.")]
        private bool manualTriggerOnly = false;
        #endregion

        #region 유니티 수명주기
        private void Awake()
        {
            ServiceLocator.TryGet<IInputService>(out inputService);
            if (debugLog)
            {
                Debug.Log($"[HotspotMove] Awake targetStageId={targetStageId} hasInput={(inputService!=null)}");
            }
        }

        private void OnEnable()
        {
            if (!manualTriggerOnly && inputService != null)
            {
                inputService.ObjectClicked += OnObjectClicked;
            }
        }

        private void OnDisable()
        {
            if (!manualTriggerOnly && inputService != null)
            {
                inputService.ObjectClicked -= OnObjectClicked;
            }
        }
        #endregion

        #region 공개 API
        public void SetTarget(string stageId)
        {
            targetStageId = stageId;
        }
        
        /// <summary>
        /// 수동 트리거 모드를 켜거나 끕니다. 켜면 입력 구독을 해제합니다.
        /// </summary>
        public void SetManualTriggerOnly(bool enabled)
        {
            if (manualTriggerOnly == enabled) return;
            // 기존 구독 해제
            if (inputService != null)
            {
                inputService.ObjectClicked -= OnObjectClicked;
            }
            manualTriggerOnly = enabled;
            // 필요 시 재구독
            if (!manualTriggerOnly && inputService != null && isActiveAndEnabled)
            {
                inputService.ObjectClicked += OnObjectClicked;
            }
        }
        
        /// <summary>
        /// 카메라 확대 효과 완료 후 호출되어 스테이지 이동을 실행합니다.
        /// </summary>
        public void MoveToStage()
        {
            if (string.IsNullOrEmpty(targetStageId))
            {
                Debug.LogWarning("[HotspotMove] MoveToStage: targetStageId가 설정되지 않았습니다.");
                return;
            }
            
            if (debugLog) Debug.Log($"[HotspotMove] MoveToStage 호출됨 -> {targetStageId}");
            
            // 우선 전역 전환 이벤트 발행(코어가 있을 때 정상 처리)
            GameEvents.RaiseStageChangeRequested(targetStageId);

            // 코어( SceneService )가 없는 스탠드얼론 스테이지에서도 동작하도록 폴백
            if (!ServiceLocator.TryGet<ISceneService>(out _))
            {
                if (debugLog) Debug.Log("[HotspotMove] SceneService missing. Fallback to SceneManager.LoadScene");
                SceneManager.LoadScene(targetStageId, LoadSceneMode.Single);
            }
        }
        #endregion

        #region 이벤트 핸들러
        private void OnObjectClicked(GameObject clicked)
        {
            if (manualTriggerOnly) return;
            if (clicked == gameObject && !string.IsNullOrEmpty(targetStageId))
            {
                // 우선 전역 전환 이벤트 발행(코어가 있을 때 정상 처리)
                if (debugLog) Debug.Log($"[HotspotMove] Raise StageChangeRequested -> {targetStageId}");
                GameEvents.RaiseStageChangeRequested(targetStageId);

                // SceneService를 통한 안전한 씬 전환
                if (ServiceLocator.TryGet<ISceneService>(out var sceneService))
                {
                    if (debugLog) Debug.Log("[HotspotMove] SceneService를 통한 씬 전환");
                    sceneService.LoadScene(targetStageId, LoadSceneMode.Single);
                }
                else
                {
                    if (debugLog) Debug.Log("[HotspotMove] SceneService missing. Fallback to SceneManager.LoadScene");
                    SceneManager.LoadScene(targetStageId, LoadSceneMode.Single);
                }
            }
        }
        #endregion

        #region 마우스 폴백(스테이지 단독 실행 지원)
        private void OnMouseDown()
        {
            if (manualTriggerOnly) return;
            if (string.IsNullOrEmpty(targetStageId)) return;
            if (!enabled || !gameObject.activeInHierarchy) return;
            if (debugLog) Debug.Log($"[HotspotMove] OnMouseDown Fallback -> {targetStageId}");
            GameEvents.RaiseStageChangeRequested(targetStageId);
            
            // SceneService를 통한 안전한 씬 전환
            if (ServiceLocator.TryGet<ISceneService>(out var sceneService))
            {
                if (debugLog) Debug.Log("[HotspotMove] SceneService를 통한 씬 전환");
                sceneService.LoadScene(targetStageId, LoadSceneMode.Single);
            }
            else
            {
                if (debugLog) Debug.Log("[HotspotMove] SceneService missing. Fallback to SceneManager.LoadScene");
                SceneManager.LoadScene(targetStageId, LoadSceneMode.Single);
            }
        }
        #endregion
    }
}


