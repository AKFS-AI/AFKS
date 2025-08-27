using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using AFKS.Core.Services.Save;

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
        private ISaveSerializer serializer;
        private ISaveStorage storage;
        #endregion

        #region 유니티 수명주기
        private void Awake()
        {
            AFKS.Core.Services.ServiceLocator.Register<ISaveService>(this, overwriteExisting: true);
            serializer = new JsonUnitySaveSerializer();
            storage = new FileSystemSaveStorage();
            // 부팅 시 저장 파일을 먼저 로드해(오디오 설정 포함) 초기값이 튀지 않도록 함
            try { TryLoadAll(); }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveService] 초기 로드 실패: {e.Message}");
            }
        }

        private void OnApplicationQuit()
        {
            if (autoSaveOnQuit)
            {
                try
                {
                    // 메뉴에서는 저장하지 않습니다
                    if (AFKS.Core.Services.ServiceLocator.TryGet<AFKS.Core.Services.GameState.IGameStateService>(out var gs))
                    {
                        if (string.Equals(gs.CurrentStageId, "Menu", StringComparison.Ordinal)) return;
                    }
                    SaveAll();
                }
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
            // 파일 존재만으로 판단하지 않고, 유효한 SaveModel이며 lastStageId가 존재할 때만 true
            try
            {
                if (!File.Exists(SavePath)) return false;
                string json = File.ReadAllText(SavePath);
                if (string.IsNullOrWhiteSpace(json)) return false;
                var model = JsonUtility.FromJson<SaveModel>(json);
                if (model == null) return false;
                return !string.IsNullOrEmpty(model.lastStageId) && model.version >= 1;
            }
            catch
            {
                return false;
            }
        }

        public void SaveAll()
        {
            var model = BuildSaveModel();
            string json = serializer.Serialize(model, prettyPrint: true);
            if (!Directory.Exists(SaveDirectory)) Directory.CreateDirectory(SaveDirectory);

            // 원자적 저장 + 백업
            string backupPath = SavePath + ".bak";
            try
            {
                if (storage.Exists(SavePath))
                {
                    // 백업 생성(덮어쓰기 허용)
                    File.Copy(SavePath, backupPath, overwrite: true);
                }
                storage.WriteAllTextAtomic(SavePath, json);
            }
            finally { }

            Debug.Log($"[SaveService] 저장 완료 → {SavePath}");
        }

        public bool TryLoadAll()
        {
            try
            {
                if (!storage.Exists(SavePath)) return false;
                string json = storage.ReadAllText(SavePath);
                var model = serializer.Deserialize(json);
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

        public void SetFloat(string key, float value)
        {
            PlayerPrefs.SetFloat(key, value);
            PlayerPrefs.Save();
        }

        public float GetFloat(string key, float defaultValue = 0f)
        {
            return PlayerPrefs.GetFloat(key, defaultValue);
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
            // JSON 저장 파일 및 백업 파일도 함께 제거
            storage.DeleteFile(SavePath);
            storage.DeleteFile(SavePath + ".bak");
        }

        public void ResetProgress(bool keepSettings = true)
        {
            // 진행 관련 키 초기화
            PlayerPrefs.DeleteKey("Stage_Stage1.Checkpoint");
            for (int i = 1; i <= 10; i++)
            {
                PlayerPrefs.DeleteKey($"Stage_Stage{i}.Checkpoint");
            }
            // ChainUnlocked 상태도 초기화
            PlayerPrefs.DeleteKey("Stage1.ChainUnlocked");

            // 세이브 파일 제거(설정은 PlayerPrefs라 유지)
            storage.DeleteFile(SavePath);
            storage.DeleteFile(SavePath + ".bak");

            if (!keepSettings)
            {
                // 오디오 설정까지 초기화
                PlayerPrefs.DeleteKey("Volume.Master");
                PlayerPrefs.DeleteKey("Volume.BGM");
                PlayerPrefs.DeleteKey("Volume.SFX");
                PlayerPrefs.DeleteKey("Volume.Ambience");
            }
            PlayerPrefs.Save();
        }

        // 진행도(체크포인트) 저장/조회: Stage_<ID>.Checkpoint = <checkpointId>
        public void SetStageCheckpoint(string stageId, string checkpointId)
        {
            if (string.IsNullOrEmpty(stageId)) return;
            PlayerPrefs.SetString($"Stage_{stageId}.Checkpoint", checkpointId ?? "");
            PlayerPrefs.Save();
        }

        public string GetStageCheckpoint(string stageId)
        {
            if (string.IsNullOrEmpty(stageId)) return "";
            return PlayerPrefs.GetString($"Stage_{stageId}.Checkpoint", "");
        }
        #endregion

        #region 내부 메서드
        private SaveModel BuildSaveModel()
        {
            var model = new SaveModel
            {
                version = 1,
                savedAtUtc = DateTime.UtcNow.ToString("o"),
                items = new List<string>(),
                checkpoints = new List<CheckpointEntry>()
            };

            if (AFKS.Core.Services.ServiceLocator.TryGet<AFKS.Core.Services.Inventory.IInventoryService>(out var inv))
            {
                foreach (var id in inv.Items) model.items.Add(id);
            }

            if (AFKS.Core.Services.ServiceLocator.TryGet<AFKS.Core.Services.GameState.IGameStateService>(out var gs))
            {
                // 메뉴는 진행 저장 대상 아님
                var sid = gs.CurrentStageId;
                if (!string.Equals(sid, "Menu", StringComparison.Ordinal) && !string.IsNullOrEmpty(sid))
                {
                    model.lastStageId = sid;
                }
            }

            // 체크포인트(현재 간단 구현: 존재하면 수집)
            for (int i = 1; i <= 10; i++)
            {
                string sid = $"Stage{i}";
                string cp = GetStageCheckpoint(sid);
                if (!string.IsNullOrEmpty(cp))
                {
                    model.checkpoints.Add(new CheckpointEntry { stageId = sid, checkpointId = cp });
                }
            }

            // 오디오 설정(슬라이더) 저장: PlayerPrefs/키값에서 수집하여 JSON에도 포함
            model.volumeMaster = GetFloat("Volume.Master", 1f);
            model.volumeBgm = GetFloat("Volume.BGM", 0.6f);
            model.volumeSfx = GetFloat("Volume.SFX", 0.8f);
            model.volumeAmbience = GetFloat("Volume.Ambience", 0.6f);

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

            // 체크포인트 복원
            if (model.checkpoints != null)
            {
                for (int i = 0; i < model.checkpoints.Count; i++)
                {
                    var e = model.checkpoints[i];
                    SetStageCheckpoint(e.stageId, e.checkpointId);
                }
            }

            // 오디오 설정 복원: 키값에도 반영 후 즉시 적용
            if (model.volumeMaster >= 0f || model.volumeBgm >= 0f || model.volumeSfx >= 0f || model.volumeAmbience >= 0f)
            {
                float master = model.volumeMaster >= 0f ? model.volumeMaster : GetFloat("Volume.Master", 1f);
                float bgm = model.volumeBgm >= 0f ? model.volumeBgm : GetFloat("Volume.BGM", 0.6f);
                float sfx = model.volumeSfx >= 0f ? model.volumeSfx : GetFloat("Volume.SFX", 0.8f);
                float amb = model.volumeAmbience >= 0f ? model.volumeAmbience : GetFloat("Volume.Ambience", 0.6f);

                SetFloat("Volume.Master", master);
                SetFloat("Volume.BGM", bgm);
                SetFloat("Volume.SFX", sfx);
                SetFloat("Volume.Ambience", amb);

                if (AFKS.Core.Services.ServiceLocator.TryGet<AFKS.Core.Services.Audio.IAudioService>(out var audio))
                {
                    audio.SetVolume(master, bgm, sfx, amb);
                }
            }
        }
        #endregion

        #region 데이터 모델
        // 모델 타입은 SaveModel.cs로 이동
        #endregion
    }
}


