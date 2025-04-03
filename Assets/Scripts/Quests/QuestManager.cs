// file: Scripts/Quests/QuestManager.cs

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Quests
{
    /// <summary>
    /// Manages the runtime state of all quests and tasks.
    /// </summary>
    public class QuestManager : MonoBehaviour
    {
        public static QuestManager Instance { get; private set; }

        [SerializeField] private QuestSO[] quests;

        // Stores the runtime status of each known quest
        private readonly Dictionary<string, QuestStatus> _questStates = new(StringComparer.OrdinalIgnoreCase);

        // Stores the runtime status of each task within each quest
        // Key: QuestShortName, Value: Dictionary<TaskShortName, TaskStatus>
        private readonly Dictionary<string, Dictionary<string, TaskStatus>> _taskStates =
            new(StringComparer.OrdinalIgnoreCase);

        // TODO: Load quest definitions (e.g., all QuestSO assets) into a lookup dictionary
        private readonly Dictionary<string, QuestSO> _questDefinitions = new(StringComparer.OrdinalIgnoreCase);

        public event Action<string> OnQuestStatusChanged; // Parameter: QuestShortName
        public event Action<string, string> OnTaskStatusChanged; // Parameters: QuestShortName, TaskShortName

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            // DontDestroyOnLoad(gameObject); // Optional persistence

            LoadQuestDefinitions();
        }

        private void LoadQuestDefinitions()
        {
            _questDefinitions.Clear();
            foreach (var quest in quests)
            {
                if (_questDefinitions.TryAdd(quest.QuestShortName, quest))
                {
                    // Initialize state if not already present (e.g., from save game)
                    _questStates.TryAdd(quest.QuestShortName, QuestStatus.NotStarted);
                    if (!_taskStates.ContainsKey(quest.QuestShortName))
                    {
                        _taskStates.Add(quest.QuestShortName,
                            new Dictionary<string, TaskStatus>(StringComparer.OrdinalIgnoreCase));
                        foreach (QuestTask task in quest.Tasks)
                        {
                            _taskStates[quest.QuestShortName].Add(task.TaskShortName, TaskStatus.NotStarted);
                        }
                    }
                }
                else
                {
                    Debug.LogWarning(
                        $"Duplicate Quest Short Name '{quest.QuestShortName}' found. Ignoring duplicate '{quest.name}'.");
                }
            }
            Debug.Log($"QuestManager loaded {_questDefinitions.Count} quest definitions.");
        }

        // --- Status Query Methods ---

        public QuestStatus GetQuestStatus(string questShortName)
        {
            _questStates.TryGetValue(questShortName, out QuestStatus status);
            return status; // Defaults to NotStarted (0) if not found
        }

        public TaskStatus GetTaskStatus(string questShortName, string taskShortName)
        {
            if (_taskStates.TryGetValue(questShortName, out var tasks))
            {
                if (tasks.TryGetValue(taskShortName, out TaskStatus status))
                {
                    return status;
                }
            }
            return TaskStatus.NotStarted; // Default if quest or task not found
        }

        // --- Status Setting Methods ---

        public bool SetQuestStatus(string questShortName, QuestStatus newStatus)
        {
            if (!_questDefinitions.ContainsKey(questShortName))
            {
                Debug.LogWarning($"QuestManager: Cannot set status for unknown quest '{questShortName}'.");
                return false;
            }

            QuestStatus currentStatus = GetQuestStatus(questShortName);
            if (currentStatus != newStatus)
            {
                _questStates[questShortName] = newStatus;
                Debug.Log($"Quest '{questShortName}' status changed to {newStatus}");
                OnQuestStatusChanged?.Invoke(questShortName);

                // Basic logic: If quest becomes active, set the first task active (if not already completed/failed)
                if (newStatus == QuestStatus.Active && currentStatus == QuestStatus.NotStarted)
                {
                    QuestSO quest = _questDefinitions[questShortName];
                    if (quest.Tasks.Count > 0)
                    {
                        QuestTask firstTask = quest.Tasks[0];
                        if (GetTaskStatus(questShortName, firstTask.TaskShortName) == TaskStatus.NotStarted)
                        {
                            SetTaskStatus(questShortName, firstTask.TaskShortName, TaskStatus.Active);
                        }
                    }
                }
                // Add more logic here if needed (e.g., failing tasks if quest fails)

                return true;
            }
            return false; // Status was already set to this value
        }

        public bool SetTaskStatus(string questShortName, string taskShortName, TaskStatus newStatus)
        {
            if (!_taskStates.TryGetValue(questShortName, out var tasks))
            {
                Debug.LogWarning($"QuestManager: Cannot set task status for unknown quest '{questShortName}'.");
                return false;
            }
            if (!tasks.TryGetValue(taskShortName, out TaskStatus currentStatus))
            {
                Debug.LogWarning(
                    $"QuestManager: Cannot set status for unknown task '{taskShortName}' in quest '{questShortName}'.");
                return false;
            }

            if (currentStatus != newStatus)
            {
                tasks[taskShortName] = newStatus;
                Debug.Log($"Task '{taskShortName}' in Quest '{questShortName}' status changed to {newStatus}");
                OnTaskStatusChanged?.Invoke(questShortName, taskShortName);

                // Add logic here, e.g., auto-activate next task on completion
                if (newStatus == TaskStatus.Completed)
                {
                    // TODO: Make invocation of this function configurable. Not sure where, though...
                    // ActivateNextTask(questShortName, taskShortName);
                }
                // Add logic for quest completion/failure based on task status if desired

                return true;
            }
            return false; // Status unchanged
        }

        // Helper to activate the next task in sequence
        private void ActivateNextTask(string questShortName, string completedTaskShortName)
        {
            if (!_questDefinitions.TryGetValue(questShortName, out QuestSO quest)) return;

            var taskList = quest.Tasks;
            for (int i = 0; i < taskList.Count - 1; i++) // Iterate up to the second to last task
            {
                if (taskList[i].TaskShortName.Equals(completedTaskShortName, StringComparison.OrdinalIgnoreCase))
                {
                    QuestTask nextTask = taskList[i + 1];
                    if (GetTaskStatus(questShortName, nextTask.TaskShortName) == TaskStatus.NotStarted)
                    {
                        SetTaskStatus(questShortName, nextTask.TaskShortName, TaskStatus.Active);
                        break; // Stop after activating the next task
                    }
                }
            }
            // Optional: Check if all tasks are complete after the last one finishes
            if (taskList.Count > 0 && taskList[taskList.Count - 1].TaskShortName
                    .Equals(completedTaskShortName, StringComparison.OrdinalIgnoreCase))
            {
                if (AreAllTasksComplete(questShortName))
                {
                    SetQuestStatus(questShortName, QuestStatus.Completed);
                }
            }
        }

        // Helper to check if all tasks for a quest are complete
        private bool AreAllTasksComplete(string questShortName)
        {
            if (!_questDefinitions.TryGetValue(questShortName, out QuestSO quest)) return false;
            if (!_taskStates.TryGetValue(questShortName, out var tasks)) return false;

            foreach (var taskDef in quest.Tasks)
            {
                if (!tasks.TryGetValue(taskDef.TaskShortName, out TaskStatus status) || status != TaskStatus.Completed)
                {
                    return false; // Found a task not completed
                }
            }
            return true; // All tasks are complete
        }


        // --- Getters for UI ---

        public IEnumerable<QuestSO> GetAllQuests()
        {
            return _questDefinitions.Values;
        }

        public QuestSO GetQuestDefinition(string questShortName)
        {
            _questDefinitions.TryGetValue(questShortName, out var quest);
            return quest;
        }


        // --- TODO: Save/Load Functionality ---
        // Add methods here to serialize/deserialize _questStates and _taskStates
        // for saving and loading game progress.
    }
}
