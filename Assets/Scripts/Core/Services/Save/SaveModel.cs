using System;
using System.Collections.Generic;

namespace AFKS.Core.Services.Save
{
    /// <summary>
    /// 저장 데이터의 직렬화 모델. 버전 필드를 포함하여 마이그레이션을 지원합니다.
    /// </summary>
    [Serializable]
    public sealed class SaveModel
    {
        public int version;
        public string savedAtUtc;
        public string lastStageId;
        public List<string> items;
        public List<CheckpointEntry> checkpoints;
        public float volumeMaster = -1f;
        public float volumeBgm = -1f;
        public float volumeSfx = -1f;
        public float volumeAmbience = -1f;
    }

    [Serializable]
    public sealed class CheckpointEntry
    {
        public string stageId;
        public string checkpointId;
    }
}


