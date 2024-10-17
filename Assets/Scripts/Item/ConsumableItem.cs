
using UnityEngine;

[CreateAssetMenu(fileName = "New Consumable", menuName = "Inventory/Consumable")]
public class ConsumableItem : BaseItem
{
    public bool isHealth;
    public float restoreAmount;

    public void Use(Character character)
    {
        character.Restore(isHealth, restoreAmount);
    }
}