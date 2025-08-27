using System.Collections.Generic;
using UnityEngine;
using AFKS.Features.Stage.Events;

namespace AFKS.Features.Stage
{
    /// <summary>
    /// 특정 스테이지의 이벤트 시퀀스를 데이터로 보관하는 ScriptableObject.
    /// </summary>
    [CreateAssetMenu(fileName = "Stage Definition", menuName = "AFKS/Stage/Stage Definition")]
    public sealed class StageDefinition : ScriptableObject
    {
        [Header("스테이지 정보")]
        [SerializeField] private string stageId = "Stage1";
        [SerializeField] private int version = 1;

        [Header("이벤트 시퀀스")]
        [SerializeField] private List<StageEvent> events = new List<StageEvent>();

        public string StageId => stageId;
        public int Version => version;
        public List<StageEvent> Events => events;

        public void Set(string id, IEnumerable<StageEvent> seq)
        {
            stageId = id;
            events.Clear();
            if (seq != null) events.AddRange(seq);
        }
    }
}


