using UnityEngine;
using System.Collections;
using AFKS.Core.Audio;

namespace AFKS.Features.Stage.Effects
{
    /// <summary>
    /// 체인 오브젝트에 실제 흔들림 효과를 제공하는 컴포넌트입니다.
    /// </summary>
    public class ChainShakeEffect : MonoBehaviour
    {
        [Header("흔들림 설정")]
        [SerializeField] private float shakeIntensity = 0.1f;
        [SerializeField] private float shakeSpeed = 10f;
        
        [Header("효과음 설정")]
        [SerializeField] private AudioClip chainShakeSound;
        [SerializeField] private float soundVolume = 0.8f;
        
        private Vector3 originalPosition;
        private bool isShaking = false;
        
        /// <summary>
        /// 현재 흔들림 애니메이션이 진행 중인지 확인합니다.
        /// </summary>
        public bool IsShaking => isShaking;
        
        private void Awake()
        {
            originalPosition = transform.localPosition;
            
            // 기본 체인 흔들림 효과음 로드
            if (chainShakeSound == null)
            {
                chainShakeSound = Resources.Load<AudioClip>("Sounds/Sfx/ChainShake");
                if (chainShakeSound != null)
                {
                    Debug.Log("[ChainShakeEffect] 체인 흔들림 효과음 자동 로드됨: ChainShake");
                }
            }
        }
        
        /// <summary>
        /// 흔들림 효과를 시작합니다.
        /// </summary>
        /// <param name="duration">흔들림 지속 시간(초)</param>
        public void StartShake(float duration)
        {
            if (isShaking) return;
            
            StartCoroutine(ShakeCoroutine(duration));
        }
        
        private IEnumerator ShakeCoroutine(float duration)
        {
            isShaking = true;
            float elapsed = 0f;
            
            // 흔들림 시작 시 효과음 재생
            PlayChainShakeSound();
            
            while (elapsed < duration)
            {
                // 사인파를 사용한 자연스러운 흔들림
                float xOffset = Mathf.Sin(Time.time * shakeSpeed) * shakeIntensity;
                float yOffset = Mathf.Cos(Time.time * shakeSpeed * 0.7f) * shakeIntensity * 0.5f;
                
                transform.localPosition = originalPosition + new Vector3(xOffset, yOffset, 0f);
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            // 원래 위치로 복원
            transform.localPosition = originalPosition;
            isShaking = false;
        }
        
        private void OnDisable()
        {
            // 비활성화 시 원래 위치로 복원
            if (isShaking)
            {
                transform.localPosition = originalPosition;
                isShaking = false;
            }
        }
        
        /// <summary>
        /// 체인 흔들림 효과음을 재생합니다.
        /// </summary>
        private void PlayChainShakeSound()
        {
            if (chainShakeSound != null)
            {
                // AudioSource로 직접 재생
                var audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                }
                
                // AudioSource 설정
                audioSource.clip = chainShakeSound;
                audioSource.volume = soundVolume;
                audioSource.playOnAwake = false;
                audioSource.loop = false;
                audioSource.outputAudioMixerGroup = null; // 기본 마스터 그룹 사용
                
                // 효과음 재생
                audioSource.Play();
                Debug.Log("[ChainShakeEffect] 체인 흔들림 효과음 재생됨");
            }
            else
            {
                Debug.LogWarning("[ChainShakeEffect] 체인 흔들림 효과음이 설정되지 않았습니다!");
            }
        }
    }
}
