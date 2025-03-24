using System;
using UnityEngine;
using UI;

namespace Interaction
{
    public class InteractionMenu : MonoBehaviour
    {
        [SerializeField] private ButtonBox buttonBox;

        private System.Action _onCanceled;

        public void Initialize(InteractionOptionSO[] options, Action<InteractionOptionSO> onOptionSelected)
        {
            buttonBox.Initialize(onOptionSelected);

            foreach (InteractionOptionSO option in options)
            {
                buttonBox.Add(option);
            }
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

        public void Cancel()
        {
            _onCanceled?.Invoke();
        }
    }
}
