using System;
using UnityEngine;
using UnityEngine.UI;

namespace Interaction
{
    public class InteractionMenu : MonoBehaviour
    {
        [SerializeField] private GameObject buttonPrefab;
        [SerializeField] private Transform buttonContainer;
        [SerializeField] private float buttonSpacing = 35f;

        private System.Action<InteractionOptionSO> _onOptionSelected;
        private System.Action _onCanceled;

        public void Initialize(InteractionOptionSO[] options, Action<InteractionOptionSO> onOptionSelected)
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
                InteractionOptionSO option = options[i] ?? throw new ArgumentNullException(nameof(options));
                TMPro.TextMeshProUGUI tmpText = buttonObj.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (tmpText != null)
                {
                    tmpText.text = option.Text;
                }
                else
                {
                    Text legacyText = buttonObj.GetComponentInChildren<Text>();
                    if (legacyText != null)
                        legacyText.text = option.Text;
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

        private void SelectOption(InteractionOptionSO option)
        {
            _onOptionSelected?.Invoke(option);
        }

        public void Cancel()
        {
            _onCanceled?.Invoke();
        }

    }
}
