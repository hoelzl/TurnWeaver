using UI.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Input
{
    public class InputManager : MonoBehaviour
    {
        private PlayerInput _playerInput;

        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();
        }

        public void OpenMenu()
        {
            // Switch to UI controls
            _playerInput.SwitchCurrentActionMap("UI");
        }

        public void CloseMenu()
        {
            // Switch back to gameplay controls
            _playerInput.SwitchCurrentActionMap("Gameplay");
        }
    }
}
