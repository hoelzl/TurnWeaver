using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[InitializeOnLoad]
public static class HideUIDocumentsInEditor
{
    static HideUIDocumentsInEditor()
    {
        EditorApplication.delayCall += HideUIDocuments;
    }

    private static void HideUIDocuments()
    {
        Debug.Log("HideUIDocumentsInEditor::HideUIDocuments");

        // Iterate through all root GameObjects in the scene
        foreach (GameObject rootGameObject in UnityEngine.SceneManagement.SceneManager.GetActiveScene()
                     .GetRootGameObjects())
        {
            // Recursively check each GameObject and its children
            CheckAndHideUIDocument(rootGameObject);
        }
    }

    private static void CheckAndHideUIDocument(GameObject gameObject)
    {
        // Check if the GameObject has a UIDocument component
        UIDocument uiDocument = gameObject.GetComponent<UIDocument>();
        if (uiDocument != null)
        {
            Debug.Log($"Hiding UIDocument on GameObject: {gameObject.name}");
        }

        // Recursively check children
        foreach (Transform child in gameObject.transform)
        {
            CheckAndHideUIDocument(child.gameObject);
        }
    }
}
