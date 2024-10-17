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
            // �÷��� ����
            IconImage.color = Color.white;
        }
        else
        {
            // ������� ����
            IconImage.color = Color.gray;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log(eventData);
        if (SkillDescriptionText != null)
        {
            Debug.Log(SkillData + "����");
            
            SkillDescriptionText.text = $"{SkillData.Name}\n{SkillData.Description}\n��Ÿ�� {SkillData.Cooldown}��, ���� {SkillData.Cost}\n�ʿ� ����Ʈ : {SkillData.Point}";

            if (SkillEventManager.Instance != null)
            {
                Debug.Log($"�̺�Ʈ ���� : {SkillData}");
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
            string costAndCooldown = $"���� {SkillData.Cost} �Ҹ�\n��Ÿ��: {SkillData.Cooldown}��";

            tooltip.ShowTooltip(title, description, costAndCooldown);
        }
    }

    // ���콺 ȣ�� ���� �� ���� �����
    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltip != null)
        {
            tooltip.HideTooltip();
        }
    }

}
