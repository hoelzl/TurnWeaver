using UnityEngine;
using Player;

namespace Interaction.Highlighting
{
    public class InteractionHighlightManager : MonoBehaviour
    {
        [SerializeField] private LayerMask interactableLayer;
        [SerializeField] private float maxHighlightDistance = 100f;
        [SerializeField] private bool debugRaycasts = false;

        private Camera _mainCamera;
        private InteractableHighlight _currentHighlightedObject;
        private Player.PlayerController _playerController;
        private RPGInputActions _inputActions;

        private void Awake()
        {
            _mainCamera = Camera.main;
            _playerController = FindFirstObjectByType<PlayerController>();
            _inputActions = new RPGInputActions();
        }

        private void OnEnable()
        {
            _inputActions.Enable();
        }

        private void OnDisable()
        {
            _inputActions.Disable();
        }

        private void Update()
        {
            // Only highlight if player isn't in the middle of an interaction
            if (_playerController != null && _playerController.IsInteractionBlocked)
            {
                ClearHighlight();
                return;
            }

            CheckForHighlight();
        }

        private void ClearHighlight()
        {
            if (_currentHighlightedObject != null)
            {
                _currentHighlightedObject.SetHighlight(false);
                _currentHighlightedObject = null;
            }
        }

        private void CheckForHighlight()
        {
            // Get mouse position using the new Input System
            Vector2 mousePosition = _inputActions.Gameplay.Point.ReadValue<Vector2>();

            // Create a ray from the camera through the mouse position
            Ray ray = _mainCamera.ScreenPointToRay(mousePosition);

            if (debugRaycasts)
            {
                Debug.DrawRay(ray.origin, ray.direction * maxHighlightDistance, Color.green, 0.1f);
            }

            // Raycast to find interactable objects
            if (Physics.Raycast(ray, out RaycastHit hit, maxHighlightDistance, interactableLayer))
            {
                // Try to get a highlight component
                InteractableHighlight highlightComponent = hit.collider.GetComponent<InteractableHighlight>();
                if (highlightComponent == null)
                {
                    highlightComponent = hit.collider.GetComponentInParent<InteractableHighlight>();
                }

                // If we found a valid highlight component
                if (highlightComponent != null)
                {
                    // If it's different from the current one
                    if (highlightComponent != _currentHighlightedObject)
                    {
                        // Clear the current highlight
                        ClearHighlight();

                        // Set the new highlighted object
                        _currentHighlightedObject = highlightComponent;
                        _currentHighlightedObject.SetHighlight(true);
                    }
                }
                else
                {
                    // We hit something without a highlight component
                    ClearHighlight();
                }
            }
            else
            {
                // If we didn't hit anything, clear highlight
                ClearHighlight();
            }
        }
    }
}
