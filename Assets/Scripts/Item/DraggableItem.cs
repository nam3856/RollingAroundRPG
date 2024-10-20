using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Vector3 originalPosition;
    private Action<ItemInstance, Vector3> onDragEnd;
    private DroppableSlot targetDroppableSlot;
    private GameObject dragImageInstance;
    public MonoBehaviour originalSlot;
    public ItemInstance itemInstance;

    public static bool IsDragging { get; private set; } = false;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();

        canvas = GetComponentInParent<Canvas>();
    }

    // �巡�� ���� �� ȣ��
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (itemInstance == null) return;
        IsDragging = true;

        InventorySlot inventorySlot = GetComponentInParent<InventorySlot>();
        DroppableSlot droppableSlot = GetComponentInParent<DroppableSlot>();

        if (inventorySlot != null && inventorySlot.slotType == SlotType.Inventory)
        {
            originalSlot = inventorySlot;
        }
        else if (inventorySlot != null && inventorySlot.slotType == SlotType.Equipment)
        {
            originalSlot = droppableSlot;
        }
        else
        {
            Debug.LogError("���� ������ ã�� �� �����ϴ�.");
        }

        Debug.Log("draggable: " + originalSlot);

        canvasGroup.blocksRaycasts = false; // �巡�� �߿� Raycast ����
        UIManager uIManager = FindObjectOfType<UIManager>();

        // �������� EquipmentItem���� Ȯ��
        if (itemInstance.baseItem is EquipmentItem equipmentItem && originalSlot == inventorySlot)
        {
            // UIManager���� �ش� ��� ������ ������
            targetDroppableSlot = uIManager.GetDroppableSlot(equipmentItem.slot);
            if (targetDroppableSlot != null && targetDroppableSlot.highlightImage != null)
            {
                targetDroppableSlot.highlightImage.gameObject.SetActive(true);
            }
        }
        // �巡�� �̹��� ����
        if (uIManager.dragImagePrefab != null)
        {
            dragImageInstance = Instantiate(uIManager.dragImagePrefab, canvas.transform);
            Image dragImage = dragImageInstance.GetComponent<Image>();
            if (dragImage != null && itemInstance != null)
            {
                dragImage.sprite = itemInstance.baseItem.icon;
                dragImage.SetNativeSize();
                dragImage.gameObject.SetActive(true);
                dragImage.GetComponent<CanvasGroup>().alpha = 0.5f;
            }
        }
        else
        {
            Debug.LogError("DraggableItem: DragImage �������� �Ҵ���� �ʾҽ��ϴ�.");
        }
    }


    // �巡�� ���� �� ȣ��
    public void OnDrag(PointerEventData eventData)
    {
        if (itemInstance == null) return;
        if (dragImageInstance != null)
        {
            Vector3 globalMousePos;
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(canvas.transform as RectTransform, eventData.position, canvas.worldCamera, out globalMousePos))
            {
                dragImageInstance.transform.position = globalMousePos;
            }
        }
    }


    // �巡�� ���� �� ȣ��
    public void OnEndDrag(PointerEventData eventData)
    {
        IsDragging = false;

        canvasGroup.blocksRaycasts = true; // Raycast �ٽ� Ȱ��ȭ

        if (dragImageInstance != null)
        {
            Destroy(dragImageInstance); // �巡�� �̹��� ����
        }

        if (targetDroppableSlot != null && targetDroppableSlot.highlightImage != null)
        {
            targetDroppableSlot.highlightImage.gameObject.SetActive(false);
            targetDroppableSlot = null;
        }
        onDragEnd?.Invoke(itemInstance, eventData.position);
    }


    // �巡�� ���� �� ȣ���� �׼� ����
    public void SetOnDragEndAction(Action<ItemInstance, Vector3> action)
    {
        onDragEnd = action;
    }
}
