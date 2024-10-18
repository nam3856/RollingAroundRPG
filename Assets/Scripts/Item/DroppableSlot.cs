// DroppableSlot.cs
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using TMPro;

public class DroppableSlot : MonoBehaviour, IDropHandler
{
    public EquipmentSlot slotType; // �ش� ������ ���� (��� ������ ���)
    public Image iconImage; // ���Կ� ǥ�õ� ������
    public Image highlightImage;
    public TMP_Text typeText;
    private EquipmentItem currentItem;
    

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
                if (equipment.slot == slotType)
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


    public void OnPointerEnter(PointerEventData eventData)
    {
        if (highlightImage != null)
        {
            highlightImage.gameObject.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (highlightImage != null)
        {
            highlightImage.gameObject.SetActive(false);
        }
    }


    public void UnEquipItem()
    {
        if(currentItem is EquipmentItem equipment && currentItem != null) 
        {
            UIManager uI = FindObjectOfType<UIManager>();
            equipment.Unequip(uI.character);
            iconImage.sprite = null;
            iconImage.color = Color.clear;
            currentItem = null;
        }
        
    }
}
