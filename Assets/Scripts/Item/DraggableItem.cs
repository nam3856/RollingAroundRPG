using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public BaseItem item; // 드래그할 아이템 데이터
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

    // 드래그 시작 시 호출
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (item == null) return;
        IsDragging = true;
        originalPosition = rectTransform.position;
        canvasGroup.blocksRaycasts = false; // 드래그 중에 Raycast 차단
        UIManager uIManager = FindObjectOfType<UIManager>();
        // 드래그 이미지 생성
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
            Debug.LogError("DraggableItem: DragImage 프리팹이 할당되지 않았습니다.");
        }

    }

    // 드래그 중일 때 호출
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

    // 드래그 끝날 때 호출
    public void OnEndDrag(PointerEventData eventData)
    {
        IsDragging = false;

        canvasGroup.blocksRaycasts = true; // Raycast 다시 활성화

        if (dragImageInstance != null)
        {
            Destroy(dragImageInstance); // 드래그 이미지 삭제
        }
        Debug.Log("DraggableItem.cs : 이벤트 호출");
        onDragEnd?.Invoke(item, eventData.position);
    }

    // 드래그 종료 시 호출할 액션 설정
    public void SetOnDragEndAction(Action<BaseItem, Vector3> action)
    {
        onDragEnd = action;
    }
}
