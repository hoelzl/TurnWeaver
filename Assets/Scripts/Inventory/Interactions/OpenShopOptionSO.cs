using Interaction;
using UnityEngine;

namespace Inventory.Interactions
{
    [CreateAssetMenu(fileName = "New Shop Option", menuName = "Interaction System/Shop Option", order = 2)]
    public class OpenShopOptionSO : InteractionOptionSO
    {
        public override IInteractionOption CreateInstance(IInteractable interactable)
        {
            return new OpenShopOption(interactable);
        }
    }

    public class OpenShopOption : InteractionOptionBase
    {
        private readonly Shop _shop;

        public OpenShopOption(IInteractable interactable)
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
            var playerInventory = source.GetComponent<Inventory>();
            if (playerInventory == null)
            {
                Debug.LogWarning("Player doesn't have an inventory component!");
                return;
            }

            // Show shop UI - you'll implement this in UI layer
            Debug.Log($"Opening shop UI for {_shop.ShopName}");
            // UI.Core.UIManager.ShowShop(_shop, playerInventory);
        }
    }
}
