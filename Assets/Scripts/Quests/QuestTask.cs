// file: Scripts/Quests/QuestTask.cs

using System;
using UnityEngine;

namespace Quests
{
    /// <summary>
    /// Represents a single task within a quest. Defined within a QuestSO.
    /// </summary>
    [Serializable]
    public class QuestTask
    {
        [Tooltip("Unique identifier for this task within its parent quest (for scripting/dialogue).")]
        [SerializeField] private string taskShortName;

        [Tooltip("The user-visible name or description of the task.")]
        [SerializeField] private string taskName;

        // Note: Runtime status is NOT stored here. It's managed by QuestManager.

        public string TaskShortName => taskShortName;
        public string TaskName => taskName;

        // Validate that short name is provided
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(taskShortName))
            {
                Debug.LogError($"Quest Task '{taskName}' is missing a unique Short Name.",
                    null); // Can't pass context here easily
            }
        }
    }
}
