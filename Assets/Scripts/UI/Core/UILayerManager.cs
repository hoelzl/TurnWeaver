using System.Collections.Generic;
using UnityEngine;
using System;

namespace UI.Core
{
    [Serializable]
    public class LayerPrefabMapping
    {
        public string layerId;
        public GameObject prefab;
    }

    public class UILayerManager : MonoBehaviour
    {
        public static UILayerManager Instance { get; private set; }

        [SerializeField] private List<LayerPrefabMapping> layerPrefabs = new List<LayerPrefabMapping>();
        [SerializeField] private Transform layerContainer;

        private readonly Dictionary<string, GameObject> _layerPrefabMap = new();
        private readonly Stack<UILayer> _layerStack = new();

        public static bool HasTopLayer(string layerName) => Instance?.TopLayer?.LayerName == layerName;
        public static int ActiveLayerCount => Instance?._layerStack.Count ?? 0;
        public UILayer TopLayer => _layerStack.Count > 0 ? _layerStack.Peek() : null;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            // DontDestroyOnLoad(gameObject);

            // Create layer container if not assigned
            if (layerContainer == null)
            {
                var containerObj = new GameObject("UI_Layers");
                containerObj.transform.SetParent(transform);
                layerContainer = containerObj.transform;
            }

            // Map layer prefabs
            foreach (var mapping in layerPrefabs)
            {
                if (mapping.prefab != null)
                {
                    _layerPrefabMap[mapping.layerId] = mapping.prefab;
                }
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                PopAllLayers();
                Instance = null;
            }
        }

        // Allow runtime registration of prefabs
        public void RegisterLayerPrefab(string layerId, GameObject prefab)
        {
            if (!_layerPrefabMap.TryAdd(layerId, prefab))
            {
                Debug.LogWarning($"Layer prefab with ID {layerId} already registered!");
            }
        }

        public GameObject GetLayerPrefab(string layerId)
        {
            if (_layerPrefabMap.TryGetValue(layerId, out GameObject prefab))
            {
                return prefab;
            }

            Debug.LogWarning($"Layer prefab with ID {layerId} not found!");
            return null;
        }

        public void PushLayer(string layerId, System.Action<UILayer> initializeAction = null)
        {
            GameObject prefab = GetLayerPrefab(layerId);
            if (prefab == null) return;

            // Cover the current top layer if there is one
            if (_layerStack.Count > 0)
            {
                UILayer currentTop = _layerStack.Peek();
                currentTop.OnLayerCovered();
            }

            // Instantiate a new layer instance
            GameObject layerInstance = Instantiate(prefab, layerContainer);
            UILayer layer = layerInstance.GetComponent<UILayer>();

            if (layer == null)
            {
                Debug.LogError($"Prefab for layer {layerId} does not have a UILayer component!");
                Destroy(layerInstance);
                return;
            }

            // Initialize the layer
            layer.Initialize();

            // Allow for custom initialization
            initializeAction?.Invoke(layer);

            // Add to stack and activate
            _layerStack.Push(layer);
            layer.OnLayerPushed();
        }

        public void PopLayer()
        {
            if (_layerStack.Count == 0) return;

            UILayer topLayer = _layerStack.Pop();
            topLayer.OnLayerPopped();

            // Destroy the layer instance
            Destroy(topLayer.gameObject);

            if (_layerStack.Count > 0)
            {
                UILayer newTop = _layerStack.Peek();
                newTop.OnLayerUncovered();
            }
        }

        public void PopAllLayers()
        {
            while (_layerStack.Count > 0)
            {
                PopLayer();
            }
        }
    }
}
