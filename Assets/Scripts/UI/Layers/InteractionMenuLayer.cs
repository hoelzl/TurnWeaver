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
        private VisualElement _interactionButtonsContainer;
        private Button _cancelButton;

        private readonly List<Button> _interactionButtons = new();
        private Action<InteractionOptionSO> _onOptionSelected;
        private Action _onCancelled;

        protected override void SetupUI()
        {
            base.SetupUI();

            if (Root == null) return;

            // Get references to UI elements
            _interactionButtonsContainer = Root.Q<VisualElement>("interaction-buttons-container");
            _cancelButton = Root.Q<Button>("cancel-button");

            if (_cancelButton != null)
            {
                _cancelButton.clicked += OnCancelClicked;
            }
        }

        public void SetInteractionOptions(
            InteractionOptionSO[] options,
            Action<InteractionOptionSO> onOptionSelected,
            Action onCancelled)
        {
            // Clear existing options
            ClearOptions();

            _onOptionSelected = onOptionSelected;
            _onCancelled = onCancelled;

            // Create buttons for each option
            foreach (var option in options)
            {
                CreateOptionButton(option);
            }
        }

        private void CreateOptionButton(InteractionOptionSO option)
        {
            if (optionButtonTemplate == null || _interactionButtonsContainer == null) return;

            // Instantiate the button template
            TemplateContainer buttonElement = optionButtonTemplate.Instantiate();
            _interactionButtonsContainer.Add(buttonElement);

            SetupInteractionOptionButton(buttonElement, option);
        }

        private void SetupInteractionOptionButton(TemplateContainer buttonElement, InteractionOptionSO option)
        {
            // Set up button
            var button = buttonElement.Q<Button>("option-button");
            if (button != null)
            {
                _interactionButtons.Add(button);
                button.clicked += () => OnOptionClicked(option);
            }

            // Set text
            var buttonText = buttonElement.Q<Label>("option-text");
            if (buttonText != null)
            {
                buttonText.text = option.Text;
            }

            // Set icon if available
            var iconElement = buttonElement.Q<VisualElement>("option-icon");
            if (iconElement != null && option.Icon != null)
            {
                iconElement.style.backgroundImage = new StyleBackground(option.Icon);
                iconElement.style.display = DisplayStyle.Flex;
            }
        }

        private void OnOptionClicked(InteractionOptionSO option)
        {
            UILayerManager.Instance.PopLayer();
            _onOptionSelected?.Invoke(option);
        }

        private void OnCancelClicked()
        {
            UILayerManager.Instance.PopLayer();
            _onCancelled?.Invoke();
        }

        private void ClearOptions()
        {
            _interactionButtonsContainer.Clear();
            _interactionButtons.Clear();
        }

        public override void OnLayerPopped()
        {
            base.OnLayerPopped();
            ClearOptions();
        }

    }
}
