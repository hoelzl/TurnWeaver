using UnityEngine;

namespace Interaction.Dialogue
{
    [CreateAssetMenu(fileName = "NewDialogue", menuName = "Interaction System/Dialogue Option", order = 0)]
    public class DialogueOptionSO : InteractionOptionSO
    {
        [SerializeField] private string dialogueText;
        [SerializeField] private string[] dialogueOptions;

        public override IInteractionOption CreateInstance(IInteractable interactable)
        {
            return new DialogueOption(interactable, dialogueText, dialogueOptions);
        }
    }
}
