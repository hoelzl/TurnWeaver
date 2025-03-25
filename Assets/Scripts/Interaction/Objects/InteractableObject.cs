// For containers and usable objects

using UnityEngine;
using UnityEngine.Serialization;

namespace Interaction.Objects
{
    public class InteractableObject : MonoBehaviour, IInteractable
    {
        [SerializeField] private string objectName = "Object";
        [SerializeField] private InteractionOptionSO[] interactionOptions;
        [SerializeField] private bool isLocked;
        [FormerlySerializedAs("requiredKeyItem")] [SerializeField] private string requiredKey = "";

        public string Name => objectName;
        public InteractionOptionSO[] InteractionOptions => interactionOptions;
        public bool AutoInvokeSingleOption => false;

        public bool IsLocked => isLocked;

        public void Unlock(string key)
        {
            if (key == requiredKey) isLocked = false;
        }
    }
}
