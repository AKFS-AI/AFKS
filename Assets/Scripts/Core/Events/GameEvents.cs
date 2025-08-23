using System;

namespace AFKS.Core.Events
{
    /// <summary>
    /// Central game event hub. Lightweight C# events to decouple features.
    /// </summary>
    public static class GameEvents
    {
        // Stage flow
        public static event Action<string> StageChangeRequested;
        public static event Action<string> StageLoaded;
        public static event Action<string> StageUnloaded;

        // Inventory (future use)
        public static event Action<string> ItemPicked;

        // Jump scare (future use)
        public static event Action<string> JumpScareTriggered;

        public static void RaiseStageChangeRequested(string targetStageId)
        {
            StageChangeRequested?.Invoke(targetStageId);
        }

        public static void RaiseStageLoaded(string stageId)
        {
            StageLoaded?.Invoke(stageId);
        }

        public static void RaiseStageUnloaded(string stageId)
        {
            StageUnloaded?.Invoke(stageId);
        }

        public static void RaiseItemPicked(string itemId)
        {
            ItemPicked?.Invoke(itemId);
        }

        public static void RaiseJumpScareTriggered(string clipId)
        {
            JumpScareTriggered?.Invoke(clipId);
        }
    }
}


