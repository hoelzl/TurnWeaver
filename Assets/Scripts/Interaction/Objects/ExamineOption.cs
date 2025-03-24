using UI;
using UnityEngine;

namespace Interaction.Objects
{
    public class ExamineOption : IInteractionOption
    {
        private readonly string _objectDescription;
        public ExamineOption(IInteractable interactable, string objectDescription)
        {
            _objectDescription = objectDescription;
            Interactable = interactable;
        }

        public string Text => "Examine";
        public IInteractable Interactable { get; }

        public void Invoke(GameObject source, UIManager uiManager)
        {
            Debug.Log("Examine: " + _objectDescription?.ToString() ?? "null");
        }
    }
}
