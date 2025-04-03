using UnityEngine;

namespace Interaction.Options
{
    public class OpenOption : InteractionOptionBase
    {
        public OpenOption(IInteractable interactable)
        {
            Interactable = interactable;
        }

        public override string Text => "Open";
        public override IInteractable Interactable { get; }

        public override void Invoke(GameObject source)
        {

        }
    }
}
