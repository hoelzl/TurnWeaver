using UnityEngine;

namespace Interaction.Options
{
    [CreateAssetMenu(fileName = "NewOpenOption", menuName = "Interaction System/Open Option", order = 0)]
    public class OpenOptionSO : InteractionOptionSO
    {
        public override IInteractionOption CreateInstance(IInteractable interactable)
        {
            return new OpenOption(interactable);
        }
    }
}
