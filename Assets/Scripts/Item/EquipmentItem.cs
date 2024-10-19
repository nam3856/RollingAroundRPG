// EquipmentItem.cs
using UnityEngine;

[CreateAssetMenu(fileName = "New Equipment", menuName = "Inventory/Equipment")]
public class EquipmentItem : BaseItem
{
    public EquipmentSlot slot;
    public int attackBonus;
    public int defenseBonus;
    public float hpBonus;
    public float mpBonus;
    public float hpRecoveryBonus = 1;
    public float mpRecoveryBonus = 1;
    public string traitName;

    public void Equip(Character character)
    {
        character.ApplyEquipment(id,defenseBonus,attackBonus,hpBonus,mpBonus,hpRecoveryBonus,mpRecoveryBonus,traitName);
    }
    public void Unequip(Character character)
    {
        character.ApplyEquipment(id,defenseBonus, attackBonus, hpBonus, mpBonus, hpRecoveryBonus, mpRecoveryBonus, traitName, false);
    }
}