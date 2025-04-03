using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Tools
{
    [ExecuteAlways]
    public class UIDocumentEditorHider : MonoBehaviour
    {
        [SerializeField] private bool showInEditor = false;

        void Update()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                // Get the UIDocument's rootVisualElement
                var uiDocument = GetComponent<UIDocument>();
                if (uiDocument != null && uiDocument.rootVisualElement != null)
                {
                    // Only change visibility in editor
                    uiDocument.rootVisualElement.style.display =
                        showInEditor ? DisplayStyle.Flex : DisplayStyle.None;
                }
            }
#endif
        }
    }
}
