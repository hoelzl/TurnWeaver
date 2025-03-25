using System;
using UI;
using UnityEngine;
using UnityEngine.Serialization;

namespace Interaction
{
    public class InteractionMenu : MonoBehaviour
    {
        [SerializeField] private GameObject buttonContainer;

        private System.Action _onCanceled;

        public void Initialize(InteractionOptionSO[] options, Action<InteractionOptionSO> onOptionSelected)
        {
            var buttonContainerComponent = buttonContainer.GetComponent<ButtonContainer>();
            if (buttonContainerComponent == null)
            {
                Debug.LogWarning("ButtonContainerComponent is missing from ButtonContainer");
                return;
            }

            buttonContainerComponent.Initialize(onOptionSelected);

            foreach (InteractionOptionSO option in options)
            {
                buttonContainerComponent.Add(option);
            }
        }

        public void Cancel()
        {
            _onCanceled?.Invoke();
        }
    }
}
