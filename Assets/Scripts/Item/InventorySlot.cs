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
    public BaseItem item; // 슬롯에 담긴 아이템
    public Image iconImage; // 슬롯에 표시될 아이콘
    public TMP_Text quantityText;
    private Tooltip tooltip;
    public SlotType slotType = SlotType.Inventory;

    public int quantity = 1;
    private DraggableItem draggableItem;

    private void Awake()
    {
        draggableItem = GetComponent<DraggableItem>();
    }

    // 슬롯에 아이템을 설정하는 메서드
    public void SetItem(BaseItem newItem, int newQuantity = 1)
    {
        item = newItem;
        quantity = newQuantity;

        if (draggableItem == null)
        {
            draggableItem = GetComponent<DraggableItem>();
        }

        UIManager uI = FindObjectOfType<UIManager>();
        if (item != null)
        {
            tooltip = uI.tooltip;
            if (draggableItem != null)
            {
                draggableItem.item = newItem;
            }
            iconImage.sprite = item.icon;
            iconImage.color = Color.white;
            uI.character.playerData.InventoryItems[item.id] = uI.character.playerData.InventoryItems.TryGetValue(item.id, out int cur) ? cur + 1 : quantity;
            if (quantityText != null) quantityText.text = quantity > 1 ? quantity.ToString() : "";
        }
        else
        {
            if (draggableItem != null)
            {
                draggableItem.item = null;
            }
            iconImage.sprite = null;
            iconImage.color = Color.clear;
            uI.character.playerData.InventoryItems.Remove(item.id);
            if (quantityText != null) quantityText.text = "";
        }
        SaveSystem.SavePlayerData(uI.character.playerData);
    }

    // 슬롯 비우기
    public void ClearSlot()
    {
        SetItem(null, 0);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (item == null) return;
        UIManager uI = FindObjectOfType<UIManager>();
        if (slotType == SlotType.Inventory)
        {
            if (item is ConsumableItem consumable)
            {
                consumable.Use(uI.character);
                quantity--;
                if (quantity <= 0)
                {
                    ClearSlot();
                }
                else
                {
                    SetItem(item, quantity);
                }
            }
            else if (item is EquipmentItem equip)
            {
                uI.EquipItem(equip);
            }
        }
        else if (slotType == SlotType.Equipment)
        {
            DroppableSlot equipmentSlot = GetComponent<DroppableSlot>();
            if (equipmentSlot != null)
            {
                equipmentSlot.UnEquipItem();
            }
        }
        tooltip.HideTooltip();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (item != null && tooltip != null)
        {
            string title = item.itemName;
            string description = item.itemDescription + "\n";

            if (item is ConsumableItem consumable)
            {
                if (consumable.isHealth) description += "HP";
                else description += "MP";
                description += $" 회복량: {consumable.restoreAmount}";
            }
            else if (item is EquipmentItem equipment)
            {
                description += equipment.attackBonus > 0 ? $"공격력 증가: {equipment.attackBonus}\n" : "";
                description += equipment.defenseBonus > 0 ? $"방어력 증가: {equipment.defenseBonus}\n" : "";
                description += equipment.hpBonus > 0 ? $"최대 체력 증가: {equipment.hpBonus}\n" : "";
                description += equipment.mpBonus > 0 ? $"최대 마나 증가: {equipment.mpBonus}\n" : "";
                description += equipment.hpRecoveryBonus > 1 ? $"체력 회복량 증가: {equipment.hpRecoveryBonus*100}%\n" : "";
                description += equipment.mpRecoveryBonus > 1 ? $"마나 회복량 증가: {equipment.mpRecoveryBonus * 100}%\n" : "";
                description += equipment.traitName !="" ? $"(고유) {equipment.traitName}" : "";
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
        quantity += amount;
        SetItem(item, quantity);
    }

}
