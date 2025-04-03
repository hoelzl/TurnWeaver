// file: Scripts/UI/Layers/Ink/DialogueUILayer.cs

using UnityEngine;
using UnityEngine.UIElements;
using UI.Core;
using System.Collections.Generic;
using System.Text;
using System;
using System.Linq;
using System.Text.RegularExpressions; // Added for stripping prefix in choices
using Dialogue.Ink;
using Ink.Runtime;


namespace UI.Layers.Dialogue
{
    /// <summary>
    /// Manages the dialogue UI elements and interacts with StoryProcessor.
    /// Displays pre-formatted content blocks received from StoryProcessor.
    /// </summary>
    public class DialogueUILayer : UILayer
    {
        [Header("UI Templates")]
        [SerializeField] private VisualTreeAsset choiceButtonTemplate;

        [Header("UI Element Queries")]
        [SerializeField] private string scrollViewName = "dialogue-scroll-view";
        [SerializeField] private string textLabelName = "dialogue-text";
        [SerializeField] private string choicesContainerName = "choices-container";
        [SerializeField] private string closeButtonName = "close-button";
        [SerializeField] private string choiceButtonQueryName = "choice-button";

        [Header("History Settings")]
        [SerializeField] private int maxHistoryLinesToShow = 200; // Configurable limit

        // UI Element References
        private ScrollView _dialogueScrollView;
        private Label _dialogueTextLabel;
        private VisualElement _choicesContainer;
        private Button _closeButton;

        // Dialogue state
        private StoryProcessor _processor;
        private Action _onClosingCallback;
        private bool _appendMode;

        // Regex for stripping player prefix from choice button text
        private static readonly Regex _playerPrefixRegex = new Regex(@"^\s*p: ?", RegexOptions.Compiled);


        protected override void SetupUI()
        {
            base.SetupUI();
            if (Root == null) return;

            // Query UI elements
            _dialogueScrollView = Root.Q<ScrollView>(scrollViewName);
            _dialogueTextLabel = Root.Q<Label>(textLabelName);
            _choicesContainer = Root.Q<VisualElement>(choicesContainerName);
            _closeButton = Root.Q<Button>(closeButtonName);

            // Validate UI elements
            if (_dialogueScrollView == null || _dialogueTextLabel == null ||
                _choicesContainer == null || _closeButton == null ||
                choiceButtonTemplate == null)
            {
                Debug.LogError(
                    $"DialogueUILayer: One or more essential UI elements or templates are missing!", this);
                return;
            }

            // Enable rich text for the label to support <b> tags
            _dialogueTextLabel.enableRichText = true;

            // Initial state
            _dialogueTextLabel.text = string.Empty;
            _choicesContainer.Clear();
            _closeButton.style.display = DisplayStyle.None;

            // Bind close button event
            _closeButton.clicked -= OnCloseClicked; // Prevent double registration
            _closeButton.clicked += OnCloseClicked;
        }

        /// <summary>
        /// Initializes the dialogue UI with a story processor and configuration.
        /// </summary>
        public void SetupDialogue(StoryProcessor processor, bool appendMode, Action onClosingCallback)
        {
            if (processor == null || processor.NPCData == null)
            {
                Debug.LogError(
                    "DialogueUILayer: SetupDialogue called with null processor or processor has null NPCData!", this);
                CloseUI();
                return;
            }

            // Clean up previous state
            CleanupCurrentProcessor();

            // Set new state
            _processor = processor;
            _appendMode = appendMode;
            _onClosingCallback = onClosingCallback;

            // Reset UI
            _dialogueTextLabel.text = string.Empty;
            ClearChoicesAndCloseButton();

            // --- History Display ---
            if (_appendMode)
            {
                var history = _processor.NPCData.dialogueHistory;
                if (history != null && history.Count > 0)
                {
                    var relevantHistory = history.TakeLast(maxHistoryLinesToShow);
                    // Join with double newline for spacing between history entries
                    _dialogueTextLabel.text = string.Join("\n\n", relevantHistory);
                }
            }
            // ---------------------

            // Subscribe to processor events AFTER setting up initial state
            _processor.OnContentAvailable += HandleContent;
            _processor.OnChoicesAvailable += HandleChoices;
            _processor.OnStoryComplete += HandleStoryComplete;
            _processor.OnRequestClose += HandleRequestClose;

            // Scroll to bottom after initial setup
            ScrollToBottom();
        }

        /// <summary>
        /// Receives a pre-formatted block (Speaker Title + Content) from StoryProcessor.
        /// </summary>
        private void HandleContent(string formattedBlock)
        {
            if (_processor == null || _dialogueTextLabel == null || string.IsNullOrEmpty(formattedBlock)) return;

            if (_appendMode)
            {
                // Add spacing before the new block if there's existing text
                if (!string.IsNullOrEmpty(_dialogueTextLabel.text))
                {
                    _dialogueTextLabel.text += "\n\n"; // Double newline for spacing
                }
                _dialogueTextLabel.text += formattedBlock;
            }
            else // Overwrite mode
            {
                _dialogueTextLabel.text = formattedBlock;
            }

            ScrollToBottom();
        }


        private void HandleChoices(List<Choice> choices)
        {
            if (_processor == null || _choicesContainer == null) return;

            ClearChoicesAndCloseButton(); // Clear previous choices/close button

            // Add visual spacing before choices appear in the container
            if (!string.IsNullOrEmpty(_dialogueTextLabel.text) && _appendMode)
            {
                _dialogueTextLabel.text += "\n"; // Add a single newline for spacing before choice buttons
            }
            // ------------------------------------------


            for (int i = 0; i < choices.Count; i++)
            {
                Choice choice = choices[i];
                int choiceIndex = i; // Capture index for the lambda

                TemplateContainer choiceInstance = choiceButtonTemplate.Instantiate();
                Button choiceButton = choiceInstance.Q<Button>(choiceButtonQueryName);

                if (choiceButton != null)
                {
                    // --- Strip "p: " prefix from choice text for button --- << NEW
                    string rawChoiceText = choice.text;
                    string buttonText = StripPlayerPrefix(rawChoiceText).Trim();
                    // -------------------------------------------------------

                    choiceButton.text = buttonText;
                    choiceButton.RegisterCallback<ClickEvent>(evt => OnChoiceClicked(choiceIndex));
                    _choicesContainer.Add(choiceInstance);
                }
            }
            ScrollToBottom(); // Scroll after adding choices
        }


        private void HandleStoryComplete()
        {
            ClearChoicesAndCloseButton();
            ShowCloseButton();
        }

        private void HandleRequestClose()
        {
            CloseUI();
        }

        private void OnChoiceClicked(int choiceIndex)
        {
            if (_processor != null)
            {
                ClearChoicesAndCloseButton();
                _processor.SelectChoice(choiceIndex);
            }
        }

        private void OnCloseClicked()
        {
            CloseUI();
        }

        private void CloseUI()
        {
            if (_onClosingCallback == null) return;

            Debug.Log("DialogueUILayer: CloseUI called.");
            CleanupCurrentProcessor();

            var callback = _onClosingCallback;
            _onClosingCallback = null;
            callback?.Invoke();

            if (UILayerManager.Instance != null)
            {
                UILayerManager.Instance.PopLayer();
            }
            else
            {
                Debug.LogWarning("DialogueUILayer: UILayerManager instance is null, cannot pop layer.");
            }
        }

        private void CleanupCurrentProcessor()
        {
            if (_processor != null)
            {
                Debug.Log("DialogueUILayer: Cleaning up processor subscriptions.");
                _processor.OnContentAvailable -= HandleContent;
                _processor.OnChoicesAvailable -= HandleChoices;
                _processor.OnStoryComplete -= HandleStoryComplete;
                _processor.OnRequestClose -= HandleRequestClose;
                _processor = null;
            }
        }

        private void ShowCloseButton()
        {
            if (_closeButton != null)
            {
                _closeButton.style.display = DisplayStyle.Flex;
                ScrollToBottom();
            }
        }

        private void ClearChoicesAndCloseButton()
        {
            if (_choicesContainer != null)
            {
                _choicesContainer.Clear();
            }
            if (_closeButton != null)
            {
                _closeButton.style.display = DisplayStyle.None;
            }
        }

        private void ScrollToBottom()
        {
            if (_dialogueScrollView != null)
            {
                _dialogueScrollView.schedule.Execute(() =>
                {
                    if (_dialogueScrollView?.verticalScroller != null)
                    {
                        _dialogueScrollView.verticalScroller.value = _dialogueScrollView.verticalScroller.highValue;
                    }
                }).StartingIn(10);
            }
        }

        /// <summary>
        /// Helper method to strip the player prefix from text.
        /// </summary>
        private string StripPlayerPrefix(string line)
        {
            if (string.IsNullOrEmpty(line)) return line;
            // Use regex to remove the prefix and optional space
            return _playerPrefixRegex.Replace(line, "");
        }


        // --- Layer Lifecycle ---

        public override void OnLayerPopped()
        {
            Debug.Log("DialogueUILayer: OnLayerPopped.");
            CleanupCurrentProcessor();
            base.OnLayerPopped();
            if (_closeButton != null) _closeButton.clicked -= OnCloseClicked;
        }

        private void OnDestroy()
        {
            Debug.Log("DialogueUILayer: OnDestroy.");
            CleanupCurrentProcessor();
            if (_closeButton != null)
            {
                try
                {
                    _closeButton.clicked -= OnCloseClicked;
                }
                catch
                {
                }
            }
        }
    }
}
