using Interaction;
using UI.Core;
using UnityEngine;

namespace Inventory.Interactions
{
    [CreateAssetMenu(fileName = "New Open Inventory Option", menuName = "Interaction System/Open Inventory Option",
        order = 1)]
    public class OpenInventoryOptionSO : InteractionOptionSO
    {
        public override IInteractionOption CreateInstance(IInteractable interactable)
        {
            return new OpenInventoryOption(interactable);
        }
    }

    public class OpenInventoryOption : InteractionOptionBase
    {
        public OpenInventoryOption(IInteractable interactable)
        {
            Interactable = interactable;
        }

        public override string Text => "Open Inventory";

        public override IInteractable Interactable { get; }

        public override void Invoke(GameObject source)
        {
            // Get player's inventory
            var playerInventory = source.GetComponent<RPGInventory>();
            if (playerInventory == null)
            {
                Debug.LogWarning("Player doesn't have an inventory component!");
                return;
            }

            // Show inventory UI
            UIManager.ShowInventory(playerInventory);
        }
    }
}
