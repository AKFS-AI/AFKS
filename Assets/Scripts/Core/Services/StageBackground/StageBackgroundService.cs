using UnityEngine;

namespace AFKS.Core.Services.StageBackground
{
    public interface IStageBackgroundService
    {
        string LastStageId { get; }
        Sprite LastSprite { get; }
        void ReportBackground(string stageId, Sprite sprite);
    }

    [AddComponentMenu("AFKS/Stage/Stage Background Service")]
    public sealed class StageBackgroundService : MonoBehaviour, IStageBackgroundService
    {
        [SerializeField]
        [InspectorName("마지막 스테이지 ID")] private string lastStageId;
        [SerializeField]
        [InspectorName("마지막 배경 스프라이트")] private Sprite lastSprite;

        public string LastStageId => lastStageId;
        public Sprite LastSprite => lastSprite;

        private void Awake()
        {
            AFKS.Core.Services.ServiceLocator.Register<IStageBackgroundService>(this, overwriteExisting: true);
        }

        private void OnDestroy()
        {
            AFKS.Core.Services.ServiceLocator.Unregister<IStageBackgroundService>();
        }

        public void ReportBackground(string stageId, Sprite sprite)
        {
            lastStageId = stageId;
            lastSprite = sprite;
        }
    }
}


