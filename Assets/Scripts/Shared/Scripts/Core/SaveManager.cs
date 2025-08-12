using System;
using System.Collections.Generic;
using UnityEngine;
using AFKS.Shared.Interfaces;

namespace AFKS.Shared.Core
{
    /// <summary>
    /// 저장 가능한 오브젝트들을 중앙에서 관리하는 세이브 매니저
    /// PlayerPrefs에 하나의 JSON 번들로 저장/로드합니다.
    /// </summary>
    public class SaveManager : BaseSingleton<SaveManager>
    {
        private const string PlayerPrefsKey = "AFKS_SaveGame";

        private readonly Dictionary<string, ISaveable> idToProvider = new Dictionary<string, ISaveable>();
        private readonly Dictionary<string, string> storageCache = new Dictionary<string, string>();
        private readonly Dictionary<string, string> variablesCache = new Dictionary<string, string>();

        protected override void OnSingletonAwake()
        {
            LoadFromStorageIntoCache();
        }

        /// <summary>
        /// 저장 대상 등록. 기존 저장 데이터가 있으면 즉시 로드합니다.
        /// </summary>
        public void Register(ISaveable provider)
        {
            if (provider == null || string.IsNullOrEmpty(provider.SaveID))
            {
                Debug.LogWarning("[SaveManager] 잘못된 저장 대상입니다.");
                return;
            }

            idToProvider[provider.SaveID] = provider;

            if (storageCache.TryGetValue(provider.SaveID, out string json) && !string.IsNullOrEmpty(json))
            {
                try
                {
                    provider.LoadSaveData(json);
                    Debug.Log($"[SaveManager] 등록 시 저장 데이터 로드: {provider.SaveID}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"[SaveManager] 로드 실패 ({provider.SaveID}): {e.Message}");
                }
            }
        }

        /// <summary>
        /// 저장 대상 등록 해제.
        /// </summary>
        public void Unregister(ISaveable provider)
        {
            if (provider == null) return;
            if (idToProvider.ContainsKey(provider.SaveID))
            {
                idToProvider.Remove(provider.SaveID);
            }
        }

        /// <summary>
        /// 모든 등록 대상 저장 후 PlayerPrefs에 반영합니다.
        /// </summary>
        public void SaveAll()
        {
            var bundle = new SaveBundle();
            foreach (var kv in idToProvider)
            {
                try
                {
                    string data = kv.Value.GetSaveData();
                    storageCache[kv.Key] = data;
                    bundle.entries.Add(new SaveEntry { id = kv.Key, data = data });
                }
                catch (Exception e)
                {
                    Debug.LogError($"[SaveManager] 저장 실패 ({kv.Key}): {e.Message}");
                }
            }

            // 변수 번들도 함께 저장
            foreach (var kv in variablesCache)
            {
                bundle.variables.Add(new SaveEntry { id = kv.Key, data = kv.Value });
            }

            string json = JsonUtility.ToJson(bundle);
            PlayerPrefs.SetString(PlayerPrefsKey, json);
            PlayerPrefs.Save();
            Debug.Log("[SaveManager] 전체 저장 완료");
        }

        /// <summary>
        /// PlayerPrefs에서 번들을 읽어 등록 대상에게 로드합니다.
        /// </summary>
        public void LoadAll()
        {
            LoadFromStorageIntoCache();

            foreach (var kv in idToProvider)
            {
                if (storageCache.TryGetValue(kv.Key, out string data))
                {
                    try
                    {
                        kv.Value.LoadSaveData(data);
                        Debug.Log($"[SaveManager] 로드 완료: {kv.Key}");
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[SaveManager] 로드 실패 ({kv.Key}): {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// 저장소에서 캐시로 로드만 수행합니다.
        /// </summary>
        private void LoadFromStorageIntoCache()
        {
            storageCache.Clear();
            variablesCache.Clear();

            string json = PlayerPrefs.GetString(PlayerPrefsKey, string.Empty);
            if (string.IsNullOrEmpty(json)) return;

            try
            {
                var bundle = JsonUtility.FromJson<SaveBundle>(json);
                if (bundle != null && bundle.entries != null)
                {
                    foreach (var entry in bundle.entries)
                    {
                        storageCache[entry.id] = entry.data;
                    }
                }

                // 변수 번들 로드 (과거 저장본에는 없을 수 있음)
                if (bundle != null && bundle.variables != null)
                {
                    foreach (var v in bundle.variables)
                    {
                        variablesCache[v.id] = v.data;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] 저장소 파싱 실패: {e.Message}");
            }
        }

        // === GAME VARIABLES API ===
        public void SetVariable(string key, string value)
        {
            if (string.IsNullOrEmpty(key)) return;
            variablesCache[key] = value ?? string.Empty;
        }

        public bool TryGetVariable(string key, out string value)
        {
            if (string.IsNullOrEmpty(key))
            {
                value = string.Empty;
                return false;
            }
            return variablesCache.TryGetValue(key, out value);
        }

        [Serializable]
        private class SaveBundle
        {
            public List<SaveEntry> entries = new List<SaveEntry>();
            public List<SaveEntry> variables = new List<SaveEntry>();
        }

        [Serializable]
        private class SaveEntry
        {
            public string id;
            public string data;
        }
    }
}


