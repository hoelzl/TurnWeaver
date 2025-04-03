using System.Collections.Generic;
using Inventory.Items;
using UI.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Layers.Inventory
{
    public class InventoryLayer : UILayer
    {
        [SerializeField] private VisualTreeAsset itemSlotTemplate;

        private VisualElement _inventoryGrid;
        private Label _weightLabel;
        private Label _goldLabel;
        private Button _closeButton;

        private global::Inventory.RPGInventory _inventory;
        private List<VisualElement> _itemSlots = new List<VisualElement>();

        public void SetInventory(global::Inventory.RPGInventory inventory)
        {
            // Unsubscribe from previous inventory if exists
            if (_inventory != null)
            {
                _inventory.OnInventoryChanged -= RefreshInventoryDisplay;
            }

            _inventory = inventory;

            if (_inventory != null)
            {
                _inventory.OnInventoryChanged += RefreshInventoryDisplay;
                RefreshInventoryDisplay();
            }
        }

        protected override void SetupUI()
        {
            base.SetupUI();

            if (Root == null) return;

            // Get references to UI elements
            _inventoryGrid = Root.Q<VisualElement>("inventory-grid");
            _weightLabel = Root.Q<Label>("weight-label");
            _goldLabel = Root.Q<Label>("gold-label");
            _closeButton = Root.Q<Button>("close-button");

            if (_closeButton != null)
            {
                _closeButton.clicked += OnCloseClicked;
            }
        }

        private void OnCloseClicked()
        {
            UILayerManager.Instance.PopLayer();
        }

        private void RefreshInventoryDisplay()
        {
            if (_inventory == null || _inventoryGrid == null) return;

            // Clear existing items
            _inventoryGrid.Clear();
            _itemSlots.Clear();

            // Update weight and gold
            if (_weightLabel != null)
            {
                _weightLabel.text = $"Weight: {_inventory.CurrentWeight:F1}/{_inventory.MaxWeight:F1}";
            }

            if (_goldLabel != null)
            {
                _goldLabel.text = $"Gold: {_inventory.Currency}";
            }

            // Add item slots up to max capacity
            for (int i = 0; i < _inventory.MaxSlots; i++)
            {
                TemplateContainer slotElement = itemSlotTemplate.Instantiate();
                _inventoryGrid.Add(slotElement);
                _itemSlots.Add(slotElement);

                // Set up empty slot
                SetupItemSlot(slotElement, i);
            }

            // Populate slots with actual items
            for (int i = 0; i < _inventory.Items.Count; i++)
            {
                if (i < _itemSlots.Count)
                {
                    UpdateItemSlot(_itemSlots[i], i, _inventory.Items[i]);
                }
            }
        }

        private void SetupItemSlot(VisualElement slotElement, int index)
        {
            // Get the slot button
            Button slotButton = slotElement.Q<Button>("item-slot");

            if (slotButton != null)
            {
                // Clear existing click handler and add new one
                slotButton.clicked += () => OnItemSlotClicked(index);

                // Clear the slot initially
                VisualElement iconElement = slotButton.Q<VisualElement>("item-icon");
                Label quantityLabel = slotButton.Q<Label>("item-quantity");

                if (iconElement != null)
                {
                    iconElement.style.backgroundImage = null;
                }

                if (quantityLabel != null)
                {
                    quantityLabel.text = "";
                    quantityLabel.style.display = DisplayStyle.None;
                }
            }
        }

        private void UpdateItemSlot(VisualElement slotElement, int index, ItemStack itemStack)
        {
            Button slotButton = slotElement.Q<Button>("item-slot");

            if (slotButton != null)
            {
                VisualElement iconElement = slotButton.Q<VisualElement>("item-icon");
                Label quantityLabel = slotButton.Q<Label>("item-quantity");

                if (iconElement != null && itemStack.Item != null)
                {
                    // Set the item icon
                    iconElement.style.backgroundImage = new StyleBackground(itemStack.Item.Icon);
                }

                if (quantityLabel != null)
                {
                    // Show quantity for stacks > 1
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
            }
        }

        private void OnItemSlotClicked(int index)
        {
            if (_inventory == null || index >= _inventory.Items.Count) return;

            // Show item detail layer
            UIManager.ShowItemDetail(_inventory, index);
        }

        private void OnDestroy()
        {
            if (_closeButton != null)
            {
                _closeButton.clicked -= OnCloseClicked;
            }

            if (_inventory != null)
            {
                _inventory.OnInventoryChanged -= RefreshInventoryDisplay;
            }

            // Clean up slot button click handlers
            foreach (var slot in _itemSlots)
            {
                Button slotButton = slot.Q<Button>("item-slot");
                if (slotButton != null)
                {
                    // TODO: Want to remove click only?
                    slotButton.ClearBindings();
                }
            }
        }
    }
}
