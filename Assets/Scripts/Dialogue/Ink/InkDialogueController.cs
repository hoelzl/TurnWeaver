using System;
using Core;
using Dialogue.Ink;
using Ink.Runtime;
using UI.Core;
using UnityEngine;

namespace Dialogue
{
    /// <summary>
    /// Coordinates dialogue sessions between NPCs, story, and UI.
    /// Acts as the central coordinator of the dialogue system.
    /// </summary>
    public class InkDialogueController : MonoBehaviour
    {
        public static InkDialogueController Instance { get; private set; }

        [SerializeField] private InkStoryManager storyManager;
        [SerializeField] private InkFunctionRegistry functionRegistry;

        private StoryProcessor _activeProcessor;
        private bool _processorBeingSetup;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Find StoryManager if not assigned
            if (storyManager == null)
            {
                storyManager = FindFirstObjectByType<InkStoryManager>();
                if (storyManager == null)
                {
                    Debug.LogError("InkDialogueController: No InkStoryManager found in scene!", this);
                }
            }

            if (functionRegistry == null)
            {
                functionRegistry = FindAnyObjectByType<InkFunctionRegistry>();
            }
        }

        /// <summary>
        /// Starts a dialogue session with the given NPC.
        /// </summary>
        public void StartDialogue(NPCInkData npcData)
        {
            // Guard clauses
            if (storyManager == null)
            {
                Debug.LogError("InkDialogueController: Cannot start dialogue - InkStoryManager not available!", this);
                return;
            }

            if (npcData == null)
            {
                Debug.LogError("InkDialogueController: Cannot start dialogue - NPCInkData is null!", this);
                return;
            }

            // Prevent starting multiple dialogues at once
            if (_activeProcessor != null || _processorBeingSetup)
            {
                Debug.LogWarning("InkDialogueController: Dialogue already in progress.", this);
                return;
            }

            _processorBeingSetup = true;

            try
            {
                // Get prepared story from manager
                Story story = storyManager.PrepareStoryForDialogue(npcData);
                if (story == null)
                {
                    Debug.LogError(
                        $"InkDialogueController: Failed to prepare story for NPC '{npcData.gameObject.name}'",
                        npcData.gameObject);
                    _processorBeingSetup = false;
                    return;
                }

                var uniqueIdComponent = npcData.GetComponent<UniqueId>();
                if (uniqueIdComponent != null && !string.IsNullOrEmpty(uniqueIdComponent.UniqueIdGuidString))
                {
                    try
                    {
                        story.variablesState["npc_id"] = uniqueIdComponent.UniqueIdGuidString;
                        Debug.Log(
                            $"InkDialogueController: Set Ink variable 'npc_id' to '{uniqueIdComponent.UniqueIdGuidString}'");
                    }
                    catch (Exception e)
                    {
                        // Catch potential errors if the variable doesn't exist in Ink
                        Debug.LogError(
                            $"InkDialogueController: Failed to set Ink variable 'npc_id'. Does it exist in your Ink script? Error: {e.Message}",
                            this);
                    }
                }
                else
                {
                    Debug.LogWarning(
                        $"InkDialogueController: NPC '{npcData.gameObject.name}' does not have a valid UniqueId component/GUID. 'npc_id' variable not set.",
                        npcData.gameObject);
                }

                // Set context for external functions BEFORE creating processor
                if (functionRegistry != null)
                {
                    functionRegistry.SetDialogueContext(npcData);
                }

                // Create processor (but DON'T start processing yet)
                _activeProcessor = new StoryProcessor(story, npcData);

                // Show UI first, then start processing when UI is ready
                ShowDialogueUI(npcData.AppendDialogue);
            }
            catch (Exception e)
            {
                Debug.LogError($"InkDialogueController: Error starting dialogue: {e.Message}", this);
                CleanupProcessor();
                _processorBeingSetup = false;
            }
        }

        private void ShowDialogueUI(bool appendMode)
        {
            if (_activeProcessor == null)
            {
                Debug.LogError("InkDialogueController: No active processor when showing dialogue UI!", this);
                _processorBeingSetup = false;
                return;
            }

            UIManager.ShowDialogueUI(
                _activeProcessor,
                appendMode,
                OnUIReady,
                OnUIClosing);

            _processorBeingSetup = false;
        }

        private void OnUIReady()
        {
            if (_activeProcessor != null)
            {
                // Start processing AFTER UI is ready
                _activeProcessor.BeginProcessing();
            }
        }

        private void OnUIClosing()
        {
            CleanupProcessor();
        }

        private void CleanupProcessor()
        {
            if (_activeProcessor != null)
            {
                _activeProcessor.Dispose();
                _activeProcessor = null;

                // Clear function context
                if (functionRegistry != null)
                {
                    functionRegistry.ClearDialogueContext();
                }
            }
        }

        private void OnDestroy()
        {
            CleanupProcessor();

            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
