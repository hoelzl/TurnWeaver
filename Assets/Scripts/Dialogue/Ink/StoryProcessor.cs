using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Ink.Runtime;
using UnityEngine;

namespace Dialogue.Ink
{
    /// <summary>
    /// Processes an Ink story, handling continuations, choices, and commands.
    /// Uses 'p: ' prefix in Ink lines to identify player lines.
    /// </summary>
    public class StoryProcessor : IDisposable
    {
        private enum SpeakerType
        {
            None,
            NPC,
            Player
        }

        public event Action<string> OnContentAvailable;
        public event Action<List<Choice>> OnChoicesAvailable;
        public event Action OnStoryComplete;
        public event Action OnRequestClose;
        public NPCInkData NPCData { get; }

        private readonly Story _story;
        private bool _isProcessing;
        private bool _isDisposed;

        private SpeakerType _currentSpeaker = SpeakerType.None;
        private SpeakerType _lastSpeakerSentToUi = SpeakerType.None;
        private StringBuilder _accumulatedContent = new StringBuilder();
        private readonly string _playerPrefix = "p:";
        private readonly Regex _playerPrefixRegex;

        private string PlayerSpeakerTitle => "<b>=== Player ===</b>";
        private string NpcSpeakerTitle => $"<b>=== {NPCData.NpcComponent.NpcName} ===</b>";


        /// <summary>
        /// Creates a new processor for an Ink story.
        /// </summary>
        public StoryProcessor(Story story, NPCInkData npcData)
        {
            _story = story ?? throw new ArgumentNullException(nameof(story));
            NPCData = npcData ?? throw new ArgumentNullException(nameof(npcData));
            _playerPrefixRegex = new Regex(@"^\s*" + Regex.Escape(_playerPrefix) + @" ?");
        }

        /// <summary>
        /// Begins processing the story from its current position.
        /// </summary>
        public void BeginProcessing()
        {
            if (_isDisposed || _isProcessing) return;
            _isProcessing = true;
            _currentSpeaker = SpeakerType.None;
            _accumulatedContent.Clear();
            _lastSpeakerSentToUi = SpeakerType.None;
            ContinueStory();
        }

        /// <summary>
        /// Selects a choice and continues the story.
        /// </summary>
        public void SelectChoice(int index)
        {
            if (_isDisposed || !_isProcessing) return;
            if (index >= 0 && index < _story.currentChoices.Count)
            {
                ProcessAndSendAccumulatedContent(true);
                string rawChoiceText = _story.currentChoices[index].text;
                string strippedChoiceText = StripPlayerPrefix(rawChoiceText).Trim();
                DialogueLogManager.Instance?.LogChoice(strippedChoiceText);
                _currentSpeaker = SpeakerType.Player; // Set speaker state for next block
                _story.ChooseChoiceIndex(index);
                ContinueStory();
            }
            else
            {
                Debug.LogError($"StoryProcessor: Invalid choice index {index}");
            }
        }

        private void ContinueStory()
        {
            if (_isDisposed || !_isProcessing) return;
            ProcessLinearContent();
            if (!_isProcessing || _isDisposed) return;
            ProcessChoices();
        }

        private void ProcessLinearContent()
        {
            while (!_isDisposed && _isProcessing && _story.canContinue)
            {
                string line = _story.Continue();
                List<string> tags = _story.currentTags;

                if (IsCommand(line))
                {
                    ProcessAndSendAccumulatedContent(true); // Send previous block
                    ProcessCommand(line.Substring(">>>".Length).Trim()); // Process the command
                    // If ProcessCommand called ForceClose, _isProcessing becomes false
                    if (!_isProcessing || _isDisposed) return; // Exit if command closed dialogue
                    continue; // Skip rest of loop for command line
                }

                // --- Speaker/Content processing remains the same ---
                bool lineIsPlayer = DoesLineHavePlayerPrefix(line);
                SpeakerType lineSpeaker = lineIsPlayer ? SpeakerType.Player : SpeakerType.NPC;
                string contentLine = lineIsPlayer ? StripPlayerPrefix(line) : line;
                string trimmedContentLine = contentLine.Trim();

                if (string.IsNullOrEmpty(trimmedContentLine))
                {
                    if (!string.IsNullOrWhiteSpace(line) && _accumulatedContent.Length > 0)
                    {
                        _accumulatedContent.AppendLine();
                    }
                    continue;
                }

                if ((lineSpeaker != _currentSpeaker && _currentSpeaker != SpeakerType.None) ||
                    _accumulatedContent.Length == 0)
                {
                    if (_accumulatedContent.Length > 0)
                    {
                        ProcessAndSendAccumulatedContent(false);
                    }
                    _currentSpeaker = lineSpeaker;
                }

                if (_accumulatedContent.Length > 0)
                {
                    _accumulatedContent.AppendLine();
                }
                _accumulatedContent.Append(trimmedContentLine);
                // --- End Speaker/Content processing ---

                // ProcessTags(tags);
            }
            ProcessAndSendAccumulatedContent(true); // Process final block after loop
        }

        private void ProcessChoices()
        {
            if (_isDisposed || !_isProcessing) return;
            if (_story.currentChoices.Count > 0)
            {
                OnChoicesAvailable?.Invoke(_story.currentChoices);
            }
            else if (_isProcessing)
            {
                // Check _isProcessing before completing
                HandleStoryThreadCompletion();
            }
        }

        /// <summary>
        /// Formats and sends the accumulated content block IF IT EXISTS to the UI
        /// and adds the appropriate entry to history.
        /// </summary>
        /// <param name="forceSend">If true, indicates this is the final send before a break (choice, command, end).</param>
        private void ProcessAndSendAccumulatedContent(bool forceSend = false)
        {
            // Only proceed if there's content to send
            if (_accumulatedContent.Length > 0 && _currentSpeaker != SpeakerType.None)
            {
                string speakerTitle = _currentSpeaker == SpeakerType.Player ? PlayerSpeakerTitle : NpcSpeakerTitle;
                string contentToSend = _accumulatedContent.ToString(); // The actual text block

                // --- History Logging ---
                // Add to history, including title only if speaker changed since last history entry
                string historyBlock;
                if (NPCData.dialogueHistory.Count == 0 || !DoesLastHistoryEntryMatchSpeaker(_currentSpeaker))
                {
                    historyBlock = $"{speakerTitle}\n{contentToSend}";
                }
                else
                {
                    historyBlock = contentToSend; // Append content without title
                }
                NPCData.AddHistoryLine(historyBlock);
                // ---------------------

                // --- UI Event ---
                // Determine if the title should be sent to the UI this time
                string uiBlockToSend;
                if (_currentSpeaker != _lastSpeakerSentToUi)
                {
                    uiBlockToSend = $"{speakerTitle}\n{contentToSend}"; // Send title + content
                }
                else
                {
                    uiBlockToSend = contentToSend; // Send only content (continuation)
                }

                // Send the potentially modified block to the UI
                OnContentAvailable?.Invoke(uiBlockToSend);

                // ** Update the last speaker sent to UI **
                _lastSpeakerSentToUi = _currentSpeaker;
                // ----------------

                // Log the raw content part for console/debugging
                DialogueLogManager.Instance?.LogLine(contentToSend);

                // Clear the accumulator for the next block
                _accumulatedContent.Clear();

                // Important: Do NOT reset _currentSpeaker here. It holds the state until a line from a different speaker arrives.
            }
            else if (forceSend) // If forced, ensure accumulator is clear even if nothing was sent
            {
                _accumulatedContent.Clear();
            }
        }


        /// <summary>
        /// Checks if the last entry in the dialogue history starts with the title matching the given speaker.
        /// </summary>
        private bool DoesLastHistoryEntryMatchSpeaker(SpeakerType speaker)
        {
            if (NPCData.dialogueHistory.Count == 0) return false;
            string lastEntry = NPCData.dialogueHistory[NPCData.dialogueHistory.Count - 1];

            string expectedTitle;
            if (speaker == SpeakerType.Player)
            {
                expectedTitle = PlayerSpeakerTitle;
            }
            else if (speaker == SpeakerType.NPC)
            {
                expectedTitle = NpcSpeakerTitle;
            }
            else
            {
                return false; // Should not happen with SpeakerType.None
            }

            // Check if the last history entry *starts with* the relevant speaker title.
            // This handles cases where content was added without the title on subsequent lines.
            return lastEntry.StartsWith(expectedTitle);
        }


        private void HandleStoryThreadCompletion()
        {
            Debug.Log($"StoryProcessor: No choices or continuation available. Story thread complete.");
            NPCData.AddHistoryLine("--- Conversation Ended ---");
            _isProcessing = false;
            OnStoryComplete?.Invoke();

            if (NPCData.StartOption == DialogueStartOption.ContinueAndLoop)
            {
                // (Looping logic remains the same)
                try
                {
                    string flowToRemove = NPCData.InkFlowName;
                    if (!string.IsNullOrEmpty(flowToRemove) && flowToRemove != "DEFAULT")
                    {
                        _story.RemoveFlow(flowToRemove);
                        Debug.Log($"StoryProcessor: Removed flow '{flowToRemove}' for looping.");
                    }
                    else
                    {
                        Debug.LogWarning(
                            "StoryProcessor: ContinueAndLoop on default flow - behavior needs clarification. Not removing flow.");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(
                        $"StoryProcessor: Error removing flow '{NPCData.InkFlowName}' for looping: {e.Message}");
                }
            }
            _currentSpeaker = SpeakerType.None; // Reset speaker state at end
            _lastSpeakerSentToUi = SpeakerType.None; // Reset UI speaker state at end << NEW
        }


        private bool IsCommand(string line)
        {
            return line != null && line.TrimStart().StartsWith(">>>");
        }

        private void ProcessCommand(string commandLine)
        {
            if (_isDisposed) return;
            Debug.Log($"StoryProcessor: Command Received: {commandLine}");

            // Make check case-insensitive and trim command
            string command = commandLine.Trim();
            if (command.Equals("close_dialogue", StringComparison.OrdinalIgnoreCase) ||
                command.Equals("close", StringComparison.OrdinalIgnoreCase))
            {
                Debug.Log($"StoryProcessor: Processing CLOSE command.");
                NPCData.AddHistoryLine("--- Dialogue Closed by Command ---");
                ForceClose(); // This will set _isProcessing = false
            }
            // Add other commands here if needed
            // else if (command.StartsWith("..."))...
        }

        private void ForceClose()
        {
            if (_isDisposed || !_isProcessing) return;
            ProcessAndSendAccumulatedContent(true); // Send any last bits first
            Debug.Log("StoryProcessor: ForceClose called.");
            _isProcessing = false; // Stop processing loops
            OnRequestClose?.Invoke(); // Signal UI/Controller to close
            // Reset state AFTER sending request and final content
            _currentSpeaker = SpeakerType.None;
            _lastSpeakerSentToUi = SpeakerType.None;
        }

        // --- Helper Methods ---

        private bool DoesLineHavePlayerPrefix(string line)
        {
            if (string.IsNullOrEmpty(line)) return false;
            return _playerPrefixRegex.IsMatch(line);
        }

        private string StripPlayerPrefix(string line)
        {
            if (string.IsNullOrEmpty(line)) return line;
            return _playerPrefixRegex.Replace(line, "");
        }


        // --- Cleanup ---

        public void Dispose()
        {
            if (_isDisposed) return;
            Debug.Log("StoryProcessor: Disposing.");
            _isDisposed = true;
            _isProcessing = false;
            OnContentAvailable = null;
            OnChoicesAvailable = null;
            OnStoryComplete = null;
            OnRequestClose = null;
            _accumulatedContent.Clear();
        }
    }
}
