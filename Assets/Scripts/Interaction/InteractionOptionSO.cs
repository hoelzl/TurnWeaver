using UnityEngine;

namespace Interaction
{
    public abstract class InteractionOptionSO : ScriptableObject
    {
        [SerializeField] protected string optionText;
        [SerializeField] protected Sprite icon;

        public string Text => optionText;
        public Sprite Icon => icon;

        public abstract IInteractionOption CreateInstance(IInteractable interactable);
    }
}
