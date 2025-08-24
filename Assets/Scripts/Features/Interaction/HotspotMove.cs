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
        #endregion

        #region 유니티 수명주기
        private void Awake()
        {
            ServiceLocator.TryGet<IInputService>(out inputService);
        }

        private void OnEnable()
        {
            if (inputService != null)
            {
                inputService.ObjectClicked += OnObjectClicked;
            }
        }

        private void OnDisable()
        {
            if (inputService != null)
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
        #endregion

        #region 이벤트 핸들러
        private void OnObjectClicked(GameObject clicked)
        {
            if (clicked == gameObject && !string.IsNullOrEmpty(targetStageId))
            {
                // 우선 전역 전환 이벤트 발행(코어가 있을 때 정상 처리)
                GameEvents.RaiseStageChangeRequested(targetStageId);

                // 코어( SceneService )가 없는 스탠드얼론 스테이지에서도 동작하도록 폴백
                if (!ServiceLocator.TryGet<ISceneService>(out _))
                {
                    SceneManager.LoadScene(targetStageId, LoadSceneMode.Single);
                }
            }
        }
        #endregion
    }
}


