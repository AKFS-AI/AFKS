using UnityEngine;

namespace AFKS.Shared.Interfaces
{
    /// <summary>
    /// 공포 이벤트를 위한 인터페이스
    /// </summary>
    public interface IHorrorEvent
    {
        /// <summary>
        /// 이벤트 트리거
        /// </summary>
        void Trigger();
        
        /// <summary>
        /// 이벤트 실행 준비 상태 확인
        /// </summary>
        /// <returns>실행 가능하면 true</returns>
        bool IsReady();
        
        /// <summary>
        /// 이벤트 완료 여부
        /// </summary>
        /// <returns>완료되었으면 true</returns>
        bool IsCompleted();
        
        /// <summary>
        /// 이벤트 리셋
        /// </summary>
        void Reset();
    }
}