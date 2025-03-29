using System;
using UI.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Layers.Inventory
{
    public class QuantitySelectorLayer : UILayer
    {
        private Label _titleLabel;
        private Slider _quantitySlider;
        private Label _quantityLabel;
        private Button _confirmButton;
        private Button _cancelButton;

        private int _minQuantity = 1;
        private int _maxQuantity = 99;
        private int _currentQuantity = 1;

        private Action<int> _onConfirm;

        public void SetupSelector(string title, int min, int max, Action<int> onConfirm)
        {
            _minQuantity = Mathf.Max(1, min);
            _maxQuantity = Mathf.Max(_minQuantity, max);
            _currentQuantity = _minQuantity;
            _onConfirm = onConfirm;

            if (_titleLabel != null)
            {
                _titleLabel.text = title;
            }

            if (_quantitySlider != null)
            {
                _quantitySlider.lowValue = _minQuantity;
                _quantitySlider.highValue = _maxQuantity;
                _quantitySlider.value = _currentQuantity;
            }

            UpdateQuantityLabel();
        }

        protected override void SetupUI()
        {
            base.SetupUI();

            if (Root == null) return;

            // Get references to UI elements
            _titleLabel = Root.Q<Label>("title-label");
            _quantitySlider = Root.Q<Slider>("quantity-slider");
            _quantityLabel = Root.Q<Label>("quantity-label");
            _confirmButton = Root.Q<Button>("confirm-button");
            _cancelButton = Root.Q<Button>("cancel-button");

            // Set up event handlers
            if (_quantitySlider != null)
            {
                _quantitySlider.RegisterValueChangedCallback(OnSliderValueChanged);
            }

            if (_confirmButton != null)
            {
                _confirmButton.clicked += OnConfirmClicked;
            }

            if (_cancelButton != null)
            {
                _cancelButton.clicked += OnCancelClicked;
            }
        }

        private void OnSliderValueChanged(ChangeEvent<float> evt)
        {
            _currentQuantity = Mathf.RoundToInt(evt.newValue);
            _quantitySlider.SetValueWithoutNotify(_currentQuantity);
            UpdateQuantityLabel();
        }

        private void UpdateQuantityLabel()
        {
            if (_quantityLabel != null)
            {
                _quantityLabel.text = _currentQuantity.ToString();
            }
        }

        private void OnConfirmClicked()
        {
            Debug.Log("QuantitySelector: Confirm clicked");
            UILayerManager.Instance.PopLayer();
            if (_onConfirm != null)
            {
                Debug.Log($"QuantitySelector: Invoking callback with quantity {_currentQuantity}");
                _onConfirm.Invoke(_currentQuantity);
            }
        }


        private void OnCancelClicked()
        {
            Debug.Log("QuantitySelector: Cancel clicked");
            UILayerManager.Instance.PopLayer();
        }

        private void OnDestroy()
        {
            // Clean up event handlers
            if (_quantitySlider != null)
            {
                _quantitySlider.UnregisterValueChangedCallback(OnSliderValueChanged);
            }

            if (_confirmButton != null)
            {
                _confirmButton.clicked -= OnConfirmClicked;
            }

            if (_cancelButton != null)
            {
                _cancelButton.clicked -= OnCancelClicked;
            }
        }
    }
}
