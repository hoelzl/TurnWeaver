using UnityEngine;

namespace Interaction.Options
{
    [CreateAssetMenu(fileName = "NewExamineOption", menuName = "Interaction System/Examine Option", order = 0)]
    public class ExamineOptionSO : InteractionOptionSO
    {
        [SerializeField]
        [TextArea(2, 10)]
        private string objectDescription;

        public override IInteractionOption CreateInstance(IInteractable interactable)
        {
            return new ExamineOption(interactable, objectDescription);
        }
    }
}
