using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Inventory/ItemDatabase", order = 1)]
public class ItemDatabase : ScriptableObject
{
    [SerializeField] public List<BaseItem> items = new List<BaseItem>();
}