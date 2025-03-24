using System;
using UI;
using UnityEngine;

namespace Interaction.Dialogue
{
    public class DialogueOption : IInteractionOption
    {
        private string[] _dialogueOptions;

        public DialogueOption(IInteractable interactable, string dialogueText, string[] dialogueOptions)
        {
            this.Text = dialogueText;
            this.Interactable = interactable;
            this._dialogueOptions = dialogueOptions;
        }

        public string Text { get; }
        public IInteractable Interactable { get; }

        public void Invoke(GameObject source, UIManager uiManager)
        {
        }
    }
}
