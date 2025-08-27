using UnityEngine;

namespace AFKS.Core.Services.Input
{
    /// <summary>
    /// 입력 상태/이벤트를 제공하고 전역 입력 잠금 기능을 제공하는 인터페이스입니다.
    /// </summary>
    public interface IInputService
    {
        #region 상태
        /// <summary>입력이 잠금 상태인지 여부.</summary>
        bool IsLocked { get; }
        /// <summary>현재 포인터(마우스) 위치.</summary>
        Vector2 PointerPosition { get; }
        #endregion

        #region 질의/제어
        /// <summary>현재 포인터가 UI 위에 있는지 여부를 반환.</summary>
        bool IsPointerOverUI();
        /// <summary>입력 잠금을 설정/해제.</summary>
        void Lock(bool locked);
        /// <summary>화면 포인터 위치로 레이캐스트하여 클릭된 오브젝트를 반환합니다.</summary>
        bool TryRaycast(out GameObject clickedObject);
        #endregion

        #region 이벤트
        /// <summary>좌클릭이 눌렸을 때 발생.</summary>
        event System.Action Clicked;
        /// <summary>좌클릭으로 특정 오브젝트가 클릭되었을 때 발생.</summary>
        event System.Action<GameObject> ObjectClicked;
        #endregion

        #region 통합 입력 훅
        /// <summary>
        /// Input System(또는 StandaloneInputModule)에서 PrimaryDown 발생 시 호출하여
        /// 내부 Clicked/ObjectClicked 이벤트를 발생시킵니다.
        /// </summary>
        void NotifyPointerPrimaryDown();
        #endregion
    }
}


