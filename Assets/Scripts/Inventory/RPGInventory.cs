using System;
using System.Collections.Generic;
using System.Linq;
using Inventory.Items;
using UnityEngine;

namespace Inventory
{
    public class RPGInventory : MonoBehaviour
    {
        [SerializeField] private float maxWeight = 100f;
        [SerializeField] private int maxSlots = 30;
        [SerializeField] private int currency = 0;

        private List<ItemStack> items = new List<ItemStack>();

        public float CurrentWeight { get; private set; }
        public int Currency => currency;
        public float MaxWeight => maxWeight;
        public int MaxSlots => maxSlots;

        // Events
        public event Action OnInventoryChanged;

        public IReadOnlyList<ItemStack> Items => items.AsReadOnly();

        private void Awake()
        {
            CalculateWeight();
        }

        // Add an item to the inventory
        public bool AddItem(ItemSO item, int quantity = 1)
        {
            if (item == null || quantity <= 0) return false;

            // Check weight limit
            if (CurrentWeight + (item.Weight * quantity) > maxWeight)
                return false;

            // For stackable items, try to add to existing stacks first
            if (item.IsStackable)
            {
                int remainingToAdd = quantity;

                // Try to add to existing stacks first
                foreach (var stack in items.Where(s => s.Item == item && s.CanAddToStack(1)))
                {
                    remainingToAdd = stack.AddToStack(remainingToAdd);
                    if (remainingToAdd <= 0) break;
                }

                // If we still have items to add, create new stacks
                while (remainingToAdd > 0 && items.Count < maxSlots)
                {
                    int stackSize = Mathf.Min(remainingToAdd, item.MaxStackSize);
                    items.Add(new ItemStack(item, stackSize));
                    remainingToAdd -= stackSize;
                }

                // If we couldn't add all items, return false
                if (remainingToAdd > 0) return false;
            }
            else
            {
                // For non-stackable items, each item needs its own slot
                if (items.Count + quantity > maxSlots)
                    return false;

                for (int i = 0; i < quantity; i++)
                {
                    items.Add(new ItemStack(item, 1));
                }
            }

            CalculateWeight();
            OnInventoryChanged?.Invoke();
            return true;
        }

        // Remove an item from the inventory
        public bool RemoveItem(ItemSO item, int quantity = 1)
        {
            if (item == null || quantity <= 0) return false;

            int remainingToRemove = quantity;
            List<ItemStack> stacksToRemove = new List<ItemStack>();

            // Process in reverse order to handle removal properly
            for (int i = items.Count - 1; i >= 0; i--)
            {
                var stack = items[i];
                if (stack.Item != item) continue;

                if (stack.Quantity <= remainingToRemove)
                {
                    // Remove the entire stack
                    remainingToRemove -= stack.Quantity;
                    stacksToRemove.Add(stack);
                }
                else
                {
                    // Remove part of the stack
                    stack.RemoveFromStack(remainingToRemove);
                    remainingToRemove = 0;
                }

                if (remainingToRemove <= 0) break;
            }

            // Actually remove the marked stacks
            foreach (var stack in stacksToRemove)
            {
                items.Remove(stack);
            }

            // If we couldn't remove all requested items, return false
            if (remainingToRemove > 0) return false;

            CalculateWeight();
            OnInventoryChanged?.Invoke();
            return true;
        }

        // Move an item to another inventory
        public bool MoveItemTo(RPGInventory targetInventory, int sourceIndex, int quantity = 1)
        {
            if (targetInventory == null || sourceIndex < 0 || sourceIndex >= items.Count || quantity <= 0)
                return false;

            var sourceStack = items[sourceIndex];
            if (sourceStack.Quantity < quantity)
                quantity = sourceStack.Quantity;

            // Check if target inventory can accept the item
            if (!targetInventory.CanAddItem(sourceStack.Item, quantity))
                return false;

            // Remove from source inventory
            sourceStack.RemoveFromStack(quantity);
            if (sourceStack.Quantity <= 0)
                items.RemoveAt(sourceIndex);

            // Add to target inventory
            targetInventory.AddItem(sourceStack.Item, quantity);

            CalculateWeight();
            OnInventoryChanged?.Invoke();
            return true;
        }

        // Check if we can add an item without actually adding it
        public bool CanAddItem(ItemSO item, int quantity = 1)
        {
            if (item == null || quantity <= 0) return false;

            // Check weight
            if (CurrentWeight + (item.Weight * quantity) > maxWeight)
                return false;

            // For stackable items, check existing stacks
            if (item.IsStackable)
            {
                int remainingToAdd = quantity;

                // Calculate how many we can add to existing stacks
                foreach (var stack in items.Where(s => s.Item == item))
                {
                    int canAddToStack = stack.Item.MaxStackSize - stack.Quantity;
                    remainingToAdd -= Mathf.Min(remainingToAdd, canAddToStack);
                    if (remainingToAdd <= 0) return true;
                }

                // Calculate how many new stacks we'd need
                int newStacksNeeded = Mathf.CeilToInt((float)remainingToAdd / item.MaxStackSize);
                return items.Count + newStacksNeeded <= maxSlots;
            }
            else
            {
                // For non-stackable items, each item needs its own slot
                return items.Count + quantity <= maxSlots;
            }
        }

        // Use an item at the given index
        public bool UseItem(int index, GameObject user)
        {
            if (index < 0 || index >= items.Count)
                return false;

            var stack = items[index];
            if (!stack.Item.CanUse(user))
                return false;

            stack.Item.Use(user);

            // Remove the item if it's consumable
            if (stack.Item is ConsumableItemSO)
            {
                stack.RemoveFromStack(1);
                if (stack.Quantity <= 0)
                    items.RemoveAt(index);

                CalculateWeight();
                OnInventoryChanged?.Invoke();
            }

            return true;
        }

        // Add currency to the inventory
        public void AddCurrency(int amount)
        {
            if (amount <= 0) return;

            currency += amount;
            OnInventoryChanged?.Invoke();
        }

        // Remove currency from the inventory
        public bool RemoveCurrency(int amount)
        {
            if (amount <= 0) return false;
            if (currency < amount) return false;

            currency -= amount;
            OnInventoryChanged?.Invoke();
            return true;
        }

        // Calculate the total weight of the inventory
        private void CalculateWeight()
        {
            CurrentWeight = items.Sum(stack => stack.TotalWeight);
        }

        // Split a stack at the given index
        public bool SplitStack(int index, int amount)
        {
            if (index < 0 || index >= items.Count || amount <= 0)
                return false;

            var sourceStack = items[index];
            if (sourceStack.Quantity <= amount || !sourceStack.Item.IsStackable)
                return false;

            // Check if we have a free slot
            if (items.Count >= maxSlots)
                return false;

            var newStack = sourceStack.SplitStack(amount);
            if (newStack != null)
            {
                items.Add(newStack);
                OnInventoryChanged?.Invoke();
                return true;
            }

            return false;
        }

        // Combine two stacks
        public bool CombineStacks(int sourceIndex, int targetIndex)
        {
            if (sourceIndex < 0 || sourceIndex >= items.Count ||
                targetIndex < 0 || targetIndex >= items.Count ||
                sourceIndex == targetIndex)
                return false;

            var sourceStack = items[sourceIndex];
            var targetStack = items[targetIndex];

            if (sourceStack.Item != targetStack.Item || !sourceStack.Item.IsStackable)
                return false;

            int amountToMove = Mathf.Min(sourceStack.Quantity, targetStack.Item.MaxStackSize - targetStack.Quantity);
            if (amountToMove <= 0) return false;

            sourceStack.RemoveFromStack(amountToMove);
            targetStack.AddToStack(amountToMove);

            if (sourceStack.Quantity <= 0)
                items.RemoveAt(sourceIndex);

            OnInventoryChanged?.Invoke();
            return true;
        }

        // Clear the inventory
        public void Clear()
        {
            items.Clear();
            CalculateWeight();
            OnInventoryChanged?.Invoke();
        }

        // Set the currency directly (for loading saves)
        public void SetCurrency(int amount)
        {
            currency = Mathf.Max(0, amount);
            OnInventoryChanged?.Invoke();
        }
    }
}
