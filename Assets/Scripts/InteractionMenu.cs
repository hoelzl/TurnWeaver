// Interaction menu component
using UnityEngine;
using UnityEngine.UI;
using System;

// Interface for interactable objects
public interface IInteractable
{
    string[] GetInteractionOptions();
    void Interact(PlayerController player); // Default interaction
    void InteractWithOption(PlayerController player, string option);
}

public class InteractionMenu : MonoBehaviour
{
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private Transform buttonContainer;
    [SerializeField] private float buttonSpacing = 35f;

    private Action<string> _onOptionSelected;

    public void Initialize(string[] options, Action<string> onOptionSelected)
    {
        _onOptionSelected = onOptionSelected;

        // Create buttons for each option
        for (int i = 0; i < options.Length; i++)
        {
            GameObject buttonObj = Instantiate(buttonPrefab, buttonContainer);

            // Position the button
            RectTransform rectTransform = buttonObj.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(0, -i * buttonSpacing);

            // Set text and click handler
            string option = options[i];
            Text buttonText = buttonObj.GetComponentInChildren<Text>();
            if (buttonText != null)
                buttonText.text = option;

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
            pos.x = Mathf.Clamp(pos.x, menuSize.x/2, canvasSize.x - menuSize.x/2);
            pos.y = Mathf.Clamp(pos.y, menuSize.y/2, canvasSize.y - menuSize.y/2);
            rectTransform.anchoredPosition = pos;
        }
    }

    private void SelectOption(string option)
    {
        _onOptionSelected?.Invoke(option);
    }

    public void Cancel()
    {
        Destroy(gameObject);
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
