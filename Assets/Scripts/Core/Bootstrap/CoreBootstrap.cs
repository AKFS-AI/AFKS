using AFKS.Core.Services;
using AFKS.Core.Utils;
using UnityEngine;

namespace AFKS.Core.Bootstrap
{
	/// <summary>
	/// Core 씬 상주 부트스트랩. 최소 서비스 등록(Scene/Save).
	/// </summary>
	public sealed class CoreBootstrap : MonoBehaviour
	{
		[SerializeField] private SceneService sceneService;

		private void Awake()
		{
			if (sceneService == null)
			{
				sceneService = FindAnyObjectByType<SceneService>();
			}
			if (sceneService != null)
			{
				ServiceLocator.Register<ISceneService>(sceneService);
			}
			ServiceLocator.Register<ISaveService>(new PlayerPrefsSaveService());
			Log.Info("CoreBootstrap initialized.");
		}
	}

	/// <summary>
	/// 임시 간이 세이브 구현(PlayerPrefs). 추후 파일 JSON 저장으로 대체.
	/// </summary>
	internal sealed class PlayerPrefsSaveService : ISaveService
	{
		private const string KeyStage = "AFKS.Stage";
		private const string KeyMaster = "AFKS.Vol.Master";
		private const string KeyBgm = "AFKS.Vol.Bgm";
		private const string KeySfx = "AFKS.Vol.Sfx";
		private const string KeyDyn = "AFKS.Vol.DR";
		private const string KeyFlicker = "AFKS.Acc.Flicker";
		private const string KeyShake = "AFKS.Acc.Shake";

		public bool TryLoadAll()
		{
			// PlayerPrefs는 값이 없으면 기본값 반환
			return true;
		}

		public void SaveAll()
		{
			PlayerPrefs.Save();
		}

		public string CurrentStageId
		{
			get => PlayerPrefs.GetString(KeyStage, string.Empty);
			set => PlayerPrefs.SetString(KeyStage, value ?? string.Empty);
		}

		public float MasterVolume
		{
			get => PlayerPrefs.GetFloat(KeyMaster, 0.7f);
			set => PlayerPrefs.SetFloat(KeyMaster, Mathf.Clamp01(value));
		}

		public float BgmVolume
		{
			get => PlayerPrefs.GetFloat(KeyBgm, 0.6f);
			set => PlayerPrefs.SetFloat(KeyBgm, Mathf.Clamp01(value));
		}

		public float SfxVolume
		{
			get => PlayerPrefs.GetFloat(KeySfx, 0.7f);
			set => PlayerPrefs.SetFloat(KeySfx, Mathf.Clamp01(value));
		}

		public bool DynamicRange
		{
			get => PlayerPrefs.GetInt(KeyDyn, 0) != 0;
			set => PlayerPrefs.SetInt(KeyDyn, value ? 1 : 0);
		}

		public bool ReduceFlicker
		{
			get => PlayerPrefs.GetInt(KeyFlicker, 1) != 0;
			set => PlayerPrefs.SetInt(KeyFlicker, value ? 1 : 0);
		}

		public bool ReduceShake
		{
			get => PlayerPrefs.GetInt(KeyShake, 1) != 0;
			set => PlayerPrefs.SetInt(KeyShake, value ? 1 : 0);
		}
	}
}
