using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    [RequireComponent(typeof(PlayerController))]
    public class PlayerInputHandler : MonoBehaviour
    {
        // Event for click actions
        public delegate void ClickEvent(Vector2 screenPosition);

        public delegate void OpenInventoryEvent();
        public delegate void OpenQuestLogEvent();

        public event ClickEvent OnClickPerformed;
        public event OpenInventoryEvent OnOpenInventory;
        public event OpenQuestLogEvent OnOpenQuestLog;


        private RPGInputActions _inputActions;
        private InputAction _clickAction;
        private InputAction _pointAction;
        private InputAction _openInventoryAction;
        private InputAction _openQuestLogAction;
        private PlayerController _playerController;

        private void Awake()
        {
            _inputActions = new RPGInputActions();
            _playerController = GetComponent<PlayerController>();
        }

        private void OnEnable()
        {
            _clickAction = _inputActions.Gameplay.Designate;
            _pointAction = _inputActions.Gameplay.Point;
            _openInventoryAction = _inputActions.Gameplay.OpenInventory;
            _openQuestLogAction = _inputActions.Gameplay.OpenQuestLog;

            _clickAction.performed += HandleClick;
            _openInventoryAction.performed += HandleOpenInventory;
            _openQuestLogAction.performed += HandleOpenQuestLog;
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

        private void HandleOpenInventory(InputAction.CallbackContext obj)
        {
            OnOpenInventory?.Invoke();
        }

        private void HandleOpenQuestLog(InputAction.CallbackContext obj)
        {
            OnOpenQuestLog?.Invoke();
        }

    }
}
