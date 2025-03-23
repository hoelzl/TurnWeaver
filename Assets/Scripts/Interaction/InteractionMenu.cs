using UnityEngine;
using UnityEngine.UI;

namespace Interaction
{
    public class InteractionMenu : MonoBehaviour
    {
        [SerializeField] private GameObject buttonPrefab;
        [SerializeField] private Transform buttonContainer;
        [SerializeField] private float buttonSpacing = 35f;

        private System.Action<string> _onOptionSelected;

        public void Initialize(string[] options, System.Action<string> onOptionSelected)
        {
            _onOptionSelected = onOptionSelected;

            // Clear any existing buttons
            foreach (Transform child in buttonContainer)
            {
                Destroy(child.gameObject);
            }

            // Create buttons for each option
            for (int i = 0; i < options.Length; i++)
            {
                GameObject buttonObj = Instantiate(buttonPrefab, buttonContainer);

                // Position the button
                RectTransform rectTransform = buttonObj.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = new Vector2(0, -i * buttonSpacing);

                // Set text - try TMPro first, then legacy UI Text
                string option = options[i];
                TMPro.TextMeshProUGUI tmpText = buttonObj.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (tmpText != null)
                {
                    tmpText.text = option;
                }
                else
                {
                    Text legacyText = buttonObj.GetComponentInChildren<Text>();
                    if (legacyText != null)
                        legacyText.text = option;
                }

                Button button = buttonObj.GetComponent<Button>();
                if (button != null)
                    button.onClick.AddListener(() => SelectOption(option));
            }

            // Adjust container size
            RectTransform containerRect = buttonContainer.GetComponent<RectTransform>();
            if (containerRect != null)
                containerRect.sizeDelta = new Vector2(containerRect.sizeDelta.x, options.Length * buttonSpacing);
        }

        public void SetPosition(Vector3 screenPosition)
        {
            // Convert screen position to local canvas position
            RectTransform rectTransform = GetComponent<RectTransform>();
            rectTransform.position = screenPosition;

            // Make sure the menu stays on screen
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                Vector2 canvasSize = canvas.GetComponent<RectTransform>().sizeDelta;
                Vector2 menuSize = rectTransform.sizeDelta;

                Vector2 pos = rectTransform.anchoredPosition;
                pos.x = Mathf.Clamp(pos.x, menuSize.x / 2, canvasSize.x - menuSize.x / 2);
                pos.y = Mathf.Clamp(pos.y, menuSize.y / 2, canvasSize.y - menuSize.y / 2);
                rectTransform.anchoredPosition = pos;
            }
        }

        private void SelectOption(string option)
        {
            _onOptionSelected?.Invoke(option);
        }

        public void Cancel()
        {
            // Find and use the interaction system to cancel properly
            IInteractionSystem interactionSystem = FindAnyObjectByType<InteractionManager>();
            interactionSystem?.CancelInteraction();
        }

        private void Update()
        {
            // Cancel on right-click or Escape
            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
            {
                Cancel();
            }
        }
    }
}
