#if UNITY_EDITOR
using Inventory.Items;
using UnityEditor;
using UnityEngine;

namespace Inventory
{
    [CustomEditor(typeof(InventoryPresetSO))]
    public class InventoryPresetEditor : Editor
    {
        private bool _showGuaranteedItems = true;
        private bool _showRandomItems = true;
        private bool _showNestedPresets = true;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("presetName"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Currency Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("baseCurrency"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("currencyVariation"));

            EditorGUILayout.Space();

            // Guaranteed Items section
            _showGuaranteedItems = EditorGUILayout.Foldout(_showGuaranteedItems, "Guaranteed Items", true);
            if (_showGuaranteedItems)
            {
                DrawItemList("guaranteedItems", false);
            }

            EditorGUILayout.Space();

            // Random Items section
            _showRandomItems = EditorGUILayout.Foldout(_showRandomItems, "Random Items", true);
            if (_showRandomItems)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("maxRandomItems"));
                DrawItemList("randomItems");
            }

            EditorGUILayout.Space();

            // Nested Presets section
            _showNestedPresets = EditorGUILayout.Foldout(_showNestedPresets, "Included Presets", true);
            if (_showNestedPresets)
            {
                var includedPresetsProperty = serializedObject.FindProperty("includedPresets");

                for (int i = 0; i < includedPresetsProperty.arraySize; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(includedPresetsProperty.GetArrayElementAtIndex(i), GUIContent.none);

                    if (GUILayout.Button("×", GUILayout.Width(20)))
                    {
                        includedPresetsProperty.DeleteArrayElementAtIndex(i);
                        i--;
                    }
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Add Preset", GUILayout.Width(100)))
                {
                    includedPresetsProperty.arraySize++;
                }
                EditorGUILayout.EndHorizontal();
            }


            serializedObject.ApplyModifiedProperties();

            // Add test buttons
            EditorGUILayout.Space();
            if (GUILayout.Button("Preview in Test Inventory"))
            {
                PreviewPreset();
            }
        }

        private void DrawItemList(string propertyName, bool showSpawnChance=true)
        {
            SerializedProperty itemsProperty = serializedObject.FindProperty(propertyName);

            for (int i = 0; i < itemsProperty.arraySize; i++)
            {
                SerializedProperty element = itemsProperty.GetArrayElementAtIndex(i);
                SerializedProperty itemProp = element.FindPropertyRelative("item");
                SerializedProperty minQuantityProp = element.FindPropertyRelative("minQuantity");
                SerializedProperty maxQuantityProp = element.FindPropertyRelative("maxQuantity");
                SerializedProperty chanceProp = element.FindPropertyRelative("spawnChance");

                EditorGUILayout.BeginVertical(GUI.skin.box);

                EditorGUILayout.PropertyField(itemProp);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Quantity", GUILayout.Width(60));
                EditorGUILayout.PropertyField(minQuantityProp, GUIContent.none, GUILayout.Width(40));
                EditorGUILayout.LabelField("to", GUILayout.Width(20));
                EditorGUILayout.PropertyField(maxQuantityProp, GUIContent.none, GUILayout.Width(40));
                EditorGUILayout.EndHorizontal();

                // Only show spawn chance if requested
                if (showSpawnChance)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Chance", GUILayout.Width(60));
                    EditorGUILayout.Slider(chanceProp, 0, 1);
                    EditorGUILayout.EndHorizontal();
                }
                else if (chanceProp != null)
                {
                    // For guaranteed items, ensure chance is always 1
                    chanceProp.floatValue = 1.0f;
                }

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Remove", GUILayout.Width(80)))
                {
                    itemsProperty.DeleteArrayElementAtIndex(i);
                    i--;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Add Item", GUILayout.Width(100)))
            {
                itemsProperty.arraySize++;
            }
            EditorGUILayout.EndHorizontal();
        }

        private void PreviewPreset()
        {
            InventoryPresetSO preset = (InventoryPresetSO)target;

            // Create a preview window with a test inventory
            PreviewInventoryWindow.ShowWindow(preset);
        }
    }

    public class PreviewInventoryWindow : EditorWindow
    {
        private Vector2 _scrollPosition;
        private InventoryPresetSO _preset;
        private GameObject _tempGameObject;
        private RPGInventory _testInventory;

        public static void ShowWindow(InventoryPresetSO preset)
        {
            PreviewInventoryWindow window = GetWindow<PreviewInventoryWindow>("Preset Preview");
            window._preset = preset;
            window.Initialize();
        }

        private void Initialize()
        {
            // Create a temporary game object with inventory
            if (_tempGameObject != null)
            {
                DestroyImmediate(_tempGameObject);
            }

            _tempGameObject = new GameObject("TempInventory");
            _testInventory = _tempGameObject.AddComponent<RPGInventory>();

            // Apply the preset
            if (_preset != null)
            {
                _preset.ApplyToInventory(_testInventory);
            }
        }

        private void OnGUI()
        {
            if (_preset == null)
            {
                EditorGUILayout.LabelField("No preset selected");
                return;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Preset: " + _preset.PresetName, EditorStyles.boldLabel);

            if (GUILayout.Button("Refresh", GUILayout.Width(100)))
            {
                Initialize();
            }
            EditorGUILayout.EndHorizontal();

            if (_testInventory == null)
            {
                return;
            }

            EditorGUILayout.LabelField("Currency: " + _testInventory.Currency);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Items:", EditorStyles.boldLabel);
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            if (_testInventory.Items.Count == 0)
            {
                EditorGUILayout.LabelField("No items in inventory");
            }
            else
            {
                foreach (var itemStack in _testInventory.Items)
                {
                    EditorGUILayout.BeginHorizontal();

                    // Try to get the item icon
                    Texture2D icon = null;
                    if (itemStack.Item != null && itemStack.Item.Icon != null)
                    {
                        icon = AssetPreview.GetAssetPreview(itemStack.Item.Icon);
                    }

                    if (icon != null)
                    {
                        GUILayout.Box(icon, GUILayout.Width(40), GUILayout.Height(40));
                    }
                    else
                    {
                        GUILayout.Box("No Icon", GUILayout.Width(40), GUILayout.Height(40));
                    }

                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.LabelField(itemStack.Item != null ? itemStack.Item.ItemName : "Unknown Item", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField("Quantity: " + itemStack.Quantity);

                    if (itemStack.Item != null)
                    {
                        EditorGUILayout.LabelField("Value: " + itemStack.Item.Value);
                    }
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space();
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void OnDestroy()
        {
            if (_tempGameObject != null)
            {
                DestroyImmediate(_tempGameObject);
            }
        }
    }
}
#endif
