using UnityEngine;

namespace AFKS.Shared.Core
{
    /// <summary>
    /// 런타임 시작 시 핵심 싱글턴들을 보장하는 부트스트랩퍼
    /// </summary>
    public static class Bootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureCoreSingletons()
        {
            // SaveManager 인스턴스를 미리 생성하여 다른 매니저의 등록 타이밍을 보장
            var _ = SaveManager.Instance;
        }
    }
}


