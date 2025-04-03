using Inventory;
using UI.Core;
using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(PlayerInputHandler))]
    [RequireComponent(typeof(PlayerMovement))]
    [RequireComponent(typeof(PlayerInteractionController))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private LayerMask interactableLayer;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private bool debugRaycasts;

        private PlayerInputHandler _inputHandler;
        private PlayerMovement _movement;
        private PlayerInteractionController _interaction;
        private RPGInventory _inventory;
        private Camera _camera;

        public bool IsInteractionBlocked => _interaction != null &&
                                            (GetComponent<PlayerInteractionController>().IsInteractionBlocked ||
                                             UILayerManager.ActiveLayerCount > 0);

        private void Awake()
        {
            _inputHandler = GetComponent<PlayerInputHandler>();
            _movement = GetComponent<PlayerMovement>();
            _interaction = GetComponent<PlayerInteractionController>();
            _inventory = GetComponent<RPGInventory>();
            _camera = Camera.main;
        }

        private void Start()
        {
            _inputHandler.OnClickPerformed += HandleClick;
            _inputHandler.OnOpenInventory += HandleOpenInventory;
            _inputHandler.OnOpenQuestLog += HandleOpenQuestLog;
        }

        private void OnDestroy()
        {
            _inputHandler.OnClickPerformed -= HandleClick;
            _inputHandler.OnOpenInventory -= HandleOpenInventory;
            _inputHandler.OnOpenQuestLog -= HandleOpenQuestLog;
        }

        private void HandleClick(Vector2 screenPosition)
        {
            // Create a ray from the camera through the mouse position
            Ray ray = _camera.ScreenPointToRay(screenPosition);

            if (debugRaycasts)
                Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red, 1f);

            // First, check for interactable objects
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, interactableLayer))
            {
                if (_interaction.AttemptInteraction(hit))
                    return; // Interaction handled successfully
            }

            // Then check for ground to move
            if (Physics.Raycast(ray, out hit, 100f, groundLayer))
            {
                _movement.MoveTo(hit.point);
            }
        }

        private void HandleOpenInventory()
        {
            if (_inventory != null)
            {
                UIManager.ShowInventory(_inventory);
            }
            else
            {
                Debug.LogWarning($"PlayerController: No inventory available for {gameObject.name}", this);
            }
        }

        private void HandleOpenQuestLog()
        {
            UIManager.ShowQuestLog();
        }

        public void StopMoving()
        {
            _movement.StopMoving();
        }

        public void StartTurn()
        {
            // Turn-based logic for player
        }

        public void EndTurn()
        {
            // Turn-based logic for player
        }
    }
}
