using System.Collections.Generic;
using System.Linq;
using Inventory.Items;
using UnityEditor;
using UnityEngine;

// Make sure to include the namespace for ItemSO and ItemDatabase

namespace Inventory
{
    [CustomEditor(typeof(ItemDatabase))]
    public class ItemDatabaseEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector(); // Draws the default inspector fields (like the allItems list)

            ItemDatabase itemDatabase = (ItemDatabase)target;

            if (GUILayout.Button("Find and Populate All Items"))
            {
                PopulateDatabase(itemDatabase);
            }
        }

        private void PopulateDatabase(ItemDatabase database)
        {
            // Clear the current list
            List<ItemSO> foundItems = new List<ItemSO>();
            HashSet<string> uniqueNames = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase); // Case-insensitive check
            bool duplicatesFound = false;

            // Find all assets of type ItemSO in the project
            string[] guids = AssetDatabase.FindAssets("t:ItemSO");

            Debug.Log($"Found {guids.Length} ItemSO assets. Populating database '{database.name}'...");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ItemSO item = AssetDatabase.LoadAssetAtPath<ItemSO>(path);

                if (item != null)
                {
                    foundItems.Add(item);

                    // Check for duplicate UniqueItemName
                    string uniqueName = item.UniqueItemName;
                    if (!uniqueNames.Add(uniqueName))
                    {
                        Debug.LogWarning($"Duplicate UniqueItemName '{uniqueName}' found for item at path: {path}. Please ensure UniqueItemNames are distinct.", item);
                        duplicatesFound = true;
                    }
                }
            }

            // Sort items alphabetically by UniqueItemName for consistency
            foundItems = foundItems.OrderBy(item => item.UniqueItemName).ToList();

            // Update the database's list
            database.SetItemListForEditor(foundItems);

            Debug.Log($"ItemDatabase '{database.name}' populated with {foundItems.Count} items." + (duplicatesFound ? " WARNING: Duplicate UniqueItemNames found!" : ""));

            // Optional: Ping the object in the project window to show it was updated
            EditorGUIUtility.PingObject(database);
        }
    }
}
