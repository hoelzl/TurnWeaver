using System.Collections.Generic;
using UnityEngine;

public class URPOcclusionSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform cameraTransform;

    [Header("Occlusion Settings")]
    [SerializeField] private LayerMask buildingLayerMask;
    [SerializeField] private float raycastInterval = 0.1f;
    [SerializeField] private float transitionSpeed = 5f;
    [SerializeField] private float minTransparency = 0.2f;

    [Header("Visual Settings")]
    [SerializeField] private Color occludedTint = new Color(0.8f, 0.8f, 1.0f, 1.0f);
    [SerializeField] private bool showEdgeHighlight = true;

    private static readonly int OcclusionAmount = Shader.PropertyToID("_OcclusionAmount");
    private static readonly int OcclusionColor = Shader.PropertyToID("_OcclusionColor");
    private static readonly int EdgeHighlight = Shader.PropertyToID("_EdgeHighlight");

    private readonly Dictionary<Renderer, RendererData> _trackedRenderers = new Dictionary<Renderer, RendererData>();
    private float _raycastTimer;
    private Material _occlusionMaterial;

    private class RendererData
    {
        public Material[] OriginalMaterials;
        public Material[] OcclusionMaterials;
        public float CurrentOcclusionValue;
        public bool IsOccluding;
    }

    private void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (cameraTransform == null)
            cameraTransform = Camera.main?.transform;

        // Create the occlusion material if it doesn't exist
        if (_occlusionMaterial == null)
        {
            // Load our shader
            Shader occlusionShader = Shader.Find("Custom/URPOcclusionShader");
            if (occlusionShader == null)
            {
                Debug.LogError("Occlusion shader not found! Make sure you've imported the shader.");
                return;
            }

            _occlusionMaterial = new Material(occlusionShader);
            _occlusionMaterial.SetFloat(OcclusionAmount, 0);
            _occlusionMaterial.SetColor(OcclusionColor, occludedTint);
            _occlusionMaterial.SetFloat(EdgeHighlight, showEdgeHighlight ? 1 : 0);
        }
    }

    private void Update()
    {
        if (player == null || cameraTransform == null || _occlusionMaterial == null)
            return;

        _raycastTimer += Time.deltaTime;

        // Check occlusion at intervals
        if (_raycastTimer >= raycastInterval)
        {
            CheckOcclusion();
            _raycastTimer = 0;
        }

        // Update occlusion values for all tracked renderers
        UpdateOcclusionValues();
    }

    private void CheckOcclusion()
    {
        // Reset occlusion state for all tracked renderers
        foreach (var data in _trackedRenderers.Values)
        {
            data.IsOccluding = false;
        }

        // Check if anything is between camera and player
        Vector3 dirToPlayer = (player.position - cameraTransform.position).normalized;
        float distToPlayer = Vector3.Distance(cameraTransform.position, player.position);

        RaycastHit[] hits = Physics.RaycastAll(
            cameraTransform.position,
            dirToPlayer,
            distToPlayer,
            buildingLayerMask
        );

        foreach (RaycastHit hit in hits)
        {
            Renderer[] renderers = hit.collider.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                if (renderer == null)
                    continue;

                // Skip renderers that don't use standard materials (like particles)
                if (renderer.GetType() == typeof(ParticleSystemRenderer))
                    continue;

                // Mark this renderer as occluding
                if (_trackedRenderers.TryGetValue(renderer, out RendererData data))
                {
                    data.IsOccluding = true;
                }
                else
                {
                    // First time seeing this renderer - set up tracking
                    SetupRenderer(renderer);
                }
            }
        }
    }

    private void SetupRenderer(Renderer renderer)
    {
        if (renderer == null) return;

        var data = new RendererData
        {
            OriginalMaterials = renderer.sharedMaterials,
            OcclusionMaterials = new Material[renderer.sharedMaterials.Length],
            CurrentOcclusionValue = 0f,
            IsOccluding = true
        };

        // Create instance materials using our occlusion shader
        for (int i = 0; i < data.OriginalMaterials.Length; i++)
        {
            // Create a new material based on the occlusion material
            data.OcclusionMaterials[i] = new Material(_occlusionMaterial);

            // Copy main texture and color from original if available
            if (data.OriginalMaterials[i].HasProperty("_BaseMap"))
                data.OcclusionMaterials[i].SetTexture("_BaseMap", data.OriginalMaterials[i].GetTexture("_BaseMap"));

            if (data.OriginalMaterials[i].HasProperty("_BaseColor"))
                data.OcclusionMaterials[i].SetColor("_BaseColor", data.OriginalMaterials[i].GetColor("_BaseColor"));
        }

        // Apply the occlusion materials
        renderer.materials = data.OcclusionMaterials;

        // Add to tracked renderers
        _trackedRenderers[renderer] = data;
    }

    private void UpdateOcclusionValues()
    {
        // Process all renderers we're tracking
        List<Renderer> renderersToRemove = new List<Renderer>();

        foreach (var kvp in _trackedRenderers)
        {
            Renderer renderer = kvp.Key;
            RendererData data = kvp.Value;

            if (renderer == null)
            {
                renderersToRemove.Add(renderer);
                continue;
            }

            // Target occlusion value (0 = opaque, 1 = transparent)
            float targetValue = data.IsOccluding ? 1f : 0f;

            // Smoothly transition the value
            data.CurrentOcclusionValue = Mathf.MoveTowards(
                data.CurrentOcclusionValue,
                targetValue,
                transitionSpeed * Time.deltaTime
            );

            // Apply occlusion value to all occlusion materials
            foreach (Material material in data.OcclusionMaterials)
            {
                if (material != null)
                {
                    material.SetFloat(OcclusionAmount, data.CurrentOcclusionValue);
                }
            }

            // If not occluding and fully opaque, restore original materials
            if (!data.IsOccluding && Mathf.Approximately(data.CurrentOcclusionValue, 0f))
            {
                renderer.materials = data.OriginalMaterials;

                // Clean up the occlusion materials
                foreach (Material material in data.OcclusionMaterials)
                {
                    if (material != null)
                        Destroy(material);
                }

                renderersToRemove.Add(renderer);
            }
        }

        // Remove renderers that are no longer tracked
        foreach (Renderer renderer in renderersToRemove)
        {
            _trackedRenderers.Remove(renderer);
        }
    }

    private void OnDestroy()
    {
        // Clean up all materials we created
        foreach (var data in _trackedRenderers.Values)
        {
            foreach (Material material in data.OcclusionMaterials)
            {
                if (material != null)
                    Destroy(material);
            }
        }

        _trackedRenderers.Clear();

        if (_occlusionMaterial != null)
            Destroy(_occlusionMaterial);
    }
}
