using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using UnityEngine.EventSystems;
using static UnityEditor.Progress;

public class TraitItemUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image traitIcon;
    public TMP_Text traitName;
    public Button traitButton;

    private Trait trait;
    private UnityAction<Trait> onTraitSelected;
    private Tooltip tooltip;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (trait != null && tooltip != null)
        {
            string title = trait.TraitName;
            string description = trait.Description;

            tooltip.ShowTooltip(title, description);
        }
    }

    // 마우스 호버 종료 시 툴팁 숨기기
    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltip != null)
        {
            tooltip.HideTooltip();
        }
    }

    public void Setup(Trait traitData, UnityAction<Trait> onSelected)
    {
        trait = traitData;
        onTraitSelected = onSelected;
        UIManager uI = FindObjectOfType<UIManager>();
        tooltip = uI.tooltip;
        traitIcon.sprite = trait.Icon;
        traitName.text = trait.TraitName;

        if (trait.IsCompletelyLearned())
        {
            traitIcon.color = Color.gray;
        }
        traitButton.onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        onTraitSelected?.Invoke(trait);
    }

    private void OnDestroy()
    {
        traitButton.onClick.RemoveListener(OnClick);
    }
}
