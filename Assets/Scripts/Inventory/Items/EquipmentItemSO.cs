using UnityEngine;

namespace Inventory.Items
{
    public enum EquipmentSlot
    {
        Head,
        Chest,
        Hands,
        Legs,
        Feet,
        MainHand,
        OffHand,
        Accessory
    }

    [CreateAssetMenu(fileName = "New Equipment", menuName = "Inventory/Equipment", order = 1)]
    public class EquipmentItemSO : ItemSO
    {
        [Header("Equipment Properties")]
        [SerializeField] private EquipmentSlot slot;
        [SerializeField] private int armor;
        [SerializeField] private int damage;
        [SerializeField] private int strength;
        [SerializeField] private int dexterity;
        [SerializeField] private int intelligence;

        // Public getters
        public EquipmentSlot Slot => slot;
        public int Armor => armor;
        public int Damage => damage;
        public int Strength => strength;
        public int Dexterity => dexterity;
        public int Intelligence => intelligence;

        public override bool CanUse(GameObject user) => true;

        public override void Use(GameObject user)
        {
            // Equip the item
            var equipment = user.GetComponent<Equipment>();
            if (equipment != null)
            {
                equipment.EquipItem(this);
            }
        }
    }
}
