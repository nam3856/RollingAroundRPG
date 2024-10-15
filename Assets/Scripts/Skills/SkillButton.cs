using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class SkillButton : MonoBehaviour, IPointerClickHandler
{
    public Skill SkillData { get; private set; }
    public Image IconImage;

    public TMP_Text SkillDescriptionText;

    private SkillTreeManager skillTreeManager;
    private Character character;

    private UIManager UIManager;

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

    
}
