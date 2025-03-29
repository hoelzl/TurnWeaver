using System;
using Interaction;
using Inventory;
using UI.Layers;
using UI.Layers.Inventory;
using UnityEngine;
using static Inventory.RPGInventory;

namespace UI.Core
{
    public static class UIManager
    {
        public static string InteractionMenuLayerName => "InteractionMenu";
        public static string DescriptionLayerName => "Description";
        public static string InventoryLayerName => "Inventory";
        public static string ItemDetailLayerName => "ItemDetail";
        public static string TransferLayerName => "TransferItems";
        public static string ShopLayerName => "Shop";
        public static string QuantitySelectorLayerName => "QuantitySelector";

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

                public static void ShowInventory(RPGInventory inventory)
        {
            if (UILayerManager.Instance == null)
            {
                Debug.LogError("UILayerManager instance not found!");
                return;
            }

            UILayerManager.Instance.PushLayer(InventoryLayerName, layer =>
            {
                var inventoryLayer = layer as InventoryLayer;
                if (inventoryLayer != null)
                {
                    inventoryLayer.SetInventory(inventory);
                }
                else
                {
                    Debug.LogError($"Layer is not an InventoryLayer: {layer.LayerName}");
                }
            });
        }

        // Show item details
        public static void ShowItemDetail(RPGInventory inventory, int itemIndex)
        {
            if (UILayerManager.Instance == null)
            {
                Debug.LogError("UILayerManager instance not found!");
                return;
            }

            UILayerManager.Instance.PushLayer(ItemDetailLayerName, layer =>
            {
                var itemDetailLayer = layer as ItemDetailLayer;
                if (itemDetailLayer != null)
                {
                    itemDetailLayer.SetItem(inventory, itemIndex);
                }
                else
                {
                    Debug.LogError($"Layer is not an ItemDetailLayer: {layer.LayerName}");
                }
            });
        }

        // Show transfer interface
        public static void ShowInventoryTransfer(RPGInventory sourceInventory, RPGInventory targetInventory)
        {
            if (UILayerManager.Instance == null)
            {
                Debug.LogError("UILayerManager instance not found!");
                return;
            }

            UILayerManager.Instance.PushLayer(TransferLayerName, layer =>
            {
                var transferLayer = layer as TransferLayer;
                if (transferLayer != null)
                {
                    transferLayer.SetInventories(sourceInventory, targetInventory);
                }
                else
                {
                    Debug.LogError($"Layer is not a TransferLayer: {layer.LayerName}");
                }
            });
        }

        // Show shop interface
        public static void ShowShop(Shop shop, RPGInventory playerInventory)
        {
            if (UILayerManager.Instance == null)
            {
                Debug.LogError("UILayerManager instance not found!");
                return;
            }

            UILayerManager.Instance.PushLayer(ShopLayerName, layer =>
            {
                var shopLayer = layer as ShopLayer;
                if (shopLayer != null)
                {
                    shopLayer.SetShop(shop, playerInventory);
                }
                else
                {
                    Debug.LogError($"Layer is not a ShopLayer: {layer.LayerName}");
                }
            });
        }

        // Show quantity selector
        public static void ShowQuantitySelector(string title, int min, int max, Action<int> onConfirm)
        {
            if (UILayerManager.Instance == null)
            {
                Debug.LogError("UILayerManager instance not found!");
                return;
            }

            UILayerManager.Instance.PushLayer(QuantitySelectorLayerName, layer =>
            {
                var quantitySelectorLayer = layer as QuantitySelectorLayer;
                if (quantitySelectorLayer != null)
                {
                    quantitySelectorLayer.SetupSelector(title, min, max, onConfirm);
                }
                else
                {
                    Debug.LogError($"Layer is not a QuantitySelectorLayer: {layer.LayerName}");
                }
            });
        }
    }
}
