using UnityEngine;

namespace AFKS.Features.Stage
{
    /// <summary>
    /// 스테이지 단위의 라이프사이클 훅을 제공합니다.
    /// 각 Stage_* 씬의 루트 오브젝트에 부착하여 사용합니다.
    /// </summary>
    [AddComponentMenu("AFKS/Stage/Stage Root")]
    public sealed class StageRoot : MonoBehaviour
    {
        #region 공개 API
        public void Initialize()
        {
            // 스테이지 진입 시 카메라/오디오/핫스팟 등의 상태를 준비합니다.
        }

        public void Teardown()
        {
            // 스테이지 종료 시 임시 리소스/구독을 정리합니다.
        }
        #endregion

        #region 유니티 수명주기(선택적 훅)
        private void OnEnable()
        {
            // 필요 시 구독 등록
        }

        private void OnDisable()
        {
            // 필요 시 구독 해제
        }
        #endregion
    }
}


