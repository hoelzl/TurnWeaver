using System;
using System.Collections.Generic;
using System.Linq;
using Inventory.Items;
using UnityEngine;

namespace Inventory
{
    [Serializable]
    public class InitialItemEntry
    {
        public ItemSO item;
        public int quantity = 1;
    }

    public class RPGInventory : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private ItemDatabase itemDatabase;

        [Header("Inventory Settings")] // Renamed header for clarity
        [SerializeField] private float maxWeight = 100f;
        [SerializeField] private int maxSlots = 30;
        // Keep currency here for runtime state, initialCurrency is for setup
        [SerializeField] private int currency;

        [Header("Initial Inventory Configuration")]
        [SerializeField] private List<InitialItemEntry> initialItems = new();
        [SerializeField] private int initialCurrency;
        [SerializeField] private InventoryPresetSO initialPreset; // <-- ADDED preset field
        [SerializeField] private bool initializeOnStart = true; // Renamed from loadItemsOnStart

        private readonly List<ItemStack> _items = new();

        public float CurrentWeight { get; private set; }
        public int Currency => currency;
        public float MaxWeight => maxWeight;
        public int MaxSlots => maxSlots;

        // Events
        public event Action OnInventoryChanged;

        public IReadOnlyList<ItemStack> Items => _items.AsReadOnly();

        // Awake is good for setting initial state based on serialized fields
        private void Awake()
        {
            if (itemDatabase == null)
            {
                Debug.LogError($"RPGInventory on {gameObject.name} requires an ItemDatabase reference!", this);
            }
            currency = initialCurrency;
            CalculateWeight();
        }

        private void Start()
        {
            if (initializeOnStart)
            {
                InitializeInventoryFromConfig();
            }
        }

        // Public method to manually trigger initialization (e.g., from Editor button)
        // Can also be used if initializeOnStart is false.
        public void InitializeInventoryFromConfig(bool clearExisting = true)
        {
            if (clearExisting)
            {
                Clear();
                currency = 0;
            }

            // Apply initial currency and items defined directly on the component
            AddCurrency(initialCurrency);

            foreach (InitialItemEntry entry in initialItems)
            {
                if (entry.item != null && entry.quantity > 0)
                {
                    AddItem(entry.item, entry.quantity);
                }
            }

            if (initialPreset != null)
            {
                // Debug.Log($"Applying initial preset '{initialPreset.name}' to inventory '{gameObject.name}'");
                initialPreset.ApplyToInventory(this, false);
            }

            // Ensure final state is consistent
            CalculateWeight();
            OnInventoryChanged?.Invoke();
            Debug.Log(
                $"Inventory '{gameObject.name}' initialized. Items: {_items.Count}, Currency: {currency}, Weight: {CurrentWeight}");
        }


        // Add an item to the inventory
        public bool AddItem(ItemSO item, int quantity = 1)
        {
            if (item == null || quantity <= 0) return false;

            // Check weight limit BEFORE trying to add
            float weightToAdd = item.Weight * quantity;
            if (CurrentWeight + weightToAdd > maxWeight)
            {
                Debug.LogWarning(
                    $"Cannot add item {item.ItemName}: Overweight. Current: {CurrentWeight}, Adding: {weightToAdd}, Max: {maxWeight}");
                return false;
            }

            bool changed = false;

            // For stackable items, try to add to existing stacks first
            if (item.IsStackable)
            {
                int remainingToAdd = quantity;

                // Try to add to existing stacks first
                foreach (var stack in _items.Where(s => s.Item == item && s.CanAddToStack(1)))
                {
                    int added = stack.AddToStack(remainingToAdd); // AddToStack returns amount *not* added
                    int actuallyAdded = remainingToAdd - added;
                    if (actuallyAdded > 0) changed = true;
                    remainingToAdd = added; // Update remaining
                    if (remainingToAdd <= 0) break;
                }

                // If we still have items to add, create new stacks
                while (remainingToAdd > 0)
                {
                    // Check slot limit before creating a new stack
                    if (_items.Count >= maxSlots)
                    {
                        Debug.LogWarning(
                            $"Cannot add item {item.ItemName}: Not enough slots. Current: {_items.Count}, Max: {maxSlots}");
                        // If we partially added items before hitting slot limit, we should ideally roll back or return a partial success state.
                        // For simplicity now, just return false if *any* part couldn't be added.
                        if (changed)
                        {
                            // We did add *some* items to existing stacks. Recalculate and notify.
                            CalculateWeight();
                            OnInventoryChanged?.Invoke();
                        }
                        return false; // Couldn't add everything
                    }

                    int stackSize = Mathf.Min(remainingToAdd, item.MaxStackSize);
                    _items.Add(new ItemStack(item, stackSize));
                    remainingToAdd -= stackSize;
                    changed = true;
                }
            }
            else // For non-stackable items
            {
                // Check if enough slots are available for all items
                if (_items.Count + quantity > maxSlots)
                {
                    Debug.LogWarning(
                        $"Cannot add item {item.ItemName}: Not enough slots for {quantity} non-stackable items. Current: {_items.Count}, Max: {maxSlots}");
                    return false;
                }

                for (int i = 0; i < quantity; i++)
                {
                    _items.Add(new ItemStack(item)); // Non-stackable always added as 1
                    changed = true;
                }
            }

            // If anything changed, update state and notify
            if (changed)
            {
                CalculateWeight();
                OnInventoryChanged?.Invoke();
                return true;
            }
            // This might happen if trying to add 0 quantity or if checks failed early
            return false;
        }

        public bool AddItem(string itemUniqueName, int quantity = 1) // <-- ADDED OVERLOAD
        {
            if (itemDatabase == null)
            {
                Debug.LogError("ItemDatabase reference is missing in RPGInventory.", this);
                return false;
            }
            ItemSO item = itemDatabase.GetItemByUniqueName(itemUniqueName);
            if (item == null)
            {
                Debug.LogWarning($"Item with unique name '{itemUniqueName}' not found in database.", this);
                return false;
            }
            return AddItem(item, quantity); // Call the original method
        }

        // Remove an item from the inventory
        public bool RemoveItem(ItemSO item, int quantity = 1)
        {
            if (item == null || quantity <= 0) return false;

            int totalAvailable = CountItem(item);
            if (totalAvailable < quantity)
            {
                Debug.LogWarning($"Cannot remove {quantity} of {item.ItemName}, only {totalAvailable} available.");
                return false; // Don't have enough to remove
            }

            int remainingToRemove = quantity;
            bool changed = false;

            // Process in reverse order to handle removing stacks safely
            for (int i = _items.Count - 1; i >= 0; i--)
            {
                var stack = _items[i];
                if (stack.Item != item) continue;

                int amountToRemoveFromStack = Mathf.Min(remainingToRemove, stack.Quantity);

                if (amountToRemoveFromStack > 0)
                {
                    stack.RemoveFromStack(amountToRemoveFromStack);
                    remainingToRemove -= amountToRemoveFromStack;
                    changed = true;

                    if (stack.Quantity <= 0)
                    {
                        _items.RemoveAt(i);
                    }
                }

                if (remainingToRemove <= 0) break;
            }

            // Should theoretically always be true if initial check passed, but good failsafe
            if (remainingToRemove > 0)
            {
                Debug.LogError(
                    $"Failed to remove all requested items ({item.ItemName}). Remainder: {remainingToRemove}. This indicates a logic error.");
                // Decide how to handle this - potentially rollback or log error
            }

            if (changed)
            {
                CalculateWeight();
                OnInventoryChanged?.Invoke();
            }
            return changed; // Return true if any item was actually removed
        }

        public bool RemoveItem(string itemUniqueName, int quantity = 1) // <-- ADDED OVERLOAD
        {
            if (itemDatabase == null)
            {
                Debug.LogError("ItemDatabase reference is missing in RPGInventory.", this);
                return false;
            }
            ItemSO item = itemDatabase.GetItemByUniqueName(itemUniqueName);
            if (item == null)
            {
                Debug.LogWarning($"Item with unique name '{itemUniqueName}' not found in database.", this);
                return false;
            }
            return RemoveItem(item, quantity); // Call the original method
        }

        // Count how many of a specific item are in the inventory
        public int CountItem(ItemSO item)
        {
            if (item == null) return 0;
            return _items.Where(stack => stack.Item == item).Sum(stack => stack.Quantity);
        }

        public int CountItem(string itemUniqueName) // <-- ADDED OVERLOAD
        {
            if (itemDatabase == null)
            {
                Debug.LogError("ItemDatabase reference is missing in RPGInventory.", this);
                return 0;
            }
            ItemSO item = itemDatabase.GetItemByUniqueName(itemUniqueName);
            if (item == null)
            {
                // Don't warn here, just return 0 as the count is zero if the item doesn't exist
                return 0;
            }
            return CountItem(item); // Call the original method
        }

        // Move an item to another inventory (simplified, assumes quantity <= stack size)
        // Needs enhancement for splitting stacks during move if quantity < stack.Quantity
        public bool MoveItemTo(RPGInventory targetInventory, int sourceIndex, int quantity = 1)
        {
            if (targetInventory == null || sourceIndex < 0 || sourceIndex >= _items.Count || quantity <= 0)
                return false;

            var sourceStack = _items[sourceIndex];
            Debug.Log(
                $"Attempting to move {quantity} of {sourceStack.Item.ItemName} from {gameObject.name} to {targetInventory.gameObject.name}");

            // Ensure we don't try to move more than exists in the stack
            quantity = Mathf.Min(quantity, sourceStack.Quantity);
            if (quantity <= 0) return false;

            var itemToMove = sourceStack.Item;

            // Check if target inventory can accept the item
            if (!targetInventory.CanAddItem(itemToMove, quantity))
            {
                Debug.LogWarning(
                    $"Target inventory {targetInventory.gameObject.name} cannot accept {quantity} of {itemToMove.ItemName}");
                return false;
            }

            // *** Transaction ***
            // 1. Remove from source (handle stack emptying)
            bool removed = sourceStack.RemoveFromStack(quantity);
            if (!removed)
            {
                Debug.LogError($"Failed to remove {quantity} from source stack {itemToMove.ItemName}");
                return false; // Should not happen if quantity was checked
            }

            bool sourceStackEmpty = sourceStack.Quantity <= 0;

            // 2. Add to target
            bool added = targetInventory.AddItem(itemToMove, quantity);
            if (!added)
            {
                Debug.LogError(
                    $"Failed to add {quantity} of {itemToMove.ItemName} to target inventory {targetInventory.gameObject.name}, rolling back source.");
                // Rollback: Add back to source stack
                sourceStack.AddToStack(quantity); // Assume this succeeds
                // Don't remove source stack if it wasn't empty before rollback
                return false;
            }

            // 3. If source stack became empty, remove it from the list
            if (sourceStackEmpty)
            {
                Debug.Log($"Source stack of {itemToMove.ItemName} is now empty, removing slot.");
                _items.RemoveAt(sourceIndex); // Remove the now-empty stack slot
            }

            // Update source inventory state
            CalculateWeight();
            OnInventoryChanged?.Invoke();

            Debug.Log($"Successfully moved {quantity} of {itemToMove.ItemName}");
            return true;
        }

        // Check if we can add an item without actually adding it
        public bool CanAddItem(ItemSO item, int quantity = 1)
        {
            if (item == null || quantity <= 0) return false;

            // Check weight
            if (CurrentWeight + (item.Weight * quantity) > maxWeight)
                return false;

            // Check slots
            int slotsRequired = 0;
            if (item.IsStackable)
            {
                int remainingToAdd = quantity;
                // Check existing stacks
                foreach (var stack in _items.Where(s => s.Item == item))
                {
                    int canAddToStack = stack.Item.MaxStackSize - stack.Quantity;
                    remainingToAdd -= Mathf.Min(remainingToAdd, canAddToStack);
                    if (remainingToAdd <= 0) break; // Fits entirely into existing stacks
                }

                // Calculate how many new stacks we'd need if items remain
                if (remainingToAdd > 0)
                {
                    slotsRequired = Mathf.CeilToInt((float) remainingToAdd / item.MaxStackSize);
                }
            }
            else // Non-stackable
            {
                slotsRequired = quantity;
            }

            return _items.Count + slotsRequired <= maxSlots;
        }

        public bool CanAddItem(string itemUniqueName, int quantity = 1) // <-- ADDED OVERLOAD
        {
            if (itemDatabase == null)
            {
                Debug.LogError("ItemDatabase reference is missing in RPGInventory.", this);
                return false;
            }
            ItemSO item = itemDatabase.GetItemByUniqueName(itemUniqueName);
            if (item == null)
            {
                Debug.LogWarning(
                    $"Item with unique name '{itemUniqueName}' not found in database. Cannot check if addable.", this);
                return false;
            }
            return CanAddItem(item, quantity); // Call the original method
        }

        // Use an item at the given index
        public bool UseItem(int index, GameObject user)
        {
            if (index < 0 || index >= _items.Count)
                return false;

            var stack = _items[index];
            var item = stack.Item; // Cache item before potential removal

            if (!item.CanUse(user))
                return false;

            item.Use(user);

            // Check if item should be consumed after use (e.g., Consumable)
            // We could add an `IsConsumedOnUse` property to ItemSO,
            // or check the type as before. Checking type is less flexible.
            bool consumed = item is ConsumableItemSO; // Example check

            if (consumed)
            {
                // Remove one from the stack
                stack.RemoveFromStack(1);
                if (stack.Quantity <= 0)
                    _items.RemoveAt(index);

                // Update state only if consumed
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
            if (amount <= 0) return false; // Cannot remove zero or negative
            if (currency < amount) return false; // Not enough currency

            currency -= amount;
            OnInventoryChanged?.Invoke();
            return true;
        }

        // Calculate the total weight of the inventory
        private void CalculateWeight()
        {
            CurrentWeight = _items.Sum(stack => stack.TotalWeight);
            // Clamp weight just in case? Should not be necessary if AddItem checks correctly.
            // CurrentWeight = Mathf.Clamp(CurrentWeight, 0, maxWeight);
        }

        // Split a stack at the given index
        public bool SplitStack(int index, int amount)
        {
            if (index < 0 || index >= _items.Count || amount <= 0)
                return false;

            var sourceStack = _items[index];
            // Cannot split if amount is too large, or item isn't stackable, or stack only has 1
            if (sourceStack.Quantity <= amount || !sourceStack.Item.IsStackable || sourceStack.Quantity <= 1)
                return false;

            // Check if we have a free slot for the new stack
            if (_items.Count >= maxSlots)
                return false;

            var newStack = sourceStack.SplitStack(amount); // SplitStack already reduces source quantity
            if (newStack != null)
            {
                _items.Add(newStack); // Add the newly created stack
                // Weight change is handled by CalculateWeight, but could optimize
                CalculateWeight();
                OnInventoryChanged?.Invoke();
                return true;
            }

            return false;
        }

        // Combine two stacks if possible
        public bool CombineStacks(int sourceIndex, int targetIndex)
        {
            if (sourceIndex < 0 || sourceIndex >= _items.Count ||
                targetIndex < 0 || targetIndex >= _items.Count ||
                sourceIndex == targetIndex)
                return false;

            var sourceStack = _items[sourceIndex];
            var targetStack = _items[targetIndex];

            // Can only combine identical, stackable items
            if (sourceStack.Item != targetStack.Item || !sourceStack.Item.IsStackable)
                return false;

            // Calculate how much can be moved to the target stack
            int spaceInTarget = targetStack.Item.MaxStackSize - targetStack.Quantity;
            int amountToMove = Mathf.Min(sourceStack.Quantity, spaceInTarget);

            if (amountToMove <= 0) return false; // Target stack is already full

            // Perform the transfer
            targetStack.AddToStack(amountToMove);
            sourceStack.RemoveFromStack(amountToMove);

            // Remove the source stack if it became empty
            if (sourceStack.Quantity <= 0)
                _items.RemoveAt(sourceIndex);

            // No weight change, but list structure changed
            OnInventoryChanged?.Invoke();
            return true;
        }

        // Clear the inventory completely
        public void Clear()
        {
            _items.Clear();
            // Reset currency too? Depends on desired behavior. Let's assume Clear means items only.
            // currency = 0;
            CalculateWeight(); // Weight will be 0
            OnInventoryChanged?.Invoke();
        }

        // Method specifically for loading/setting state (e.g., from save games)
        // Bypasses normal checks but ensures consistency.
        public void LoadState(List<ItemStack> loadedItems, int loadedCurrency)
        {
            _items.Clear();
            _items.AddRange(loadedItems); // Assumes loadedItems are valid ItemStacks
            currency = Mathf.Max(0, loadedCurrency);
            CalculateWeight();
            OnInventoryChanged?.Invoke();
            Debug.Log($"Inventory state loaded for {gameObject.name}. Items: {_items.Count}, Currency: {currency}");
        }

        // Set the currency directly (use with caution, prefer Add/Remove)
        public void SetCurrency(int amount)
        {
            currency = Mathf.Max(0, amount);
            OnInventoryChanged?.Invoke();
        }
    }
}
