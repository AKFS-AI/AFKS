using System.Collections;
using UnityEngine;

namespace AFKS.Core.Services.Audio
{
    /// <summary>
    /// 배경음/효과음을 재생하고, 볼륨을 제어하는 오디오 서비스 인터페이스입니다.
    /// </summary>
    public interface IAudioService
    {
        void PlayBGM(AudioClip clip, float volume = 1f, bool loop = true);
        IEnumerator CrossFadeBGMAsync(AudioClip nextClip, float seconds = 0.5f, float nextVolume = 1f, bool loop = true);
        void PlaySFX(AudioClip clip, float volume = 1f);
        void SetVolume(float master = 1f, float bgm = 1f, float sfx = 1f);
    }
}


