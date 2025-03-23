// For containers and usable objects

using UnityEngine;

namespace Interaction
{
    public class InteractableObject : MonoBehaviour, IInteractable
    {
        [SerializeField] private string objectName = "Object";
        [SerializeField] private string[] interactionOptions = {"Use"};
        [SerializeField] private bool isLocked;
        [SerializeField] private string requiredKeyItem = "";

        public string[] GetInteractionOptions()
        {
            return interactionOptions;
        }

        public void Interact(IInteractionHandler handler)
        {
            // Default interaction
            InteractWithOption(handler, interactionOptions[0]);
        }

        public void InteractWithOption(IInteractionHandler handler, string option)
        {
            switch (option)
            {
                case "Open":
                    OpenObject(handler);
                    break;

                case "Use":
                    UseObject(handler);
                    break;

                case "Examine":
                    ExamineObject(handler);
                    break;

                default:
                    Debug.Log($"Interaction {option} not implemented for {objectName}");
                    break;
            }
        }

        public Transform GetTransform()
        {
            return transform;
        }

        private void OpenObject(IInteractionHandler handler)
        {
            if (isLocked)
            {
                // Check if player has the key (would need an inventory system)
                // This would use a more sophisticated approach with an inventory interface
                Debug.Log($"{objectName} is locked and requires {requiredKeyItem}");

                // Here you'd check if handler has an inventory interface
                // var inventory = handler as IInventoryHolder;
                // if (inventory != null && inventory.HasItem(requiredKeyItem)) { ... }
            }
            else
            {
                Debug.Log($"Opening {objectName}");
                // Implement your opening logic here (animation, spawn items, etc.)
            }
        }

        private void UseObject(IInteractionHandler handler)
        {
            Debug.Log($"Using {objectName}");
            // Implement your use logic here

            // Example of utilizing the handler's transform
            Vector3 handlerPosition = handler.Transform.position;
            Debug.Log($"Used by entity at position {handlerPosition}");
        }

        private void ExamineObject(IInteractionHandler handler)
        {
            Debug.Log($"Examining {objectName}: It appears to be a {objectName}.");
            // Show description UI, highlight object, etc.
        }
    }
}
