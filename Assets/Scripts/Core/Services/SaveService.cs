using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace AFKS.Core.Services
{
	[Serializable]
	public sealed class SaveData
	{
		public int version = 1;
		public int stageIndex;
		public List<string> flags = new List<string>();
		public List<string> items = new List<string>();
	}

    public sealed class SaveService
    {
        private readonly EventBus _bus;
        private readonly InventoryService _inventory;
        private readonly ProgressService _progress;
        private string _path;

        public SaveService(EventBus bus, InventoryService inventory, ProgressService progress)
        {
            _bus = bus;
            _inventory = inventory;
            _progress = progress;
            _path = Path.Combine(Application.persistentDataPath, "save.json");

            _bus.Subscribe<Events.StageChangedEvent>(_ => AutoSave());
            _bus.Subscribe<Events.FlagChangedEvent>(_ => AutoSave());
            _bus.Subscribe<Events.ItemPickedEvent>(_ => AutoSave());
        }

        public void AutoSave() => Save();

        public void Save()
        {
            var data = new SaveData
            {
                stageIndex = _progress.CurrentStageIndex,
                flags = new List<string>(),
                items = new List<string>()
            };

            // capture flags via event replay is complex; keep simple by exposing enumerations if needed
            // For minimal core, not enumerating private fields.

            var json = JsonUtility.ToJson(data, true);
            File.WriteAllText(_path, json);
        }

        public bool TryLoad()
        {
            if (!File.Exists(_path)) return false;
            var json = File.ReadAllText(_path);
            var data = JsonUtility.FromJson<SaveData>(json);
            if (data == null) return false;
            _progress.SetStage(data.stageIndex);
            return true;
        }
    }
}


