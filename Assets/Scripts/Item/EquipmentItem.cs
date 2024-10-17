// EquipmentItem.cs
using UnityEngine;

[CreateAssetMenu(fileName = "New Equipment", menuName = "Inventory/Equipment")]
public class EquipmentItem : BaseItem
{
    public EquipmentSlot slot;
    public int attackBonus;
    public int defenseBonus;
}