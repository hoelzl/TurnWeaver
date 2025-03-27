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
            // If the top layer is the interaction menu, pop it
            UILayer currentLayer = UILayerManager.Instance.GetCurrentLayer();
            if (currentLayer == null || currentLayer.LayerId != InteractionMenuLayerId)
            {
                Debug.LogWarning($"Trying to close interaction menu, but got Layer {currentLayer.LayerId}!");
                return;
            }
            UILayerManager.Instance.PopLayer();
        }
    }
}
