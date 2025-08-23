using UnityEngine;
using AFKS.Core.Events;
using AFKS.Features.Stage;

namespace AFKS.Core.Systems
{
    /// <summary>
    /// 재생 시작 시 초기 스테이지 전환을 요청합니다.
    /// </summary>
    [AddComponentMenu("AFKS/Boot/Startup Loader")]
    public sealed class StartupLoader : MonoBehaviour
    {
        #region 필드
        [SerializeField]
        [InspectorName("초기 스테이지 ID")]
        [Tooltip("게임 시작 시 로드할 스테이지의 ID (예: Stage_Menu, Stage_Front)")]
        private string initialStageId = StageIds.Menu;

        [SerializeField]
        [InspectorName("시작 시 자동 로드")]
        [Tooltip("체크 시 Start()에서 자동으로 초기 스테이지를 요청합니다.")]
        private bool autoLoadOnStart = true;
        #endregion

        #region 유니티 수명주기
        private void Start()
        {
            if (autoLoadOnStart && !string.IsNullOrEmpty(initialStageId))
            {
                GameEvents.RaiseStageChangeRequested(initialStageId);
            }
        }
        #endregion
    }
}


