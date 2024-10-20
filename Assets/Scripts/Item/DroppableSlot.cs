using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
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
    }
    // ���Կ� �������� ����� �� ȣ��
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
                    Debug.LogWarning($"�ش� ���Կ� ���� �ʴ� ��� ������: {equipment.itemName}");
                }
            }
            else
            {
                Debug.LogWarning("��� �������� �ƴմϴ�.");
            }
        }
    }
}
