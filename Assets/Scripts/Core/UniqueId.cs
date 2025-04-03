using System;
using UnityEngine;

namespace Core
{
    public class UniqueId : MonoBehaviour
    {
        [SerializeField]
        private string uniqueIdVariable;
        [SerializeField]
        private string uniqueIdGuidString;

        public string UniqueIdGuidString => uniqueIdGuidString;

        // Runtime access property for the GUID
        public Guid UniqueIdGuid
        {
            get
            {
                if (Guid.TryParse(uniqueIdGuidString, out Guid result))
                {
                    return result;
                }
                Debug.LogError($"Could not parse GUID string '{uniqueIdGuidString}' on object {gameObject.name}", this);
                return Guid.Empty;
            }
        }


        // --- Helper for Editor Script ---
        // Call this from an editor script to assign a new GUID
        [ContextMenu("Generate GUID")]
        public void GenerateGuid()
        {
            uniqueIdGuidString = Guid.NewGuid().ToString();
#if UNITY_EDITOR
            // Ensure changes are saved if called from ContextMenu
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        private void OnEnable()
        {
            if (!string.IsNullOrEmpty(uniqueIdGuidString))
            {
                UniqueIdManager.Instance?.Register(uniqueIdGuidString, this.gameObject);
            }
        }

        private void OnDisable()
        {
            if (!string.IsNullOrEmpty(uniqueIdGuidString))
            {
                UniqueIdManager.Instance?.Unregister(uniqueIdGuidString);
            }
        }
    }
}
