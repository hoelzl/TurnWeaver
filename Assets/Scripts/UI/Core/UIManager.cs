using System;
using Interaction;
using UnityEngine;
using UI.Services;

namespace UI.Core
{
    public class UIManager
    {
        private static UIManager _instance;
        public static UIManager Instance => _instance ??= new UIManager();

        public void ShowInteractionMenu(InteractionOptionSO[] options,
            Action<InteractionOptionSO> onOptionSelected,
            Action onCancelled)
        {
            InteractionUIService.Instance.ShowInteractionOptions(
                options, onOptionSelected, onCancelled);
        }

        public void CloseInteractionMenu()
        {
            InteractionUIService.Instance.CloseInteractionMenu();
        }
    }
}
