using UnityEngine;

namespace Interaction.Objects
{
    [CreateAssetMenu(fileName = "NewExamineOption", menuName = "Interaction System/Examine Option", order = 0)]
    public class ExamineOptionSO : InteractionOptionSO
    {
        [SerializeField] private string objectDescription;

        public override IInteractionOption CreateInstance(IInteractable interactable)
        {
            return new ExamineOption(interactable, objectDescription);
        }
    }
}
