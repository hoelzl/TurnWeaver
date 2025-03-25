using System;
using Interaction;
using UnityEngine;
using UI.Services;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        private void Awake()
        {
            // Ensure we have the UI services
            if (InteractionUIService.Instance == null)
            {
                GameObject serviceGO = new GameObject("InteractionUIService");
                serviceGO.AddComponent<InteractionUIService>();
            }
        }

        public void ShowInteractionMenu(
            InteractionOptionSO[] options,
            Vector3 worldPosition,
            Action<InteractionOptionSO> onOptionSelected,
            Action onCancelled)
        {
            InteractionUIService.Instance.ShowInteractionOptions(
                options, worldPosition, onOptionSelected, onCancelled);
        }

        public void CloseInteractionMenu()
        {
            InteractionUIService.Instance.CloseInteractionMenu();
        }
    }
}
