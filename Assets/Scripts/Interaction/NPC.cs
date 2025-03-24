using Interaction.Objects;
using UnityEngine;

namespace Interaction
{
    public class NPC : MonoBehaviour, IInteractable
    {
        [SerializeField] private string npcName = "NPC";
        [SerializeField] private InteractionOptionSO[] interactionOptions;

        public InteractionOptionSO[] InteractionOptions => interactionOptions;
        public bool AutoInvokeSingleOption => true;
    }
}
