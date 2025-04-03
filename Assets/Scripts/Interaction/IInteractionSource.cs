using UnityEngine;

namespace Interaction
{
    // Interface for entities that can interact with objects
    public interface IInteractionSource
    {
        // Called by the interactable/interaction system to indicate that the interaction is complete
        void FinalizeInteraction(IInteractable interactable);

        // The range at which we can trigger interactions
        float InteractionRange { get; }

        // Our current transform, necessary to determine whether we are in range for a transaction
        Transform Transform { get; }
    }
}
