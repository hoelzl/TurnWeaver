using UI;
using UnityEngine;

namespace Interaction.Objects
{
    public class OpenOption : InteractionOptionBase
    {
        public OpenOption(IInteractable interactable)
        {
            Interactable = interactable;
        }

        public override string Text => "Open";
        public override IInteractable Interactable { get; }

        public override void Invoke(GameObject source, UIManager uiManager)
        {

        }
    }
}
