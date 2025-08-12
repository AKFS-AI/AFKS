using UnityEngine;
using AFKS.Core; // GameState
using AFKS.Shared.Interfaces; // InteractionType

namespace AFKS.Shared.Events
{
    /// <summary>
    /// 전역 이벤트 버스. 런타임 이벤트를 중앙에서 브로드캐스트합니다.
    /// 기존 각 매니저의 정적 이벤트와 병행 사용 가능하며, 점진 전환을 지원합니다.
    /// </summary>
    public static class EventBus
    {
        // 기존 각 매니저의 static GameEvent<T>는 내부 용도로만 사용하고,
        // 외부 브로드캐스트는 EventBus 이벤트 사용을 권장합니다.
        // 향후 마이그레이션 과정에서 중복 이벤트를 제거하기 위해, EventBus를 표준 인터페이스로 유지합니다.
        // Game lifecycle
        public static readonly GameEvent<GameState> GameStateChanged = new GameEvent<GameState>();
        public static readonly GameEvent<int> StageChanged = new GameEvent<int>();
        public static readonly GameEvent<float> GameTimeUpdated = new GameEvent<float>();

        // Audio
        public static readonly GameEvent<AudioClip> BGMChanged = new GameEvent<AudioClip>();
        public static readonly GameEvent<string> SFXPlayed = new GameEvent<string>();
        public static readonly GameEvent<float> VolumeChanged = new GameEvent<float>();

        // Interaction
        public static readonly GameEvent<string> GlobalInteraction = new GameEvent<string>();
        public static readonly GameEvent<InteractionType> InteractionTypeChanged = new GameEvent<InteractionType>();

        // Two-stage door sample
        public static readonly GameEvent<string> DoorClicked = new GameEvent<string>();
        public static readonly GameEvent<string> ChainClicked = new GameEvent<string>();
        public static readonly GameEvent<string> DoorUnlocked = new GameEvent<string>();

        // Inventory / Items
        public static readonly GameEvent InventoryChanged = new GameEvent();
        public static readonly GameEvent<string> KeyItemObtained = new GameEvent<string>();
        public static readonly GameEvent<string> KeyItemUsed = new GameEvent<string>();

        /// <summary>
        /// 씬 전환 등에서 리스너 누수를 방지하기 위해 리스너를 초기화합니다.
        /// </summary>
        public static void Reset()
        {
            GameStateChanged.RemoveAllListeners();
            StageChanged.RemoveAllListeners();
            GameTimeUpdated.RemoveAllListeners();

            BGMChanged.RemoveAllListeners();
            SFXPlayed.RemoveAllListeners();
            VolumeChanged.RemoveAllListeners();

            GlobalInteraction.RemoveAllListeners();
            InteractionTypeChanged.RemoveAllListeners();

            DoorClicked.RemoveAllListeners();
            ChainClicked.RemoveAllListeners();
            DoorUnlocked.RemoveAllListeners();

            InventoryChanged.RemoveAllListeners();
            KeyItemObtained.RemoveAllListeners();
            KeyItemUsed.RemoveAllListeners();
        }
    }
}


