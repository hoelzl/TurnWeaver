using UI;
using UI.Core;
using UnityEngine;

namespace Interaction
{
    public interface IInteractionOption
    {
        string Text { get; }
        Sprite Icon { get; }
        IInteractable Interactable { get; }
        void Invoke(GameObject source);
    }
}
