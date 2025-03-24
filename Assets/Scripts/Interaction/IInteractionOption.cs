using UI;
using UnityEngine;

namespace Interaction
{
    public interface IInteractionOption
    {
        string Text { get; }
        IInteractable Interactable { get; }
        void Invoke(GameObject source, UIManager uiManager);
    }
}
