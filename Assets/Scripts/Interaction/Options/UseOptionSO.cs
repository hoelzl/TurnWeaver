using UnityEngine;

namespace Interaction.Options
{
    [CreateAssetMenu(fileName = "NewUseOption", menuName = "Interaction System/Use Option", order = 0)]
    public class UseOptionSO : InteractionOptionSO
    {
        public override IInteractionOption CreateInstance(IInteractable interactable)
        {
            return new UseOption(interactable);
        }
    }
}
