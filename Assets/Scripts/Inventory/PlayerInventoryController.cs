// PlayerInventoryController.cs

using Inventory.Items;
using Player;
using UnityEngine;

namespace Inventory
{
    [RequireComponent(typeof(PlayerController))]
    [RequireComponent(typeof(RPGInventory))]
    [RequireComponent(typeof(Equipment))]
    public class PlayerInventoryController : MonoBehaviour
    {
        private PlayerController _playerController;
        private RPGInventory _inventory;
        private Equipment _equipment;

        private void Awake()
        {
            _playerController = GetComponent<PlayerController>();
            _inventory = GetComponent<RPGInventory>();
            _equipment = GetComponent<Equipment>();
        }

        // Reference to check if the player can interact with items
        public bool CanInteractWithInventory => !_playerController.IsInteractionBlocked;

        // Methods to expose inventory functionality to UI and other systems

        public bool EquipItem(int inventoryIndex)
        {
            if (!CanInteractWithInventory) return false;

            if (inventoryIndex >= 0 && inventoryIndex < _inventory.Items.Count)
            {
                var item = _inventory.Items[inventoryIndex].Item as EquipmentItemSO;
                if (item != null)
                {
                    return _equipment.EquipItem(item);
                }
            }

            return false;
        }

        public bool UseItem(int inventoryIndex)
        {
            if (!CanInteractWithInventory) return false;

            return _inventory.UseItem(inventoryIndex, gameObject);
        }

        public bool DropItem(int inventoryIndex, int quantity = 1)
        {
            if (!CanInteractWithInventory) return false;

            if (inventoryIndex < 0 || inventoryIndex >= _inventory.Items.Count)
                return false;

            var stack = _inventory.Items[inventoryIndex];

            // Here you would create a dropped item GameObject at the player's location
            Debug.Log($"Dropping {quantity}x {stack.Item.ItemName}");

            // Remove from inventory
            return _inventory.RemoveItem(stack.Item, quantity);
        }
    }
}
