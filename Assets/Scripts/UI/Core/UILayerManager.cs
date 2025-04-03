using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;
using UnityEngine.InputSystem; // Added for PlayerInput

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

        [Header("Layer Configuration")]
        [SerializeField] private List<LayerPrefabMapping> layerPrefabs = new List<LayerPrefabMapping>();
        [SerializeField] private Transform layerContainer;

        [Header("Input Handling")]
        [SerializeField] private PlayerInput playerInput; // Assign PlayerInput component here
        [SerializeField] private string gameplayActionMap = "Gameplay";
        [SerializeField] private string uiActionMap = "UI";


        private readonly Dictionary<string, GameObject> _layerPrefabMap = new();
        private readonly Stack<UILayer> _layerStack = new();

        public static bool HasTopLayer(string layerName) => Instance?.TopLayer?.LayerName == layerName;
        public static int ActiveLayerCount => Instance?._layerStack.Count ?? 0;
        private UILayer TopLayer => _layerStack.Count > 0 ? _layerStack.Peek() : null;

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

            // Try to find PlayerInput if not assigned
            if (playerInput == null)
            {
                playerInput = FindFirstObjectByType<PlayerInput>();
                if (playerInput == null)
                {
                    Debug.LogWarning("UILayerManager: PlayerInput component not found in scene and not assigned.",
                        this);
                }
            }


            // Map layer prefabs
            foreach (var mapping in layerPrefabs)
            {
                if (mapping.prefab != null && !string.IsNullOrEmpty(mapping.layerId))
                {
                    if (!_layerPrefabMap.TryAdd(mapping.layerId, mapping.prefab))
                    {
                        Debug.LogWarning($"Layer prefab with ID '{mapping.layerId}' already registered!", this);
                    }
                }
                else
                {
                    Debug.LogWarning("Invalid layer prefab mapping found (missing ID or prefab).", this);
                }
            }
        }

        private void Start()
        {
            // Ensure gameplay map is active initially if no layers are pushed on Awake/Start elsewhere
            if (_layerStack.Count == 0 && playerInput != null &&
                playerInput.currentActionMap?.name != gameplayActionMap)
            {
                SwitchToActionMap(gameplayActionMap);
            }
        }


        private void OnDestroy()
        {
            if (Instance == this)
            {
                // Ensure gameplay map is active when manager is destroyed? Or rely on scene reload?
                // SwitchToActionMap(gameplayActionMap); // Consider implications

                PopAllLayers(); // Clean up layers
                Instance = null;
            }
        }

        // Allow runtime registration of prefabs
        public void RegisterLayerPrefab(string layerId, GameObject prefab)
        {
            if (!_layerPrefabMap.TryAdd(layerId, prefab))
            {
                Debug.LogWarning($"Layer prefab with ID {layerId} already registered!", this);
            }
        }

        private GameObject GetLayerPrefab(string layerId)
        {
            if (_layerPrefabMap.TryGetValue(layerId, out GameObject prefab))
            {
                return prefab;
            }

            Debug.LogWarning($"Layer prefab with ID {layerId} not found!", this);
            return null;
        }

        public void PushLayer(string layerId, Action<UILayer> initializeAction = null)
        {
            GameObject prefab = GetLayerPrefab(layerId);
            if (prefab == null) return;

            bool wasEmpty = _layerStack.Count == 0;

            // Cover the current top layer if there is one
            if (!wasEmpty)
            {
                UILayer currentTop = _layerStack.Peek();
                currentTop.OnLayerCovered();
            }

            // Instantiate a new layer instance
            GameObject layerInstance = Instantiate(prefab, layerContainer);
            UILayer layer = layerInstance.GetComponent<UILayer>();

            if (layer == null)
            {
                Debug.LogError($"Prefab for layer {layerId} does not have a UILayer component!", layerInstance);
                Destroy(layerInstance);
                // Uncover previous layer if instantiation failed?
                if (!wasEmpty) _layerStack.Peek().OnLayerUncovered();
                return;
            }

            // Initialize the layer
            layer.Initialize();

            // Allow for custom initialization
            initializeAction?.Invoke(layer);

            // Add to stack and activate
            _layerStack.Push(layer);
            layer.OnLayerPushed(); // This sets sorting order etc.

            // Switch to UI map if this is the first layer being pushed
            if (wasEmpty)
            {
                SwitchToActionMap(uiActionMap);
            }
        }

        public void PopLayer()
        {
            if (_layerStack.Count == 0) return;

            // Get the layer we want to pop
            UILayer topLayer = _layerStack.Peek();
            string layerToPopName = topLayer != null ? topLayer.LayerName : "Unknown Layer"; // Safe access

            // Pop the layer from stack
            _layerStack.Pop();

            // Debug logging
            Debug.Log($"Popping layer: {layerToPopName}");

            // Call layer event (OnLayerPopped might trigger CloseLayer in DialogueUILayer)
            // Ensure topLayer is not null before calling
            if (topLayer != null)
            {
                topLayer.OnLayerPopped();

                // Destroy the layer instance using coroutine
                GameObject layerObject = topLayer.gameObject;
                // Ensure object still exists before starting coroutine
                if (layerObject != null && isActiveAndEnabled)
                {
                    StartCoroutine(DestroyLayerAtEndOfFrame(layerObject));
                }
            }


            // Check if stack is now empty to switch back input map
            if (_layerStack.Count == 0)
            {
                SwitchToActionMap(gameplayActionMap);
            }
            else // Otherwise, uncover the new top layer
            {
                UILayer newTop = _layerStack.Peek();
                if (newTop != null) // Check if stack wasn't empty after all
                {
                    newTop.OnLayerUncovered();
                }
                else
                {
                    // This case should ideally not happen if PopLayer logic is correct
                    Debug.LogError("Layer stack inconsistency detected after PopLayer.", this);
                    SwitchToActionMap(gameplayActionMap); // Switch back just in case
                }
            }
        }

        private IEnumerator DestroyLayerAtEndOfFrame(GameObject layerObject)
        {
            yield return new WaitForEndOfFrame();
            if (layerObject != null) // Check if it wasn't destroyed by other means
            {
                Destroy(layerObject);
            }
        }


        private void PopAllLayers()
        {
            // Pop layers one by one to ensure proper cleanup and input switching
            while (_layerStack.Count > 0)
            {
                PopLayer();
            }
            // Explicitly ensure gameplay map is active after clearing all
            SwitchToActionMap(gameplayActionMap);
        }

        // Helper method to switch action maps safely
        private void SwitchToActionMap(string mapName)
        {
            if (playerInput == null)
            {
                //Debug.LogWarning("Cannot switch action map: PlayerInput is not available.");
                return;
            }
            if (playerInput.currentActionMap?.name == mapName)
            {
                //Debug.Log($"Action map '{mapName}' is already active.");
                return; // Already on the target map
            }

            try
            {
                if (playerInput.inputIsActive) // Use as proxy for m_Enabled
                {
                    playerInput.SwitchCurrentActionMap(mapName);
                    Debug.Log($"Switched Action Map to: {mapName}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError(
                    $"Failed to switch action map to '{mapName}': {e.Message}.",
                    this);
            }
        }
    }
}
