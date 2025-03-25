using System.Collections.Generic;
using UnityEngine;

namespace UI.Core
{
    public class UILayerManager : MonoBehaviour
    {
        private static UILayerManager _instance;
        public static UILayerManager Instance => _instance;

        private readonly Stack<UILayer> _layerStack = new Stack<UILayer>();
        private readonly Dictionary<string, UILayer> _registeredLayers = new Dictionary<string, UILayer>();

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void RegisterLayer(string layerId, UILayer layer)
        {
            if (_registeredLayers.ContainsKey(layerId))
            {
                Debug.LogWarning($"Layer with ID {layerId} already registered!");
                return;
            }

            _registeredLayers[layerId] = layer;
            layer.gameObject.SetActive(false); // Hide by default
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

            // Hide the current top layer if it exists
            if (_layerStack.Count > 0)
            {
                UILayer currentTop = _layerStack.Peek();
                currentTop.OnLayerCovered();
            }

            // Activate and show the new layer
            layer.gameObject.SetActive(true);
            layer.OnLayerPushed();

            // Add to stack
            _layerStack.Push(layer);
        }

        public void PopLayer()
        {
            if (_layerStack.Count == 0) return;

            // Remove and hide the top layer
            UILayer topLayer = _layerStack.Pop();
            topLayer.OnLayerPopped();

            // If there's another layer, show it
            if (_layerStack.Count > 0)
            {
                UILayer newTop = _layerStack.Peek();
                newTop.OnLayerUncovered();
            }
        }

        public UILayer GetCurrentLayer() => _layerStack.Count > 0 ? _layerStack.Peek() : null;

        public void ClearAllLayers()
        {
            while (_layerStack.Count > 0)
            {
                PopLayer();
            }
        }
    }
}
