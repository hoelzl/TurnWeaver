using UnityEngine;

namespace Interaction.Options
{
    public class UseOption : InteractionOptionBase
    {
        public UseOption(IInteractable interactable)
        {
            Interactable = interactable;
        }

        public override string Text => "Use";
        public override IInteractable Interactable { get; }

        public override void Invoke(GameObject source)
        {
            Debug.Log("Use");
        }
    }
}
