using UnityEngine;

namespace Interaction.Highlighting
{
    [RequireComponent(typeof(Collider))]
    public class InteractableHighlight : MonoBehaviour
    {
        [Header("Highlight Settings")]
        [SerializeField] private Color highlightColor = new Color(1f, 0.8f, 0f, 1f); // Golden yellow by default
        [SerializeField] private float highlightIntensity = 0.5f;
        [SerializeField] private bool useRendererMaterials = true;
        [SerializeField] private Renderer[] renderers;

        // Properties for URP shaders
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        private bool _isHighlighted = false;
        private Renderer[] _cachedRenderers;
        private MaterialPropertyBlock _propertyBlock;
        private Color[] _originalColors;

        private void Awake()
        {
            _propertyBlock = new MaterialPropertyBlock();

            // Cache renderers
            if (useRendererMaterials)
            {
                _cachedRenderers = GetComponentsInChildren<Renderer>();
            }
            else
            {
                _cachedRenderers = renderers;
            }

            // Cache original colors
            _originalColors = new Color[_cachedRenderers.Length];
            for (int i = 0; i < _cachedRenderers.Length; i++)
            {
                if (_cachedRenderers[i] != null)
                {
                    // Get the material's color
                    _originalColors[i] = _cachedRenderers[i].material.GetColor(BaseColor);
                }
            }
        }

        public void SetHighlight(bool highlight)
        {
            if (_isHighlighted == highlight) return; // No change needed

            _isHighlighted = highlight;
            UpdateHighlightState();
        }

        private void UpdateHighlightState()
        {
            if (_cachedRenderers == null || _cachedRenderers.Length == 0) return;

            for (int i = 0; i < _cachedRenderers.Length; i++)
            {
                if (_cachedRenderers[i] == null) continue;

                // Get the current property block
                _cachedRenderers[i].GetPropertyBlock(_propertyBlock);

                if (_isHighlighted)
                {
                    // Apply highlight color
                    Color tintedColor = Color.Lerp(_originalColors[i], highlightColor, highlightIntensity);
                    _propertyBlock.SetColor(BaseColor, tintedColor);
                }
                else
                {
                    // Reset to original color
                    _propertyBlock.SetColor(BaseColor, _originalColors[i]);
                }

                // Apply the updated property block
                _cachedRenderers[i].SetPropertyBlock(_propertyBlock);
            }
        }

        #if UNITY_EDITOR
        // This allows for updating the highlight in the editor
        public void OnValidate()
        {
            if (_isHighlighted && Application.isPlaying)
            {
                UpdateHighlightState();
            }
        }
        #endif
    }
}
