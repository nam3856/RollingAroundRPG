// DroppableSlot.cs
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using TMPro;

public class DroppableSlot : MonoBehaviour, IDropHandler
{
    public EquipmentSlot slotType; // 해당 슬롯의 유형 (장비 슬롯일 경우)
    public Image iconImage; // 슬롯에 표시될 아이콘
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
    // 슬롯에 아이템을 드롭할 때 호출
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
                    Debug.LogWarning($"해당 슬롯에 맞지 않는 장비 아이템: {equipment.itemName}");
                }
            }
            else
            {
                Debug.LogWarning("장비 아이템이 아닙니다.");
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
