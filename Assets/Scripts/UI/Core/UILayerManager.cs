using System.Collections.Generic;
using UnityEngine;

namespace UI.Core
{
    public class UILayerManager : MonoBehaviour
    {
        private static UILayerManager _instance;

        public static UILayerManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var managerObject = FindObjectOfType<UILayerManager>();
                    if (managerObject == null)
                    {
                        Debug.LogError("No UILayerManager found in scene!");
                    }
                    else
                    {
                        _instance = managerObject;
                    }
                }
                return _instance;
            }
        }

        [SerializeField] private int initializationOrder = -100; // Lower numbers initialize earlier

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

            // Ensure this script runs early in the initialization sequence
            ScriptExecutionOrder.SetOrder(this, initializationOrder);
        }

        public void RegisterLayer(string layerId, UILayer layer)
        {
            if (_registeredLayers.ContainsKey(layerId))
            {
                Debug.LogWarning($"Layer with ID {layerId} already registered!");
                return;
            }

            _registeredLayers[layerId] = layer;
            layer.Hide(); // Hide by default
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
            layer.Show();
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


    // Helper class to set script execution order at runtime
    public static class ScriptExecutionOrder
    {
        public static void SetOrder(MonoBehaviour script, int order)
        {
#if UNITY_EDITOR
            UnityEditor.MonoScript monoScript = UnityEditor.MonoScript.FromMonoBehaviour(script);
            if (UnityEditor.MonoImporter.GetExecutionOrder(monoScript) != order)
            {
                UnityEditor.MonoImporter.SetExecutionOrder(monoScript, order);
            }
#endif
        }
    }
}
