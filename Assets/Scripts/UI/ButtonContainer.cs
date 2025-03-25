using System.Collections.Generic;
using Interaction;
using UnityEngine;
using UnityEngine;

namespace UI
{
    public class ButtonContainer : MonoBehaviour
    {
        [SerializeField] private GameObject buttonPrefab;

        private readonly List<GameObject> _buttons = new List<GameObject>();
        private System.Action<InteractionOptionSO> _onOptionSelected;

        public void Initialize(System.Action<InteractionOptionSO> onOptionSelected)
        {
            _onOptionSelected = onOptionSelected;
            Clear();
        }

        public void Clear()
        {
            foreach (GameObject button in _buttons)
            {
                Destroy(button);
            }
            _buttons.Clear();
        }

        public void Add(InteractionOptionSO option)
        {
            if (option == null) return;

            GameObject buttonObj = Instantiate(buttonPrefab);
            buttonObj.transform.SetParent(transform, false);
            _buttons.Add(buttonObj);

            // Set up the button
            var interactionButton = buttonObj.GetComponent<InteractionButton>();
            if (interactionButton == null)
            {
                interactionButton = buttonObj.AddComponent<InteractionButton>();
            }

            interactionButton.Setup(option, _onOptionSelected);
        }
    }
}
