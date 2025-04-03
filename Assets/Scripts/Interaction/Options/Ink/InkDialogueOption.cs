using UnityEngine;
using Dialogue.Ink;
using Dialogue;

namespace Interaction.Options.Ink
{
    /// <summary>
    /// Interaction option that starts a dialogue session with an NPC.
    /// </summary>
    public class InkDialogueOption : IInteractionOption
    {
        private readonly InkDialogueOptionSO _optionSO;

        public InkDialogueOption(IInteractable interactable, InkDialogueOptionSO optionSO)
        {
            this.Interactable = interactable;
            this._optionSO = optionSO;
        }

        // --- IInteractionOption Implementation ---
        public string Text => _optionSO.Text; // Text shown in the Interaction Menu
        public Sprite Icon => _optionSO.Icon; // Icon shown in the Interaction Menu
        public IInteractable Interactable { get; }

        public void Invoke(GameObject source) // 'source' is the Player typically
        {
            // Get required components
            var npcInkData = (Interactable as Component)?.GetComponent<NPCInkData>();
            var dialogueController = InkDialogueController.Instance;

            if (dialogueController == null)
            {
                Debug.LogError($"InkDialogueOption: InkDialogueController instance not found!");
                FinalizeInteraction(source);
                return;
            }

            if (npcInkData == null)
            {
                Debug.LogError($"InkDialogueOption: NPC '{Interactable}' is missing NPCInkData component!",
                    (Interactable as Component));
                FinalizeInteraction(source);
                return;
            }

            Debug.Log($"InkDialogueOption: Invoked for NPC '{npcInkData.gameObject.name}'. Starting dialogue session.");

            // Start dialogue via the controller - this is the key change
            dialogueController.StartDialogue(npcInkData);

            // The InteractionManager automatically calls FinalizeInteraction after this method
        }

        // Helper to ensure interaction source is released in case of early exit
        private void FinalizeInteraction(GameObject source)
        {
            var interactionSource = source.GetComponent<IInteractionSource>();
            interactionSource?.FinalizeInteraction(Interactable);
        }
    }
}
