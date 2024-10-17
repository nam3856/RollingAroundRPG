// BaseItem.cs
using UnityEngine;

public abstract class BaseItem : ScriptableObject
{
    public string itemName;
    public string itemDescription;
    public Sprite icon;
    public int id;
}
public enum EquipmentSlot
{
    Head,
    Chest,
    Legs,
    Weapon,
    Shield,
}
