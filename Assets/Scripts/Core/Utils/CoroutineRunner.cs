using System;
using System.Collections;
using UnityEngine;

namespace AFKS.Core.Utils
{
    /// <summary>
    /// 전역에서 안전하게 코루틴을 실행하기 위한 유틸리티.
    /// 내부적으로 숨겨진 GameObject에 MonoBehaviour를 보유합니다.
    /// </summary>
    public static class CoroutineRunner
    {
        private sealed class CoroutineRunnerBehaviour : MonoBehaviour { }

        private static CoroutineRunnerBehaviour _runner;

        private static CoroutineRunnerBehaviour EnsureRunner()
        {
            if (_runner != null) return _runner;
            var go = new GameObject("CoroutineRunner");
            go.hideFlags = HideFlags.HideAndDontSave;
            UnityEngine.Object.DontDestroyOnLoad(go);
            _runner = go.AddComponent<CoroutineRunnerBehaviour>();
            return _runner;
        }

        public static Coroutine Start(IEnumerator routine)
        {
            if (routine == null) return null;
            return EnsureRunner().StartCoroutine(routine);
        }

        public static void Stop(Coroutine coroutine)
        {
            if (coroutine == null || _runner == null) return;
            _runner.StopCoroutine(coroutine);
        }

        /// <summary>
        /// delay(초) 후 callback을 호출합니다.
        /// </summary>
        public static Coroutine InvokeAfterSeconds(float delaySeconds, Action callback)
        {
            return Start(InvokeAfterSecondsImpl(delaySeconds, callback));
        }

        private static IEnumerator InvokeAfterSecondsImpl(float delaySeconds, Action callback)
        {
            if (delaySeconds > 0f)
            {
                yield return new WaitForSeconds(delaySeconds);
            }
            callback?.Invoke();
        }
    }
}


