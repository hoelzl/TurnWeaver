using Interaction;
using UnityEngine;
using UI.Core;
using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace UI.Layers
{
    public class InteractionMenuLayer : UILayer
    {
        [SerializeField] private VisualTreeAsset optionButtonTemplate;

        // UI Element References (will be set in SetupUI)
        private VisualElement _optionsContainer;
        private Button _cancelButton;

        private List<VisualElement> _currentOptions = new List<VisualElement>();
        private Action<InteractionOptionSO> _onOptionSelected;
        private Action _onCancelled;

        protected override void SetupUI()
        {
            base.SetupUI();

            if (Root == null) return;

            // Get references to UI elements
            _optionsContainer = Root.Q<VisualElement>("options-container");
            _cancelButton = Root.Q<Button>("cancel-button");

            if (_cancelButton != null)
            {
                _cancelButton.clicked += OnCancelClicked;
            }
        }

        public void SetOptions(
            InteractionOptionSO[] options,
            Vector3 worldPosition,
            Action<InteractionOptionSO> onOptionSelected,
            Action onCancelled)
        {
            // Clear existing options
            ClearOptions();

            _onOptionSelected = onOptionSelected;
            _onCancelled = onCancelled;

            // Position the menu near the world position
            if (Camera.main != null && Root != null)
            {
                Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPosition);

                // Convert to UI position (accounting for screen DPI and UI scaling)
                var panel = Root.panel;
                if (panel != null)
                {
                    Vector2 uiPos = RuntimePanelUtils.ScreenToPanel(panel, screenPos);

                    // Position the options container
                    if (_optionsContainer != null)
                    {
                        _optionsContainer.style.left = uiPos.x;
                        _optionsContainer.style.top = uiPos.y;
                    }
                }
            }

            // Create buttons for each option
            foreach (var option in options)
            {
                CreateOptionButton(option);
            }
        }

        private void CreateOptionButton(InteractionOptionSO option)
        {
            if (optionButtonTemplate == null || _optionsContainer == null) return;

            // Instantiate the button template
            TemplateContainer buttonElement = optionButtonTemplate.Instantiate();
            _currentOptions.Add(buttonElement);
            _optionsContainer.Add(buttonElement);

            // Set up button
            Button button = buttonElement.Q<Button>("option-button");
            if (button != null)
            {
                button.clicked += () => OnOptionClicked(option);
            }

            // Set text
            Label buttonText = buttonElement.Q<Label>("option-text");
            if (buttonText != null)
            {
                buttonText.text = option.Text;
            }

            // Set icon if available
            VisualElement iconElement = buttonElement.Q<VisualElement>("option-icon");
            if (iconElement != null && option.Icon != null)
            {
                iconElement.style.backgroundImage = new StyleBackground(option.Icon);
                iconElement.style.display = DisplayStyle.Flex;
            }
        }

        private void OnOptionClicked(InteractionOptionSO option)
        {
            _onOptionSelected?.Invoke(option);
            UILayerManager.Instance.PopLayer(); // Close the menu
        }

        private void OnCancelClicked()
        {
            _onCancelled?.Invoke();
            UILayerManager.Instance.PopLayer(); // Close the menu
        }

        private void ClearOptions()
        {
            if (_optionsContainer == null) return;

            foreach (var option in _currentOptions)
            {
                _optionsContainer.Remove(option);
            }
            _currentOptions.Clear();
        }

        public override void OnLayerPopped()
        {
            base.OnLayerPopped();
            ClearOptions();
        }
    }
}
