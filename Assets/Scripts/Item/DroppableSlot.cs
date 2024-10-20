using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class DroppableSlot : MonoBehaviour, IDropHandler
{
    public SlotType slotType = SlotType.Equipment;
    public EquipmentSlot itemSlotType; // 해당 슬롯의 유형 (장비 슬롯일 경우)
    public Image iconImage; // 슬롯에 표시될 아이콘
    public Image highlightImage;
    public TMP_Text typeText;
    public InventorySlot inventorySlot;

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
        if (draggable != null && draggable.itemInstance != null)
        {
            ItemInstance droppedItemInstance = draggable.itemInstance;

            if (droppedItemInstance.baseItem is EquipmentItem equipment)
            {
                if (equipment.slot == itemSlotType)
                {
                    UIManager uI = FindObjectOfType<UIManager>();
                    uI.EquipItem(droppedItemInstance);
                    draggable.itemInstance = null;
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
}
