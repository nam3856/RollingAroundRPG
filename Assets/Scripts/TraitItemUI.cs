using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class TraitItemUI : MonoBehaviour
{
    public Image traitIcon;
    public TMP_Text traitName;
    public Button traitButton;

    private Trait trait;
    private UnityAction<Trait> onTraitSelected;

    public void Setup(Trait traitData, UnityAction<Trait> onSelected)
    {
        trait = traitData;
        onTraitSelected = onSelected;

        traitIcon.sprite = trait.Icon;
        traitName.text = trait.TraitName;

        if (trait.IsStackTrait())
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
