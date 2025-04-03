// file: Scripts/Quests/TaskStatus.cs

namespace Quests
{
    /// <summary>
    /// Represents the possible states of a single task within a quest.
    /// </summary>
    public enum TaskStatus
    {
        NotStarted, // The task is not yet active (e.g., previous task not complete).
        Active, // The task is currently the objective.
        Completed, // The task has been successfully finished.
        Failed, // The task cannot be completed successfully (may fail the quest).
        Hidden // The task exists but should not be visible in the quest log yet.
    }
}
