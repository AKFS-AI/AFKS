using UnityEngine;

namespace AFKS.Core.Services.GameState
{
    public interface IGameStateService
    {
        string CurrentStageId { get; }
        void SetCurrentStage(string stageId);
    }

    /// <summary>
    /// 현재 진행 중인 스테이지 ID 등 경량 런타임 상태를 보관합니다.
    /// </summary>
    [AddComponentMenu("AFKS/State/Game State Service")]
    public sealed class GameStateService : MonoBehaviour, IGameStateService
    {
        [SerializeField]
        [InspectorName("현재 스테이지 ID")]
        private string currentStageId;

        public string CurrentStageId => currentStageId;

        private void Awake()
        {
            AFKS.Core.Services.ServiceLocator.Register<IGameStateService>(this, overwriteExisting: true);
        }

        private void OnDestroy()
        {
            AFKS.Core.Services.ServiceLocator.Unregister<IGameStateService>();
        }

        public void SetCurrentStage(string stageId)
        {
            currentStageId = stageId;
        }
    }
}


