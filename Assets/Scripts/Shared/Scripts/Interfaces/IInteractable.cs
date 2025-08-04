using UnityEngine;

namespace AFKS.Shared.Interfaces
{
    /// <summary>
    /// 상호작용 가능한 오브젝트를 위한 인터페이스
    /// </summary>
    public interface IInteractable
    {
        /// <summary>
        /// 상호작용 실행
        /// </summary>
        void OnInteract();
        
        /// <summary>
        /// 상호작용 가능 여부 확인
        /// </summary>
        /// <returns>상호작용 가능하면 true</returns>
        bool CanInteract();
        
        /// <summary>
        /// 상호작용 타입 반환
        /// </summary>
        InteractionType GetInteractionType();
    }
    
    public enum InteractionType
    {
        Click,          // 단순 클릭
        Examine,        // 자세히 보기
        Collect,        // 수집
        Use,            // 사용
        Trigger         // 트리거 발동
    }
}