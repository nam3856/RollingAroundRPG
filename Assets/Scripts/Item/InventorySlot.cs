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
    public BaseItem item; // ���Կ� ��� ������
    public Image iconImage; // ���Կ� ǥ�õ� ������
    public TMP_Text quantityText;
    private Tooltip tooltip;
    public SlotType slotType = SlotType.Inventory;

    public int quantity = 1;
    private DraggableItem draggableItem;

    private void Awake()
    {
        draggableItem = GetComponent<DraggableItem>();
    }

    // ���Կ� �������� �����ϴ� �޼���
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

    // ���� ����
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
                description += $" ȸ����: {consumable.restoreAmount}";
            }
            else if (item is EquipmentItem equipment)
            {
                description += equipment.attackBonus > 0 ? $"���ݷ� ����: {equipment.attackBonus}\n" : "";
                description += equipment.defenseBonus > 0 ? $"���� ����: {equipment.defenseBonus}\n" : "";
                description += equipment.hpBonus > 0 ? $"�ִ� ü�� ����: {equipment.hpBonus}\n" : "";
                description += equipment.mpBonus > 0 ? $"�ִ� ���� ����: {equipment.mpBonus}\n" : "";
                description += equipment.hpRecoveryBonus > 1 ? $"ü�� ȸ���� ����: {equipment.hpRecoveryBonus*100}%\n" : "";
                description += equipment.mpRecoveryBonus > 1 ? $"���� ȸ���� ����: {equipment.mpRecoveryBonus * 100}%\n" : "";
                description += equipment.traitName !="" ? $"(����) {equipment.traitName}" : "";
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
        quantity += amount;
        SetItem(item, quantity);
    }

}
