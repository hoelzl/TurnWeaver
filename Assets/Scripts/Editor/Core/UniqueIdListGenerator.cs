#if UNITY_EDITOR
using System.Text;
using UnityEditor;
using UnityEngine;

// Assuming your UniqueId script is in the Core namespace

namespace Core
{
    public class UniqueIdListGenerator : EditorWindow
    {
        private string _generatedListOutput = "Click the button to generate the list...";
        private Vector2 _scrollPosition = Vector2.zero;

        [MenuItem("Tools/Generate Unique ID List")]
        public static void ShowWindow()
        {
            // Get existing open window or if none, make a new one:
            UniqueIdListGenerator window = GetWindow<UniqueIdListGenerator>("Unique ID List Generator");
            window.minSize = new Vector2(300, 250); // Set a minimum size for the window
        }

        void OnGUI()
        {
            GUILayout.Label("Generate Unique ID List for Scene Objects", EditorStyles.boldLabel);

            if (GUILayout.Button("Generate List"))
            {
                _generatedListOutput = GenerateList(); // Store the generated list
            }

            // Add a scroll view to display the output
            EditorGUILayout.Space();
            GUILayout.Label("Generated List:", EditorStyles.label);
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.ExpandHeight(true));
            // Use a selectable label inside the scroll view for easy copying
            EditorGUILayout.SelectableLabel(_generatedListOutput, EditorStyles.textArea, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
        }

        // Modified to return the string instead of logging
        private static string GenerateList()
        {
            StringBuilder output = new StringBuilder();
            output.AppendLine("// --- Generated Unique ID List ---");

            UniqueId[] uniqueIds = FindObjectsByType<UniqueId>(FindObjectsSortMode.None);

            if (uniqueIds.Length == 0)
            {
                output.AppendLine("// No GameObjects with UniqueId component found in the active scene.");
            }
            else
            {
                foreach (UniqueId idComponent in uniqueIds)
                {
                    SerializedObject serializedObject = new SerializedObject(idComponent);
                    SerializedProperty variableProp = serializedObject.FindProperty("uniqueIdVariable");
                    SerializedProperty guidProp = serializedObject.FindProperty("uniqueIdGuidString");

                    string gameObjectName = idComponent.gameObject.name;
                    string variableName = variableProp.stringValue;
                    string guidString = guidProp.stringValue;

                    bool variableSet = !string.IsNullOrEmpty(variableName);
                    bool guidSet = !string.IsNullOrEmpty(guidString);

                    if (variableSet && guidSet)
                    {
                        output.AppendLine($"VAR {variableName} = \"{guidString}\"");
                    }
                    else
                    {
                        if (!variableSet)
                        {
                            output.AppendLine($"// INFO: {gameObjectName}: uniqueIdVariable not set");
                        }
                        if (!guidSet)
                        {
                            bool isValidGuid = System.Guid.TryParse(guidString, out System.Guid parsedGuid);
                            if(!isValidGuid || parsedGuid == System.Guid.Empty)
                            {
                                output.AppendLine($"// WARNING: {gameObjectName}: uniqueGuidString not set or invalid");
                            } else {
                                output.AppendLine($"// WARNING: {gameObjectName}: uniqueGuidString is set butIsNullOrEmpty check failed? Value: {guidString}");
                            }
                        }
                    }
                }
            }

            output.AppendLine("// --- End of List ---");

            // Return the generated string
            return output.ToString();
        }
    }
}
#endif
