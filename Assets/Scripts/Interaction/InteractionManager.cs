using UI;
using UnityEngine;
using UnityEngine.Serialization;

namespace Interaction
{
    public class InteractionManager : MonoBehaviour
    {
        [SerializeField] private GameObject interactionSource;
        [SerializeField] private UIManager uiManager;

        private IInteractionSource _interactionSourceComponent;

        private void Awake()
        {
            if (uiManager == null)
            {
                uiManager = FindAnyObjectByType<UIManager>();
                if (uiManager == null)
                    Debug.LogError("UIManager not specified and not found in scene!");
            }
            _interactionSourceComponent = interactionSource.GetComponent<IInteractionSource>();
        }

        public void ShowInteractionOptions(IInteractable interactable, Vector3 worldPosition)
        {
            InteractionOptionSO[] options = interactable.InteractionOptions;
            if (_interactionSourceComponent == null)
            {
                Debug.LogWarning("Interaction Source not available. Cannot interact!");
                return;
            }

            switch (options.Length)
            {
                case <= 0:
                    Debug.LogWarning("Interaction needs at least one option!");

                    // Notify source that the interaction is complete, so that it can clean up even if no interaction
                    // took place
                    _interactionSourceComponent.FinalizeInteraction(interactable);
                    break;
                case 1 when interactable.AutoInvokeSingleOption:
                {
                    InteractionOptionSO optionSO = options[0];
                    InteractWithOption(interactable, optionSO);
                    break;
                }
                default:
                    uiManager.ShowInteractionMenu(options, worldPosition,
                        (optionSO) => { InteractWithOption(interactable, optionSO); },
                        () => CancelInteraction(interactable));
                    break;
            }
        }

        private void InteractWithOption(IInteractable interactable, InteractionOptionSO optionSO)
        {
            IInteractionOption option = optionSO.CreateInstance(interactable);
            option.Invoke(interactionSource, uiManager);
            _interactionSourceComponent.FinalizeInteraction(interactable);
        }

        private void CancelInteraction(IInteractable interactable)
        {
            uiManager.CloseInteractionMenu();
            _interactionSourceComponent?.FinalizeInteraction(interactable);
        }

        public void SetInteractionSource(GameObject source)
        {
            interactionSource = source;
        }
    }
}
