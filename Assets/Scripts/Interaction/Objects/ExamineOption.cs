using UI;
using UnityEngine;

namespace Interaction.Objects
{
    public class ExamineOption : InteractionOptionBase
    {
        private readonly string _objectDescription;
        public ExamineOption(IInteractable interactable, string objectDescription)
        {
            _objectDescription = objectDescription;
            Interactable = interactable;
        }

        public override string Text => "Examine";
        public override IInteractable Interactable { get; }

        public override void Invoke(GameObject source, UIManager uiManager)
        {
            Debug.Log("Examine: " + _objectDescription?.ToString() ?? "null");
        }
    }
}
