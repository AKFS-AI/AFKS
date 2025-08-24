using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace AFKS.Core.Utils
{
    /// <summary>
    /// 씬이 애디티브로 공존할 때 전역 단일 컴포넌트(EventSystem/AudioListener)를 1개만 활성화로 유지합니다.
    /// 코어 씬에 붙여 사용하며, 씬 로드/언로드 시마다 자동으로 정리합니다.
    /// </summary>
    [AddComponentMenu("AFKS/Utils/Global Singleton Guard")]
    public sealed class GlobalSingletonGuard : MonoBehaviour
    {
        [SerializeField] private bool enforceEventSystem = true;
        [SerializeField] private bool enforceAudioListener = true;

        private Scene coreScene;

        private void Awake()
        {
            coreScene = gameObject.scene;
            SanitizeGlobals();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            SanitizeGlobals();
        }

        private void OnSceneUnloaded(Scene scene)
        {
            SanitizeGlobals();
        }

        private void SanitizeGlobals()
        {
            if (enforceEventSystem)
            {
                var systems = UnityEngine.Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
                if (systems != null && systems.Length > 1)
                {
                    EventSystem keep = null;
                    for (int i = 0; i < systems.Length; i++)
                    {
                        var es = systems[i];
                        if (es == null) continue;
                        if (es.gameObject.scene == coreScene)
                        {
                            keep = es; break;
                        }
                    }
                    if (keep == null) keep = systems[0];
                    for (int i = 0; i < systems.Length; i++)
                    {
                        var es = systems[i];
                        if (es == null) continue;
                        es.enabled = (es == keep);
                    }
                }
            }

            if (enforceAudioListener)
            {
                var listeners = UnityEngine.Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
                if (listeners != null && listeners.Length > 1)
                {
                    AudioListener keep = null;
                    for (int i = 0; i < listeners.Length; i++)
                    {
                        var al = listeners[i];
                        if (al == null) continue;
                        if (al.gameObject.scene == coreScene)
                        {
                            keep = al; break;
                        }
                    }
                    if (keep == null) keep = listeners[0];
                    for (int i = 0; i < listeners.Length; i++)
                    {
                        var al = listeners[i];
                        if (al == null) continue;
                        al.enabled = (al == keep);
                    }
                }
            }
        }
    }
}


