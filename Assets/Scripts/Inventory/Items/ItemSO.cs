using UnityEngine;

namespace Inventory.Items
{
    public enum ItemCategory
    {
        Weapon,
        Armor,
        Consumable,
        Material,
        Quest,
        Misc
    }

    [CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item", order = 0)]
    public class ItemSO : ScriptableObject
    {
        [Header("Basic Info")]
        [SerializeField] private string itemName;
        [Tooltip(
            "Optional unique identifier if multiple items share the same Item Name. Falls back to Item Name if empty.")]
        [SerializeField] private string uniqueItemName;
        [SerializeField] [TextArea(2, 5)] private string description;
        [SerializeField] private Sprite icon;
        [SerializeField] private ItemCategory category;

        [Header("Properties")]
        [SerializeField] private float weight = 0.1f;
        [SerializeField] private int value = 1;
        [SerializeField] private bool isStackable = true;
        [SerializeField] private int maxStackSize = 99;

        // Public getters
        public string ItemName => itemName;
        public string Description => description;
        public Sprite Icon => icon;
        public ItemCategory Category => category;
        public float Weight => weight;
        public int Value => value;
        public bool IsStackable => isStackable;
        public int MaxStackSize => maxStackSize;

        // Unique identifier for lookup, falls back to itemName if uniqueItemName is not set
        public string UniqueItemName => string.IsNullOrEmpty(uniqueItemName) ? itemName : uniqueItemName; // <-- ADDED


        // Virtual methods for item use/interaction
        public virtual bool CanUse(GameObject user) => false;

        public virtual void Use(GameObject user)
        {
        }
    }
}
