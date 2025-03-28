using UnityEngine;

namespace Inventory.Items
{
    [System.Serializable]
    public class ItemStack
    {
        [SerializeField] private ItemSO item;
        [SerializeField] private int quantity;

        public ItemSO Item => item;
        public int Quantity => quantity;

        public ItemStack(ItemSO item, int quantity = 1)
        {
            this.item = item;
            this.quantity = Mathf.Clamp(quantity, 0, item.IsStackable ? item.MaxStackSize : 1);
        }

        public float TotalWeight => item.Weight * quantity;

        public bool CanAddToStack(int amount)
        {
            if (!item.IsStackable) return quantity == 0;
            return quantity + amount <= item.MaxStackSize;
        }

        public int AddToStack(int amount)
        {
            if (!item.IsStackable && quantity > 0) return amount; // Can't add to non-stackable items

            int maxCanAdd = item.MaxStackSize - quantity;
            int actuallyAdded = Mathf.Min(amount, maxCanAdd);

            quantity += actuallyAdded;
            return amount - actuallyAdded; // Return remaining amount
        }

        public bool RemoveFromStack(int amount)
        {
            if (quantity < amount) return false;

            quantity -= amount;
            return true;
        }

        // Split the stack, returning a new stack with the specified amount
        public ItemStack SplitStack(int amount)
        {
            if (amount <= 0 || amount >= quantity) return null;

            RemoveFromStack(amount);
            return new ItemStack(item, amount);
        }
    }
}
