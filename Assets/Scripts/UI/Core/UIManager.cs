using Interaction;
using UI.Layers;
using UnityEngine;

namespace UI.Core
{
    public static class UIManager
    {
        public static string InteractionMenuLayerName => "InteractionMenu";

        public static void ShowInteractionMenu(
            InteractionOptionSO[] options,
            System.Action<InteractionOptionSO> onOptionSelected,
            System.Action onCancelled)
        {
            var interactionMenuLayer = UILayerManager.Instance.GetLayer(InteractionMenuLayerName) as InteractionMenuLayer;
            if (interactionMenuLayer == null)
            {
                Debug.LogError("InteractionMenuLayer not found!");
                return;
            }
            interactionMenuLayer.SetInteractionOptions(options, onOptionSelected, onCancelled);
            UILayerManager.Instance.PushLayer(InteractionMenuLayerName);
        }

        public static string DescriptionLayerName => "Description";

        public static void ShowDescription(string description)
        {
            var descriptionLayer = UILayerManager.Instance.GetLayer(DescriptionLayerName) as DescriptionLayer;
            if (descriptionLayer == null)
            {
                Debug.LogError("Description layer not found!");
                return;
            }
            descriptionLayer.SetDescription(description);
            UILayerManager.Instance.PushLayer(DescriptionLayerName);
        }
    }
}
