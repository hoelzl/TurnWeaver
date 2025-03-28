using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    [RequireComponent(typeof(PlayerController))]
    public class PlayerInputHandler : MonoBehaviour
    {
        // Event for click actions
        public delegate void ClickEvent(Vector2 screenPosition);

        public event ClickEvent OnClickPerformed;

        private RPGInputActions _inputActions;
        private InputAction _clickAction;
        private InputAction _pointAction;
        private PlayerController _playerController;

        private void Awake()
        {
            _inputActions = new RPGInputActions();
            _playerController = GetComponent<PlayerController>();
        }

        private void OnEnable()
        {
             _clickAction = _inputActions.Gameplay.Click;
            _pointAction = _inputActions.Gameplay.Point;

            _clickAction.performed += HandleClick;
            _inputActions.Gameplay.Enable();
        }

        private void OnDisable()
        {
            _clickAction.performed -= HandleClick;
            _inputActions.Gameplay.Disable();
        }

        private void HandleClick(InputAction.CallbackContext context)
        {
            // Only handle clicks when interaction isn't blocked
            if (_playerController.IsInteractionBlocked) return;

            Vector2 mousePosition = _pointAction.ReadValue<Vector2>();
            OnClickPerformed?.Invoke(mousePosition);
        }
    }
}
