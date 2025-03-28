using UnityEngine;

namespace Inventory.Items
{
    public enum ConsumableEffect
    {
        Health,
        Mana,
        Stamina,
        StatBoost,
        StatusEffect
    }

    [CreateAssetMenu(fileName = "New Consumable", menuName = "Inventory/Consumable", order = 2)]
    public class ConsumableItemSO : ItemSO
    {
        [Header("Consumable Properties")]
        [SerializeField] private ConsumableEffect effectType;
        [SerializeField] private int effectValue;
        [SerializeField] private float duration;

        // Public getters
        public ConsumableEffect EffectType => effectType;
        public int EffectValue => effectValue;
        public float Duration => duration;

        public override bool CanUse(GameObject user) => true;

        public override void Use(GameObject user)
        {
            // Apply effect to user
            Debug.Log($"Using consumable: {ItemName} with effect {effectType} of value {effectValue}");

            // Here you would implement the actual effect logic
            // For example:
            // var stats = user.GetComponent<CharacterStats>();
            // if (stats != null) stats.ModifyHealth(effectValue);
        }
    }
}
