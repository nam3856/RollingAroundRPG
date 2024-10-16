using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;

public class UIManager : MonoBehaviour
{
    #region Singleton
    public static UIManager Instance { get; private set; }
    #endregion

    #region Public Fields

    [Header("Skill UI Elements")]
    public Image[] skillIcons;
    public Image[] cooldownOverlays;
    public GameObject SkillTreePanel;
    public Button ResetButton;
    public Transform SkillsContainer;
    public GameObject SkillButtonPrefab;
    public Button learnButton;

    [Header("Menu UI Elements")]
    public GameObject menuPanel;
    public GameObject chatting;
    public GameObject chattingInput;
    public GameObject skillBar;

    [Header("Character UI Elements")]
    public Image hp;
    public Image mp;
    public Image exp;
    public TMP_Text hpText;
    public TMP_Text mpText;
    public TMP_Text levelText;
    public TMP_Text skillPointText;

    [Header("References")]
    public Character character;

    #endregion

    #region Private Fields

    private string[] skillNames = new string[5];
    private Sprite[] warriorSkillIcons;  // ���� ��ų �����ܵ�
    private Sprite[] gunnerSkillIcons;   // �ų� ��ų �����ܵ�
    private Sprite[] mageSkillIcons;     // ������ ��ų �����ܵ�
    private SkillTreeManager skillTreeManager;
    private List<SkillButton> skillButtons = new List<SkillButton>();
    private int characterIndex;
    private Skill selectedSkill;

    private float[] skillCooldowns = new float[5];
    private float[] skillLastUsedTimes = new float[5];

    #endregion

    #region Unity Callbacks

    private void Awake()
    {
        // Initialize Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Initialize Skill Icons Arrays
        warriorSkillIcons = new Sprite[5];
        gunnerSkillIcons = new Sprite[5];
        mageSkillIcons = new Sprite[5];
    }

    private void Start()
    {
        skillTreeManager = FindObjectOfType<SkillTreeManager>();
        InitializeCooldownTimers();
        LoadSkillIcons();
        InitializeUI();

        if (SkillEventManager.Instance != null)
        {
            SubscribeToSkillEvents();
        }

        UpdateSkillPointsUI();
    }

    private void Update()
    {
        HandleInput();
    }

    private void OnDestroy()
    {
        UnsubscribeFromSkillEvents();
    }

    #endregion

    #region Initialization Methods

    private void InitializeUI()
    {
        if (learnButton != null)
            learnButton.onClick.AddListener(OnLearnButtonClicked);

        if (ResetButton != null)
            ResetButton.onClick.AddListener(OnResetButtonClicked);
        else
            Debug.LogError("UIManager: ResetButton�� �Ҵ���� �ʾҽ��ϴ�.");
    }

    private void InitializeCooldownTimers()
    {
        for (int i = 0; i < skillNames.Length; i++)
        {
            skillCooldowns[i] = 0;
            skillLastUsedTimes[i] = -9999f;
        }
    }

    private void LoadSkillIcons()
    {
        // ���� ��ų ������ �ε�
        warriorSkillIcons[0] = Resources.Load<Sprite>("Icons/Warrior_Skill1");
        warriorSkillIcons[1] = Resources.Load<Sprite>("Icons/Warrior_Skill2");
        warriorSkillIcons[2] = Resources.Load<Sprite>("Icons/Warrior_Skill3");
        warriorSkillIcons[3] = Resources.Load<Sprite>("Icons/Warrior_Skill4");
        warriorSkillIcons[4] = Resources.Load<Sprite>("Icons/Warrior_Skill5");

        // �ų� ��ų ������ �ε�
        gunnerSkillIcons[0] = Resources.Load<Sprite>("Icons/Gunner_Skill1");
        gunnerSkillIcons[1] = Resources.Load<Sprite>("Icons/Gunner_Skill2");
        gunnerSkillIcons[2] = Resources.Load<Sprite>("Icons/Gunner_Skill3");
        gunnerSkillIcons[3] = Resources.Load<Sprite>("Icons/Gunner_Skill4");
        gunnerSkillIcons[4] = Resources.Load<Sprite>("Icons/Gunner_Skill5");

        // ������ ��ų ������ �ε�
        mageSkillIcons[0] = Resources.Load<Sprite>("Icons/Mage_Skill1");
        mageSkillIcons[1] = Resources.Load<Sprite>("Icons/Mage_Skill2");
        mageSkillIcons[2] = Resources.Load<Sprite>("Icons/Mage_Skill3");
        mageSkillIcons[3] = Resources.Load<Sprite>("Icons/Mage_Skill4");
        mageSkillIcons[4] = Resources.Load<Sprite>("Icons/Mage_Skill5");
    }

    private void SubscribeToSkillEvents()
    {
        SkillEventManager.Instance.OnSkillLearned += OnSkillLearned;
        SkillEventManager.Instance.OnSkillsReset += OnSkillsReset;
        SkillEventManager.Instance.OnSkillSelected += OnSkillSelected;
        SkillEventManager.Instance.OnSkillUsed += HandleSkillUsed;
        SkillEventManager.Instance.OnSkillReady += HandleSkillReady;
    }

    private void UnsubscribeFromSkillEvents()
    {
        if (SkillEventManager.Instance != null)
        {
            SkillEventManager.Instance.OnSkillLearned -= OnSkillLearned;
            SkillEventManager.Instance.OnSkillsReset -= OnSkillsReset;
            SkillEventManager.Instance.OnSkillSelected -= OnSkillSelected;
            SkillEventManager.Instance.OnSkillUsed -= HandleSkillUsed;
            SkillEventManager.Instance.OnSkillReady -= HandleSkillReady;
        }
    }

    #endregion

    #region Input Handling

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            ToggleSkillTree();
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleInventoryWindow();
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            ToggleTraitWindow();
        }
    }

    #endregion

    #region UI Toggle Methods

    private void ToggleMenu()
    {
        if (menuPanel.activeSelf)
        {
            menuPanel.SetActive(false);
            chatting.SetActive(true);
            skillBar.SetActive(true);
            chattingInput.SetActive(true);
        }
        else
        {
            menuPanel.SetActive(true);
            SkillTreePanel.SetActive(false);
            chatting.SetActive(false);
            skillBar.SetActive(false);
            chattingInput.SetActive(false);
        }
    }

    private void ToggleInventoryWindow()
    {
        throw new NotImplementedException();
    }

    private void ToggleTraitWindow()
    {
        throw new NotImplementedException();
    }

    public void ToggleSkillTree()
    {
        bool isActive = SkillTreePanel.activeSelf;
        SkillTreePanel.SetActive(!isActive);

        if (!isActive)
        {
            PopulateSkills();
        }
    }

    public void CloseSkillMenu()
    {
        SkillTreePanel.SetActive(false);
    }

    public void CloseMenu()
    {
        menuPanel.SetActive(false);
    }

    #endregion

    #region Event Handlers

    private void OnSkillLearned(Skill skill)
    {
        Debug.Log($"OnSkillLearned �̺�Ʈ ����: {skill.Name}");
        PopulateSkills();
        UpdateSkillPointsUI();
        UpdateLearnButton();
        SetSkillIconToColor(skill.Name);
    }

    private void OnSkillsReset()
    {
        Debug.Log("OnSkillsReset �̺�Ʈ ����");
        PopulateSkills();
        UpdateSkillPointsUI();
        selectedSkill = null;
        UpdateLearnButton();

        foreach (var skill in skillTreeManager.PlayerClass.Skills)
        {
            if (skill.IsAcquired)
            {
                SetSkillIconToGrayscale(skill.Name);
            }
        }
    }

    private void OnSkillSelected(Skill skill)
    {
        selectedSkill = skill;
        UpdateLearnButton();
    }

    private void HandleSkillUsed(int viewID, string skillName, float cooldown)
    {
        if (viewID != character.PV.ViewID)
            return;

        int index = GetSkillIconIndex(skillName);
        if (index >= 0)
        {
            CooldownOverlayAsync(index, cooldown).Forget();
        }
    }

    private void HandleSkillReady(int viewID, string skillName)
    {
        if (viewID != character.PV.ViewID)
            return;

        int index = GetSkillIconIndex(skillName);
        if (index >= 0)
        {
            cooldownOverlays[index].fillAmount = 0;
        }
    }

    #endregion

    #region UI Update Methods

    public void UpdateSkillPointsUI()
    {
        skillPointText.text = $"���� ��ų ����Ʈ : {skillTreeManager.PlayerSkillPoints}";
    }

    public void UpdateTraitPointsUI()
    {
        skillPointText.text = $"���� Ư�� ����Ʈ : {skillTreeManager.PlayerSkillPoints}";
    }

    public void SetHp(int currentHp, int maxHp, Character character)
    {
        hp.fillAmount = (float)currentHp / maxHp;
        hpText.text = $"{currentHp}/{maxHp}";
        character.HealthImage.fillAmount = hp.fillAmount;
    }

    public void SetMp(int currentMp, int maxMp)
    {
        mp.fillAmount = (float)currentMp / maxMp;
        mpText.text = $"{currentMp}/{maxMp}";
    }

    public void SetExp(int currentExp, int maxExp)
    {
        exp.fillAmount = (float)currentExp / maxExp;
    }

    public void SetLevel(int level)
    {
        levelText.text = level.ToString();
    }

    #endregion

    #region Skill Management

    private void PopulateSkills()
    {
        CharacterClass playerClass = skillTreeManager.PlayerClass;
        if (playerClass == null)
        {
            Debug.LogError("UIManager: PlayerClass�� null�Դϴ�.");
            return;
        }

        // Clear existing skill buttons
        foreach (Transform child in SkillsContainer)
        {
            Destroy(child.gameObject);
        }
        skillButtons.Clear();

        // Instantiate new skill buttons
        foreach (var skill in playerClass.Skills)
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

        // Update skill names and icons
        for (int i = 0; i < playerClass.Skills.Count; i++)
        {
            if (i < skillNames.Length)
            {
                skillNames[i] = playerClass.Skills[i].Name;
                skillIcons[i].sprite = playerClass.Skills[i].icon;
                skillIcons[i].color = playerClass.Skills[i].IsAcquired ? Color.white : Color.gray;
                cooldownOverlays[i].fillAmount = 0;
            }
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
            skillTreeManager.AcquireSkill(selectedSkill, character);
        }
        else
        {
            Debug.Log("��ų ����Ʈ�� �����մϴ�.");
        }
    }

    void UpdateLearnButton()
    {
        if (selectedSkill == null)
        {
            learnButton.interactable = false;
            return;
        }

        learnButton.interactable = !selectedSkill.IsAcquired && skillTreeManager.PlayerSkillPoints >= selectedSkill.Point;
    }

    private void OnResetButtonClicked()
    {
        ResetSkills();
    }

    public void ResetSkills()
    {
        skillTreeManager.ResetSkills(character);
        PopulateSkills();
        UpdateSkillPointsUI();
    }
    #endregion

    #region Cooldown Management


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

    #endregion

    #region Skill Icon Management

    public void InitializeSkillsForClass(string characterClass)
    {
        switch (characterClass)
        {
            case "Warrior":
                characterIndex = 0;
                skillNames = new string[] { "�⺻ �˼�", "��� �˼�", "����", "�ʻ� �ϰ�", "���� ����" };
                UpdateSkillIcons(warriorSkillIcons);
                break;
            case "Gunner":
                characterIndex = 1;
                skillNames = new string[] { "�⺻ ���", "����", "����ź ��ô", "����", "������" };
                UpdateSkillIcons(gunnerSkillIcons);
                break;
            case "Mage":
                characterIndex = 2;
                skillNames = new string[] { "ȭ����", "���� ��ȣ��", "ġ���� �ĵ�", "���׿�", "�ڷ���Ʈ" };
                UpdateSkillIcons(mageSkillIcons);
                break;
            default:
                Debug.LogError($"Unknown character class: {characterClass}");
                break;
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
                skillIcons[i].sprite = newIcons[i];  // Update icon
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
                skillIcons[i].color = Color.gray;  // Unacquired skills are gray
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
        if (index >= 0)
        {
            skillIcons[index].gameObject.SetActive(true);
            skillIcons[index].color = Color.white;  // Set to color
        }
    }

    public void SetSkillIconToGrayscale(string skillName)
    {
        int index = GetSkillIconIndex(skillName);
        if (index >= 0)
        {
            skillIcons[index].color = Color.gray;  // Set to grayscale
        }
    }

    private int GetSkillIconIndex(string skillName)
    {
        return Array.IndexOf(skillNames, skillName);
    }


    
    #endregion
}
