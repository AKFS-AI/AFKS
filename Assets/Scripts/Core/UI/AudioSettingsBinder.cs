using UnityEngine;
using UnityEngine.UI;
using AFKS.Core.Services;
using AFKS.Core.Services.Audio;

namespace AFKS.Core.UI
{
	/// <summary>
	/// 설정창의 볼륨 슬라이더를 오디오 서비스에 바인딩하고 영속화합니다.
	/// 슬라이더 값 범위: 0..4 (20% 단계)
	/// </summary>
	[AddComponentMenu("AFKS/UI/Audio Settings Binder")]
	public sealed class AudioSettingsBinder : MonoBehaviour
	{
		[SerializeField] private Slider sfxSlider;
		[SerializeField] private Slider ambienceSlider;
		[SerializeField] private Slider bgmSlider;

		private const string KEY_SFX = "Volume.SFX";
		private const string KEY_AMBIENCE = "Volume.Ambience";
		private const string KEY_BGM = "Volume.BGM";
		private const string KEY_MASTER = "Volume.Master";

		private void Awake()
		{
			// 로드 및 초기 적용 (슬라이더 UI도 로드 값으로 동기화)
			float master = Load(KEY_MASTER, 1f);
			float sfx = Load(KEY_SFX, 0.8f);
			float amb = Load(KEY_AMBIENCE, 0.6f);
			float bgm = Load(KEY_BGM, 0.6f);

			// 슬라이더를 로드 값에 맞춰 설정(이벤트 발동 없이)
			if (sfxSlider != null)
				sfxSlider.SetValueWithoutNotify(Mathf.RoundToInt(Mathf.Clamp01(sfx) * 4f));
			if (ambienceSlider != null)
				ambienceSlider.SetValueWithoutNotify(Mathf.RoundToInt(Mathf.Clamp01(amb) * 4f));
			if (bgmSlider != null)
				bgmSlider.SetValueWithoutNotify(Mathf.RoundToInt(Mathf.Clamp01(bgm) * 4f));

			Apply(master, bgm, sfx, amb);

			// 이벤트 바인딩
			if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(_ => OnSliderChanged());
			if (ambienceSlider != null) ambienceSlider.onValueChanged.AddListener(_ => OnSliderChanged());
			if (bgmSlider != null) bgmSlider.onValueChanged.AddListener(_ => OnSliderChanged());
		}

		private void OnSliderChanged()
		{
			SaveCurrentImmediately();
		}

		public void SaveCurrentImmediately()
		{
			float master = Load(KEY_MASTER, 1f);
			float sfx = SliderToVolume(sfxSlider);
			float amb = SliderToVolume(ambienceSlider);
			float bgm = SliderToVolume(bgmSlider);
			Apply(master, bgm, sfx, amb);
			Save(KEY_SFX, sfx);
			Save(KEY_AMBIENCE, amb);
			Save(KEY_BGM, bgm);
		}

		private static float SliderToVolume(Slider slider, float defaultValue = 0.6f)
		{
			if (slider == null) return defaultValue;
			return Mathf.Clamp01(slider.wholeNumbers ? slider.value / 4f : slider.value);
		}

		private static void Apply(float master, float bgm, float sfx, float ambience)
		{
			if (ServiceLocator.TryGet<IAudioService>(out var audio))
			{
				audio.SetVolume(master, bgm, sfx, ambience);
			}
		}

		private static float Load(string key, float fallback)
		{
			if (ServiceLocator.TryGet<AFKS.Core.Services.Save.ISaveService>(out var save))
			{
				return save.GetFloat(key, fallback);
			}
			return PlayerPrefs.GetFloat(key, fallback);
		}

		private static void Save(string key, float value)
		{
			if (ServiceLocator.TryGet<AFKS.Core.Services.Save.ISaveService>(out var save))
			{
				save.SetFloat(key, value);
			}
			else
			{
				PlayerPrefs.SetFloat(key, value);
				PlayerPrefs.Save();
			}
		}
	}
}


