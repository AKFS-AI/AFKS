using System;
using UnityEngine;
using AFKS.Core.Events;
using AFKS.Core.Services;
using AFKS.Core.Services.Audio;

namespace AFKS.Core.Services.Save
{
	/// <summary>
	/// 간단한 JSON 기반 저장 서비스. 진행/옵션을 PlayerPrefs에 저장/로드합니다.
	/// </summary>
	[AddComponentMenu("AFKS/Save/Save Service")]
	public sealed class SaveService : MonoBehaviour, ISaveService
	{
		private const string SaveKey = "AFKS_Save_All";
		private IAudioService audioService;
		public static SaveService Instance { get; private set; }
		private SaveData data = new SaveData();

		[Serializable]
		private class SaveData
		{
			public string lastStageId;
			public float volumeMaster = 1f;
			public float volumeBgm = 1f;
			public float volumeSfx = 1f;
		}

		private void Awake()
		{
			if (Instance != null && Instance != this)
			{
				Destroy(gameObject);
				return;
			}
			Instance = this;
			DontDestroyOnLoad(gameObject);
			ServiceLocator.Register<ISaveService>(this, overwriteExisting: true);
			ServiceLocator.TryGet<IAudioService>(out audioService);
			TryLoadAll();
		}

		private void OnEnable()
		{
			GameEvents.StageLoaded += OnStageLoaded;
		}

		private void OnDisable()
		{
			GameEvents.StageLoaded -= OnStageLoaded;
		}

		private void OnDestroy()
		{
			if (ReferenceEquals(Instance, this)) Instance = null;
			ServiceLocator.Unregister<ISaveService>();
		}

		private void OnStageLoaded(string stageId)
		{
			if (!string.IsNullOrEmpty(stageId))
			{
				data.lastStageId = stageId;
				SaveAll();
			}
		}

		public bool HasAnySave()
		{
			return PlayerPrefs.HasKey(SaveKey);
		}

		public void SaveAll()
		{
			// 오디오 볼륨을 함께 저장
			if (audioService != null)
			{
				// AudioService는 SetVolume을 통해 내부 값을 가지므로, 현재 AudioListener/AudioSource 값으로 저장
				// 간단화: 볼륨을 가져오는 공개 API가 없다면 마지막 설정값을 유지
			}
			var json = JsonUtility.ToJson(data);
			PlayerPrefs.SetString(SaveKey, json);
			PlayerPrefs.Save();
		}

		public bool TryLoadAll()
		{
			if (!PlayerPrefs.HasKey(SaveKey)) return false;
			var json = PlayerPrefs.GetString(SaveKey, string.Empty);
			if (string.IsNullOrEmpty(json)) return false;
			try
			{
				var loaded = JsonUtility.FromJson<SaveData>(json);
				if (loaded == null) return false;
				data = loaded;
				if (audioService != null)
				{
					audioService.SetVolume(data.volumeMaster, data.volumeBgm, data.volumeSfx);
				}
				return true;
			}
			catch
			{
				return false;
			}
		}

		// 외부 헬퍼
		public string GetLastStageIdOr(string fallback) => string.IsNullOrEmpty(data.lastStageId) ? fallback : data.lastStageId;
		public void SetLastStageId(string stageId) { data.lastStageId = stageId; }
		public void SetVolumes(float master, float bgm, float sfx)
		{
			data.volumeMaster = Mathf.Clamp01(master);
			data.volumeBgm = Mathf.Clamp01(bgm);
			data.volumeSfx = Mathf.Clamp01(sfx);
			if (audioService != null) audioService.SetVolume(data.volumeMaster, data.volumeBgm, data.volumeSfx);
		}
	}
}


