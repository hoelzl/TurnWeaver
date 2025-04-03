// file: C:/Users/tc/Programming/Games/Unity/Projects/TurnWeaver/Assets/Editor/Inventory/InventoryEditor.cs
#if UNITY_EDITOR
using Inventory.Items; // Make sure ItemSO is accessible if needed
using UnityEditor;
using UnityEngine;

namespace Inventory
{
    [CustomEditor(typeof(RPGInventory))]
    public class InventoryEditor : Editor
    {
        private bool _showInitialItems = true;
        private SerializedProperty _initialItemsProperty;
        private SerializedProperty _initialCurrencyProperty;
        private SerializedProperty _initialPresetProperty;
        private SerializedProperty _initializeOnStartProperty;

        private void OnEnable()
        {
            _initialItemsProperty = serializedObject.FindProperty("initialItems");
            _initialCurrencyProperty = serializedObject.FindProperty("initialCurrency");
            _initialPresetProperty = serializedObject.FindProperty("initialPreset");
            _initializeOnStartProperty = serializedObject.FindProperty("initializeOnStart");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Draw default properties excluding our custom ones
            DrawPropertiesExcluding(serializedObject, "m_Script", "initialItems", "initialCurrency", "initialPreset", "initializeOnStart"); // Exclude m_Script too

            EditorGUILayout.Space();

            // Draw initial items foldout
            _showInitialItems = EditorGUILayout.Foldout(_showInitialItems, "Initial Inventory Configuration", true, EditorStyles.foldoutHeader); // Use Header style
            if (_showInitialItems)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(_initialCurrencyProperty, new GUIContent("Initial Currency"));
                EditorGUILayout.PropertyField(_initialPresetProperty, new GUIContent("Initial Preset")); // <-- Draw the preset field
                EditorGUILayout.PropertyField(_initializeOnStartProperty, new GUIContent("Initialize On Start")); // <-- Draw the renamed bool

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Initial Items List", EditorStyles.boldLabel);

                // Draw a better array interface for initial items
                DrawInitialItemsList(); // <-- Extracted to method for clarity

                 EditorGUI.indentLevel--; // Match the foldout indent level
            }


            serializedObject.ApplyModifiedProperties();

            // Add buttons for operations (Runtime or Editor)
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Editor Actions", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            // GUILayout.FlexibleSpace(); // Removed to align left

            // Button to apply the configuration in the Editor
            if (GUILayout.Button("Apply Initial Config Now", GUILayout.Width(160)))
            {
                 if (EditorUtility.DisplayDialog("Apply Initial Configuration",
                                     "This will clear the current runtime inventory (if playing) and apply the Initial Currency, Items List, and Preset defined above. Continue?", "Yes", "Cancel"))
                 {
                    var inventory = (RPGInventory)target;
                    // Call the consolidated initialization method directly
                    inventory.InitializeInventoryFromConfig(true); // Clear existing runtime items
                    EditorUtility.SetDirty(target); // Mark as dirty to save changes in editor
                    Debug.Log($"Applied initial configuration to {inventory.name} in Editor.");
                 }
            }


            if (GUILayout.Button("Clear Runtime Inventory", GUILayout.Width(160)))
            {
                if (EditorUtility.DisplayDialog("Clear Runtime Inventory",
                        "Are you sure you want to clear the current runtime items and currency in this inventory? (Affects Play mode instance)", "Yes", "Cancel"))
                {
                    var inventory = (RPGInventory)target;
                    inventory.Clear(); // Clears items
                    inventory.SetCurrency(0); // Manually clear currency too for this button
                    if (Application.isPlaying) // Only log if playing
                         Debug.Log($"Cleared runtime inventory for {inventory.name}.");
                    else // Mark dirty if not playing so scene saves the cleared state (if that's desired)
                        EditorUtility.SetDirty(target);

                }
            }

            EditorGUILayout.EndHorizontal();

            // Keep Shop Refresh button if Shop component exists (useful for shop-specific logic)
             var shopComponent = (target as MonoBehaviour)?.GetComponent<Shop>();
             if (shopComponent != null && shopComponent.ShopRefreshPreset != null)
             {
                 EditorGUILayout.Space();
                 EditorGUILayout.LabelField("Shop Actions", EditorStyles.boldLabel);
                 if (GUILayout.Button("Refresh Shop Runtime Items", GUILayout.Width(180)))
                 {
                     if (Application.isPlaying)
                     {
                        shopComponent.RefreshInventory(); // Use shop's own refresh logic
                        Debug.Log($"Triggered shop refresh for {shopComponent.name}.");
                     } else {
                         EditorUtility.DisplayDialog("Shop Refresh", "Shop refresh only applies during Play mode using the Shop's assigned preset.", "OK");
                     }
                 }
             }
        }

        // Helper method to draw the initial items list
        private void DrawInitialItemsList()
        {
            EditorGUI.indentLevel++; // Indent items list

            if (_initialItemsProperty.arraySize == 0)
            {
                EditorGUILayout.HelpBox("No initial items defined.", MessageType.Info);
            }

            for (int i = 0; i < _initialItemsProperty.arraySize; i++)
            {
                EditorGUILayout.BeginHorizontal();

                SerializedProperty element = _initialItemsProperty.GetArrayElementAtIndex(i);
                SerializedProperty itemProp = element.FindPropertyRelative("item");
                SerializedProperty quantityProp = element.FindPropertyRelative("quantity");

                EditorGUILayout.PropertyField(itemProp, GUIContent.none, GUILayout.ExpandWidth(true));

                // Ensure quantity is at least 1
                quantityProp.intValue = Mathf.Max(1, EditorGUILayout.IntField(quantityProp.intValue, GUILayout.Width(50)));


                if (GUILayout.Button("×", EditorStyles.miniButton, GUILayout.Width(20)))
                {
                     // Optional: Check if itemProp is null before removing, sometimes happens with array ops
                     if (itemProp.objectReferenceValue != null) {
                         _initialItemsProperty.DeleteArrayElementAtIndex(i);
                     } else {
                         // If item is null, just remove the empty slot cleanly
                         _initialItemsProperty.DeleteArrayElementAtIndex(i);
                     }
                     i--; // Adjust index after deletion
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace(); // Pushes button to the right
            if (GUILayout.Button("Add Item Slot", GUILayout.Width(100)))
            {
                _initialItemsProperty.arraySize++;
                 // Optional: Initialize the new element's quantity to 1
                 if(_initialItemsProperty.arraySize > 0) {
                     var newElement = _initialItemsProperty.GetArrayElementAtIndex(_initialItemsProperty.arraySize - 1);
                     newElement.FindPropertyRelative("quantity").intValue = 1;
                     newElement.FindPropertyRelative("item").objectReferenceValue = null; // Ensure item is null initially
                 }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel--; // Restore indent level
        }
    }
}
#endif
