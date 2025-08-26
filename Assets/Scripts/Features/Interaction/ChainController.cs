using UnityEngine;
using UnityEngine.Events;
using AFKS.Core.Services;

namespace AFKS.Features.Interaction
{
    /// <summary>
    /// 체인의 클릭 상호작용과 해금 로직을 관리합니다.
    /// 지정된 횟수만큼 클릭하면 체인이 해금됩니다.
    /// </summary>
    [AddComponentMenu("AFKS/Interaction/Chain Controller")]
    public sealed class ChainController : MonoBehaviour
    {
        [Header("해금 설정")]
        [SerializeField]
        [InspectorName("해금에 필요한 클릭 횟수")] private int clicksToUnlock = 3;
        
        [SerializeField]
        [InspectorName("클릭 간격 제한 (초)")] private float clickCooldown = 0.5f;
        
        [Header("애니메이션")]
        [SerializeField]
        [InspectorName("흔들림 애니메이션")] private bool enableShakeAnimation = true;
        
        [SerializeField]
        [InspectorName("흔들림 강도")] private float shakeIntensity = 0.1f;
        
        [SerializeField]
        [InspectorName("흔들림 지속 시간")] private float shakeDuration = 0.3f;
        
        [Header("이벤트")]
        [SerializeField]
        [InspectorName("체인 해금 이벤트")] private UnityEvent onChainUnlocked;
        
        [SerializeField]
        [InspectorName("클릭 이벤트")] private UnityEvent<int> onChainClicked;

        private int currentClickCount = 0;
        private float lastClickTime = 0f;
        private Vector3 originalPosition;
        private bool isUnlocked = false;
        private bool isShaking = false;

        #region Unity 수명주기

        private void Awake()
        {
            originalPosition = transform.localPosition;
        }

        private void Start()
        {
            // 초기 상태 확인
            CheckUnlockedState();
        }

        #endregion

        #region 공개 API

        /// <summary>
        /// 체인을 클릭합니다. 지정된 횟수만큼 클릭하면 해금됩니다.
        /// </summary>
        public void OnChainClicked()
        {
            if (isUnlocked) return;
            
            // 쿨다운 체크
            if (Time.time - lastClickTime < clickCooldown) return;
            
            lastClickTime = Time.time;
            currentClickCount++;
            
            // 클릭 이벤트 발생
            onChainClicked?.Invoke(currentClickCount);
            
            // 흔들림 애니메이션
            if (enableShakeAnimation)
            {
                StartShakeAnimation();
            }
            
            // 해금 체크
            if (currentClickCount >= clicksToUnlock)
            {
                UnlockChain();
            }
            
            Debug.Log($"Chain: {currentClickCount}/{clicksToUnlock} 클릭");
        }

        /// <summary>
        /// 체인을 강제로 해금합니다.
        /// </summary>
        public void ForceUnlock()
        {
            if (!isUnlocked)
            {
                UnlockChain();
            }
        }

        /// <summary>
        /// 체인을 잠금 상태로 되돌립니다.
        /// </summary>
        public void RelockChain()
        {
            isUnlocked = false;
            currentClickCount = 0;
            transform.localPosition = originalPosition;
            
            // SaveService에 상태 저장
            SaveUnlockedState(false);
            
            Debug.Log("Chain: 체인이 다시 잠겼습니다.");
        }

        #endregion

        #region 내부 메서드

        private void CheckUnlockedState()
        {
            bool savedUnlocked = LoadUnlockedState();
            if (savedUnlocked)
            {
                UnlockChain();
            }
        }

        private void UnlockChain()
        {
            if (isUnlocked) return;
            
            isUnlocked = true;
            
            // 해금 이벤트 발생
            onChainUnlocked?.Invoke();
            
            // SaveService에 상태 저장
            SaveUnlockedState(true);
            
            Debug.Log("Chain: 체인이 해금되었습니다!");
        }

        private void StartShakeAnimation()
        {
            if (isShaking) return;
            
            StartCoroutine(ShakeCoroutine());
        }

        private System.Collections.IEnumerator ShakeCoroutine()
        {
            isShaking = true;
            float elapsed = 0f;
            
            while (elapsed < shakeDuration)
            {
                float x = originalPosition.x + Random.Range(-shakeIntensity, shakeIntensity);
                float y = originalPosition.y + Random.Range(-shakeIntensity, shakeIntensity);
                
                transform.localPosition = new Vector3(x, y, originalPosition.z);
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            transform.localPosition = originalPosition;
            isShaking = false;
        }

        private bool LoadUnlockedState()
        {
            if (ServiceLocator.TryGet<AFKS.Core.Services.Save.ISaveService>(out var saveService))
            {
                return saveService.GetBool("Chain.Unlocked", false);
            }
            
            // SaveService가 없으면 PlayerPrefs 사용
            return PlayerPrefs.GetInt("Chain.Unlocked", 0) == 1;
        }

        private void SaveUnlockedState(bool unlocked)
        {
            if (ServiceLocator.TryGet<AFKS.Core.Services.Save.ISaveService>(out var saveService))
            {
                saveService.SetBool("Chain.Unlocked", unlocked);
            }
            else
            {
                // SaveService가 없으면 PlayerPrefs 사용
                PlayerPrefs.SetInt("Chain.Unlocked", unlocked ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        #endregion

        #region 인스펙터 검증

        private void OnValidate()
        {
            clicksToUnlock = Mathf.Max(1, clicksToUnlock);
            clickCooldown = Mathf.Max(0.1f, clickCooldown);
            shakeIntensity = Mathf.Max(0.01f, shakeIntensity);
            shakeDuration = Mathf.Max(0.1f, shakeDuration);
        }

        #endregion

        #region 디버그

        private void OnDrawGizmosSelected()
        {
            // 클릭 영역 표시
            var collider = GetComponent<Collider2D>();
            if (collider != null)
            {
                Gizmos.color = isUnlocked ? Color.green : Color.yellow;
                Gizmos.DrawWireCube(transform.position, collider.bounds.size);
            }
        }

        #endregion
    }
}
