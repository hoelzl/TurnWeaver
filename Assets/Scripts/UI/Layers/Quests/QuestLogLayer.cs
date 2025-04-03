// file: Scripts/UI/Layers/Quests/QuestLogLayer.cs

using System.Collections.Generic;
using Quests; // Namespace for QuestManager, QuestSO, etc.
using UI.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Layers.Quests
{
    public class QuestLogLayer : UILayer
    {
        [Header("UI Templates")]
        [SerializeField] private VisualTreeAsset questEntryTemplate;
        [SerializeField] private VisualTreeAsset taskEntryTemplate;

        // UI Element References
        private VisualElement _questListContainer;
        private Label _taskDetailTitle;
        private VisualElement _taskListContainer;
        private Button _closeButton;

        // State
        private string _selectedQuestShortName;
        private readonly List<Button> _questButtons = new List<Button>();

        protected override void SetupUI()
        {
            base.SetupUI();
            if (Root == null) return;

            _questListContainer = Root.Q<VisualElement>("quest-list");
            _taskDetailTitle = Root.Q<Label>("task-detail-title");
            _taskListContainer = Root.Q<VisualElement>("task-list");
            _closeButton = Root.Q<Button>("close-button");

            if (_closeButton != null)
            {
                _closeButton.clicked += OnCloseClicked;
            }

            if (_questListContainer == null || _taskListContainer == null || _taskDetailTitle == null ||
                questEntryTemplate == null || taskEntryTemplate == null)
            {
                Debug.LogError("QuestLogLayer: Missing essential UI elements or templates!", this);
                return;
            }

            // Subscribe to QuestManager events
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.OnQuestStatusChanged += HandleQuestStatusChanged;
                QuestManager.Instance.OnTaskStatusChanged += HandleTaskStatusChanged;
            }

            RefreshQuestList();
            ClearTaskDetails(); // Start with empty task details
        }

        private void RefreshQuestList()
        {
            if (QuestManager.Instance == null || _questListContainer == null) return;

            // Clear existing buttons and handlers
            foreach (var button in _questButtons)
            {
                button.clicked -= () => OnQuestSelected(button.userData as string);
            }
            _questListContainer.Clear();
            _questButtons.Clear();

            var quests = QuestManager.Instance.GetAllQuests();

            foreach (var questDef in quests)
            {
                QuestStatus status = QuestManager.Instance.GetQuestStatus(questDef.QuestShortName);

                // Only show non-hidden quests (or adjust filter as needed)
                if (status != QuestStatus.Hidden /* && status != QuestStatus.NotStarted */)
                {
                    CreateQuestEntry(questDef, status);
                }
            }

            // Optionally: Automatically select the first quest in the list
            if (_questButtons.Count > 0)
            {
                OnQuestSelected(_questButtons[0].userData as string);
                // Ensure the first button gets focus visually
                _questButtons[0].Focus();
            }
            else
            {
                // If no quests are visible, clear the details pane
                ClearTaskDetails();
            }
        }

        private void CreateQuestEntry(QuestSO questDef, QuestStatus status)
        {
            TemplateContainer entryInstance = questEntryTemplate.Instantiate();
            Button questButton = entryInstance.Q<Button>("quest-entry-button");

            if (questButton == null) return;

            questButton.Q<Label>("quest-name").text = questDef.QuestName;
            var statusLabel = questButton.Q<Label>("quest-status");
            var trackedIndicator = questButton.Q<VisualElement>("tracked-indicator");

            // Store short name for selection logic
            questButton.userData = questDef.QuestShortName;

            UpdateQuestEntryVisuals(questButton, status); // Use helper to set visuals

            questButton.clicked += () => OnQuestSelected(questDef.QuestShortName);

            _questListContainer.Add(entryInstance);
            _questButtons.Add(questButton);
        }

        private void UpdateQuestEntryVisuals(Button questButton, QuestStatus status)
        {
            if (questButton == null) return;

            var statusLabel = questButton.Q<Label>("quest-status");
            var trackedIndicator = questButton.Q<VisualElement>("tracked-indicator");

            statusLabel.text = $"[{status}]";
            // Add/Remove USS classes based on status for styling
            statusLabel.EnableInClassList("quest-status--completed", status == QuestStatus.Completed);
            statusLabel.EnableInClassList("quest-status--failed", status == QuestStatus.Failed);

            // Show/hide tracked indicator
            bool isTracked = status == QuestStatus.Tracked;
            if (trackedIndicator != null)
            {
                trackedIndicator.EnableInClassList("tracked-indicator--visible", isTracked);
            }
        }


        private void OnQuestSelected(string questShortName)
        {
            if (string.IsNullOrEmpty(questShortName) || QuestManager.Instance == null)
            {
                ClearTaskDetails();
                return;
            }

            _selectedQuestShortName = questShortName;

            QuestSO questDef = QuestManager.Instance.GetQuestDefinition(questShortName);
            if (questDef == null)
            {
                ClearTaskDetails();
                return;
            }

            // Update title
            _taskDetailTitle.text = questDef.QuestName; // Or "Tasks for: Quest Name"

            // Refresh task list
            RefreshTaskList(questDef);

            // Visually indicate selection (e.g., focus or custom style)
            foreach (var button in _questButtons)
            {
                if ((button.userData as string) == questShortName)
                {
                    // Optional: Add a specific USS class for selection
                    button.AddToClassList("quest-entry--selected");
                }
                else
                {
                    button.RemoveFromClassList("quest-entry--selected");
                }
            }
        }

        private void RefreshTaskList(QuestSO questDef)
        {
            if (questDef == null || _taskListContainer == null || QuestManager.Instance == null) return;

            _taskListContainer.Clear();

            foreach (var taskDef in questDef.Tasks)
            {
                TaskStatus status = QuestManager.Instance.GetTaskStatus(questDef.QuestShortName, taskDef.TaskShortName);

                // Only show non-hidden tasks
                if (status != TaskStatus.Hidden)
                {
                    CreateTaskEntry(taskDef, status);
                }
            }
        }

        private void CreateTaskEntry(QuestTask taskDef, TaskStatus status)
        {
            TemplateContainer entryInstance = taskEntryTemplate.Instantiate();
            var taskNameLabel = entryInstance.Q<Label>("task-name");
            var taskStatusLabel = entryInstance.Q<Label>("task-status"); // Using label for status example

            taskNameLabel.text = taskDef.TaskName;

            // Update status visual based on TaskStatus
            UpdateTaskEntryVisuals(entryInstance, status);

            _taskListContainer.Add(entryInstance);
        }

        private void UpdateTaskEntryVisuals(VisualElement taskEntryElement, TaskStatus status)
        {
            var taskStatusLabel = taskEntryElement.Q<Label>("task-status");
            if (taskStatusLabel == null) return;

            // Basic text representation:
            switch (status)
            {
                case TaskStatus.Completed:
                    taskStatusLabel.text = "[\u2714]"; // X or checkmark symbol ✔
                    break;
                case TaskStatus.Active:
                    taskStatusLabel.text = "[\u25cb]"; // O or circle symbol ○
                    break;
                case TaskStatus.Failed:
                    taskStatusLabel.text = "[\u2718]"; // F or cross symbol ✘
                    break;
                case TaskStatus.NotStarted:
                default:
                    taskStatusLabel.text = "[\u2014]"; // - or em dash —
                    break;
            }

            // Apply USS classes for styling
            taskStatusLabel.EnableInClassList("task-status--completed", status == TaskStatus.Completed);
            taskStatusLabel.EnableInClassList("task-status--failed", status == TaskStatus.Failed);
            taskStatusLabel.EnableInClassList("task-status--active", status == TaskStatus.Active); // If needed
        }

        private void ClearTaskDetails()
        {
            _taskDetailTitle.text = "Tasks";
            _taskListContainer.Clear();
            _selectedQuestShortName = null;
        }

        private void OnCloseClicked()
        {
            UILayerManager.Instance.PopLayer();
        }

        // --- Event Handlers for QuestManager Updates ---

        private void HandleQuestStatusChanged(string changedQuestShortName)
        {
            // Update the specific quest entry in the list
            foreach (var button in _questButtons)
            {
                if ((button.userData as string) == changedQuestShortName)
                {
                    QuestStatus newStatus = QuestManager.Instance.GetQuestStatus(changedQuestShortName);
                    UpdateQuestEntryVisuals(button, newStatus);
                    break;
                }
            }

            // If the currently selected quest's status changed, potentially refresh tasks
            if (changedQuestShortName == _selectedQuestShortName)
            {
                QuestSO questDef = QuestManager.Instance.GetQuestDefinition(_selectedQuestShortName);
                RefreshTaskList(questDef); // Refresh tasks if quest state changes (e.g., becomes active)
            }

            // Potentially fully refresh the list if quests should appear/disappear
            // RefreshQuestList(); // Use this if filtering logic needs re-evaluation
        }

        private void HandleTaskStatusChanged(string changedQuestShortName, string changedTaskShortName)
        {
            // If the change belongs to the currently selected quest, refresh the task list
            if (changedQuestShortName == _selectedQuestShortName)
            {
                QuestSO questDef = QuestManager.Instance.GetQuestDefinition(_selectedQuestShortName);
                RefreshTaskList(questDef);
            }
        }

        // --- Layer Lifecycle Overrides ---

        public override void OnLayerPopped()
        {
            base.OnLayerPopped();
            // Unsubscribe from QuestManager events
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.OnQuestStatusChanged -= HandleQuestStatusChanged;
                QuestManager.Instance.OnTaskStatusChanged -= HandleTaskStatusChanged;
            }
            // Clean up button handlers
            if (_closeButton != null) _closeButton.clicked -= OnCloseClicked;
            foreach (var button in _questButtons)
            {
                button.clicked -= () => OnQuestSelected(button.userData as string);
            }
            _questButtons.Clear();
        }

        private void OnDestroy()
        {
            // Ensure unsubscription on destroy as well
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.OnQuestStatusChanged -= HandleQuestStatusChanged;
                QuestManager.Instance.OnTaskStatusChanged -= HandleTaskStatusChanged;
            }
            // Clean up button handlers
            if (_closeButton != null)
                try
                {
                    _closeButton.clicked -= OnCloseClicked;
                }
                catch
                {
                }
            _questButtons.Clear(); // List is cleared anyway on destroy
        }
    }
}
