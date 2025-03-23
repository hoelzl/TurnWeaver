using UI;
using UnityEngine;

namespace Interaction
{
    public class InteractionManager : MonoBehaviour, IInteractionSystem
    {
        private UIManager _uiManager;
        private IInteractionHandler _currentHandler;
        private IInteractable _currentInteractable;

        private void Awake()
        {
            _uiManager = FindAnyObjectByType<UIManager>();
            if (_uiManager == null)
                Debug.LogError("UIManager not found in scene!");
        }

        public void ShowInteractionOptions(IInteractable interactable, Vector3 worldPosition)
        {
            _currentInteractable = interactable;

            // Get interaction options
            string[] options = interactable.GetInteractionOptions();

            // If there's only one option, execute it directly
            if (options.Length <= 1)
            {
                interactable.Interact(_currentHandler);

                // Always notify handler when the interaction is complete
                _currentHandler?.OnInteractionComplete();
                return;
            }

            // Show menu with options
            _uiManager.ShowInteractionMenu(options, worldPosition,
                (option) => { HandleInteractionSelection(interactable, option); });
        }

        public void HandleInteractionSelection(IInteractable interactable, string option)
        {
            _uiManager.CloseInteractionMenu();
            interactable.InteractWithOption(_currentHandler, option);

            Debug.Log("Notifying handler that interaction is complete");
            // Notify handler that interaction is complete
            _currentHandler?.OnInteractionComplete();
            _currentInteractable = null;
        }

        public void CancelInteraction()
        {
            _uiManager.CloseInteractionMenu();
            _currentHandler?.OnInteractionComplete();
            _currentInteractable = null;
        }

        public void SetCurrentHandler(IInteractionHandler handler)
        {
            _currentHandler = handler;
        }
    }

}
