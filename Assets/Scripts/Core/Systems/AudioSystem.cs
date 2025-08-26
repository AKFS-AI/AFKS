using UnityEngine;
using AFKS.Core.Services;

namespace AFKS.Core.Systems
{
	public sealed class AudioSystem : MonoBehaviour
	{
		private EventBus _bus;
		private AudioSource _ambient;
		private AudioSource _sfx;
		private AFKS.Core.Data.AudioDatabase _db;

		public void Initialize(EventBus bus)
		{
			_bus = bus;
			_ambient = gameObject.AddComponent<AudioSource>();
			_ambient.loop = true;
			_sfx = gameObject.AddComponent<AudioSource>();
		}

		public void PlayAmbient(AudioClip clip)
		{
			if (_ambient.clip == clip && _ambient.isPlaying) return;
			_ambient.clip = clip;
			if (clip != null) _ambient.Play();
			else _ambient.Stop();
		}

		public void PlaySfx(AudioClip clip)
		{
			if (clip == null) return;
			_sfx.PlayOneShot(clip);
		}

		public void SetDatabase(AFKS.Core.Data.AudioDatabase db)
		{
			_db = db;
			if (_bus != null)
			{
				_bus.Subscribe<AFKS.Core.Events.PlaySfxEvent>(e =>
				{
					var c = _db != null ? _db.Find(e.sfxId) : null;
					if (c != null) PlaySfx(c);
				});
			}
		}
	}
}


