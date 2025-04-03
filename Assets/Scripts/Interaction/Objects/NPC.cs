using UnityEngine;
using Dialogue.Ink; // Add this

namespace Interaction.Objects
{
    [RequireComponent(typeof(NPCInkData))] // Ensure NPCInkData is present
    public class NPC : MonoBehaviour, IInteractable
    {
        [SerializeField] private string npcName = "NPC";
        [SerializeField] private InteractionOptionSO[] interactionOptions;
        [SerializeField] private bool autoInvokeSingleOption = true;

        // Public property for NPCInkData to access
        public string NpcName => npcName;

        // Reference to the NPCInkData component (fetched automatically)
        private NPCInkData _inkData;
        public NPCInkData InkData => _inkData ??= GetComponent<NPCInkData>();

        // IInteractable Implementation
        public InteractionOptionSO[] InteractionOptions => interactionOptions;
        public bool AutoInvokeSingleOption => autoInvokeSingleOption;

        // Optional: You might want methods here related to the NPC's state
        // that could be accessed by Ink external functions via the InkStoryManager
        // or directly if the function gets a reference to the specific NPC.
    }
}
