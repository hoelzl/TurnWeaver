using UI;
using UnityEngine;

namespace Interaction
{
    public abstract class InteractionOptionBase : IInteractionOption
    {
        [SerializeField] private Sprite _icon;

        public abstract string Text { get; }
        public Sprite Icon => this._icon;

        public abstract IInteractable Interactable { get; }
        public abstract void Invoke(GameObject source);
    }
}
