// For NPCs
using UnityEngine;

public class NPC : MonoBehaviour, IInteractable
{
    [SerializeField] private string npcName = "NPC";
    [SerializeField] private string[] interactionOptions = { "Talk", "Trade", "Attack" };
    [SerializeField] private bool isHostile = false;
    [SerializeField] private int health = 100;

    [Header("Dialogue")]
    [SerializeField] private string[] dialogueLines;

    public string[] GetInteractionOptions()
    {
        // Could dynamically change based on NPC state, quests, etc.
        return interactionOptions;
    }

    public void Interact(PlayerController player)
    {
        // Default to talking
        InteractWithOption(player, "Talk");
    }

    public void InteractWithOption(PlayerController player, string option)
    {
        switch (option)
        {
            case "Talk":
                StartDialogue(player);
                break;

            case "Trade":
                OpenTradeWindow(player);
                break;

            case "Attack":
                StartCombat(player);
                break;

            default:
                Debug.Log($"Interaction {option} not implemented for {npcName}");
                break;
        }
    }

    private void StartDialogue(PlayerController player)
    {
        Debug.Log($"{npcName} says: {(dialogueLines.Length > 0 ? dialogueLines[0] : "Hello!")}");
        // Here you would trigger your dialogue system
    }

    private void OpenTradeWindow(PlayerController player)
    {
        Debug.Log($"Trading with {npcName}");
        // Open your trade UI here
    }

    private void StartCombat(PlayerController player)
    {
        Debug.Log($"Starting combat with {npcName}");
        isHostile = true;
        // Trigger combat system
    }

    public void TakeDamage(int amount)
    {
        health -= amount;
        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"{npcName} has been defeated!");
        // Death animation, loot drops, quest updates, etc.

        // Update interaction options
        interactionOptions = new[] { "Loot" };
    }
}
