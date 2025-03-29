using System.Collections.Generic;
using Inventory;
using Inventory.Items;
using UI.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Layers.Inventory
{
    public class TransferLayer : UILayer
    {
        [SerializeField] private VisualTreeAsset itemSlotTemplate;

        private RPGInventory _sourceInventory;
        private RPGInventory _targetInventory;

        private Label _sourceNameLabel;
        private Label _sourceWeightLabel;
        private VisualElement _sourceGrid;
        private readonly List<VisualElement> _sourceSlots = new();

        private Label _targetNameLabel;
        private Label _targetWeightLabel;
        private VisualElement _targetGrid;
        private readonly List<VisualElement> _targetSlots = new();

        private Button _closeButton;

        public void SetInventories(RPGInventory source, RPGInventory target)
        {
            // Unsubscribe from previous inventories
            if (_sourceInventory != null)
            {
                _sourceInventory.OnInventoryChanged -= RefreshInventoryDisplay;
            }

            if (_targetInventory != null)
            {
                _targetInventory.OnInventoryChanged -= RefreshInventoryDisplay;
            }

            _sourceInventory = source;
            _targetInventory = target;

            // Subscribe to new inventories
            if (_sourceInventory != null)
            {
                _sourceInventory.OnInventoryChanged += RefreshInventoryDisplay;
            }

            if (_targetInventory != null)
            {
                _targetInventory.OnInventoryChanged += RefreshInventoryDisplay;
            }

            RefreshInventoryDisplay();
        }

        protected override void SetupUI()
        {
            base.SetupUI();

            if (Root == null) return;

            // Get references to UI elements
            _sourceNameLabel = Root.Q<Label>("source-name-label");
            _sourceWeightLabel = Root.Q<Label>("source-weight-label");
            _sourceGrid = Root.Q<VisualElement>("source-grid");

            _targetNameLabel = Root.Q<Label>("target-name-label");
            _targetWeightLabel = Root.Q<Label>("target-weight-label");
            _targetGrid = Root.Q<VisualElement>("target-grid");

            _closeButton = Root.Q<Button>("close-button");

            if (_closeButton != null)
            {
                _closeButton.clicked += OnCloseClicked;
            }
        }

        private void RefreshInventoryDisplay()
        {
            RefreshSourceInventory();
            RefreshTargetInventory();
        }

        private void RefreshSourceInventory()
        {
            if (_sourceInventory == null || _sourceGrid == null) return;

            // Update labels
            if (_sourceNameLabel != null)
            {
                _sourceNameLabel.text = _sourceInventory.gameObject.name;
            }

            if (_sourceWeightLabel != null)
            {
                _sourceWeightLabel.text = $"Weight: {_sourceInventory.CurrentWeight:F1}/{_sourceInventory.MaxWeight:F1}";
            }

            // Before clearing, remove event handlers from existing buttons
            foreach (VisualElement slot in _sourceSlots)
            {
                var slotButton = slot.Q<Button>("item-slot");
                slotButton?.ClearBindings();
            }

            // Clear existing items
            _sourceGrid.Clear();
            _sourceSlots.Clear();

            // Add item slots
            for (int i = 0; i < _sourceInventory.Items.Count; i++)
            {
                TemplateContainer slotElement = itemSlotTemplate.Instantiate();
                _sourceGrid.Add(slotElement);
                _sourceSlots.Add(slotElement);

                // Set up item slot
                var itemStack = _sourceInventory.Items[i];
                SetupSourceItemSlot(slotElement, i, itemStack);
            }
        }

        private void RefreshTargetInventory()
        {
            if (_targetInventory == null || _targetGrid == null) return;

            // Update labels
            if (_targetNameLabel != null)
            {
                _targetNameLabel.text = _targetInventory.gameObject.name;
            }

            if (_targetWeightLabel != null)
            {
                _targetWeightLabel.text = $"Weight: {_targetInventory.CurrentWeight:F1}/{_targetInventory.MaxWeight:F1}";
            }

            // Clear existing items
            _targetGrid.Clear();
            _targetSlots.Clear();

            // Add item slots
            for (int i = 0; i < _targetInventory.Items.Count; i++)
            {
                TemplateContainer slotElement = itemSlotTemplate.Instantiate();
                _targetGrid.Add(slotElement);
                _targetSlots.Add(slotElement);

                // Set up item slot
                var itemStack = _targetInventory.Items[i];
                SetupTargetItemSlot(slotElement, i, itemStack);
            }
        }

        private void SetupSourceItemSlot(VisualElement slotElement, int index, ItemStack itemStack)
        {
            Button slotButton = slotElement.Q<Button>("item-slot");

            if (slotButton != null)
            {
                // Add click handler
                slotButton.clicked += () => OnSourceItemClicked(index);

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
            }
        }

        private void SetupTargetItemSlot(VisualElement slotElement, int index, ItemStack itemStack)
        {
            Button slotButton = slotElement.Q<Button>("item-slot");

            if (slotButton != null)
            {
                // Add click handler
                slotButton.clicked += () => OnTargetItemClicked(index);

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
            }
        }

        private void OnSourceItemClicked(int index)
        {
            if (_sourceInventory == null || _targetInventory == null) return;
            if (index < 0 || index >= _sourceInventory.Items.Count) return;

            var item = _sourceInventory.Items[index];

            // For stackable items with quantity > 1, show quantity selector
            if (item.Quantity > 1 && item.Item.IsStackable)
            {
                UIManager.ShowQuantitySelector(
                    "Transfer Amount",
                    1,
                    item.Quantity,
                    (quantity) => {
                        _sourceInventory.MoveItemTo(_targetInventory, index, quantity);
                    }
                );
            }
            else
            {
                // Transfer single item directly
                _sourceInventory.MoveItemTo(_targetInventory, index);
            }
        }

        private void OnTargetItemClicked(int index)
        {
            if (_sourceInventory == null || _targetInventory == null) return;
            if (index < 0 || index >= _targetInventory.Items.Count) return;

            var item = _targetInventory.Items[index];

            // For stackable items with quantity > 1, show quantity selector
            if (item.Quantity > 1 && item.Item.IsStackable)
            {
                UIManager.ShowQuantitySelector(
                    "Transfer Amount",
                    1,
                    item.Quantity,
                    (quantity) => {
                        _targetInventory.MoveItemTo(_sourceInventory, index, quantity);
                    }
                );
            }
            else
            {
                // Transfer single item directly
                _targetInventory.MoveItemTo(_sourceInventory, index);
            }
        }

        private void OnCloseClicked()
        {
            UILayerManager.Instance.PopLayer();
        }

        private void OnDestroy()
        {
            // Clean up event subscriptions
            if (_sourceInventory != null)
            {
                _sourceInventory.OnInventoryChanged -= RefreshInventoryDisplay;
            }

            if (_targetInventory != null)
            {
                _targetInventory.OnInventoryChanged -= RefreshInventoryDisplay;
            }

            if (_closeButton != null)
            {
                _closeButton.clicked -= OnCloseClicked;
            }

            // Clean up slot button click handlers
            foreach (VisualElement slot in _sourceSlots)
            {
                Button slotButton = slot.Q<Button>("item-slot");
                slotButton?.ClearBindings();
            }

            foreach (VisualElement slot in _targetSlots)
            {
                Button slotButton = slot.Q<Button>("item-slot");
                slotButton?.ClearBindings();
            }
        }
    }
}
