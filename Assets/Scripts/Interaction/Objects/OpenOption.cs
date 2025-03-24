using UI;
using UnityEngine;

namespace Interaction.Objects
{
    public class OpenOption : IInteractionOption
    {
        public OpenOption(IInteractable interactable)
        {
            Interactable = interactable;
        }

        public string Text => "Open";
        public IInteractable Interactable { get; }

        public void Invoke(GameObject source, UIManager uiManager)
        {

        }
    }
}
