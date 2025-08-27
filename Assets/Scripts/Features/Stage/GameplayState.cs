namespace AFKS.Features.Stage
{
    /// <summary>
    /// 게임플레이의 현재 상태를 정의합니다.
    /// 각 상태에 따라 상호작용 가능한 오브젝트가 제한됩니다.
    /// </summary>
    public enum GameplayState
    {
        /// <summary>
        /// 초기 상태: Door만 클릭 가능
        /// </summary>
        Initial,
        
        /// <summary>
        /// 체인 해금 중: 체인 클릭 가능, Door 클릭 불가
        /// </summary>
        ChainUnlocking,
        
        /// <summary>
        /// 체인 해금 완료: Door 클릭 가능, 체인 클릭 불가
        /// </summary>
        ChainUnlocked,
        
        /// <summary>
        /// 스테이지 전환 중: 모든 상호작용 불가
        /// </summary>
        Transitioning
    }
}
