using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Core
{
    [RequireComponent(typeof(UIDocument))]
    public class UIDocumentInitializer : MonoBehaviour
    {
        private void Awake()
        {
            // Get the UIDocument
            var document = GetComponent<UIDocument>();

            // Ensure it's enabled so we have access to the root element
            document.enabled = true;
        }

        private void Start()
        {
            // Set initial display to none after root is created
            var document = GetComponent<UIDocument>();
            if (document.rootVisualElement != null)
            {
                document.rootVisualElement.style.display = DisplayStyle.None;
                document.rootVisualElement.style.visibility = Visibility.Hidden;
            }
        }
    }
}
