// InventoryInitializer.cs

using Inventory.Items;
using UnityEngine;

namespace Inventory.Initialization
{
    public class InventoryInitializer : MonoBehaviour
    {
        [SerializeField] private RPGInventory targetInventory;

        [Header("Initialization Method")]
        [SerializeField] private bool initializeOnStart = true;
        [SerializeField] private bool clearExistingItems = true;

        [Header("Preset Configuration")]
        [SerializeField] private InventoryPresetSO inventoryPreset;
        [SerializeField] private bool usePreset = true;

        private void Awake()
        {
            if (targetInventory == null)
            {
                targetInventory = GetComponent<RPGInventory>();
            }
        }

        private void Start()
        {
            if (initializeOnStart)
            {
                InitializeInventory();
            }
        }

        private void InitializeInventory()
        {
            if (targetInventory == null) return;

            if (usePreset && inventoryPreset != null)
            {
                inventoryPreset.ApplyToInventory(targetInventory, clearExistingItems);
            }
            else
            {
                // Just use the inventory's built-in initial items
                if (clearExistingItems)
                {
                    targetInventory.Clear();
                }

                targetInventory.LoadInitialItems();
            }
        }

        // Method to change presets at runtime (useful for difficulty settings, etc.)
        public void ChangePreset(InventoryPresetSO newPreset)
        {
            inventoryPreset = newPreset;
        }
    }
}
