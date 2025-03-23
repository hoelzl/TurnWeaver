using UnityEngine;

namespace Interaction
{
    public class NPC : MonoBehaviour, IInteractable
    {
        [SerializeField] private string npcName = "NPC";
        [SerializeField] private string[] interactionOptions = {"Talk", "Trade", "Attack"};
        [SerializeField] private bool isHostile = false;
        [SerializeField] private int health = 100;

        [Header("Dialogue")]
        [SerializeField] private string[] dialogueLines;

        public string[] GetInteractionOptions()
        {
            // Could dynamically change based on NPC state, quests, etc.
            return interactionOptions;
        }

        public void Interact(IInteractionHandler handler)
        {
            // Default to talking
            InteractWithOption(handler, "Talk");
        }

        public void InteractWithOption(IInteractionHandler handler, string option)
        {
            switch (option)
            {
                case "Talk":
                    StartDialogue();
                    break;

                case "Trade":
                    OpenTradeWindow();
                    break;

                case "Attack":
                    StartCombat();
                    break;

                default:
                    Debug.Log($"Interaction {option} not implemented for {npcName}");
                    break;
            }
        }

        public Transform GetTransform()
        {
            return transform;
        }

        private void StartDialogue()
        {
            Debug.Log($"{npcName} says: {(dialogueLines.Length > 0 ? dialogueLines[0] : "Hello!")}");
            // Here you would trigger your dialogue system
        }

        private void OpenTradeWindow()
        {
            Debug.Log($"Trading with {npcName}");
            // Open your trade UI here
        }

        private void StartCombat()
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
            interactionOptions = new[] {"Loot"};
        }
    }
}
