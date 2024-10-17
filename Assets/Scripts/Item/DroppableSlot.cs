// DroppableSlot.cs
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

public class DroppableSlot : MonoBehaviour, IDropHandler
{
    public EquipmentSlot slotType; // 해당 슬롯의 유형 (장비 슬롯일 경우)
    public Image iconImage; // 슬롯에 표시될 아이콘
    public Image highlightImage;

    private void Awake()
    {
        if (highlightImage != null)
        {
            highlightImage.gameObject.SetActive(false);
        }
    }
    // 슬롯에 아이템을 드롭할 때 호출
    public void OnDrop(PointerEventData eventData)
    {
        DraggableItem draggable = eventData.pointerDrag.GetComponent<DraggableItem>();
        if (draggable != null && draggable.item != null)
        {
            BaseItem droppedItem = draggable.item;

            // 드롭된 아이템이 장비 아이템이고, 슬롯 유형에 맞는지 확인
            if (droppedItem is EquipmentItem equipment)
            {
                if (equipment.slot == slotType)
                {
                    UIManager uI = FindObjectOfType<UIManager>();
                    uI.EquipItem(equipment);
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

    private void EquipItem(EquipmentItem equipment, DraggableItem draggable)
    {
        // 기존에 장착된 아이템이 있다면 언착용
        if (iconImage.sprite != null)
        {
            // 기존 장비 아이템을 인벤토리로 반환하거나 다른 로직 구현 가능
            Debug.Log($"기존 장비 아이템 언착용: {iconImage.sprite.name}");
        }

        // 새로운 장비 아이템 장착
        iconImage.sprite = equipment.icon;
        Debug.Log($"장비 아이템 장착: {equipment.itemName}");

        // 드래그한 아이템 슬롯 비우기 또는 인벤토리 업데이트
        draggable.item = null;
        draggable.GetComponent<Image>().sprite = null;
    }
}
