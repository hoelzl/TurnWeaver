using Interaction;
using UnityEngine;

namespace Inventory.Interactions
{
    [CreateAssetMenu(fileName = "New Transfer Items Option", menuName = "Interaction System/Transfer Items Option",
        order = 3)]
    public class TransferItemsOptionSO : InteractionOptionSO
    {
        public override IInteractionOption CreateInstance(IInteractable interactable)
        {
            return new TransferItemsOption(interactable);
        }
    }

    public class TransferItemsOption : InteractionOptionBase
    {
        private readonly Inventory _targetInventory;

        public TransferItemsOption(IInteractable interactable)
        {
            Interactable = interactable;
            _targetInventory = (interactable as Component)?.GetComponent<Inventory>();
        }

        public override string Text => "Transfer Items";

        public override IInteractable Interactable { get; }

        public override void Invoke(GameObject source)
        {
            if (_targetInventory == null)
            {
                Debug.LogWarning("Interactable doesn't have an Inventory component!");
                return;
            }

            // Get player's inventory
            var playerInventory = source.GetComponent<Inventory>();
            if (playerInventory == null)
            {
                Debug.LogWarning("Player doesn't have an inventory component!");
                return;
            }

            // Show transfer UI - you'll implement this in UI layer
            Debug.Log("Opening inventory transfer UI");
            // UI.Core.UIManager.ShowInventoryTransfer(playerInventory, _targetInventory);
        }
    }
}
