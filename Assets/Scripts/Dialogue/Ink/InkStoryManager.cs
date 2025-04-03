using Ink.Runtime;
using System;
using System.Collections.Generic;
using Ink;
using UnityEngine;

namespace Dialogue.Ink
{
    [Serializable]
    public class StoryAssetEntry
    {
        public string key = "default";
        public TextAsset jsonAsset;
    }

    /// <summary>
    /// Manages Ink story assets and prepares them for dialogue.
    /// Responsible for story loading, caching, and flow/path preparation.
    /// </summary>
    public class InkStoryManager : MonoBehaviour
    {
        public static InkStoryManager Instance { get; private set; }

        [Header("Ink Story Assets")]
        [SerializeField] private List<StoryAssetEntry> storyAssets = new List<StoryAssetEntry>();
        public string DefaultStoryKey { get; } = "default";

        [Header("Error Handling")]
        [SerializeField] private bool logInkWarnings = true;
        [SerializeField] private bool logInkErrors = true;

        // Cache for loaded stories
        private readonly Dictionary<string, Story> _loadedStories = new();
        private readonly Dictionary<string, TextAsset> _storyAssetMap = new();

        private static string DefaultFlowName => "DEFAULT";
        // Get a reference to our registry
        [SerializeField] private InkFunctionRegistry functionRegistry;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            InitializeStoryAssets();

            // Find registry if not assigned
            if (functionRegistry == null)
                functionRegistry = FindAnyObjectByType<InkFunctionRegistry>();
        }

        /// <summary>
        /// Gets a Story instance for the given key, loading it if necessary.
        /// </summary>
        public Story GetStoryInstance(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                key = "default";
            }

            // Return cached instance if available
            if (_loadedStories.TryGetValue(key, out Story storyInstance))
            {
                return storyInstance;
            }

            // Load from asset map if not cached
            if (_storyAssetMap.TryGetValue(key, out TextAsset jsonAsset))
            {
                try
                {
                    var newStory = new Story(jsonAsset.text);
                    newStory.onError += HandleInkError;

                    // Register external functions once when story is loaded
                    if (functionRegistry != null)
                    {
                        functionRegistry.RegisterFunctions(newStory);
                    }

                    _loadedStories.Add(key, newStory); // Cache it
                    Debug.Log($"InkStoryManager: Loaded and cached story for key '{key}'.");
                    return newStory;
                }
                catch (Exception e)
                {
                    Debug.LogError($"InkStoryManager: Failed to load Ink story for key '{key}': {e.Message}", this);
                    return null;
                }
            }

            Debug.LogError($"InkStoryManager: Story asset not found for key '{key}'.", this);
            return null;
        }

        private void InitializeStoryAssets()
        {
            _storyAssetMap.Clear();
            foreach (var entry in storyAssets)
            {
                if (entry.jsonAsset == null || string.IsNullOrEmpty(entry.key))
                {
                    Debug.LogWarning($"InkStoryManager: Invalid StoryAssetEntry found (missing key or asset).", this);
                    continue;
                }
                if (_storyAssetMap.ContainsKey(entry.key))
                {
                    Debug.LogWarning($"InkStoryManager: Duplicate story key '{entry.key}' found. Using the first one.",
                        this);
                    continue;
                }
                _storyAssetMap.Add(entry.key, entry.jsonAsset);
            }
            Debug.Log($"InkStoryManager: Initialized with {_storyAssetMap.Count} story assets.");
        }


        private void HandleInkError(string message, ErrorType type)
        {
            if (type == ErrorType.Warning && logInkWarnings)
            {
                Debug.LogWarning($"Ink Warning: {message}");
            }
            else if (type == ErrorType.Error && logInkErrors)
            {
                Debug.LogError($"Ink Error: {message}");
            }
        }

        /// <summary>
        /// Prepares a story instance for dialogue, handling state reset and flow switching.
        /// The starting path choice is handled based on StartOption and flow state.
        /// </summary>
        public Story PrepareStoryForDialogue(NPCInkData npcData)
        {
            if (npcData == null)
            {
                Debug.LogError("InkStoryManager: NPCInkData is null!", this);
                return null;
            }

            // Get the correct story instance for the NPC
            Story story = GetStoryInstance(npcData.StoryKey);
            if (story == null)
            {
                Debug.LogError(
                    $"InkStoryManager: Could not get story instance for NPC '{npcData.gameObject.name}'. Dialogue aborted.",
                    npcData.gameObject);
                return null;
            }

            bool flowSwitched = false; // Track if flow was switched this time

            // Handle Restart Story option FIRST
            if (npcData.StartOption == DialogueStartOption.AlwaysResetStory)
            {
                Debug.Log($"InkStoryManager: Resetting state for story '{npcData.StoryKey}'...");
                try
                {
                    story.ResetState();
                }
                catch (Exception e)
                {
                    /* error */
                    return null;
                }
                // Resetting implies we need to choose the start path below
            }

            // Determine target flow and switch if needed
            string targetFlow = npcData.InkFlowName;
            bool flowExists = story.aliveFlowNames.Contains(targetFlow);

            try
            {
                if (IsTargetingDefaultFlow(targetFlow))
                {
                    if (!story.state.currentFlowIsDefaultFlow)
                    {
                        Debug.Log($"InkStoryManager: Switching story '{npcData.StoryKey}' to default flow.");
                        story.SwitchToDefaultFlow();
                        flowSwitched = true;
                    }
                }
                else if (story.state.currentFlowName != targetFlow)
                {
                    Debug.Log($"InkStoryManager: Switching story '{npcData.StoryKey}' to flow '{targetFlow}'.");
                    story.SwitchFlow(targetFlow);
                    flowSwitched = true;
                }
            }
            catch (Exception e)
            {
                /* error */
                return null;
            }

            // --- Path Selection Logic (Simplified) ---
            // Choose path if: state reset, flow switched, AlwaysFromStartPath,
            // OR if Continue/Loop option is chosen BUT the flow didn't exist before (implicitly needs starting point)
            bool shouldChoosePath = (npcData.StartOption == DialogueStartOption.AlwaysResetStory) ||
                                    (npcData.StartOption == DialogueStartOption.AlwaysFromStartPath) ||
                                    (!flowExists && (npcData.StartOption == DialogueStartOption.Continue ||
                                                     npcData.StartOption == DialogueStartOption.ContinueAndLoop));


            if (shouldChoosePath)
            {
                ChoosePathString(story, npcData.StartPath);
            }
            else
            {
                Debug.Log(
                    $"InkStoryManager: Continuing dialogue in story '{npcData.StoryKey}', flow '{story.state.currentFlowName}' from current pointer.");
            }
            // --- End Path Selection ---

            return story;
        }

        private void ChoosePathString(Story story, string path)
        {
            try
            {
                Debug.Log($"InkStoryManager: Choosing path '{path}' in story, flow '{story.state.currentFlowName}'.");
                story.ChoosePathString(path);
            }
            catch (Exception e)
            {
                /* ... error log ... */
            }
        }

        private bool IsTargetingDefaultFlow(string targetFlowName) =>
            string.IsNullOrEmpty(targetFlowName) || targetFlowName == DefaultFlowName;


        // --- Save/Load for specific stories ---
        public string GetStoryStateJson(string key)
        {
            if (_loadedStories.TryGetValue(key, out Story story))
            {
                return story.state.ToJson();
            }
            Debug.LogWarning($"InkStoryManager: Cannot get state. Story not loaded for key '{key}'.");
            return null;
        }

        public void LoadStoryStateJson(string key, string json)
        {
            if (string.IsNullOrEmpty(json)) return;

            Story story = GetStoryInstance(key);
            if (story == null)
            {
                Debug.LogError($"InkStoryManager: Cannot load state. Failed to get story instance for key '{key}'.");
                return;
            }

            try
            {
                story.state.LoadJson(json);
                Debug.Log($"InkStoryManager: Loaded Ink state successfully for story '{key}'.");
            }
            catch (Exception e)
            {
                Debug.LogError($"InkStoryManager: Failed to load Ink state for story '{key}': {e.Message}");
            }
        }

        // --- Variable Access ---
        public object GetVariableState(string key, string variableName)
        {
            Story story = GetStoryInstance(key);
            if (story == null) return null;
            try
            {
                return story.variablesState[variableName];
            }
            catch (Exception e)
            {
                Debug.LogError(
                    $"InkStoryManager: Error getting variable '{variableName}' from story '{key}': {e.Message}");
                return null;
            }
        }

        public void SetVariableState(string key, string variableName, object value)
        {
            Story story = GetStoryInstance(key);
            if (story == null) return;
            try
            {
                story.variablesState[variableName] = value;
            }
            catch (Exception e)
            {
                Debug.LogError(
                    $"InkStoryManager: Error setting variable '{variableName}' in story '{key}': {e.Message}");
            }
        }
    }
}
