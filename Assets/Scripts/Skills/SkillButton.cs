using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class SkillButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Skill SkillData { get; private set; }
    public Image IconImage;

    public TMP_Text SkillDescriptionText;

    private SkillTreeManager skillTreeManager;
    private Character character;

    private UIManager UIManager;
    private Tooltip tooltip;

    private void Awake()
    {
        if (IconImage == null)
            IconImage = GetComponentInChildren<Image>();

        UIManager = FindObjectOfType<UIManager>();
        SkillDescriptionText = GameObject.Find("UI/SkillTreePanel/Tooltip").GetComponent<TMP_Text>();

    }


    public void Initialize(Skill skill, SkillTreeManager manager, Character characterInstance)
    {
        SkillData = skill;
        skillTreeManager = manager;
        character = characterInstance;
        UIManager uI = FindObjectOfType<UIManager>();
        tooltip = uI.tooltip;
        if (IconImage != null)
            IconImage.sprite = SkillData.icon;

        UpdateVisual();
    }

    public void UpdateVisual()
    {
        if (SkillData.IsAcquired)
        {
            // 컬러로 설정
            IconImage.color = Color.white;
        }
        else
        {
            // 흑백으로 설정
            IconImage.color = Color.gray;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log(eventData);
        if (SkillDescriptionText != null)
        {
            Debug.Log(SkillData + "선택");
            
            SkillDescriptionText.text = $"{SkillData.Name}\n{SkillData.Description}\n쿨타임 {SkillData.Cooldown}초, 마나 {SkillData.Cost}\n필요 포인트 : {SkillData.Point}";

            if (SkillEventManager.Instance != null)
            {
                Debug.Log($"이벤트 발행 : {SkillData}");
                SkillEventManager.Instance.SkillSelected(SkillData);
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (SkillData != null && tooltip != null)
        {
            string title = SkillData.Name;
            string description = SkillData.Description;
            string costAndCooldown = $"마나 {SkillData.Cost} 소모\n쿨타임: {SkillData.Cooldown}초";

            tooltip.ShowTooltip(title, description, costAndCooldown);
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

}
