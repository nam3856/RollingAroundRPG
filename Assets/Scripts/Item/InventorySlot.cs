using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public BaseItem item; // ���Կ� ��� ������
    public Image iconImage; // ���Կ� ǥ�õ� ������
    public TMP_Text quantityText;
    private Tooltip tooltip;

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

        if (item != null)
        {
            UIManager uI = FindObjectOfType<UIManager>();
            tooltip = uI.tooltip;
            if (draggableItem != null)
            {
                draggableItem.item = newItem;
            }
            iconImage.sprite = item.icon;
            iconImage.color = Color.white;
            quantityText.text = quantity > 1 ? quantity.ToString() : "";
        }
        else
        {
            if (draggableItem != null)
            {
                draggableItem.item = null;
            }
            iconImage.sprite = null;
            iconImage.color = Color.clear;
            quantityText.text = "";
        }
    }

    // ���� ����
    public void ClearSlot()
    {
        SetItem(null);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (item == null) return;
        if (item is ConsumableItem consumable)
        {
            UIManager uI = FindObjectOfType<UIManager>();
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
        else if(TryGetComponent<DroppableSlot>(out DroppableSlot equipment))
        {
            equipment.UnEquipItem();
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
