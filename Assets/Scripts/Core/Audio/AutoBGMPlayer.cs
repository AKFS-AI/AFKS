using UnityEngine;
using AFKS.Core.Services;
using AFKS.Core.Services.Audio;
using AFKS.Core.Services.Save;

namespace AFKS.Core.Audio
{
    /// <summary>
    /// 씬 시작 시 배경음을 자동으로 재생합니다.
    /// AudioService가 있으면 그 경로로, 없으면 로컬 AudioSource로 재생합니다.
    /// </summary>
    [AddComponentMenu("AFKS/Audio/Auto BGM Player")]
    public sealed class AutoBGMPlayer : MonoBehaviour
    {
        [Header("설정")]
        [SerializeField] private AudioClip bgmClip;
        [SerializeField] [Range(0f, 1f)] private float volume = 0.6f;
        [SerializeField] private bool loop = true;
        [SerializeField] private bool useCrossFade = true;
        [SerializeField] [Range(0f, 3f)] private float crossFadeSeconds = 0.5f;
        [SerializeField] private bool stopOnDisable = false;

        private void Start()
        {
            if (bgmClip == null) return;

            // 저장된 BGM 볼륨을 우선 적용하여 0.0인 경우 초기 재생이 들리지 않도록 합니다.
            float savedBgm = GetSavedFloat("Volume.BGM", volume);

            if (ServiceLocator.TryGet<IAudioService>(out var audio))
            {
                if (useCrossFade)
                {
                    StartCoroutine(audio.CrossFadeBGMAsync(bgmClip, crossFadeSeconds, savedBgm, loop));
                }
                else
                {
                    audio.PlayBGM(bgmClip, savedBgm, loop);
                }
                return;
            }

            // 폴백: 로컬 AudioSource로 재생
            var src = gameObject.GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
            src.clip = bgmClip;
            src.loop = loop;
            src.volume = savedBgm;
            src.playOnAwake = true;
            src.Play();
        }

        private void OnDisable()
        {
            if (!stopOnDisable) return;
            if (ServiceLocator.TryGet<IAudioService>(out var audio))
            {
                audio.StopBGM(0.3f);
            }
        }

        public void SetClip(AudioClip clip)
        {
            bgmClip = clip;
        }

        private static float GetSavedFloat(string key, float fallback)
        {
            if (ServiceLocator.TryGet<ISaveService>(out var save))
            {
                return Mathf.Clamp01(save.GetFloat(key, fallback));
            }
            return Mathf.Clamp01(PlayerPrefs.GetFloat(key, fallback));
        }
    }
}


