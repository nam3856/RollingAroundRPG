using Photon.Pun;
using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Mathematics;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine.TextCore.Text;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public Image[] skillIcons;
    public Image[] cooldownOverlays;
    private string[] skillNames = new string[5];
    private Sprite[] warriorSkillIcons;  // ���� ��ų �����ܵ�
    private Sprite[] gunnerSkillIcons;   // �ų� ��ų �����ܵ�
    private Sprite[] mageSkillIcons;     // ������ ��ų �����ܵ�
    private SkillTreeManager skillTreeManager;
    public GameObject menuPanel;
    public GameObject SkillTreePanel;
    public Button ResetButton;
    public Transform SkillsContainer;
    public GameObject SkillButtonPrefab;
    private List<SkillButton> skillButtons = new List<SkillButton>();
    private int characterIndex;
    public Character character;
    public TMP_Text skillPointText;
    public Skill selectedSkill;
    public Button learnButton;

    public Image hp;
    public Image mp;
    public Image exp;
    public TMP_Text hpText;
    public TMP_Text mpText;
    public TMP_Text levelText;

    private float[] skillCooldowns = new float[5];
    private float[] skillLastUsedTimes = new float[5];

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            ToggleSkillTree();
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (menuPanel.activeSelf)
            {
                menuPanel.SetActive(false);
            }
            else
            {
                menuPanel.SetActive(true);
            }
        }
    }
    private void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        skillTreeManager = FindObjectOfType<SkillTreeManager>();
        InitializeCooldownTimers();

        var skillcooldownManager = FindObjectOfType<SkillCooldownManager>();
        skillcooldownManager.OnSkillUsed += HandleSkillUsed;
        skillcooldownManager.OnSkillReady += HandleSkillReady;

        if (learnButton != null)
            learnButton.onClick.AddListener(OnLearnButtonClicked);
        // �����ܵ��� ���ҽ����� �ε�
        LoadSkillIcons();

        if (ResetButton != null)
        {
            ResetButton.onClick.AddListener(OnResetButtonClicked);
        }
        else
        {
            Debug.LogError("UIManager: ResetButton�� �Ҵ���� �ʾҽ��ϴ�.");
        }
    }
    public void SelectSkill(Skill skill)
    {
        selectedSkill = skill;
        UpdateLearnButton();
    }

    void UpdateLearnButton()
    {
        if (selectedSkill == null)
        {
            learnButton.interactable = false;
            return;
        }

        // �̹� ��� ��ų�̰ų� ��ų ����Ʈ�� �����ϸ� ��Ȱ��ȭ
        if (selectedSkill.IsAcquired || skillTreeManager.PlayerSkillPoints < selectedSkill.Point)
        {
            learnButton.interactable = false;
        }
        else
        {
            learnButton.interactable = true;
        }
    }
    void OnLearnButtonClicked()
    {
        if (selectedSkill == null)
        {
            Debug.LogWarning("���õ� ��ų�� �����ϴ�.");
            return;
        }

        if (selectedSkill.IsAcquired)
        {
            Debug.Log($"{selectedSkill.Name} �̹� ������ϴ�.");
            return;
        }

        if (skillTreeManager.PlayerSkillPoints > 0)
        {
            // ��ų�� ȹ���ϰ� ��ų ����Ʈ�� �Ҹ�
            skillTreeManager.AcquireSkill(selectedSkill, character);
            PopulateSkills(); // UI ������Ʈ
            skillPointText.text = "���� ��ų ����Ʈ : "+skillTreeManager.PlayerSkillPoints.ToString();
            UpdateLearnButton(); // LearnButton ���� ������Ʈ
        }
        else
        {
            Debug.Log("��ų ����Ʈ�� �����մϴ�.");
        }
    }
    private void OnResetButtonClicked()
    {
        ResetSkills();
    }

    public void ResetSkills()
    {
        skillTreeManager.ResetSkills();
        PopulateSkills();
        skillPointText.text = "���� ��ų ����Ʈ : " + skillTreeManager.PlayerSkillPoints.ToString();
    }

    public void CloseSkillMenu()
    {
        SkillTreePanel.SetActive(false);
    }

    void ToggleSkillTree()
    {
        bool isActive = SkillTreePanel.activeSelf;
        SkillTreePanel.SetActive(!isActive);

        if (!isActive)
        {
            PopulateSkills();
        }
    }

    void PopulateSkills()
    {
        CharacterClass playerClass = skillTreeManager.PlayerClass;
        if (playerClass == null)
        {
            Debug.LogError("UIManager: PlayerClass�� null�Դϴ�.");
            return;
        }

        foreach (Transform child in SkillsContainer)
        {
            Destroy(child.gameObject);
        }

        skillButtons.Clear();

        foreach (var skill in skillTreeManager.PlayerClass.Skills)
        {
            GameObject btnObj = Instantiate(SkillButtonPrefab, SkillsContainer);
            SkillButton btnScript = btnObj.GetComponent<SkillButton>();
            if (btnScript != null)
            {
                btnScript.Initialize(skill, skillTreeManager, character);
                skillButtons.Add(btnScript);
            }
            else
            {
                Debug.LogError("SkillButtonPrefab�� SkillButton ��ũ��Ʈ�� �����ϴ�.");
            }
        }

        // ��ų �̸� �迭 ������Ʈ
        for (int i = 0; i < playerClass.Skills.Count; i++)
        {
            skillNames[i] = playerClass.Skills[i].Name;
            skillIcons[i].sprite = playerClass.Skills[i].icon;
            skillIcons[i].color = playerClass.Skills[i].IsAcquired ? Color.white : Color.gray;
            cooldownOverlays[i].fillAmount = 0;
        }
    }

    public void SetHp(int currentHp, int maxHp, Character character)
    {
        hp.fillAmount = (float)currentHp/maxHp;
        hpText.text = currentHp.ToString() + "/" + maxHp.ToString();
        character.HealthImage.fillAmount = hp.fillAmount;
    }

    public void SetMp(int currentMp, int maxMp)
    {
        mp.fillAmount = (float)currentMp / maxMp;

        mpText.text = currentMp.ToString() + "/" + maxMp.ToString();
    }


    public void SetExp(int currentExp, int maxExp)
    {
        exp.fillAmount = (float)currentExp / maxExp;
    }


    public void SetLevel(int level)
    {
        levelText.text = level.ToString();
    }
    private void OnDestroy()
    {

        SkillCooldownManager.Instance.OnSkillUsed -= HandleSkillUsed;
        SkillCooldownManager.Instance.OnSkillReady -= HandleSkillReady;
    }

    private void LoadSkillIcons()
    {
        // ���� ��ų ������ �ε�
        warriorSkillIcons = new Sprite[5];
        warriorSkillIcons[0] = Resources.Load<Sprite>("Icons/Warrior_Skill1");
        warriorSkillIcons[1] = Resources.Load<Sprite>("Icons/Warrior_Skill2");
        warriorSkillIcons[2] = Resources.Load<Sprite>("Icons/Warrior_Skill3");
        warriorSkillIcons[3] = Resources.Load<Sprite>("Icons/Warrior_Skill4");
        warriorSkillIcons[4] = Resources.Load<Sprite>("Icons/Warrior_Skill5");

        // �ų� ��ų ������ �ε�
        gunnerSkillIcons = new Sprite[5];
        gunnerSkillIcons[0] = Resources.Load<Sprite>("Icons/Gunner_Skill1");
        gunnerSkillIcons[1] = Resources.Load<Sprite>("Icons/Gunner_Skill2");
        gunnerSkillIcons[2] = Resources.Load<Sprite>("Icons/Gunner_Skill3");
        gunnerSkillIcons[3] = Resources.Load<Sprite>("Icons/Gunner_Skill4");
        gunnerSkillIcons[4] = Resources.Load<Sprite>("Icons/Gunner_Skill5");

        // ������ ��ų ������ �ε�
        mageSkillIcons = new Sprite[5];
        mageSkillIcons[0] = Resources.Load<Sprite>("Icons/Mage_Skill1");
        mageSkillIcons[1] = Resources.Load<Sprite>("Icons/Mage_Skill2");
        mageSkillIcons[2] = Resources.Load<Sprite>("Icons/Mage_Skill3");
        mageSkillIcons[3] = Resources.Load<Sprite>("Icons/Mage_Skill4");
        mageSkillIcons[4] = Resources.Load<Sprite>("Icons/Mage_Skill5");
    }

    private void HandleSkillUsed(string skillName, float cooldown)
    {
        int index = GetSkillIconIndex(skillName);

        CooldownOverlayAsync(index, cooldown).Forget();
    }

    private void HandleSkillReady(string skillName)
    {
        int index = GetSkillIconIndex(skillName);
        cooldownOverlays[index].fillAmount = 0;
    }

    /// <summary>
    /// �� ��ų�� ��ٿ��� �ð������� �����ݴϴ�. �񵿱�
    /// </summary>
    /// <param name="skillIndex">��ų ��ȣ</param>
    /// <param name="cooldown">��Ÿ��</param>
    /// <returns></returns>
    private async UniTask CooldownOverlayAsync(int skillIndex, float cooldown)
    {
        float elapsedTime = 0f;

        while (elapsedTime < cooldown)
        {
            elapsedTime += Time.deltaTime;
            cooldownOverlays[skillIndex].fillAmount = 1 - (elapsedTime / cooldown);
            await UniTask.Yield();
        }

        cooldownOverlays[skillIndex].fillAmount = 0;
    }

    private void InitializeCooldownTimers()
    {
        for (int i = 0; i < skillNames.Length; i++)
        {
            skillCooldowns[i] = 0;
            skillLastUsedTimes[i] = -9999f;
        }
    }

    public void InitializeSkillsForClass(string characterClass)
    {
        if (characterClass == "Warrior")
        {
            characterIndex = 0;
            skillNames = new string[] { "�⺻ �˼�", "��� �˼�", "����", "�ʻ� �ϰ�", "���� ����" };
            UpdateSkillIcons(warriorSkillIcons);  // ���� ��ų ���������� ������Ʈ
        }
        else if (characterClass == "Gunner")
        {
            characterIndex = 1;
            skillNames = new string[] { "�⺻ ���", "����", "����ź ��ô", "����", "������" };
            UpdateSkillIcons(gunnerSkillIcons);  // �ų� ��ų ���������� ������Ʈ
        }
        else if (characterClass == "Mage")
        {
            characterIndex = 2;
            skillNames = new string[] { "ȭ����", "���� ��ȣ��", "ġ���� �ĵ�", "���׿�", "�ڷ���Ʈ" };
            UpdateSkillIcons(mageSkillIcons);  // ������ ��ų ���������� ������Ʈ
        }

        InitializeSkillIcons();
        InitializeCooldownOverlays();
    }

    private void UpdateSkillIcons(Sprite[] newIcons)
    {
        for (int i = 0; i < skillIcons.Length; i++)
        {
            if (i < newIcons.Length && newIcons[i] != null)
            {
                skillIcons[i].sprite = newIcons[i];  // ������ ������Ʈ
            }
        }
    }

    private void InitializeSkillIcons()
    {
        for (int i = 0; i < skillIcons.Length; i++)
        {
            if (i < skillNames.Length && !string.IsNullOrEmpty(skillNames[i]))
            {
                skillIcons[i].gameObject.SetActive(true);
                skillIcons[i].color = Color.gray;  // ����� ���� ��ų�� ȸ������ ����
            }
            else
            {
                skillIcons[i].gameObject.SetActive(false);
            }
        }
    }


    private void InitializeCooldownOverlays()
    {
        for (int i = 0; i < cooldownOverlays.Length; i++)
        {
            if (i < skillNames.Length && !string.IsNullOrEmpty(skillNames[i]))
            {
                cooldownOverlays[i].gameObject.SetActive(true);
                cooldownOverlays[i].fillAmount = 0;
            }
            else
            {
                cooldownOverlays[i].gameObject.SetActive(false);
            }
        }
    }

    public void SetSkillIconToColor(string skillName)
    {
        int index = GetSkillIconIndex(skillName);
        skillIcons[index].gameObject.SetActive(true);
        skillIcons[index].color = Color.white;  // �÷��� ǥ��
    }

    public void SetSkillIconToGrayscale(string skillName)
    {
        int index = GetSkillIconIndex(skillName);
        skillIcons[index].color = Color.gray;  // ������� ǥ��
    }

    private int GetSkillIconIndex(string skillName)
    {
        return Array.IndexOf(skillNames, skillName);
    }

    public void CloseMenu()
    {
        menuPanel.SetActive(false);
    }
}
