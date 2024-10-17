using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public BaseItem item; // 슬롯에 담긴 아이템
    public Image iconImage; // 슬롯에 표시될 아이콘
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
    // 슬롯에 아이템을 설정하는 메서드
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

    // 슬롯 비우기
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
            Debug.Log($"소비 아이템 사용: {consumable.itemName}, 남은 수량: {quantity}");
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
                description += $"공격 보너스: {equipment.attackBonus}\n방어 보너스: {equipment.defenseBonus}";
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
