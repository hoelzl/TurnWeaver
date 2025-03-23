// For containers and usable objects
using UnityEngine;

public class InteractableObject : MonoBehaviour, IInteractable
{
    [SerializeField] private string objectName = "Object";
    [SerializeField] private string[] interactionOptions = { "Use" };
    [SerializeField] private bool isLocked;
    [SerializeField] private string requiredKeyItem = "";

    public string[] GetInteractionOptions()
    {
        return interactionOptions;
    }

    public void Interact(PlayerController player)
    {
        // Default interaction
        InteractWithOption(player, interactionOptions[0]);
    }

    public void InteractWithOption(PlayerController player, string option)
    {
        switch (option)
        {
            case "Open":
                OpenObject(player);
                break;

            case "Use":
                UseObject(player);
                break;

            case "Examine":
                ExamineObject(player);
                break;

            default:
                Debug.Log($"Interaction {option} not implemented for {objectName}");
                break;
        }
    }

    private void OpenObject(PlayerController player)
    {
        if (isLocked)
        {
            // Check if player has the key (would need an inventory system)
            Debug.Log($"{objectName} is locked and requires {requiredKeyItem}");
        }
        else
        {
            Debug.Log($"Opening {objectName}");
            // Implement your opening logic here (animation, spawn items, etc.)
        }
    }

    private void UseObject(PlayerController player)
    {
        Debug.Log($"Using {objectName}");
        // Implement your use logic here
    }

    private void ExamineObject(PlayerController player)
    {
        Debug.Log($"Examining {objectName}: It appears to be a {objectName}.");
        // Show description UI, highlight object, etc.
    }
}
