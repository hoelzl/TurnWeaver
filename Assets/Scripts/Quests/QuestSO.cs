// file: Scripts/Quests/QuestSO.cs
using System.Collections.Generic;
using UnityEngine;

namespace Quests
{
    /// <summary>
    /// ScriptableObject defining a single quest.
    /// </summary>
    [CreateAssetMenu(fileName = "New Quest", menuName = "Quests/Quest Definition", order = 0)]
    public class QuestSO : ScriptableObject
    {
        [Header("Identification")]
        [Tooltip("Unique identifier for this quest (for scripting/dialogue).")]
        [SerializeField] private string questShortName;

        [Tooltip("The user-visible name of the quest.")]
        [SerializeField] private string questName;

        // Add other descriptive fields as needed (e.g., Description, Recommended Level)

        [Header("Tasks")]
        [Tooltip("The sequence of tasks required to complete this quest.")]
        [SerializeField] private List<QuestTask> tasks = new List<QuestTask>();

        // Public accessors
        public string QuestShortName => questShortName;
        public string QuestName => questName;
        public IReadOnlyList<QuestTask> Tasks => tasks.AsReadOnly();

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(questShortName))
            {
                Debug.LogError($"Quest '{questName}' is missing a unique Short Name.", this);
            }

            // Validate tasks and check for duplicate short names within this quest
            HashSet<string> taskShortNames = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
            foreach (var task in tasks)
            {
                 task.Validate(); // Basic validation within the task
                 if (!string.IsNullOrWhiteSpace(task.TaskShortName))
                 {
                    if (!taskShortNames.Add(task.TaskShortName))
                    {
                        Debug.LogError($"Quest '{questName}' has duplicate Task Short Name: '{task.TaskShortName}'.", this);
                    }
                 }
            }
        }
    }
}
