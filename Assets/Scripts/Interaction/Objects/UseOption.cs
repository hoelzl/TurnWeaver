using UI;
using UnityEngine;

namespace Interaction.Objects
{
    public class UseOption : IInteractionOption
    {
        public UseOption(IInteractable interactable)
        {
            Interactable = interactable;
        }

        public string Text => "Use";
        public IInteractable Interactable { get; }

        public void Invoke(GameObject source, UIManager uiManager)
        {
            Debug.Log("Use");
        }
    }
}
