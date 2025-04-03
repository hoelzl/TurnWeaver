using UnityEngine;
using Interaction.Objects;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Dialogue.Ink
{
    /// <summary>
    /// Defines how the dialogue should start for an NPC.
    /// </summary>
    public enum DialogueStartOption
    {
        [Tooltip("Continue from where the story left off in this flow.")]
        Continue,
        [Tooltip("Continue from where the story left off in this flow, go back to the start path when the story ends.")]
        ContinueAndLoop,
        [Tooltip("Always start dialogue from the specified 'Start Path' or the generated default.")]
        AlwaysFromStartPath,
        [Tooltip("Reset the entire Ink story state (variables, visit counts) before starting. Affects ALL flows.")]
        AlwaysResetStory
    }

    /// <summary>
    /// Stores Ink-related configuration and state for an NPC.
    /// </summary>
    [RequireComponent(typeof(NPC))] // Ensure NPC component is present
    public class NPCInkData : MonoBehaviour
    {
        [Header("Ink Configuration")]
        [Tooltip(
            "Optional key for a specific story file managed by InkStoryManager. Leave empty to use the default story.")]
        [SerializeField] private string storyKey = "";

        [Tooltip("The Ink flow this NPC should use. Leave empty to use the default flow or derive from NPC Name.")]
        [SerializeField] private string inkFlowName = "";

        [Tooltip("The knot/stitch to start dialogue (e.g., 'Chapter1.Meeting'). Leave empty to derive from NPC Name.")]
        [SerializeField] private string startPath = "";

        [Tooltip("How should the dialogue session begin?")]
        [SerializeField] private DialogueStartOption startOption = DialogueStartOption.Continue;

        [Header("Dialogue UI Behaviour")]
        [Tooltip(
            "If true, dialogue text accumulates in the UI panel, including history. If false, the panel is cleared for each new line.")] // Modified tooltip
        [SerializeField] private bool appendDialogue = true;

        // Cache the NPC component
        private NPC _npc;
        public NPC NpcComponent => _npc ??= GetComponent<NPC>();

        // Dialogue History Storage
        // Note: This state will be lost when the scene reloads unless saved/loaded.
        [HideInInspector] // Hide from inspector unless you want to debug it
        public List<string> dialogueHistory = new List<string>();

        // Public Properties with Default Logic
        public string StoryKey => storyKey;
        public DialogueStartOption StartOption => startOption;
        public bool AppendDialogue => appendDialogue;

        public string InkFlowName
        {
            get
            {
                if (!string.IsNullOrEmpty(inkFlowName))
                {
                    return inkFlowName;
                }
                // Default to NPC name if available
                return NpcComponent != null ? NpcComponent.NpcName : ""; // Use the property from NPC.cs
            }
        }

        public string StartPath
        {
            get
            {
                if (!string.IsNullOrEmpty(startPath))
                {
                    return startPath;
                }
                // Default to generated path from NPC name
                if (NpcComponent != null && !string.IsNullOrEmpty(NpcComponent.NpcName))
                {
                    // Lowercase, replace non-alphanumeric with underscores, add postfix
                    string safeName = Regex.Replace(NpcComponent.NpcName.ToLower(), @"[^a-z0-9_]+", "_");
                    // Remove leading/trailing/multiple underscores if desired (optional refinement)
                    safeName = Regex.Replace(safeName, @"_+", "_").Trim('_');
                    return !string.IsNullOrEmpty(safeName)
                        ? $"{safeName}_start"
                        : "default_start"; // Fallback if name becomes empty
                }
                return "default_start"; // Final fallback
            }
        }


        // Add a new entry to the history
        public void AddHistoryLine(string line)
        {
            dialogueHistory.Add(line);
            // Optional: Limit history size if needed
            // const int MAX_HISTORY = 500;
            // while (dialogueHistory.Count > MAX_HISTORY)
            // {
            //     dialogueHistory.RemoveAt(0);
            // }
        }
    }
}
