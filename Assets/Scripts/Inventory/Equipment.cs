using System;
using System.Collections.Generic;
using Inventory.Items;
using UnityEngine;

namespace Inventory
{
    public class Equipment : MonoBehaviour
    {
        // Dictionary to store equipped items by slot
        private Dictionary<EquipmentSlot, EquipmentItemSO> equippedItems = new Dictionary<EquipmentSlot, EquipmentItemSO>();

        // Reference to the inventory
        [SerializeField] private RPGInventory inventory;

        // Events
        public event Action OnEquipmentChanged;

        private void Awake()
        {
            if (inventory == null)
                inventory = GetComponent<RPGInventory>();
        }

        // Equip an item
        public bool EquipItem(EquipmentItemSO item)
        {
            if (item == null) return false;

            // Check if something is already equipped in this slot
            if (equippedItems.TryGetValue(item.Slot, out var currentItem))
            {
                // Unequip current item first
                UnequipItem(item.Slot);
            }

            // Find the item in inventory and remove it
            int itemIndex = -1;
            for (int i = 0; i < inventory.Items.Count; i++)
            {
                if (inventory.Items[i].Item == item)
                {
                    itemIndex = i;
                    break;
                }
            }

            if (itemIndex >= 0)
            {
                // Remove from inventory
                inventory.RemoveItem(item, 1);

                // Add to equipped items
                equippedItems[item.Slot] = item;

                // Apply stats from the item (would connect to character stats)
                ApplyItemStats(item);

                OnEquipmentChanged?.Invoke();
                return true;
            }

            return false;
        }

        // Unequip an item from a specific slot
        public bool UnequipItem(EquipmentSlot slot)
        {
            if (!equippedItems.TryGetValue(slot, out var item))
                return false;

            // Check if inventory has space
            if (!inventory.CanAddItem(item))
                return false;

            // Remove item stats
            RemoveItemStats(item);

            // Remove from equipped items
            equippedItems.Remove(slot);

            // Add back to inventory
            inventory.AddItem(item);

            OnEquipmentChanged?.Invoke();
            return true;
        }

        // Get the item equipped in a specific slot
        public EquipmentItemSO GetEquippedItem(EquipmentSlot slot)
        {
            equippedItems.TryGetValue(slot, out var item);
            return item;
        }

        // Get all equipped items
        public IReadOnlyDictionary<EquipmentSlot, EquipmentItemSO> GetAllEquippedItems()
        {
            return equippedItems;
        }

        // Apply the stats from an equipped item
        private void ApplyItemStats(EquipmentItemSO item)
        {
            // This would connect to your character stats system
            Debug.Log($"Applied stats from {item.ItemName}: STR+{item.Strength}, DEX+{item.Dexterity}, INT+{item.Intelligence}");
        }

        // Remove the stats from an unequipped item
        private void RemoveItemStats(EquipmentItemSO item)
        {
            // This would connect to your character stats system
            Debug.Log($"Removed stats from {item.ItemName}: STR-{item.Strength}, DEX-{item.Dexterity}, INT-{item.Intelligence}");
        }
    }
}
