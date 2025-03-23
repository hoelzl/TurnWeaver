using UnityEngine;

namespace Interaction
{
    // Interactable interface
    public interface IInteractable
    {
        string[] GetInteractionOptions();
        void Interact(IInteractionHandler handler);
        void InteractWithOption(IInteractionHandler handler, string option);
        Transform GetTransform();
    }
}
