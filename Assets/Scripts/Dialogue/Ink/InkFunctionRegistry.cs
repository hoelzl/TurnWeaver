using System;
using Ink.Runtime;
using Player;
using UnityEngine;
using UnityEngine.Serialization;
using Core;
using Inventory;
using Quests;

namespace Dialogue.Ink
{
    /// <summary>
    /// Centralizes registration of external functions for Ink stories.
    /// Functions are registered once per Story instance when created.
    /// </summary>
    public class InkFunctionRegistry : MonoBehaviour
    {
        public static InkFunctionRegistry Instance { get; private set; }

        [SerializeField]
        private InkDialogueController inkDialogueController;

        // References to other systems that functions might need
        [SerializeField] private PlayerController playerController;
        [SerializeField] private QuestManager questManager;

        // Runtime context for function execution
        private NPCInkData _currentNpcData;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Find needed references if not assigned
            if (inkDialogueController == null)
                inkDialogueController = FindFirstObjectByType<InkDialogueController>();
            if (inkDialogueController == null)
                Debug.LogError("InkFunctionRegistry: InkDialogueController not found!", this);

            if (questManager == null)
                questManager = FindFirstObjectByType<QuestManager>();
            if (questManager == null)
                Debug.LogError("InkFunctionRegistry: QuestManager not found!", this);
        }

        /// <summary>
        /// Registers all external functions with a story instance.
        /// Call this once when a story is created, not for each processor.
        /// </summary>
        public void RegisterFunctions(Story story)
        {
            if (story == null) return;

            try
            {
                // NPC functions
                story.BindExternalFunction("get_npc_name",
                    () => _currentNpcData?.NpcComponent?.NpcName ?? "Unknown NPC",
                    true);

                RegisterInventoryFunctions(story);
                RegisterCurrencyFunctions(story);
                RegisterPlayerFunctions(story);
                RegisterWorldFunctions(story);
                RegisterQuestFunctions(story);
            }
            catch (Exception e)
            {
                Debug.LogError($"InkFunctionRegistry: Error registering functions: {e.Message}");
            }
        }

        private RPGInventory GetInventoryFromUniqueId(string uniqueId)
        {
            GameObject targetObject = UniqueIdManager.Instance?.GetObjectById(uniqueId);
            if (targetObject == null)
            {
                Debug.LogWarning($"InkFunctionRegistry: Could not find GameObject with Unique ID '{uniqueId}'.");
                return null;
            }
            RPGInventory inventory = targetObject.GetComponent<RPGInventory>();
            if (inventory == null)
            {
                Debug.LogWarning(
                    $"InkFunctionRegistry: GameObject '{targetObject.name}' (ID: {uniqueId}) does not have an RPGInventory component.");
                return null;
            }
            return inventory;
        }

        private void RegisterInventoryFunctions(Story story)
        {
            // add_item(uniqueId, uniqueItemName, quantity) -> bool
            story.BindExternalFunction("add_item", (string uniqueId, string uniqueItemName, int quantity) =>
            {
                RPGInventory inventory = GetInventoryFromUniqueId(uniqueId);
                if (inventory != null)
                {
                    return inventory.AddItem(uniqueItemName, quantity);
                }
                return false; // Return false if inventory not found or failed
            });

            // can_add_item(uniqueId, uniqueItemName, quantity) -> bool
            story.BindExternalFunction("can_add_item", (string uniqueId, string uniqueItemName, int quantity) =>
            {
                RPGInventory inventory = GetInventoryFromUniqueId(uniqueId);
                if (inventory != null)
                {
                    return inventory.CanAddItem(uniqueItemName, quantity);
                }
                return false; // Return false if inventory not found
            }, true);

            // count_item(uniqueId, uniqueItemName) -> int
            story.BindExternalFunction("count_item", (string uniqueId, string uniqueItemName) =>
            {
                RPGInventory inventory = GetInventoryFromUniqueId(uniqueId);
                if (inventory != null)
                {
                    return inventory.CountItem(uniqueItemName);
                }
                return 0; // Return 0 if inventory not found
            }, true);

            // remove_item(uniqueId, uniqueItemName, quantity) -> bool
            story.BindExternalFunction("remove_item", (string uniqueId, string uniqueItemName, int quantity) =>
            {
                RPGInventory inventory = GetInventoryFromUniqueId(uniqueId);
                if (inventory != null)
                {
                    return inventory.RemoveItem(uniqueItemName, quantity);
                }
                return false; // Return false if inventory not found or failed
            });
        }

        private void RegisterCurrencyFunctions(Story story)
        {
            // add_currency(uniqueId, amount)
            story.BindExternalFunction("add_currency", (string uniqueId, int amount) =>
            {
                RPGInventory inventory = GetInventoryFromUniqueId(uniqueId);
                if (inventory != null)
                {
                    inventory.AddCurrency(amount);
                    // No specific return value needed, maybe log success?
                    // Debug.Log($"Ink AddCurrency: Added {amount} to {uniqueId}");
                }
            });

            // get_currency(uniqueId) -> int
            story.BindExternalFunction("get_currency", (string uniqueId) =>
            {
                RPGInventory inventory = GetInventoryFromUniqueId(uniqueId);
                if (inventory != null)
                {
                    return inventory.Currency;
                }
                return 0;
            }, true);

            // remove_currency(uniqueId, amount) -> bool
            story.BindExternalFunction("remove_currency", (string uniqueId, int amount) =>
            {
                RPGInventory inventory = GetInventoryFromUniqueId(uniqueId);
                if (inventory != null)
                {
                    return inventory.RemoveCurrency(amount);
                }
                return false; // Return false if inventory not found or couldn't remove
            });
        }

        private void RegisterPlayerFunctions(Story story)
        {
            // Player-related functions
            // ...
        }

        private void RegisterWorldFunctions(Story story)
        {
            // World state and query functions

            // get_name(uniqueId) -> string
            story.BindExternalFunction("get_name", (string uniqueId) =>
            {
                GameObject targetObject = UniqueIdManager.Instance?.GetObjectById(uniqueId);
                if (targetObject == null)
                {
                    Debug.LogWarning($"InkFunctionRegistry: Could not find GameObject with UniqueID '{uniqueId}'.");
                    return null;
                }

                NPCInkData inkData = targetObject.GetComponent<NPCInkData>();
                if (inkData != null)
                {
                    string nameFromNpcComponent = inkData.NpcComponent?.NpcName;
                    if (!string.IsNullOrEmpty(nameFromNpcComponent))
                    {
                        return nameFromNpcComponent;
                    }
                }

                return targetObject.name;
            }, true);
        }

        private void RegisterQuestFunctions(Story story)
        {
            if (questManager == null)
            {
                Debug.LogWarning("QuestManager not found, skipping quest function registration.");
                return;
            }

            // --- Quest Status ---

            // get_quest_status(quest_short_name) -> string
            story.BindExternalFunction("get_quest_status",
                (string questShortName) => { return questManager.GetQuestStatus(questShortName).ToString(); },
                true);

            // set_quest_status(quest_short_name, status_string) -> bool
            story.BindExternalFunction("set_quest_status", (string questShortName, string statusString) =>
            {
                if (Enum.TryParse<QuestStatus>(statusString, true, out QuestStatus newStatus))
                {
                    return questManager.SetQuestStatus(questShortName, newStatus);
                }
                Debug.LogWarning(
                    $"InkFunctionRegistry: Invalid quest status string '{statusString}' for set_quest_status.");
                return false;
            });

            // --- Task Status ---

            // get_task_status(quest_short_name, task_short_name) -> string
            story.BindExternalFunction("get_task_status",
                (string questShortName, string taskShortName) =>
                {
                    return questManager.GetTaskStatus(questShortName, taskShortName).ToString();
                }, true); // isLookupSafe = true

            // set_task_status(quest_short_name, task_short_name, status_string) -> bool
            story.BindExternalFunction("set_task_status",
                (string questShortName, string taskShortName, string statusString) =>
                {
                    Debug.Log($"Setting task status for '{taskShortName}' to '{statusString}'");
                    if (Enum.TryParse<TaskStatus>(statusString, true, out TaskStatus newStatus))
                    {
                        return questManager.SetTaskStatus(questShortName, taskShortName, newStatus);
                    }
                    Debug.LogWarning(
                        $"InkFunctionRegistry: Invalid task status string '{statusString}' for set_task_status.");
                    return false;
                });
        }

        /// <summary>
        /// Sets context for the current dialogue session.
        /// Call this before starting story processing.
        /// </summary>
        public void SetDialogueContext(NPCInkData npcData)
        {
            _currentNpcData = npcData;
            IsCloseRequested = false;
        }

        /// <summary>
        /// Clears the current dialogue context.
        /// Call this when dialogue ends.
        /// </summary>
        public void ClearDialogueContext()
        {
            _currentNpcData = null;
        }

        /// <summary>
        /// Checks if a close was requested by an Ink function.
        /// </summary>
        public bool IsCloseRequested { get; private set; }
    }
}
