using UnityEngine;

namespace AFKS.Shared.Core
{
    /// <summary>
    /// 제네릭 싱글톤 베이스 클래스
    /// 모든 매니저 클래스의 싱글톤 패턴 중복을 해결
    /// </summary>
    /// <typeparam name="T">싱글톤으로 만들 클래스 타입</typeparam>
    public abstract class BaseSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T instance;
        private static readonly object lockObject = new object();
        private static bool applicationIsQuitting = false;

        /// <summary>
        /// 싱글톤 인스턴스 접근
        /// </summary>
        public static T Instance
        {
            get
            {
                if (applicationIsQuitting)
                {
                    Debug.LogWarning($"[BaseSingleton] {typeof(T)} 인스턴스가 이미 파괴되었습니다. null을 반환합니다.");
                    return null;
                }

                lock (lockObject)
                {
                    if (instance == null)
                    {
                        instance = FindFirstObjectByType<T>();

                        if (instance == null)
                        {
                            GameObject singletonObject = new GameObject($"{typeof(T).Name}");
                            instance = singletonObject.AddComponent<T>();
                            
                            // 루트 GameObject로 설정하여 DontDestroyOnLoad 경고 방지
                            singletonObject.transform.SetParent(null);
                            DontDestroyOnLoad(singletonObject);
                            
                            Debug.Log($"[BaseSingleton] {typeof(T).Name} 인스턴스가 자동으로 생성되었습니다.");
                        }
                    }
                }

                return instance;
            }
        }

        /// <summary>
        /// 싱글톤 인스턴스가 존재하는지 확인
        /// </summary>
        public static bool HasInstance => instance != null && !applicationIsQuitting;

        /// <summary>
        /// Awake에서 호출할 싱글톤 초기화
        /// </summary>
        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = this as T;
                
                // 루트 GameObject로 설정하여 DontDestroyOnLoad 경고 방지
                if (transform.parent != null)
                {
                    transform.SetParent(null);
                }
                DontDestroyOnLoad(gameObject);
                OnSingletonAwake();
            }
            else if (instance != this)
            {
                Debug.LogWarning($"[BaseSingleton] {typeof(T).Name}의 중복 인스턴스가 감지되어 제거됩니다.");
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 싱글톤이 초기화될 때 호출되는 가상 메서드
        /// 하위 클래스에서 오버라이드하여 초기화 로직 구현
        /// </summary>
        protected virtual void OnSingletonAwake() 
        {
            // 하위 클래스에서 구현
        }

        /// <summary>
        /// 애플리케이션 종료 시 호출
        /// </summary>
        protected virtual void OnApplicationQuit()
        {
            applicationIsQuitting = true;
        }

        /// <summary>
        /// OnDestroy에서 인스턴스 정리
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
    }
}