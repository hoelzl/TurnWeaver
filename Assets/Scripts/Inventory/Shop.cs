using System;
using Inventory.Items;
using UnityEngine;

namespace Inventory
{
    public class Shop : MonoBehaviour, Interaction.IInteractable
    {
        [Header("Shop")]
        [SerializeField] private string shopName = "Shop";
        [SerializeField] private RPGInventory shopInventory;
        [SerializeField, Range(1.0f, 5.0f)] private float buyMarkup = 1.5f;
        [SerializeField, Range(0.1f, 1.0f)] private float sellDiscount = 0.5f;

        [Header("Interaction")]
        [SerializeField] private Interaction.InteractionOptionSO[] interactionOptions;
        [SerializeField] private bool autoInvokeSingleOption = true;

        [Header("Shop Inventory Configuration")]
        [SerializeField] private InventoryPresetSO shopInventoryPreset;
        [SerializeField] private bool refreshInventoryOnTimeInterval;
        [SerializeField] private float inventoryRefreshInterval = 24.0f; // In-game hours
        [SerializeField] private float lastRefreshTime = 0f;

        // Stock limits
        [SerializeField] private bool limitStockQuantities;
        [SerializeField] private int maxStockPerItem = 10;

        // IInteractable implementation
        public Interaction.InteractionOptionSO[] InteractionOptions => interactionOptions;
        public bool AutoInvokeSingleOption => autoInvokeSingleOption;

        // Events
        public event Action OnShopInteractionComplete;

        public InventoryPresetSO ShopInventoryPreset => shopInventoryPreset;

        public string ShopName => shopName;

        private void Awake()
        {
            if (shopInventory == null)
                shopInventory = GetComponent<RPGInventory>();

            if (shopInventory == null)
                shopInventory = gameObject.AddComponent<RPGInventory>();
        }

        private void Start()
        {
            // Initialize shop inventory
            if (shopInventoryPreset != null)
            {
                shopInventoryPreset.ApplyToInventory(shopInventory);
            }
        }

        // Method to refresh the shop inventory using its preset
        public void RefreshInventory()
        {
            if (shopInventoryPreset != null)
            {
                // Preserve currency when refreshing
                int currentCurrency = shopInventory.Currency;

                // Apply preset
                shopInventoryPreset.ApplyToInventory(shopInventory);

                // Restore currency
                shopInventory.SetCurrency(currentCurrency);

                lastRefreshTime = Time.time;

                // Apply stock limits if enabled
                if (limitStockQuantities)
                {
                    ApplyStockLimits();
                }
            }
        }

        private void ApplyStockLimits()
        {
            var items = shopInventory.Items;
            for (int i = 0; i < items.Count; i++)
            {
                var itemStack = items[i];
                if (itemStack.Quantity > maxStockPerItem)
                {
                    // This is a bit of a hack - we're relying on knowing the implementation
                    // Ideally, the Inventory class would have a SetItemQuantity method
                    shopInventory.RemoveItem(itemStack.Item, itemStack.Quantity - maxStockPerItem);
                }
            }
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
        public bool BuyItem(RPGInventory playerInventory, int shopItemIndex, int quantity = 1)
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
        public bool SellItem(RPGInventory playerInventory, int playerItemIndex, int quantity = 1)
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
