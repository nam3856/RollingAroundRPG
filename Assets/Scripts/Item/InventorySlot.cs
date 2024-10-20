using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public enum SlotType
{
    Inventory,
    Equipment
}
public class InventorySlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public BaseItem item;
    public Image iconImage;
    public TMP_Text quantityText;
    public SlotType slotType = SlotType.Inventory;
    public ItemInstance itemInstance;
    public int quantity = 1;

    private Tooltip tooltip;
    private DraggableItem draggableItem;

    private void Awake()
    {
        draggableItem = GetComponent<DraggableItem>();
    }

    public void SetItemInstance(ItemInstance newItemInstance)
    {
        itemInstance = newItemInstance;

        UIManager uI = FindObjectOfType<UIManager>();

        if (draggableItem == null)
        {
            draggableItem = GetComponent<DraggableItem>();
        }

        draggableItem.itemInstance = newItemInstance;

        if (itemInstance != null)
        {
            tooltip = uI.tooltip;
            iconImage.sprite = itemInstance.baseItem.icon;
            iconImage.color = Color.white;
            UpdateQuantityText();
        }
        else
        {
            iconImage.sprite = null;
            iconImage.color = Color.clear;
            UpdateQuantityText();
        }
    }
    public void UpdateQuantityText()
    {
        if (slotType == SlotType.Equipment) return;
        if (itemInstance == null) return;
        if (itemInstance.baseItem == null)
        {
            quantityText.text = "";
        }
        else if (itemInstance.baseItem is ConsumableItem)
        {
            quantityText.text = itemInstance.quantity > 1 ? itemInstance.quantity.ToString() : "";
        }
        else
        {
            quantityText.text = "";
        }
    }

    // ���� ����
    public void ClearSlot()
    {
        SetItemInstance(null);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (itemInstance == null) return;
        UIManager uI = FindObjectOfType<UIManager>();
        if (slotType == SlotType.Inventory)
        {
            if (itemInstance.baseItem is ConsumableItem consumable)
            {
                consumable.Use(uI.character);
                itemInstance.quantity--;
                if (itemInstance.quantity <= 0)
                {
                    uI.RemoveItemFromInventory(itemInstance);
                    ClearSlot();
                }
                else
                {
                    UpdateQuantityText();
                    SaveSystem.SavePlayerData(uI.character.playerData);
                }
            }
            else if (itemInstance.baseItem is EquipmentItem)
            {
                uI.EquipItem(itemInstance);
            }
        }
        else if (slotType == SlotType.Equipment)
        {
            DroppableSlot equipmentSlot = GetComponent<DroppableSlot>();
            if (equipmentSlot != null)
            {
                uI.UnEquipItem(equipmentSlot);
            }
        }
        tooltip.HideTooltip();
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (itemInstance != null && tooltip != null)
        {
            BaseItem baseItem = itemInstance.baseItem;
            string title = baseItem.itemName;
            string description = baseItem.itemDescription + "\n";

            if (baseItem is ConsumableItem consumable)
            {
                if (consumable.isHealth) description += "HP";
                else description += "MP";
                description += $" ȸ����: {consumable.restoreAmount}";
            }
            else if (baseItem is EquipmentItem equipment)
            {
                description += equipment.attackBonus > 0 ? $"���ݷ� ����: {equipment.attackBonus}\n" : "";
                description += equipment.defenseBonus > 0 ? $"���� ����: {equipment.defenseBonus}\n" : "";
                description += equipment.hpBonus > 0 ? $"�ִ� ü�� ����: {equipment.hpBonus}\n" : "";
                description += equipment.mpBonus > 0 ? $"�ִ� ���� ����: {equipment.mpBonus}\n" : "";
                description += equipment.hpRecoveryBonus > 1 ? $"ü�� ȸ���� ����: {equipment.hpRecoveryBonus * 100}%\n" : "";
                description += equipment.mpRecoveryBonus > 1 ? $"���� ȸ���� ����: {equipment.mpRecoveryBonus * 100}%\n" : "";
                description += !string.IsNullOrEmpty(equipment.traitName) ? $"(����) {equipment.traitName}" : "";
            }

            tooltip.ShowTooltip(title, description);
        }
    }


    // ���콺 ȣ�� ���� �� ���� �����
    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltip != null)
        {
            tooltip.HideTooltip();
        }
    }
    public void AddQuantity(int amount)
    {
        itemInstance.quantity += amount;
        quantityText.text = itemInstance.quantity > 1 ? itemInstance.quantity.ToString() : "";
    }

}
