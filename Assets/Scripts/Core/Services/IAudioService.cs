using System.Threading.Tasks;
using UnityEngine;

namespace AFKS.Core.Services
{
	public interface IAudioService
	{
		void PlayBGM(AudioClip clip);
		Task CrossFadeBGMAsync(AudioClip nextClip, float seconds);
		void PlaySFX(AudioClip clip, float volume = 1f);
		void SetVolume(float master, float bgm, float sfx);
		void SetDynamicRange(bool enabled);
	}
}
