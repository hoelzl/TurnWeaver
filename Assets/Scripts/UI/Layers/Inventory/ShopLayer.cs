using System.Collections.Generic;
using Inventory;
using Inventory.Items;
using UI.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Layers.Inventory
{
    public class ShopLayer : UILayer
    {
        [SerializeField] private VisualTreeAsset itemSlotTemplate;

        private Shop _shop;
        private RPGInventory _playerInventory;

        private Label _shopTitleLabel;
        private Label _shopNameLabel;
        private Label _shopGoldLabel;
        private VisualElement _shopGrid;
        private readonly List<VisualElement> _shopSlots = new List<VisualElement>();

        private Label _playerGoldLabel;
        private Label _playerWeightLabel;
        private VisualElement _playerGrid;
        private readonly List<VisualElement> _playerSlots = new List<VisualElement>();

        private Button _closeButton;

        public void SetShop(Shop shop, RPGInventory playerInventory)
        {
            // Unsubscribe from previous events
            UnsubscribeFromEvents();

            _shop = shop;
            _playerInventory = playerInventory;

            // Subscribe to new events
            SubscribeToEvents();
            RefreshDisplay();
        }

        protected override void SetupUI()
        {
            base.SetupUI();

            if (Root == null) return;

            // Get references to UI elements
            _shopTitleLabel = Root.Q<Label>("shop-title");
            _shopNameLabel = Root.Q<Label>("shop-name-label");
            _shopGoldLabel = Root.Q<Label>("shop-gold-label");
            _shopGrid = Root.Q<VisualElement>("shop-grid");

            _playerGoldLabel = Root.Q<Label>("player-gold-label");
            _playerWeightLabel = Root.Q<Label>("player-weight-label");
            _playerGrid = Root.Q<VisualElement>("player-grid");

            _closeButton = Root.Q<Button>("close-button");

            if (_closeButton != null)
            {
                _closeButton.clicked += OnCloseClicked;
            }
        }

        private void RefreshDisplay()
        {
            RefreshShopDisplay();
            RefreshPlayerDisplay();
        }

        private void RefreshShopDisplay()
        {
            if (_shop == null || _shopGrid == null) return;

            RPGInventory shopInventory = _shop.GetComponent<RPGInventory>();
            if (shopInventory == null) return;

            if (_shopTitleLabel != null)
            {
                _shopTitleLabel.text = string.Format(_shop.HeaderFormat, _shop.ShopName);
            }

            if (_shopNameLabel != null)
            {
                _shopNameLabel.text = _shop.ShopName;
            }

            if (_shopGoldLabel != null)
            {
                _shopGoldLabel.text = $"Gold: {shopInventory.Currency}";
            }

            // Before clearing, remove event handlers from existing buttons
            foreach (VisualElement slot in _shopSlots)
            {
                var slotButton = slot.Q<Button>("item-slot");
                slotButton?.ClearBindings();
            }
            // Clear existing items
            _shopGrid.Clear();
            _shopSlots.Clear();

            // Add item slots for shop items
            for (int i = 0; i < shopInventory.Items.Count; i++)
            {
                TemplateContainer slotElement = itemSlotTemplate.Instantiate();
                _shopGrid.Add(slotElement);
                _shopSlots.Add(slotElement);

                var itemStack = shopInventory.Items[i];
                SetupShopItemSlot(slotElement, i, itemStack);
            }
        }

        private void RefreshPlayerDisplay()
        {
            if (_playerInventory == null || _playerGrid == null) return;

            // Update labels
            if (_playerGoldLabel != null)
            {
                _playerGoldLabel.text = $"Gold: {_playerInventory.Currency}";
            }

            if (_playerWeightLabel != null)
            {
                _playerWeightLabel.text = $"Weight: {_playerInventory.CurrentWeight:F1}/{_playerInventory.MaxWeight:F1}";
            }

            // Clear existing items
            _playerGrid.Clear();
            _playerSlots.Clear();

            // Add item slots for player items
            for (int i = 0; i < _playerInventory.Items.Count; i++)
            {
                TemplateContainer slotElement = itemSlotTemplate.Instantiate();
                _playerGrid.Add(slotElement);
                _playerSlots.Add(slotElement);

                var itemStack = _playerInventory.Items[i];
                SetupPlayerItemSlot(slotElement, i, itemStack);
            }
        }

        private void SetupShopItemSlot(VisualElement slotElement, int index, ItemStack itemStack)
        {
            Button slotButton = slotElement.Q<Button>("item-slot");

            if (slotButton != null)
            {
                // Add click handler
                slotButton.clicked += () => OnShopItemClicked(index);

                // Set the item icon
                VisualElement iconElement = slotButton.Q<VisualElement>("item-icon");
                if (iconElement != null && itemStack.Item != null)
                {
                    iconElement.style.backgroundImage = new StyleBackground(itemStack.Item.Icon);
                }

                // Set quantity label
                Label quantityLabel = slotButton.Q<Label>("item-quantity");
                if (quantityLabel != null)
                {
                    if (itemStack.Quantity > 1)
                    {
                        quantityLabel.text = itemStack.Quantity.ToString();
                        quantityLabel.style.display = DisplayStyle.Flex;
                    }
                    else
                    {
                        quantityLabel.text = "";
                        quantityLabel.style.display = DisplayStyle.None;
                    }
                }

                // Add price label
                Label priceLabel = slotButton.Q<Label>("price-label");
                if (priceLabel != null)
                {
                    int buyPrice = _shop.GetBuyPrice(itemStack.Item);
                    priceLabel.text = buyPrice.ToString();
                    priceLabel.style.display = DisplayStyle.Flex;
                }
            }
        }

        private void SetupPlayerItemSlot(VisualElement slotElement, int index, ItemStack itemStack)
        {
            Button slotButton = slotElement.Q<Button>("item-slot");

            if (slotButton != null)
            {
                // Add click handler
                slotButton.clicked += () => OnPlayerItemClicked(index);

                // Set the item icon
                VisualElement iconElement = slotButton.Q<VisualElement>("item-icon");
                if (iconElement != null && itemStack.Item != null)
                {
                    iconElement.style.backgroundImage = new StyleBackground(itemStack.Item.Icon);
                }

                // Set quantity label
                Label quantityLabel = slotButton.Q<Label>("item-quantity");
                if (quantityLabel != null)
                {
                    if (itemStack.Quantity > 1)
                    {
                        quantityLabel.text = itemStack.Quantity.ToString();
                        quantityLabel.style.display = DisplayStyle.Flex;
                    }
                    else
                    {
                        quantityLabel.text = "";
                        quantityLabel.style.display = DisplayStyle.None;
                    }
                }

                // Add price label
                Label priceLabel = slotButton.Q<Label>("price-label");
                if (priceLabel != null)
                {
                    int sellPrice = _shop.GetSellPrice(itemStack.Item);
                    priceLabel.text = sellPrice.ToString();
                    priceLabel.style.display = DisplayStyle.Flex;
                }
            }
        }

        private void OnShopItemClicked(int index)
        {
            if (_shop == null || _playerInventory == null) return;

            RPGInventory shopInventory = _shop.GetComponent<RPGInventory>();
            if (shopInventory == null || index < 0 || index >= shopInventory.Items.Count) return;

            var itemStack = shopInventory.Items[index];

            // For stackable items with quantity > 1, show quantity selector
            if (itemStack.Quantity > 1 && itemStack.Item.IsStackable)
            {
                UIManager.ShowQuantitySelector(
                    "Buy Amount",
                    1,
                    itemStack.Quantity,
                    (quantity) => {
                        // Check if player can afford it
                        int totalCost = _shop.GetBuyPrice(itemStack.Item) * quantity;
                        if (_playerInventory.Currency >= totalCost)
                        {
                            _shop.BuyItem(_playerInventory, index, quantity);
                        }
                        else
                        {
                            // Show message that player can't afford it
                            UIManager.ShowDescription("Not enough gold!");
                        }
                    }
                );
            }
            else
            {
                // Buy single item
                int cost = _shop.GetBuyPrice(itemStack.Item);
                if (_playerInventory.Currency >= cost)
                {
                    _shop.BuyItem(_playerInventory, index);
                }
                else
                {
                    // Show message that player can't afford it
                    UIManager.ShowDescription("Not enough gold!");
                }
            }
        }

        private void OnPlayerItemClicked(int index)
        {
            if (_shop == null || _playerInventory == null) return;
            if (index < 0 || index >= _playerInventory.Items.Count) return;

            var itemStack = _playerInventory.Items[index];

            // For stackable items with quantity > 1, show quantity selector
            if (itemStack.Quantity > 1 && itemStack.Item.IsStackable)
            {
                UIManager.ShowQuantitySelector(
                    "Sell Amount",
                    1,
                    itemStack.Quantity,
                    (quantity) => {
                        _shop.SellItem(_playerInventory, index, quantity);
                    }
                );
            }
            else
            {
                // Sell single item
                _shop.SellItem(_playerInventory, index);
            }
        }

        private void OnCloseClicked()
        {
            UILayerManager.Instance.PopLayer();
            _shop?.NotifyInteractionComplete();
        }

        private void SubscribeToEvents()
        {
            if (_shop != null)
            {
                var shopInventory = _shop.GetComponent<RPGInventory>();
                if (shopInventory != null)
                {
                    shopInventory.OnInventoryChanged += RefreshDisplay;
                }
            }

            if (_playerInventory != null)
            {
                _playerInventory.OnInventoryChanged += RefreshDisplay;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (_shop != null)
            {
                var shopInventory = _shop.GetComponent<RPGInventory>();
                if (shopInventory != null)
                {
                    shopInventory.OnInventoryChanged -= RefreshDisplay;
                }
            }

            if (_playerInventory != null)
            {
                _playerInventory.OnInventoryChanged -= RefreshDisplay;
            }
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();

            if (_closeButton != null)
            {
                _closeButton.clicked -= OnCloseClicked;
            }

            // Clean up slot button click handlers
            foreach (var slot in _shopSlots)
            {
                Button slotButton = slot.Q<Button>("item-slot");
                slotButton?.ClearBindings();
            }

            foreach (var slot in _playerSlots)
            {
                Button slotButton = slot.Q<Button>("item-slot");
                slotButton?.ClearBindings();
            }
        }
    }
}
