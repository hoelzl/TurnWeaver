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
            _text.text = description;
        }

        private void OnOkClicked()
        {
            _onCompleted?.Invoke();
            UILayerManager.Instance.PopLayer();
        }
    }
}
