using UnityEngine;

namespace Interaction
{
    // Interface for entities that can interact with objects
    public interface IInteractionHandler
    {
        Transform Transform { get; }
        void OnInteractionComplete();
        float GetInteractionRange();
    }
}
