using System;
using Interaction;
using UnityEngine;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private GameObject interactionMenuPrefab;
        [SerializeField] private Canvas interactionCanvas;

        private GameObject _currentInteractionMenu;

        public void ShowInteractionMenu(InteractionOptionSO[] options, Vector3 worldPosition,
            Action<InteractionOptionSO> onOptionSelected, Action onCancelled)
        {
            // Clean up any existing menu
            if (_currentInteractionMenu != null)
                Destroy(_currentInteractionMenu);

            // Create and set up the new menu
            _currentInteractionMenu = Instantiate(interactionMenuPrefab, interactionCanvas.transform);
            InteractionMenu menu = _currentInteractionMenu.GetComponent<InteractionMenu>();

            if (menu != null)
            {
                menu.Initialize(options, onOptionSelected);

                // Position the menu above the interactable
                Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPosition + Vector3.up * 1.5f);
                menu.SetPosition(screenPos);
            }
        }

        public void CloseInteractionMenu()
        {
            if (_currentInteractionMenu != null)
            {
                Destroy(_currentInteractionMenu);
                _currentInteractionMenu = null;
            }
        }
    }
}
