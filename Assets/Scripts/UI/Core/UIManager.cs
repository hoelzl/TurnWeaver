using Interaction;
using UI.Layers;
using UnityEngine;

namespace UI.Core
{
    public static class UIManager
    {
        public static string InteractionMenuLayerName => "InteractionMenu";
        public static string DescriptionLayerName => "Description";

        // Optional method to register additional layer prefabs at runtime
        public static void RegisterLayerPrefab(string layerId, GameObject prefab)
        {
            if (UILayerManager.Instance != null)
            {
                UILayerManager.Instance.RegisterLayerPrefab(layerId, prefab);
            }
            else
            {
                Debug.LogError("UILayerManager instance not found!");
            }
        }

        public static void ShowInteractionMenu(
            InteractionOptionSO[] options,
            System.Action<InteractionOptionSO> onOptionSelected,
            System.Action onCancelled)
        {
            if (UILayerManager.Instance == null)
            {
                Debug.LogError("UILayerManager instance not found!");
                return;
            }

            UILayerManager.Instance.PushLayer(InteractionMenuLayerName, layer =>
            {
                var interactionMenuLayer = layer as InteractionMenuLayer;
                if (interactionMenuLayer != null)
                {
                    interactionMenuLayer.SetInteractionOptions(options, onOptionSelected, onCancelled);
                }
                else
                {
                    Debug.LogError($"Layer is not an InteractionMenuLayer: {layer.LayerName}");
                }
            });
        }

        public static void ShowDescription(string description)
        {
            if (UILayerManager.Instance == null)
            {
                Debug.LogError("UILayerManager instance not found!");
                return;
            }

            UILayerManager.Instance.PushLayer(DescriptionLayerName, layer =>
            {
                var descriptionLayer = layer as DescriptionLayer;
                if (descriptionLayer != null)
                {
                    descriptionLayer.SetDescription(description);
                }
                else
                {
                    Debug.LogError($"Layer is not a DescriptionLayer: {layer.LayerName}");
                }
            });
        }
    }
}
