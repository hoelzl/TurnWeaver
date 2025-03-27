using System;
using UI;
using UnityEngine;

namespace Interaction.Dialogue
{
    public class DialogueOption : InteractionOptionBase
    {
        private string[] _dialogueOptions;

        public DialogueOption(IInteractable interactable, string dialogueText, string[] dialogueOptions)
        {
            this.Text = dialogueText;
            this.Interactable = interactable;
            this._dialogueOptions = dialogueOptions;
        }

        public override string Text { get; }
        public override IInteractable Interactable { get; }

        public override void Invoke(GameObject source)
        {
        }
    }
}
