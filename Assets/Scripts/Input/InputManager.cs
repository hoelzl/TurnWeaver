using UI.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Input
{
    /// <summary>
    /// A class that allows switching between different action maps, e.g. for
    /// gameplay and UI.
    ///
    /// Currently unused, since it not yet clear to me, what purpose it serves.
    /// Maybe it could be used, e.g., as an alternative to manually toggling on
    /// and off character movement in the player controller, but it's not yet
    /// clear to me if and how that would work.
    /// </summary>
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
