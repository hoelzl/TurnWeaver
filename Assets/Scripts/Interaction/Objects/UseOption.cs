using UI;
using UnityEngine;

namespace Interaction.Objects
{
    public class UseOption : InteractionOptionBase
    {
        public UseOption(IInteractable interactable)
        {
            Interactable = interactable;
        }

        public override string Text => "Use";
        public override IInteractable Interactable { get; }

        public override void Invoke(GameObject source, UIManager uiManager)
        {
            Debug.Log("Use");
        }
    }
}
