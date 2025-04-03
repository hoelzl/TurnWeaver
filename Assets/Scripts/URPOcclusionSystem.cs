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

    [Header("Cast Settings")]
    [SerializeField] private bool useSphereCast = true;
    [SerializeField] private float sphereRadius = 0.5f;
    [SerializeField] private bool visualizeSphereCast;

    [Header("Visual Settings")]
    [SerializeField] private Color occludedTint = new Color(0.8f, 0.8f, 1.0f, 1.0f);
    [SerializeField] private bool showEdgeHighlight = true;

    private static readonly int OcclusionAmount = Shader.PropertyToID("_OcclusionAmount");
    private static readonly int OcclusionColor = Shader.PropertyToID("_OcclusionColor");
    private static readonly int EdgeHighlight = Shader.PropertyToID("_EdgeHighlight");
    private static readonly int BaseMap = Shader.PropertyToID("_BaseMap");
    private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

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

        RaycastHit[] hits;

        if (useSphereCast)
        {
            // Use sphere cast for more generous occlusion detection
            // ReSharper disable once Unity.PreferNonAllocApi
            hits = Physics.SphereCastAll(
                cameraTransform.position,
                sphereRadius,
                dirToPlayer,
                distToPlayer,
                buildingLayerMask
            );

            // Visualize the sphere cast in the editor
            if (visualizeSphereCast && Application.isEditor)
            {
                Debug.DrawRay(cameraTransform.position, dirToPlayer * distToPlayer, Color.yellow);
                // The SphereCast is harder to visualize, but we can at least show the start and end spheres
                DebugDrawSphere(cameraTransform.position, sphereRadius, Color.green);
                DebugDrawSphere(cameraTransform.position + dirToPlayer * distToPlayer, sphereRadius, Color.red);
            }
        }
        else
        {
            // Use standard raycast for precise occlusion detection
            // ReSharper disable once Unity.PreferNonAllocApi
            hits = Physics.RaycastAll(
                cameraTransform.position,
                dirToPlayer,
                distToPlayer,
                buildingLayerMask
            );

            // Visualize the ray in the editor
            if (visualizeSphereCast && Application.isEditor)
            {
                Debug.DrawRay(cameraTransform.position, dirToPlayer * distToPlayer, Color.yellow);
            }
        }

        foreach (RaycastHit hit in hits)
        {
            Renderer[] renderers = hit.collider.GetComponentsInChildren<Renderer>();
            foreach (Renderer myRenderer in renderers)
            {
                if (myRenderer == null)
                    continue;

                // Skip renderers that don't use standard materials (like particles)
                if (myRenderer is ParticleSystemRenderer)
                    continue;

                // Mark this renderer as occluding
                if (_trackedRenderers.TryGetValue(myRenderer, out RendererData data))
                {
                    data.IsOccluding = true;
                }
                else
                {
                    // First time seeing this renderer - set up tracking
                    SetupRenderer(myRenderer);
                }
            }
        }
    }

    private void DebugDrawSphere(Vector3 center, float radius, Color color)
    {
        // Draw a wire sphere in the scene view for debugging
        Vector3 prevPos = center + new Vector3(radius, 0, 0);
        int segments = 16;

        // Draw main circles
        for (int i = 1; i <= segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            Vector3 currPos = center + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
            Debug.DrawLine(prevPos, currPos, color);
            prevPos = currPos;
        }

        // XY plane
        prevPos = center + new Vector3(radius, 0, 0);
        for (int i = 1; i <= segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            Vector3 currPos = center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
            Debug.DrawLine(prevPos, currPos, color);
            prevPos = currPos;
        }

        // YZ plane
        prevPos = center + new Vector3(0, radius, 0);
        for (int i = 1; i <= segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            Vector3 currPos = center + new Vector3(0, Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);
            Debug.DrawLine(prevPos, currPos, color);
            prevPos = currPos;
        }
    }

    private void SetupRenderer(Renderer myRenderer)
    {
        if (myRenderer == null) return;

        var data = new RendererData
        {
            OriginalMaterials = myRenderer.sharedMaterials,
            OcclusionMaterials = new Material[myRenderer.sharedMaterials.Length],
            CurrentOcclusionValue = 0f,
            IsOccluding = true
        };

        // Create instance materials using our occlusion shader
        for (int i = 0; i < data.OriginalMaterials.Length; i++)
        {
            // Create a new material based on the occlusion material
            data.OcclusionMaterials[i] = new Material(_occlusionMaterial);

            // Copy main texture and color from original if available
            if (data.OriginalMaterials[i].HasProperty(BaseMap))
                data.OcclusionMaterials[i].SetTexture(BaseMap, data.OriginalMaterials[i].GetTexture(BaseMap));

            if (data.OriginalMaterials[i].HasProperty(BaseColor))
                data.OcclusionMaterials[i].SetColor(BaseColor, data.OriginalMaterials[i].GetColor(BaseColor));
        }

        // Apply the occlusion materials
        myRenderer.materials = data.OcclusionMaterials;

        // Add to tracked renderers
        _trackedRenderers[myRenderer] = data;
    }

    private void UpdateOcclusionValues()
    {
        // Process all renderers we're tracking
        List<Renderer> renderersToRemove = new List<Renderer>();

        foreach (var kvp in _trackedRenderers)
        {
            Renderer myRenderer = kvp.Key;
            RendererData data = kvp.Value;

            if (myRenderer == null)
            {
                renderersToRemove.Add(myRenderer);
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
                myRenderer.materials = data.OriginalMaterials;

                // Clean up the occlusion materials
                foreach (Material material in data.OcclusionMaterials)
                {
                    if (material != null)
                        Destroy(material);
                }

                renderersToRemove.Add(myRenderer);
            }
        }

        // Remove renderers that are no longer tracked
        foreach (Renderer myRenderer in renderersToRemove)
        {
            _trackedRenderers.Remove(myRenderer);
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
