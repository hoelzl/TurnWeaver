using System;
using Interaction;
using Interaction.Objects;
using UI.Layers;
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

        public void ShowDescription(string description)
        {
            var descriptionLayer = UILayerManager.Instance.GetLayer("Description") as DescriptionLayer;
            if (descriptionLayer == null)
            {
                Debug.LogError("Description layer not found!");
                return;
            }
            descriptionLayer.SetDescription(description);
            UILayerManager.Instance.PushLayer("Description");
        }

        public void CloseDescription()
        {
            if (!UILayerManager.HasTopLayer("Description")) return;
            Debug.LogWarning("Description did not close its menu.");
            UILayerManager.Instance.PopLayer();
        }
    }
}
