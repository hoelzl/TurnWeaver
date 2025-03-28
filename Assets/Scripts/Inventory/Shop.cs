using System;
using Inventory.Items;
using UnityEngine;

namespace Inventory
{
    public class Shop : MonoBehaviour, Interaction.IInteractable
    {
        [SerializeField] private string shopName = "Shop";
        [SerializeField] private Inventory shopInventory;
        [SerializeField] private float buyMarkup = 1.5f; // Price multiplier when buying
        [SerializeField] private float sellDiscount = 0.5f; // Price multiplier when selling

        [Header("Interaction")]
        [SerializeField] private Interaction.InteractionOptionSO[] interactionOptions;
        [SerializeField] private bool autoInvokeSingleOption = true;

        // IInteractable implementation
        public Interaction.InteractionOptionSO[] InteractionOptions => interactionOptions;
        public bool AutoInvokeSingleOption => autoInvokeSingleOption;

        // Events
        public event Action OnShopInteractionComplete;

        public string ShopName => shopName;

        private void Awake()
        {
            if (shopInventory == null)
                shopInventory = GetComponent<Inventory>();

            if (shopInventory == null)
                shopInventory = gameObject.AddComponent<Inventory>();
        }

        // Calculate buy price for an item (what player pays)
        public int GetBuyPrice(ItemSO item)
        {
            return Mathf.RoundToInt(item.Value * buyMarkup);
        }

        // Calculate sell price for an item (what player receives)
        public int GetSellPrice(ItemSO item)
        {
            return Mathf.RoundToInt(item.Value * sellDiscount);
        }

        // Player buys an item from the shop
        public bool BuyItem(Inventory playerInventory, int shopItemIndex, int quantity = 1)
        {
            if (shopItemIndex < 0 || shopItemIndex >= shopInventory.Items.Count)
                return false;

            var itemStack = shopInventory.Items[shopItemIndex];
            var item = itemStack.Item;

            if (quantity > itemStack.Quantity)
                quantity = itemStack.Quantity;

            int totalCost = GetBuyPrice(item) * quantity;

            // Check if player has enough money
            if (playerInventory.Currency < totalCost)
                return false;

            // Check if player has inventory space
            if (!playerInventory.CanAddItem(item, quantity))
                return false;

            // Complete the transaction
            playerInventory.RemoveCurrency(totalCost);
            shopInventory.RemoveItem(item, quantity);
            playerInventory.AddItem(item, quantity);
            shopInventory.AddCurrency(totalCost);

            return true;
        }

        // Player sells an item to the shop
        public bool SellItem(Inventory playerInventory, int playerItemIndex, int quantity = 1)
        {
            if (playerItemIndex < 0 || playerItemIndex >= playerInventory.Items.Count)
                return false;

            var itemStack = playerInventory.Items[playerItemIndex];
            var item = itemStack.Item;

            if (quantity > itemStack.Quantity)
                quantity = itemStack.Quantity;

            int totalValue = GetSellPrice(item) * quantity;

            // Check if shop has enough money
            if (shopInventory.Currency < totalValue)
                return false;

            // Check if shop has inventory space
            if (!shopInventory.CanAddItem(item, quantity))
                return false;

            // Complete the transaction
            shopInventory.RemoveCurrency(totalValue);
            playerInventory.RemoveItem(item, quantity);
            shopInventory.AddItem(item, quantity);
            playerInventory.AddCurrency(totalValue);

            return true;
        }
    }
}
