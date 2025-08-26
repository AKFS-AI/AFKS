using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace AFKS.Core.Services.Save
{
    /// <summary>
    /// JSON 파일 기반 저장 서비스 구현. 인벤토리/진행(스테이지 ID)을 직렬화하여 저장/로드합니다.
    /// 파일 위치: Application.persistentDataPath/AFKS/save.json
    /// </summary>
    [AddComponentMenu("AFKS/Save/Save Service")]
    public sealed class SaveService : MonoBehaviour, ISaveService
    {
        #region 직렬화 필드
        [SerializeField]
        [InspectorName("파일 이름")]
        [Tooltip("저장 파일 이름(확장자 포함). 기본값: save.json")] 
        private string fileName = "save.json";

        [SerializeField]
        [InspectorName("자동 저장(종료 시)")]
        private bool autoSaveOnQuit = true;
        #endregion

        #region 내부 상태
        private string SaveDirectory => Path.Combine(Application.persistentDataPath, "AFKS");
        private string SavePath => Path.Combine(SaveDirectory, string.IsNullOrEmpty(fileName) ? "save.json" : fileName);
        #endregion

        #region 유니티 수명주기
        private void Awake()
        {
            AFKS.Core.Services.ServiceLocator.Register<ISaveService>(this, overwriteExisting: true);
        }

        private void OnApplicationQuit()
        {
            if (autoSaveOnQuit)
            {
                try { SaveAll(); }
                catch (Exception e) { Debug.LogWarning($"[SaveService] AutoSave 실패: {e.Message}"); }
            }
        }

        private void OnDestroy()
        {
            AFKS.Core.Services.ServiceLocator.Unregister<ISaveService>();
        }
        #endregion

        #region 공개 API
        public bool HasAnySave()
        {
            return File.Exists(SavePath);
        }

        public void SaveAll()
        {
            var model = BuildSaveModel();
            string json = JsonUtility.ToJson(model, prettyPrint: true);
            if (!Directory.Exists(SaveDirectory)) Directory.CreateDirectory(SaveDirectory);
            File.WriteAllText(SavePath, json);
            Debug.Log($"[SaveService] 저장 완료 → {SavePath}");
        }

        public bool TryLoadAll()
        {
            try
            {
                if (!File.Exists(SavePath)) return false;
                string json = File.ReadAllText(SavePath);
                var model = JsonUtility.FromJson<SaveModel>(json);
                if (model == null) return false;
                ApplySaveModel(model);
                Debug.Log($"[SaveService] 로드 완료 ← {SavePath}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveService] 로드 실패: {e.Message}");
                return false;
            }
        }

        public void SetBool(string key, bool value)
        {
            PlayerPrefs.SetInt(key, value ? 1 : 0);
            PlayerPrefs.Save();
        }

        public bool GetBool(string key, bool defaultValue = false)
        {
            return PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) == 1;
        }

        public void SetString(string key, string value)
        {
            PlayerPrefs.SetString(key, value);
            PlayerPrefs.Save();
        }

        public string GetString(string key, string defaultValue = "")
        {
            return PlayerPrefs.GetString(key, defaultValue);
        }

        public void SetInt(string key, int value)
        {
            PlayerPrefs.SetInt(key, value);
            PlayerPrefs.Save();
        }

        public int GetInt(string key, int defaultValue = 0)
        {
            return PlayerPrefs.GetInt(key, defaultValue);
        }

        public void DeleteKey(string key)
        {
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
        }

        public void DeleteAll()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }
        #endregion

        #region 내부 메서드
        private SaveModel BuildSaveModel()
        {
            var model = new SaveModel
            {
                version = 1,
                savedAtUtc = DateTime.UtcNow.ToString("o"),
                items = new List<string>()
            };

            if (AFKS.Core.Services.ServiceLocator.TryGet<AFKS.Core.Services.Inventory.IInventoryService>(out var inv))
            {
                foreach (var id in inv.Items) model.items.Add(id);
            }

            if (AFKS.Core.Services.ServiceLocator.TryGet<AFKS.Core.Services.GameState.IGameStateService>(out var gs))
            {
                model.lastStageId = gs.CurrentStageId;
            }

            return model;
        }

        private void ApplySaveModel(SaveModel model)
        {
            if (AFKS.Core.Services.ServiceLocator.TryGet<AFKS.Core.Services.Inventory.IInventoryService>(out var inv))
            {
                // 인벤토리를 초기화 하는 API가 없으므로, 간단 재구성: 현재 항목 제거 후 추가
                // 안전을 위해 새 인스턴스로 대체하는 대신, 필요한 항목만 채움(미구현 항목은 무시)
                // 여기서는 단순히 저장된 항목들을 추가하는 방식으로 동작
                foreach (var id in model.items) inv.Add(id);
            }

            if (AFKS.Core.Services.ServiceLocator.TryGet<AFKS.Core.Services.GameState.IGameStateService>(out var gs))
            {
                if (!string.IsNullOrEmpty(model.lastStageId)) gs.SetCurrentStage(model.lastStageId);
            }
        }
        #endregion

        #region 데이터 모델
        [Serializable]
        private sealed class SaveModel
        {
            public int version;
            public string savedAtUtc;
            public string lastStageId;
            public List<string> items;
        }
        #endregion
    }
}


