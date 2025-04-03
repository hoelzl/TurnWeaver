using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public class UniqueIdManager
    {
        private static UniqueIdManager _instance;

        public static UniqueIdManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new UniqueIdManager();
                }
                return _instance;
            }
            private set => _instance = value;
        }

        private readonly Dictionary<string, GameObject> _idToObjectMap = new();

        public void Register(string id, GameObject obj)
        {
            if (!_idToObjectMap.TryGetValue(id, out GameObject value))
            {
                _idToObjectMap.Add(id, obj);
                // Debug.Log($"Registered: {id} -> {obj.name}");
            }
            else
            {
                Debug.LogWarning(
                    $"ID Conflict: Attempted to register duplicate ID '{id}' for object {obj.name}. Existing object: {value?.name ?? "null"}. Ignoring registration.",
                    obj);
            }
        }

        public void Unregister(string id)
        {
            if (_idToObjectMap.ContainsKey(id))
            {
                // Debug.Log($"Unregistered: {id}");
                _idToObjectMap.Remove(id);
            }
        }

        public GameObject GetObjectById(string id)
        {
            _idToObjectMap.TryGetValue(id, out GameObject obj);
            return obj; // Returns null if not found
        }
    }
}
