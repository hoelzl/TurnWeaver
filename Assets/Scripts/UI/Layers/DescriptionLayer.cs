using System;
using UI.Core;
using UnityEngine.UIElements;

namespace UI.Layers
{
    public class DescriptionLayer : UILayer
    {
        private Label _text;
        private Button _okButton;

        private Action _onCompleted;

        public override void Initialize()
        {
            base.Initialize();
            // Any additional initialization specific to DescriptionLayer
        }

        protected override void SetupUI()
        {
            base.SetupUI();
            if (Root == null) return;

            _text = Root.Q<Label>("text");
            _okButton = Root.Q<Button>("ok-button");

            if (_okButton != null)
            {
                _okButton.clicked += OnOkClicked;
            }
        }

        public void SetDescription(string description)
        {
            if (_text != null)
            {
                _text.text = description;
            }
        }

        private void OnOkClicked()
        {
            _onCompleted?.Invoke();
            UILayerManager.Instance.PopLayer();
        }

        private void OnDestroy()
        {
            // Clean up any event subscriptions
            if (_okButton != null)
            {
                _okButton.clicked -= OnOkClicked;
            }
        }
    }
}
