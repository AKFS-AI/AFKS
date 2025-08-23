using UnityEngine;
using AFKS.Core.Events;

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
        #endregion

        #region 유니티 수명주기
        private void OnMouseDown()
        {
            if (!string.IsNullOrEmpty(targetStageId))
            {
                GameEvents.RaiseStageChangeRequested(targetStageId);
            }
        }
        #endregion

        #region 공개 API
        public void SetTarget(string stageId)
        {
            targetStageId = stageId;
        }
        #endregion
    }
}


