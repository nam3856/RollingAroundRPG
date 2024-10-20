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

    // 슬롯 비우기
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
                description += $" 회복량: {consumable.restoreAmount}";
            }
            else if (baseItem is EquipmentItem equipment)
            {
                description += equipment.attackBonus > 0 ? $"공격력 증가: {equipment.attackBonus}\n" : "";
                description += equipment.defenseBonus > 0 ? $"방어력 증가: {equipment.defenseBonus}\n" : "";
                description += equipment.hpBonus > 0 ? $"최대 체력 증가: {equipment.hpBonus}\n" : "";
                description += equipment.mpBonus > 0 ? $"최대 마나 증가: {equipment.mpBonus}\n" : "";
                description += equipment.hpRecoveryBonus > 1 ? $"체력 회복량 증가: {equipment.hpRecoveryBonus * 100}%\n" : "";
                description += equipment.mpRecoveryBonus > 1 ? $"마나 회복량 증가: {equipment.mpRecoveryBonus * 100}%\n" : "";
                description += !string.IsNullOrEmpty(equipment.traitName) ? $"(고유) {equipment.traitName}" : "";
            }

            tooltip.ShowTooltip(title, description);
        }
    }


    // 마우스 호버 종료 시 툴팁 숨기기
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
