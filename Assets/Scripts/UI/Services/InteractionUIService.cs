using Interaction;
using UnityEngine;
using UI.Core;
using UI.Layers;

namespace UI.Services
{
    public class InteractionUIService
    {
        private static InteractionUIService _instance;
        public static InteractionUIService Instance => _instance ??= new InteractionUIService();
        public static string InteractionMenuLayerId => "InteractionMenu";

        public void ShowInteractionOptions(
            InteractionOptionSO[] options,
            System.Action<InteractionOptionSO> onOptionSelected,
            System.Action onCancelled)
        {
            // Get the interaction menu layer
            var interactionMenuLayer = UILayerManager.Instance.GetLayer(InteractionMenuLayerId) as InteractionMenuLayer;
            if (interactionMenuLayer == null)
            {
                Debug.LogError("InteractionMenuLayer not found!");
                return;
            }

            // Set up the interaction menu
            interactionMenuLayer.SetInteractionOptions(options, onOptionSelected, onCancelled);

            // Push the layer
            UILayerManager.Instance.PushLayer(InteractionMenuLayerId);
        }

        public void CloseInteractionMenu()
        {
            // The interaction options should already have closed the interaction menu.
            // Do nothing if this is the case.

            if (!UILayerManager.HasTopLayer(InteractionMenuLayerId))
            {
                return;
            }

            Debug.LogWarning("Interaction did not close its interaction menu");
            UILayerManager.Instance.PopLayer();
        }
    }
}
