using System.Collections;
using UnityEngine;

namespace AFKS.Core.Services.Audio
{
    /// <summary>
    /// 간단한 BGM 크로스페이드와 SFX 재생을 제공하는 오디오 서비스.
    /// 실프로덕션에서는 AudioMixer 파라미터 연동/풀링 등을 확장하세요.
    /// </summary>
    [AddComponentMenu("AFKS/Audio/Audio Service")]
    public sealed class AudioService : MonoBehaviour, IAudioService
    {
        #region 필드
        [SerializeField] private AudioSource bgmA;
        [SerializeField] private AudioSource bgmB;
        [SerializeField] private AudioSource sfx;
        [SerializeField] private AudioSource ambience;

        private bool usingA = true;
        #endregion

        #region 유니티 수명주기
        private void Awake()
        {
            if (bgmA == null) bgmA = gameObject.AddComponent<AudioSource>();
            if (bgmB == null) bgmB = gameObject.AddComponent<AudioSource>();
            if (sfx == null) sfx = gameObject.AddComponent<AudioSource>();
            if (ambience == null) ambience = gameObject.AddComponent<AudioSource>();

            bgmA.loop = true; bgmB.loop = true;
            AFKS.Core.Services.ServiceLocator.Register<IAudioService>(this, overwriteExisting: true);
        }
        #endregion

        #region 공개 API
        public void PlayBGM(AudioClip clip, float volume = 1f, bool loop = true)
        {
            var src = usingA ? bgmA : bgmB;
            src.clip = clip;
            src.volume = volume;
            src.loop = loop;
            src.Play();
        }

        public IEnumerator CrossFadeBGMAsync(AudioClip nextClip, float seconds = 0.5f, float nextVolume = 1f, bool loop = true)
        {
            var from = usingA ? bgmA : bgmB;
            var to = usingA ? bgmB : bgmA;
            usingA = !usingA;

            to.clip = nextClip;
            to.volume = 0f;
            to.loop = loop;
            to.Play();

            float t = 0f;
            float fromStart = from.volume;
            while (t < seconds)
            {
                t += Time.unscaledDeltaTime;
                float p = seconds > 0 ? Mathf.Clamp01(t / seconds) : 1f;
                from.volume = Mathf.Lerp(fromStart, 0f, p);
                to.volume = Mathf.Lerp(0f, nextVolume, p);
                yield return null;
            }
            from.Stop();
            from.volume = fromStart;
        }

        public void PlaySFX(AudioClip clip, float volume = 1f)
        {
            if (clip == null) return;
            sfx.PlayOneShot(clip, volume);
        }

        public void SetVolume(float master = 1f, float bgm = 1f, float sfxVolume = 1f, float ambienceVolume = 1f)
        {
            AudioListener.volume = Mathf.Clamp01(master);
            bgmA.volume = Mathf.Clamp01(bgm);
            bgmB.volume = Mathf.Clamp01(bgm);
            sfx.volume = Mathf.Clamp01(sfxVolume);
            if (ambience != null) ambience.volume = Mathf.Clamp01(ambienceVolume);
        }

        public void StopBGM(float fadeSeconds = 0.3f)
        {
            var from = usingA ? bgmA : bgmB;
            if (!from.isPlaying || fadeSeconds <= 0f)
            {
                from.Stop();
                return;
            }
            StartCoroutine(FadeOutThenStop(from, fadeSeconds));
        }

        private IEnumerator FadeOutThenStop(AudioSource src, float seconds)
        {
            float t = 0f;
            float start = src.volume;
            while (t < seconds)
            {
                t += Time.unscaledDeltaTime;
                float p = seconds > 0 ? Mathf.Clamp01(t / seconds) : 1f;
                src.volume = Mathf.Lerp(start, 0f, p);
                yield return null;
            }
            src.Stop();
            src.volume = start;
        }
        #endregion

        #region 유니티 수명주기(종료)
        private void OnDestroy()
        {
            AFKS.Core.Services.ServiceLocator.Unregister<IAudioService>();
        }
        #endregion
    }
}


