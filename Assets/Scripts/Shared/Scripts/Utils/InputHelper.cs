using UnityEngine;

namespace AFKS.Shared.Utils
{
    /// <summary>
    /// 입력 시스템 헬퍼: 신(새) 입력 시스템과 구 입력 시스템을 모두 지원
    /// </summary>
    public static class InputHelper
    {
        public static bool WasEscapePressedThisFrame()
        {
#if ENABLE_INPUT_SYSTEM || UNITY_INPUT_SYSTEM
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            return keyboard != null && keyboard.escapeKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.Escape);
#endif
        }

        public static bool WasTabPressedThisFrame()
        {
#if ENABLE_INPUT_SYSTEM || UNITY_INPUT_SYSTEM
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            return keyboard != null && keyboard.tabKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.Tab);
#endif
        }
    }
}


