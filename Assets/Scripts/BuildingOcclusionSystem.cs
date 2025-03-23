using System.Collections.Generic;
using UnityEngine;

public class BuildingOcclusionSystem : MonoBehaviour
{
    private static readonly int Color1 = Shader.PropertyToID("_Color");
    private static readonly int Mode = Shader.PropertyToID("_Mode");
    private static readonly int SrcBlend = Shader.PropertyToID("_SrcBlend");
    private static readonly int DstBlend = Shader.PropertyToID("_DstBlend");
    private static readonly int ZWrite = Shader.PropertyToID("_ZWrite");

    [Header("References")] [SerializeField]
    private Transform player;

    [SerializeField] private Transform cameraTransform;

    [Header("Occlusion Settings")] [SerializeField]
    private LayerMask buildingLayerMask;

    [SerializeField] private float dissolveSpeed = 2f;
    [SerializeField] private float minTransparency = 0.3f;
    [SerializeField] private float raycastInterval = 0.1f;

    private readonly Dictionary<Renderer, Material[]> _originalMaterials = new Dictionary<Renderer, Material[]>();
    private readonly Dictionary<Renderer, Material[]> _transparentMaterials = new Dictionary<Renderer, Material[]>();
    private readonly HashSet<Renderer> _currentlyTransparent = new HashSet<Renderer>();
    private float _raycastTimer;

    private void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        if (cameraTransform == null)
            cameraTransform = Camera.main?.transform;
    }

    private void Update()
    {
        _raycastTimer += Time.deltaTime;

        // Only check for occlusion at certain intervals to save performance
        if (_raycastTimer >= raycastInterval)
        {
            CheckOcclusion();
            _raycastTimer = 0;
        }

        // Process transparency changes
        UpdateTransparency();
    }

    private void CheckOcclusion()
    {
        // First, mark all currently transparent objects for restoration
        HashSet<Renderer> renderersToRestore = new HashSet<Renderer>(_currentlyTransparent);
        _currentlyTransparent.Clear();

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
            foreach (Renderer myRenderer in renderers)
            {
                // Skip transparent objects like glass
                if (myRenderer.material.HasProperty(Color1) &&
                    myRenderer.material.color.a < 1.0f)
                    continue;

                // Mark this renderer as needing transparency
                _currentlyTransparent.Add(myRenderer);

                // Create transparent materials if needed
                if (!_transparentMaterials.ContainsKey(myRenderer))
                {
                    CreateTransparentMaterials(myRenderer);
                }

                // Remove from the restore list since we need it transparent
                renderersToRestore.Remove(myRenderer);
            }
        }
    }

    private void UpdateTransparency()
    {
        // Make occluding objects transparent
        foreach (Renderer myRenderer in _currentlyTransparent)
        {
            Material[] materials = myRenderer.materials;

            foreach (Material mat in materials)
            {
                Color color = mat.color;
                color.a = Mathf.MoveTowards(color.a, minTransparency, dissolveSpeed * Time.deltaTime);
                mat.color = color;
            }

            myRenderer.materials = materials;
        }

        // Gradually restore objects that no longer occlude
        foreach (Renderer myRenderer in _originalMaterials.Keys)
        {
            if (_currentlyTransparent.Contains(myRenderer) || !myRenderer) continue;
            Material[] materials = myRenderer.materials;
            bool fullyRestored = true;

            foreach (Material mat in materials)
            {
                Color color = mat.color;
                if (!(color.a < 1.0f)) continue;

                color.a = Mathf.MoveTowards(color.a, 1.0f, dissolveSpeed * Time.deltaTime);
                mat.color = color;

                if (color.a < 0.99f)
                    fullyRestored = false;
            }

            myRenderer.materials = materials;

            // Restore original materials when fully opaque
            if (fullyRestored && _originalMaterials.TryGetValue(myRenderer, out Material[] material))
            {
                myRenderer.materials = material;
            }
        }
    }

    private void CreateTransparentMaterials(Renderer myRenderer)
    {
        Material[] originalMats = myRenderer.sharedMaterials;
        _originalMaterials[myRenderer] = originalMats;

        var transparentMats = new Material[originalMats.Length];

        for (int i = 0; i < originalMats.Length; i++)
        {
            transparentMats[i] = new Material(originalMats[i]);

            // Enable transparency
            transparentMats[i].SetFloat(Mode, 3); // Transparent mode
            transparentMats[i].SetInt(SrcBlend, (int) UnityEngine.Rendering.BlendMode.SrcAlpha);
            transparentMats[i].SetInt(DstBlend, (int) UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            transparentMats[i].SetInt(ZWrite, 0);
            transparentMats[i].DisableKeyword("_ALPHATEST_ON");
            transparentMats[i].EnableKeyword("_ALPHABLEND_ON");
            transparentMats[i].DisableKeyword("_ALPHAPREMULTIPLY_ON");
            transparentMats[i].renderQueue = 3000;
        }

        _transparentMaterials[myRenderer] = transparentMats;
        myRenderer.materials = transparentMats;
    }

    private void OnDestroy()
    {
        // Clean up the materials when the component is destroyed
        foreach (Material[] materials in _transparentMaterials.Values)
        {
            foreach (Material mat in materials)
            {
                Destroy(mat);
            }
        }
    }
}
