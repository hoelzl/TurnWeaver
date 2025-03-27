using System.Collections.Generic;
using Input;
using UnityEngine;

namespace UI.Core
{
    public class UILayerManager
    {
        private static UILayerManager _instance;

        public static UILayerManager Instance => _instance ??= new UILayerManager();
        public static bool HasTopLayer(string layerName) => Instance.TopLayer?.LayerId == layerName;
        public static int ActiveLayerCount => Instance._layerStack.Count;

        private readonly Stack<UILayer> _layerStack = new();
        private readonly Dictionary<string, UILayer> _registeredLayers = new();

        public void RegisterLayer(UILayer layer)
        {
            if (!_registeredLayers.TryAdd(layer.LayerId, layer))
            {
                Debug.LogWarning($"Layer with ID {layer.LayerId} already registered!");
                return;
            }

            layer.Hide();
        }

        public UILayer GetLayer(string layerId)
        {
            if (_registeredLayers.TryGetValue(layerId, out UILayer layer))
            {
                return layer;
            }

            Debug.LogWarning($"Layer with ID {layerId} not found!");
            return null;
        }

        public void PushLayer(string layerId)
        {
            UILayer layer = GetLayer(layerId);
            if (layer == null) return;

            if (_layerStack.Count > 0)
            {
                UILayer currentTop = _layerStack.Peek();
                currentTop.OnLayerCovered();
            }

            _layerStack.Push(layer);
            layer.OnLayerPushed();
            layer.Show();
        }

        public void PopLayer()
        {
            if (_layerStack.Count == 0) return;

            UILayer topLayer = _layerStack.Pop();
            topLayer.Hide();
            topLayer.OnLayerPopped();

            if (_layerStack.Count > 0)
            {
                UILayer newTop = _layerStack.Peek();
                newTop.OnLayerUncovered();
            }
        }

        public UILayer TopLayer => _layerStack.Count > 0 ? _layerStack.Peek() : null;

        public void PopAllLayers()
        {
            while (_layerStack.Count > 0)
            {
                PopLayer();
            }
        }
    }
}
