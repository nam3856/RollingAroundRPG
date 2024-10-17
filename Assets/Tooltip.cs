using UnityEngine;
using TMPro;

public class Tooltip : MonoBehaviour
{

    public TextMeshProUGUI tooltipText; // Tooltip에 표시될 텍스트
    public RectTransform backgroundRectTransform; // Tooltip 배경의 RectTransform

    private Canvas canvas; // Tooltip이 속한 Canvas
    private RectTransform rectTransform; // Tooltip의 RectTransform

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        HideTooltip();
    }

    // 툴팁 표시
    public void ShowTooltip(string title, string description="", string stats="")
    {
        if (DraggableItem.IsDragging)
        {
            return;
        }
        gameObject.SetActive(true);
        tooltipText.text = $"<color=yellow>{title}</color><line-height=30>\n</line-height>{description}\n{stats}";
        ResizeTooltip();
    }

    // 툴팁 숨기기
    public void HideTooltip()
    {
        gameObject.SetActive(false);
    }

    // 툴팁 크기 조정
    private void ResizeTooltip()
    {
        Vector2 textSize = tooltipText.GetRenderedValues(false);
        backgroundRectTransform.sizeDelta = new Vector2(textSize.x + 20, textSize.y + 20);
    }

    // 툴팁 위치 업데이트 (마우스 커서 따라가기)
    private void Update()
    {
        if (gameObject.activeSelf)
        {
            Vector2 pos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                Input.mousePosition,
                canvas.worldCamera,
                out pos);
            Vector3 worldPos = canvas.transform.TransformPoint(pos + new Vector2(10, -10)); // 오프셋 추가
            rectTransform.position = ClampPositionToScreen(worldPos);
        }
    }

    private Vector3 ClampPositionToScreen(Vector3 position)
    {
        Vector3 newPosition = position;
        Vector3 viewportPos = Camera.main.WorldToViewportPoint(position);
        viewportPos.x = Mathf.Clamp(viewportPos.x, 0.05f, 0.95f);
        viewportPos.y = Mathf.Clamp(viewportPos.y, 0.05f, 0.95f);
        newPosition = Camera.main.ViewportToWorldPoint(viewportPos);
        newPosition.z = 0;
        return newPosition;
    }
}
