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

    // 드래그 시작 시 호출
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
            Debug.LogError("원래 슬롯을 찾을 수 없습니다.");
        }

        Debug.Log("draggable: " + originalSlot);

        canvasGroup.blocksRaycasts = false; // 드래그 중에 Raycast 차단
        UIManager uIManager = FindObjectOfType<UIManager>();

        // 아이템이 EquipmentItem인지 확인
        if (itemInstance.baseItem is EquipmentItem equipmentItem && originalSlot == inventorySlot)
        {
            // UIManager에서 해당 장비 슬롯을 가져옴
            targetDroppableSlot = uIManager.GetDroppableSlot(equipmentItem.slot);
            if (targetDroppableSlot != null && targetDroppableSlot.highlightImage != null)
            {
                targetDroppableSlot.highlightImage.gameObject.SetActive(true);
            }
        }
        // 드래그 이미지 생성
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
            Debug.LogError("DraggableItem: DragImage 프리팹이 할당되지 않았습니다.");
        }
    }


    // 드래그 중일 때 호출
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


    // 드래그 끝날 때 호출
    public void OnEndDrag(PointerEventData eventData)
    {
        IsDragging = false;

        canvasGroup.blocksRaycasts = true; // Raycast 다시 활성화

        if (dragImageInstance != null)
        {
            Destroy(dragImageInstance); // 드래그 이미지 삭제
        }

        if (targetDroppableSlot != null && targetDroppableSlot.highlightImage != null)
        {
            targetDroppableSlot.highlightImage.gameObject.SetActive(false);
            targetDroppableSlot = null;
        }
        onDragEnd?.Invoke(itemInstance, eventData.position);
    }


    // 드래그 종료 시 호출할 액션 설정
    public void SetOnDragEndAction(Action<ItemInstance, Vector3> action)
    {
        onDragEnd = action;
    }
}
