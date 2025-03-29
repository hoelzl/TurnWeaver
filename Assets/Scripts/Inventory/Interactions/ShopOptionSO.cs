using Interaction;
using UI.Core;
using UnityEngine;

namespace Inventory.Interactions
{
    [CreateAssetMenu(fileName = "New Shop Option", menuName = "Interaction System/Shop Option", order = 3)]
    public class ShopOptionSO : InteractionOptionSO
    {
        public override IInteractionOption CreateInstance(IInteractable interactable)
        {
            return new ShopOption(interactable);
        }
    }

    public class ShopOption : InteractionOptionBase
    {
        private readonly Shop _shop;

        public ShopOption(IInteractable interactable)
        {
            Interactable = interactable;
            _shop = (interactable as Component)?.GetComponent<Shop>();
        }

        public override string Text => "Trade";

        public override IInteractable Interactable { get; }

        public override void Invoke(GameObject source)
        {
            if (_shop == null)
            {
                Debug.LogWarning("Interactable doesn't have a Shop component!");
                return;
            }

            // Get player's inventory
            var playerInventory = source.GetComponent<RPGInventory>();
            if (playerInventory == null)
            {
                Debug.LogWarning("Player doesn't have an inventory component!");
                return;
            }

            // Show shop UI
            UIManager.ShowShop(_shop, playerInventory);
        }
    }
}
