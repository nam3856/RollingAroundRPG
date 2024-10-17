using UnityEngine;
using TMPro;

public class Tooltip : MonoBehaviour
{

    public TextMeshProUGUI tooltipText; // Tooltip�� ǥ�õ� �ؽ�Ʈ
    public RectTransform backgroundRectTransform; // Tooltip ����� RectTransform

    private Canvas canvas; // Tooltip�� ���� Canvas
    private RectTransform rectTransform; // Tooltip�� RectTransform

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        HideTooltip();
    }

    // ���� ǥ��
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

    // ���� �����
    public void HideTooltip()
    {
        gameObject.SetActive(false);
    }

    // ���� ũ�� ����
    private void ResizeTooltip()
    {
        Vector2 textSize = tooltipText.GetRenderedValues(false);
        backgroundRectTransform.sizeDelta = new Vector2(textSize.x + 20, textSize.y + 20);
    }

    // ���� ��ġ ������Ʈ (���콺 Ŀ�� ���󰡱�)
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
            Vector3 worldPos = canvas.transform.TransformPoint(pos + new Vector2(10, -10)); // ������ �߰�
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
