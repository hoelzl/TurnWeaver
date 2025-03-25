using Interaction;
using UnityEngine;
using UI.Core;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

namespace UI.Layers
{
    public class InteractionMenuLayer : UILayer
    {
        [SerializeField] private GameObject optionButtonPrefab;
        [SerializeField] private Transform optionsContainer;
        [SerializeField] private Button cancelButton;

        private List<GameObject> _currentOptions = new List<GameObject>();
        private Action<InteractionOptionSO> _onOptionSelected;
        private Action _onCancelled;

        protected override void Awake()
        {
            base.Awake();

            if (cancelButton != null)
            {
                cancelButton.onClick.AddListener(OnCancelClicked);
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
            if (Camera.main != null)
            {
                Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPosition);
                RectTransform rectTransform = transform as RectTransform;
                if (rectTransform != null)
                {
                    rectTransform.position = screenPos;
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
            if (optionButtonPrefab == null || optionsContainer == null) return;

            GameObject buttonGO = Instantiate(optionButtonPrefab, optionsContainer);
            _currentOptions.Add(buttonGO);

            // Set up button
            var button = buttonGO.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => OnOptionClicked(option));
            }

            // Set text
            var buttonText = buttonGO.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                buttonText.text = option.Text;
            }

            // Set icon if available
            var buttonImage = buttonGO.GetComponentInChildren<Image>();
            if (buttonImage != null && option.Icon != null)
            {
                buttonImage.sprite = option.Icon;
                buttonImage.enabled = true;
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
            foreach (var option in _currentOptions)
            {
                Destroy(option);
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
