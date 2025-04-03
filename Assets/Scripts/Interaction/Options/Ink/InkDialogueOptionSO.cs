using UnityEngine;
using Dialogue.Ink;

// Namespace for InkRunner, InkStoryManager if needed

namespace Interaction.Options.Ink
{
    [CreateAssetMenu(fileName = "NewInkDialogueOption", menuName = "Interaction System/Ink Dialogue Option", order = 10)]
    public class InkDialogueOptionSO : InteractionOptionSO
    {
        // Remove InkJsonAsset - Assuming one global story in InkStoryManager
        // [Tooltip("The compiled Ink JSON file.")]
        // [SerializeField] private TextAsset inkJsonAsset;

        // We still might need the start path specific TO THIS INTERACTION OPTION
        // if it overrides the NPC's default/continuation. Let's keep it simple
        // and rely on NPCInkData for start/continuation for now.

        // Keep the standard text/icon for the *interaction menu itself*
        // The actual dialogue content comes from Ink

        // public TextAsset InkJsonAsset => inkJsonAsset; // Removed

        public override IInteractionOption CreateInstance(IInteractable interactable)
        {
            // Validate essential context (like InkStoryManager existing) if desired,
            // but the Option instance itself can do runtime checks.
            // if (InkStoryManager.Instance == null) {
            //     Debug.LogError($"Cannot create InkDialogueOption '{name}': InkStoryManager not found in scene.");
            //     return null;
            // }

            // Ensure the interactable is suitable (e.g., has NPCInkData)
             var npcInkData = (interactable as Component)?.GetComponent<NPCInkData>();
             if (npcInkData == null) {
                  Debug.LogError($"Cannot create InkDialogueOption '{name}': Interactable '{interactable}' is missing NPCInkData component.", (interactable as Component));
                  return null;
             }


            return new InkDialogueOption(interactable, this);
        }
    }
}
