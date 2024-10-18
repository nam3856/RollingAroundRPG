// DroppableSlot.cs
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using TMPro;

public class DroppableSlot : MonoBehaviour, IDropHandler
{
    public SlotType slotType = SlotType.Equipment;
    public EquipmentSlot itemSlotType; // �ش� ������ ���� (��� ������ ���)
    public Image iconImage; // ���Կ� ǥ�õ� ������
    public Image highlightImage;
    public TMP_Text typeText;
    public InventorySlot inventorySlot;

    private void Awake()
    {
        if (highlightImage != null)
        {
            highlightImage.gameObject.SetActive(false);
        }
        iconImage.color = Color.clear;
    }
    // ���Կ� �������� ����� �� ȣ��
    public void OnDrop(PointerEventData eventData)
    {
        DraggableItem draggable = eventData.pointerDrag.GetComponent<DraggableItem>();
        if (draggable != null && draggable.item != null)
        {
            BaseItem droppedItem = draggable.item;

            if (droppedItem is EquipmentItem equipment)
            {
                if (equipment.slot == itemSlotType)
                {
                    UIManager uI = FindObjectOfType<UIManager>();
                    uI.EquipItem(equipment);
                    draggable.item = null;
                }
                else
                {
                    Debug.LogWarning($"�ش� ���Կ� ���� �ʴ� ��� ������: {equipment.itemName}");
                }
            }
            else
            {
                Debug.LogWarning("��� �������� �ƴմϴ�.");
            }
        }
    }

    public void UnEquipItem()
    {
        if(inventorySlot.item != null) 
        {
            UIManager uI = FindObjectOfType<UIManager>();
            EquipmentItem equipmentItem = inventorySlot.item as EquipmentItem;
            equipmentItem.Unequip(uI.character);
            uI.AddItemToInventory(equipmentItem);

            inventorySlot.SetItem(null);
        }
        
    }
}
