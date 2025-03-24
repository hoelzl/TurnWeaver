using Interaction;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class InteractionButton : MonoBehaviour
    {
        private InteractionOptionSO _option;
        private System.Action<InteractionOptionSO> _onSelected;

        public void Setup(InteractionOptionSO option, System.Action<InteractionOptionSO> onSelected)
        {
            _option = option;
            _onSelected = onSelected;

            SetButtonText(option);

            // Configure the button click
            var button = GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(OnButtonClicked);
            }
        }

        private void SetButtonText(InteractionOptionSO option)
        {
            var tmpText = GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (tmpText != null)
            {
                tmpText.text = option.Text;
            }
            else
            {
                var legacyText = GetComponentInChildren<Text>();
                if (legacyText != null)
                    legacyText.text = option.Text;
            }
        }

        private void OnButtonClicked()
        {
            _onSelected?.Invoke(_option);
        }
    }
}
