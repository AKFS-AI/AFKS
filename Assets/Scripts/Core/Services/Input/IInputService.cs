using UnityEngine;

namespace AFKS.Core.Services.Input
{
    public interface IInputService
    {
        bool IsLocked { get; }
        Vector2 PointerPosition { get; }
        bool IsPointerOverUI();
        void Lock(bool locked);

        event System.Action Clicked;
    }
}


