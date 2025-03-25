using Interaction;
using UnityEngine;
using UI.Core;
using UI.Layers;

namespace UI.Services
{
    public class InteractionUIService : MonoBehaviour
    {
        private static InteractionUIService _instance;
        public static InteractionUIService Instance => _instance;

        [SerializeField] private string interactionMenuLayerId = "InteractionMenu";

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
        }

        public void ShowInteractionOptions(
            InteractionOptionSO[] options,
            Vector3 worldPosition,
            System.Action<InteractionOptionSO> onOptionSelected,
            System.Action onCancelled)
        {
            // Get the interaction menu layer
            var interactionMenuLayer = UILayerManager.Instance.GetLayer(interactionMenuLayerId) as InteractionMenuLayer;
            if (interactionMenuLayer == null)
            {
                Debug.LogError("InteractionMenuLayer not found!");
                return;
            }

            // Set up the interaction menu
            interactionMenuLayer.SetOptions(options, worldPosition, onOptionSelected, onCancelled);

            // Push the layer
            UILayerManager.Instance.PushLayer(interactionMenuLayerId);
        }

        public void CloseInteractionMenu()
        {
            // If the top layer is the interaction menu, pop it
            var currentLayer = UILayerManager.Instance.GetCurrentLayer();
            if (currentLayer != null && currentLayer.LayerId == interactionMenuLayerId)
            {
                UILayerManager.Instance.PopLayer();
            }
        }
    }
}
