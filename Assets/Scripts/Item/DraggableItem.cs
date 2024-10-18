using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public BaseItem item; // �巡���� ������ ������
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Vector3 originalPosition;
    private Action<BaseItem, Vector3> onDragEnd;

    private GameObject dragImageInstance;

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
        if (item == null) return;
        IsDragging = true;
        originalPosition = rectTransform.position;
        canvasGroup.blocksRaycasts = false; // �巡�� �߿� Raycast ����
        UIManager uIManager = FindObjectOfType<UIManager>();
        // �巡�� �̹��� ����
        if (uIManager.dragImagePrefab != null)
        {
            dragImageInstance = Instantiate(uIManager.dragImagePrefab, canvas.transform);
            Image dragImage = dragImageInstance.GetComponent<Image>();
            if (dragImage != null && item != null)
            {
                dragImage.sprite = item.icon;
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
        if (item == null) return;
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
        Debug.Log("DraggableItem.cs : �̺�Ʈ ȣ��");
        onDragEnd?.Invoke(item, eventData.position);
    }

    // �巡�� ���� �� ȣ���� �׼� ����
    public void SetOnDragEndAction(Action<BaseItem, Vector3> action)
    {
        onDragEnd = action;
    }
}
