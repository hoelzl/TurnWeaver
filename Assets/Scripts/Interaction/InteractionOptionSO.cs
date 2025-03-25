using UnityEngine;

namespace Interaction
{
    [CreateAssetMenu(fileName = "New Interaction", menuName = "Interaction System/Option", order = 0)]
    public abstract class InteractionOptionSO : ScriptableObject
    {
        [SerializeField] protected string optionText;
        [SerializeField] protected Sprite icon;

        public string Text => optionText;
        public Sprite Icon => icon;

        public abstract IInteractionOption CreateInstance(IInteractable interactable);
    }
}
