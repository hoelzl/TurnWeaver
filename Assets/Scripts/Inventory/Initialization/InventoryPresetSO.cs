// InventoryPresetSO.cs

using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Items
{
    [CreateAssetMenu(fileName = "New Inventory Preset", menuName = "Inventory/Inventory Preset", order = 10)]
    public class InventoryPresetSO : ScriptableObject
    {
        [System.Serializable]
        public class PresetItemEntry
        {
            public ItemSO item;
            public int minQuantity = 1;
            public int maxQuantity = 1;
            [Range(0, 1)] public float spawnChance = 1.0f;
        }

        [Header("Basic Settings")]
        [SerializeField] private string presetName;
        [SerializeField] private int baseCurrency;
        [SerializeField] private int currencyVariation;

        [Header("Items")]
        [SerializeField] private List<PresetItemEntry> guaranteedItems = new List<PresetItemEntry>();
        [SerializeField] private List<PresetItemEntry> randomItems = new List<PresetItemEntry>();
        [SerializeField] private int maxRandomItems; // 0 means include all successful rolls

        [Header("Nested Presets")]
        [SerializeField] private List<InventoryPresetSO> includedPresets = new List<InventoryPresetSO>();

        public string PresetName => presetName;

        public void ApplyToInventory(RPGInventory inventory, bool clearExisting = true)
        {
            if (inventory == null) return;

            // Clear existing inventory if requested
            if (clearExisting)
            {
                inventory.Clear();
            }

            // Apply currency
            int finalCurrency = baseCurrency;
            if (currencyVariation > 0)
            {
                finalCurrency += Random.Range(-currencyVariation, currencyVariation + 1);
            }

            inventory.AddCurrency(finalCurrency);

            // Apply guaranteed items
            foreach (var entry in guaranteedItems)
            {
                if (entry.item != null && Random.value <= entry.spawnChance)
                {
                    int quantity = (entry.minQuantity == entry.maxQuantity) ?
                        entry.minQuantity :
                        Random.Range(entry.minQuantity, entry.maxQuantity + 1);

                    inventory.AddItem(entry.item, quantity);
                }
            }

            // Apply random items
            if (randomItems.Count > 0 && maxRandomItems != 0)
            {
                // Shuffle the list to get random order
                List<PresetItemEntry> shuffledItems = new List<PresetItemEntry>(randomItems);
                ShuffleList(shuffledItems);

                int itemsAdded = 0;
                foreach (var entry in shuffledItems)
                {
                    if (entry.item != null && Random.value <= entry.spawnChance)
                    {
                        int quantity = (entry.minQuantity == entry.maxQuantity) ?
                            entry.minQuantity :
                            Random.Range(entry.minQuantity, entry.maxQuantity + 1);

                        inventory.AddItem(entry.item, quantity);

                        itemsAdded++;
                        if (maxRandomItems > 0 && itemsAdded >= maxRandomItems)
                            break;
                    }
                }
            }

            // Apply included presets
            foreach (var preset in includedPresets)
            {
                if (preset != null && preset != this) // Avoid recursion
                {
                    preset.ApplyToInventory(inventory, false);
                }
            }
        }

        private static void ShuffleList<T>(List<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Random.Range(0, n + 1);
                (list[k], list[n]) = (list[n], list[k]);
            }
        }
    }
}
