#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Inventory
{
    [CustomEditor(typeof(RPGInventory))]
    public class InventoryEditor : UnityEditor.Editor
    {
        private bool _showInitialItems = true;
        private SerializedProperty _initialItemsProperty;
        private SerializedProperty _initialCurrencyProperty;
        private SerializedProperty _loadItemsOnStartProperty;

        private void OnEnable()
        {
            _initialItemsProperty = serializedObject.FindProperty("initialItems");
            _initialCurrencyProperty = serializedObject.FindProperty("initialCurrency");
            _loadItemsOnStartProperty = serializedObject.FindProperty("loadItemsOnStart");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Draw default properties excluding our custom ones
            DrawPropertiesExcluding(serializedObject, "initialItems", "initialCurrency", "loadItemsOnStart");

            EditorGUILayout.Space();

            // Draw initial items foldout
            _showInitialItems = EditorGUILayout.Foldout(_showInitialItems, "Initial Inventory Configuration", true);
            if (_showInitialItems)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(_initialCurrencyProperty);
                EditorGUILayout.PropertyField(_loadItemsOnStartProperty);

                // Draw a better array interface for initial items
                // EditorGUILayout.PropertyField(_initialItemsProperty, new GUIContent("Initial Items"), true);

                EditorGUI.indentLevel++;

                for (int i = 0; i < _initialItemsProperty.arraySize; i++)
                {
                    EditorGUILayout.BeginHorizontal();

                    SerializedProperty element = _initialItemsProperty.GetArrayElementAtIndex(i);
                    SerializedProperty itemProp = element.FindPropertyRelative("item");
                    SerializedProperty quantityProp = element.FindPropertyRelative("quantity");

                    EditorGUILayout.PropertyField(itemProp, GUIContent.none, GUILayout.ExpandWidth(true));
                    quantityProp.intValue = EditorGUILayout.IntField(quantityProp.intValue, GUILayout.Width(50));

                    if (GUILayout.Button("×", GUILayout.Width(20)))
                    {
                        _initialItemsProperty.DeleteArrayElementAtIndex(i);
                        i--;
                    }

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Add Item", GUILayout.Width(100)))
                {
                    _initialItemsProperty.arraySize++;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();

            // Add buttons for operations
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Clear Inventory", GUILayout.Width(120)))
            {
                if (EditorUtility.DisplayDialog("Clear Inventory",
                        "Are you sure you want to clear the inventory?", "Yes", "Cancel"))
                {
                    var inventory = (RPGInventory) target;
                    inventory.Clear();
                }
            }

            if (GUILayout.Button("Load Initial Items", GUILayout.Width(120)))
            {
                var inventory = (RPGInventory) target;
                inventory.LoadInitialItems();
            }

            var shopComponent = (target as MonoBehaviour)?.GetComponent<Shop>();
            if (shopComponent != null && shopComponent.ShopInventoryPreset != null)
            {
                if (GUILayout.Button("Refresh Shop Items", GUILayout.Width(120)))
                {
                    shopComponent.RefreshInventory();
                }
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}
#endif
