using UnityEngine;

namespace Interaction
{
    // A central interaction system interface
    public interface IInteractionSystem
    {
        void ShowInteractionOptions(IInteractable interactable, Vector3 worldPosition);
        void HandleInteractionSelection(IInteractable interactable, string option);
        void CancelInteraction();
    }
}
