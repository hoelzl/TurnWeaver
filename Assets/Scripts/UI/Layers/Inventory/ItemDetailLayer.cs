using Inventory;
using Inventory.Items;
using UI.Core;
using UnityEngine.UIElements;

namespace UI.Layers.Inventory
{
    public class ItemDetailLayer : UILayer
    {
        private RPGInventory _inventory;
        private int _itemIndex;

        private VisualElement _itemIcon;
        private Label _itemNameLabel;
        private Label _itemDescriptionLabel;
        private Label _itemStatsLabel;
        private Button _useButton;
        private Button _dropButton;
        private Button _splitButton;
        private Button _closeButton;

        public void SetItem(RPGInventory inventory, int itemIndex)
        {
            _inventory = inventory;
            _itemIndex = itemIndex;

            if (_inventory != null && _itemIndex >= 0 && _itemIndex < _inventory.Items.Count)
            {
                RefreshItemDisplay();
            }
        }

        protected override void SetupUI()
        {
            base.SetupUI();

            if (Root == null) return;

            // Get references to UI elements
            _itemIcon = Root.Q<VisualElement>("item-icon");
            _itemNameLabel = Root.Q<Label>("item-name");
            _itemDescriptionLabel = Root.Q<Label>("item-description");
            _itemStatsLabel = Root.Q<Label>("item-stats");

            _useButton = Root.Q<Button>("use-button");
            _dropButton = Root.Q<Button>("drop-button");
            _splitButton = Root.Q<Button>("split-button");
            _closeButton = Root.Q<Button>("close-button");

            // Set up button click handlers
            if (_useButton != null) _useButton.clicked += OnUseClicked;
            if (_dropButton != null) _dropButton.clicked += OnDropClicked;
            if (_splitButton != null) _splitButton.clicked += OnSplitClicked;
            if (_closeButton != null) _closeButton.clicked += OnCloseClicked;
        }

        private void RefreshItemDisplay()
        {
            if (_inventory == null || _itemIndex < 0 || _itemIndex >= _inventory.Items.Count) return;

            var itemStack = _inventory.Items[_itemIndex];
            var item = itemStack.Item;

            // Set item icon
            if (_itemIcon != null && item.Icon != null)
            {
                _itemIcon.style.backgroundImage = new StyleBackground(item.Icon);
            }

            // Set item name with quantity
            if (_itemNameLabel != null)
            {
                _itemNameLabel.text = itemStack.Quantity > 1
                    ? $"{item.ItemName} ({itemStack.Quantity})"
                    : item.ItemName;
            }

            // Set item description
            if (_itemDescriptionLabel != null)
            {
                _itemDescriptionLabel.text = item.Description;
            }

            // Set item stats
            if (_itemStatsLabel != null)
            {
                string statsText = $"Weight: {item.Weight} | Value: {item.Value}";

                // Add equipment stats if applicable
                if (item is EquipmentItemSO equipment)
                {
                    statsText += $"\nSlot: {equipment.Slot}";

                    if (equipment.Armor > 0) statsText += $" | Armor: {equipment.Armor}";
                    if (equipment.Damage > 0) statsText += $" | Damage: {equipment.Damage}";

                    // Add attributes
                    var attributes = "";
                    if (equipment.Strength > 0) attributes += $" STR +{equipment.Strength}";
                    if (equipment.Dexterity > 0) attributes += $" DEX +{equipment.Dexterity}";
                    if (equipment.Intelligence > 0) attributes += $" INT +{equipment.Intelligence}";

                    if (!string.IsNullOrEmpty(attributes))
                    {
                        statsText += $"\nAttributes:{attributes}";
                    }
                }
                // Add consumable stats if applicable
                else if (item is ConsumableItemSO consumable)
                {
                    statsText += $"\nEffect: {consumable.EffectType} +{consumable.EffectValue}";
                    if (consumable.Duration > 0)
                    {
                        statsText += $" | Duration: {consumable.Duration}s";
                    }
                }

                _itemStatsLabel.text = statsText;
            }

            // Enable/disable buttons based on item type
            if (_useButton != null)
            {
                _useButton.SetEnabled(item.CanUse(_inventory.gameObject));
            }

            if (_splitButton != null)
            {
                _splitButton.SetEnabled(item.IsStackable && itemStack.Quantity > 1);
            }
        }

        private void OnUseClicked()
        {
            if (_inventory == null || _itemIndex < 0 || _itemIndex >= _inventory.Items.Count) return;

            // Use the item
            _inventory.UseItem(_itemIndex, _inventory.gameObject);

            // Close this layer if the item was consumed
            if (_itemIndex >= _inventory.Items.Count)
            {
                UILayerManager.Instance.PopLayer();
            }
            else
            {
                // Refresh display
                RefreshItemDisplay();
            }
        }

        private void OnDropClicked()
        {
            if (_inventory == null || _itemIndex < 0 || _itemIndex >= _inventory.Items.Count) return;

            // Show drop quantity dialog for stacks
            if (_inventory.Items[_itemIndex].Quantity > 1)
            {
                UIManager.ShowQuantitySelector(
                    "Drop Items",
                    1,
                    _inventory.Items[_itemIndex].Quantity,
                    (quantity) =>
                    {
                        // Drop the selected quantity
                        var playerInventoryController = _inventory.GetComponent<PlayerInventoryController>();
                        if (playerInventoryController != null)
                        {
                            playerInventoryController.DropItem(_itemIndex, quantity);
                        }
                        else
                        {
                            _inventory.RemoveItem(_inventory.Items[_itemIndex].Item, quantity);
                        }

                        // Close this layer
                        UILayerManager.Instance.PopLayer();
                    }
                );
            }
            else
            {
                // Drop single item
                var playerInventoryController = _inventory.GetComponent<PlayerInventoryController>();
                if (playerInventoryController != null)
                {
                    playerInventoryController.DropItem(_itemIndex);
                }
                else
                {
                    _inventory.RemoveItem(_inventory.Items[_itemIndex].Item);
                }

                // Close this layer
                UILayerManager.Instance.PopLayer();
            }
        }

        private void OnSplitClicked()
        {
            if (_inventory == null || _itemIndex < 0 || _itemIndex >= _inventory.Items.Count) return;

            var stack = _inventory.Items[_itemIndex];
            if (stack.Quantity <= 1) return;

            // Show split stack dialog
            UIManager.ShowQuantitySelector(
                "Split Stack",
                1,
                stack.Quantity - 1,
                (quantity) =>
                {
                    // Split the stack
                    _inventory.SplitStack(_itemIndex, quantity);

                    // Close this layer
                    UILayerManager.Instance.PopLayer();
                }
            );
        }

        private void OnCloseClicked()
        {
            UILayerManager.Instance.PopLayer();
        }

        private void OnDestroy()
        {
            // Clean up button click handlers
            if (_useButton != null) _useButton.clicked -= OnUseClicked;
            if (_dropButton != null) _dropButton.clicked -= OnDropClicked;
            if (_splitButton != null) _splitButton.clicked -= OnSplitClicked;
            if (_closeButton != null) _closeButton.clicked -= OnCloseClicked;
        }
    }
}
