using System;

namespace AFKS.Features.Stage
{
    /// <summary>
    /// Stage1 전용 상태머신. 뷰/컨트롤러는 콜백으로 제공하고, 전이 규칙만 이곳에서 관리합니다.
    /// </summary>
    public sealed class Stage1StateMachine
    {
        private readonly Action showChains;
        private readonly Action<bool> setChainsClickable;
        private readonly Action<bool> toggleDoorCollider;
        private readonly Action reportBackground;

        public GameplayState CurrentState { get; private set; } = GameplayState.Initial;

        public Stage1StateMachine(Action showChains,
                                  Action<bool> setChainsClickable,
                                  Action<bool> toggleDoorCollider,
                                  Action reportBackground)
        {
            this.showChains = showChains ?? (() => { });
            this.setChainsClickable = setChainsClickable ?? (_ => { });
            this.toggleDoorCollider = toggleDoorCollider ?? (_ => { });
            this.reportBackground = reportBackground ?? (() => { });
        }

        public bool CanInteractWithDoor()
        {
            return CurrentState == GameplayState.Initial || CurrentState == GameplayState.ChainUnlocked;
        }

        public bool CanInteractWithChain()
        {
            return CurrentState == GameplayState.ChainUnlocking;
        }

        public void TransitionToChainUnlocking()
        {
            if (CurrentState != GameplayState.Initial) return;
            CurrentState = GameplayState.ChainUnlocking;
            showChains();
            setChainsClickable(true);
            toggleDoorCollider(false);
        }

        public void TransitionToChainUnlocked()
        {
            if (CurrentState != GameplayState.ChainUnlocking) return;
            CurrentState = GameplayState.ChainUnlocked;
            setChainsClickable(false);
            reportBackground();
            toggleDoorCollider(true);
        }

        public void TransitionToTransitioning()
        {
            if (CurrentState != GameplayState.ChainUnlocked) return;
            CurrentState = GameplayState.Transitioning;
        }
    }
}


