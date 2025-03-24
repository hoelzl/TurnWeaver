using UnityEngine;

namespace Interaction
{
    [CreateAssetMenu(fileName = "New Interaction", menuName = "Interaction System/Option", order = 0)]
    public abstract class InteractionOptionSO : ScriptableObject
    {
        [SerializeField] protected string optionText;

        public string Text => optionText;

        public abstract IInteractionOption CreateInstance(IInteractable interactable);
    }
}
