namespace Quests
{
    /// <summary>
    /// Represents the possible states of a quest.
    /// </summary>
    public enum QuestStatus
    {
        NotStarted, // The quest hasn't been discovered or offered yet.
        Active, // The quest has been accepted and is in progress.
        Completed, // The quest has been successfully finished.
        Failed, // The quest cannot be completed successfully.
        Hidden, // The quest exists but should not be visible to the player yet (e.g., prerequisite not met).
        Tracked // The quest is actively being tracked and shown in the HUD (only one at a time ideally).
    }
}
