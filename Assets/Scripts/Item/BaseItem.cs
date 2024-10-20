// BaseItem.cs
using System;
using UnityEngine;

public abstract class BaseItem : ScriptableObject
{
    public string itemName;
    public string itemDescription;
    public Sprite icon;
    public int id;
    public float dropChance;
    public Guid instanceId;

    public BaseItem()
    {
        instanceId = Guid.NewGuid();
    }
}
public enum EquipmentSlot
{
    Head,
    Chest,
    Legs,
    Weapon,
    Shield,
}
