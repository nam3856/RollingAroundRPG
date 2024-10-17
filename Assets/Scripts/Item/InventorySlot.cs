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
    public DraggableItem draggableItem;

    private void Awake()
    {
        if(draggableItem != null)
        {
            Debug.LogWarning($"{gameObject}: draggableItem is null");
        }
    }
    // ���Կ� �������� �����ϴ� �޼���
    public void SetItem(BaseItem newItem, int newQuantity = 1)
    {
        item = newItem;
        quantity = newQuantity;
        UIManager uI = FindObjectOfType<UIManager>();
        tooltip = uI.tooltip;

        if (draggableItem != null)
        {
            draggableItem.item = newItem;
        }

        if (item != null)
        {
            iconImage.sprite = item.icon;
            iconImage.color = Color.white;
            quantityText.text = quantity > 1 ? quantity.ToString() : "";
        }
        else
        {
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
        Debug.Log("InventorySlot clicked");
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
            Debug.Log($"�Һ� ������ ���: {consumable.itemName}, ���� ����: {quantity}");
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
                description += $"���� ���ʽ�: {equipment.attackBonus}\n��� ���ʽ�: {equipment.defenseBonus}";
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
