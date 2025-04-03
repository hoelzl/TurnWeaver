using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Inventory.Items
{
    [CreateAssetMenu(fileName = "ItemDatabase", menuName = "Inventory/Item Database", order = 99)]
    public class ItemDatabase : ScriptableObject
    {
        [SerializeField]
        private List<ItemSO> allItems = new();

        private Dictionary<string, ItemSO> _itemLookup;

        // Public read-only access to the list if needed elsewhere
        public IReadOnlyList<ItemSO> AllItems => allItems.AsReadOnly();

        // Initialize the lookup dictionary when needed
        private void EnsureLookupInitialized()
        {
            if (_itemLookup == null)
            {
                _itemLookup = new Dictionary<string, ItemSO>(System.StringComparer.OrdinalIgnoreCase); // Case-insensitive lookup
                foreach (var item in allItems)
                {
                    if (item == null) continue;

                    string key = item.UniqueItemName;
                    if (!_itemLookup.ContainsKey(key))
                    {
                        _itemLookup.Add(key, item);
                    }
                    else
                    {
                        Debug.LogWarning($"ItemDatabase: Duplicate UniqueItemName '{key}' detected between '{_itemLookup[key].name}' and '{item.name}'. Using the first one found.", this);
                    }
                }
            }
        }

        /// <summary>
        /// Retrieves an ItemSO by its UniqueItemName (case-insensitive).
        /// Returns null if not found.
        /// </summary>
        public ItemSO GetItemByUniqueName(string uniqueName)
        {
            EnsureLookupInitialized();
            _itemLookup.TryGetValue(uniqueName, out ItemSO item);
            return item;
        }

#if UNITY_EDITOR
        // ---- Methods used by the Editor Script ----
        public List<ItemSO> GetItemListForEditor()
        {
            return allItems;
        }

        public void SetItemListForEditor(List<ItemSO> items)
        {
            allItems = items;
            _itemLookup = null; // Force lookup rebuild on next access
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
