using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
    public Tooltip tooltip;

    [Header("Trait UI Elements")]
    public GameObject traitPanel;
    public Transform traitListContent;
    public GameObject traitItemPrefab;
    public Image selectedTraitIcon;
    public TMP_Text selectedTraitName;
    public TMP_Text selectedTraitDescription;
    public TMP_Text selectedTraitCost;
    public Button applyTraitButton;
    public Button resetTraitButton;
    public TMP_Text traitPointsText;
    public List<Trait> allTraits;

    [Header("Inventory UI Elements")]
    public GameObject InventoryPanel;
    public Transform inventoryContent;
    public GameObject inventorySlotPrefab;
    public GameObject dragImagePrefab;

    [Header("Equipment UI Elements")]
    public GameObject EquipmentPanel;
    public Transform equipmentPanel;
    public GameObject equipmentSlotPrefab;

    public bool IsInitialized { get; private set; } = false;


    #endregion

    #region Private Fields

    private string[] skillNames = new string[5];
    private Sprite[] warriorSkillIcons;  // 전사 스킬 아이콘들
    private Sprite[] gunnerSkillIcons;   // 거너 스킬 아이콘들
    private Sprite[] mageSkillIcons;     // 마법사 스킬 아이콘들
    private SkillTreeManager skillTreeManager;
    private List<SkillButton> skillButtons = new List<SkillButton>();
    private int characterIndex;
    private Skill selectedSkill;

    private float[] skillCooldowns = new float[5];
    private float[] skillLastUsedTimes = new float[5];

    private Trait selectedTrait;
    private TraitManager traitManager;
    private int availableTraitPoints;

    private List<InventorySlot> inventorySlots = new List<InventorySlot>();
    private Dictionary<EquipmentSlot, DroppableSlot> equipmentSlots = new Dictionary<EquipmentSlot, DroppableSlot>();

    #endregion

    #region Unity Callbacks

    private void Awake()
    {

        // Initialize Skill Icons Arrays
        warriorSkillIcons = new Sprite[5];
        gunnerSkillIcons = new Sprite[5];
        mageSkillIcons = new Sprite[5];

        selectedTraitIcon.gameObject.SetActive(false);
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
        if (character != null)
        {
            traitManager = character.traitManager;
            traitManager.OnTraitAdded += HandleTraitAdded;
            traitManager.OnTraitRemoved += HandleTraitRemoved;
        }

        InitializeInventory();
        InitializeEquipmentSlots();

        IsInitialized = true;
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
            Debug.LogError("UIManager: ResetButton이 할당되지 않았습니다.");

        if (applyTraitButton != null)
            applyTraitButton.onClick.AddListener(OnApplyTrait);
        else
            Debug.LogError("UIManager: ApplyTraitButton이 할당되지 않았습니다.");

        if(resetTraitButton != null)
            resetTraitButton.onClick.AddListener(OnResetTrait);
        else
            Debug.LogError("UIManager: ResetTraitButton이 할당되지 않았습니다.");

    }

    private void InitializeInventory()
    {
        // 인벤토리 슬롯 생성
        for (int i = 0; i < 20; i++) // 예: 20개 슬롯
        {
            GameObject slotObj = Instantiate(inventorySlotPrefab, inventoryContent);
            InventorySlot slot = slotObj.GetComponent<InventorySlot>();
            if (slot != null)
            {
                inventorySlots.Add(slot);
                slot.SetItemInstance(null);
                // DraggableItem 설정
                DraggableItem draggable = slotObj.GetComponent<DraggableItem>();
                if (draggable != null)
                {
                    draggable.SetOnDragEndAction(OnInventoryItemDragEnd);
                }
                else
                {
                    Debug.LogError("InventorySlotPrefab에 DraggableItem 스크립트가 없습니다.");
                }
            }
            else
            {
                Debug.LogError("InventorySlotPrefab에 InventorySlot 컴포넌트가 없습니다.");
            }
        }

        
    }

    private void InitializeEquipmentSlots()
    {
        // 장비 슬롯 생성 및 초기화
        foreach (EquipmentSlot slotType in Enum.GetValues(typeof(EquipmentSlot)))
        {
            GameObject slotObj = Instantiate(equipmentSlotPrefab, equipmentPanel);
            DroppableSlot droppable = slotObj.GetComponent<DroppableSlot>();
            InventorySlot inventorySlot = slotObj.GetComponent<InventorySlot>();


            DraggableItem draggable = slotObj.GetComponent<DraggableItem>();
            if (draggable != null)
            {
                draggable.SetOnDragEndAction(OnInventoryItemDragEnd);
            }

            if (droppable != null && inventorySlot != null)
            {
                droppable.itemSlotType = slotType;
                droppable.typeText.text = slotType.ToString();
                droppable.iconImage = inventorySlot.iconImage;
                droppable.inventorySlot = inventorySlot;

                inventorySlot.slotType = SlotType.Equipment;

                equipmentSlots.Add(slotType, droppable);
            }
            else
            {
                Debug.LogError("EquipmentSlotPrefab에 DroppableSlot 또는 InventorySlot 컴포넌트가 없습니다.");
            }
        }
    }
    public void InitializeTraitUI()
    {
        traitManager = character.traitManager;
        availableTraitPoints = character.GetTraitPoints();

        if (TraitRepository.Instance != null)
        {
            allTraits = TraitRepository.Instance.GetAllTraits();
        }
        else
        {
            Debug.LogError("TraitRepository 인스턴스가 존재하지 않습니다!");
            allTraits = new List<Trait>();
        }

        UpdateTraitPointsUI();
        PopulateTraitList();
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
        // 전사 스킬 아이콘 로드
        warriorSkillIcons[0] = Resources.Load<Sprite>("Icons/Warrior_Skill1");
        warriorSkillIcons[1] = Resources.Load<Sprite>("Icons/Warrior_Skill2");
        warriorSkillIcons[2] = Resources.Load<Sprite>("Icons/Warrior_Skill3");
        warriorSkillIcons[3] = Resources.Load<Sprite>("Icons/Warrior_Skill4");
        warriorSkillIcons[4] = Resources.Load<Sprite>("Icons/Warrior_Skill5");

        // 거너 스킬 아이콘 로드
        gunnerSkillIcons[0] = Resources.Load<Sprite>("Icons/Gunner_Skill1");
        gunnerSkillIcons[1] = Resources.Load<Sprite>("Icons/Gunner_Skill2");
        gunnerSkillIcons[2] = Resources.Load<Sprite>("Icons/Gunner_Skill3");
        gunnerSkillIcons[3] = Resources.Load<Sprite>("Icons/Gunner_Skill4");
        gunnerSkillIcons[4] = Resources.Load<Sprite>("Icons/Gunner_Skill5");

        // 마법사 스킬 아이콘 로드
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
            chatting.SetActive(false);
            skillBar.SetActive(false);
            chattingInput.SetActive(false);
        }
    }

    private void ToggleInventoryWindow()
    {
        bool isActive = InventoryPanel.activeSelf;
        InventoryPanel.SetActive(!isActive);
        EquipmentPanel.SetActive(!isActive);
        if (!isActive)
        {
            tooltip.HideTooltip();
            SkillTreePanel.SetActive(false);
            traitPanel.SetActive(false);
        }
    }

    private void ToggleTraitWindow()
    {
        bool isActive = traitPanel.activeSelf;
        traitPanel.SetActive(!isActive);
        if (!isActive)
        {
            tooltip.HideTooltip();
            SkillTreePanel.SetActive(false);
            InventoryPanel.SetActive(false);
            EquipmentPanel.SetActive(false);
            InitializeTraitUI();
            allTraits = TraitRepository.Instance.GetAllTraits();
        }
    }

    public void ToggleSkillTree()
    {
        bool isActive = SkillTreePanel.activeSelf;
        SkillTreePanel.SetActive(!isActive);

        if (!isActive)
        {
            tooltip.HideTooltip();
            InventoryPanel.SetActive(false);
            EquipmentPanel.SetActive(false);
            traitPanel.SetActive(false);
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
        Debug.Log($"OnSkillLearned 이벤트 수신: {skill.Name}");
        PopulateSkills();
        UpdateSkillPointsUI();
        UpdateLearnButton();
        SetSkillIconToColor(skill.Name);
    }

    private void OnSkillsReset()
    {
        Debug.Log("OnSkillsReset 이벤트 수신");
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
        int index = GetSkillIconIndex(skillName);
        if (index >= 0)
        {
            CooldownOverlayAsync(index, cooldown).Forget();
        }
    }

    private void HandleSkillReady(int viewID, string skillName)
    {
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
        skillPointText.text = $"현재 스킬 포인트 : {skillTreeManager.PlayerSkillPoints}";
    }

    public void UpdateTraitPointsUI()
    {
        availableTraitPoints = character.GetTraitPoints();
        traitPointsText.text = $"현재 특성 포인트 : {availableTraitPoints}";
    }

    public void SetHp(int currentHp, int maxHp)
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
            Debug.LogError("UIManager: PlayerClass이 null입니다.");
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
                Debug.LogError("SkillButtonPrefab에 SkillButton 스크립트가 없습니다.");
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
            Debug.LogWarning("선택된 스킬이 없습니다.");
            return;
        }

        if (selectedSkill.IsAcquired)
        {
            Debug.Log($"{selectedSkill.Name} 이미 배웠습니다.");
            return;
        }

        if (skillTreeManager.PlayerSkillPoints > 0)
        {
            skillTreeManager.AcquireSkill(selectedSkill, character);
        }
        else
        {
            Debug.Log("스킬 포인트가 부족합니다.");
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
                skillNames = new string[] { "기본 검술", "고급 검술", "돌진", "필살 일격", "방패 막기" };
                UpdateSkillIcons(warriorSkillIcons);
                break;
            case "Gunner":
                characterIndex = 1;
                skillNames = new string[] { "기본 사격", "난사", "수류탄 투척", "저격", "구르기" };
                UpdateSkillIcons(gunnerSkillIcons);
                break;
            case "Mage":
                characterIndex = 2;
                skillNames = new string[] { "화염구", "비전 보호막", "치유의 파동", "메테오", "텔레포트" };
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

    #region Trait Management
    void PopulateTraitList()
    {
        foreach (Transform child in traitListContent)
        {
            Destroy(child.gameObject);
        }

        foreach (Trait trait in allTraits)
        {
            GameObject traitItem = Instantiate(traitItemPrefab, traitListContent);
            TraitItemUI itemUI = traitItem.GetComponent<TraitItemUI>();
            itemUI.Setup(trait, OnTraitSelected);
        }
    }

    void OnTraitSelected(Trait trait)
    {
        selectedTrait = trait;
        selectedTraitIcon.sprite = trait.Icon;
        selectedTraitName.text = trait.TraitName;
        selectedTraitDescription.text = trait.Description;
        if (selectedTrait.maxStack > 1) selectedTraitDescription.text += $"\n {trait.stackCount} / {trait.maxStack}";
        selectedTraitCost.text = $"{trait.Cost} 포인트 필요";

        selectedTraitIcon.gameObject.SetActive(true);
        availableTraitPoints = character.GetTraitPoints();

        if (availableTraitPoints >= trait.Cost && !selectedTrait.IsCompletelyLearned()) 
        {
            applyTraitButton.interactable = true;
        }
        else
        {
            selectedTraitCost.text = $"획득한 특성";
            applyTraitButton.interactable = false;
        }
    }
    void OnResetTrait()
    {
        traitManager.ClearTraits();
        PopulateTraitList();
    }
    void OnApplyTrait()
    {
        if (selectedTrait == null)
        {
            Debug.LogError("selectedTrait is null");
            return;
        }


        if (availableTraitPoints < selectedTrait.Cost)
        {
            Debug.LogWarning("Trait 포인트가 부족합니다.");
            return;
        }

        // Trait 적용
        bool success = traitManager.AddTrait(selectedTrait);
        if (success)
        {
            availableTraitPoints -= selectedTrait.Cost;
            UpdateTraitPointsUI();

            // UI 업데이트
            selectedTrait = null;
            selectedTraitIcon.sprite = null;
            selectedTraitName.text = "특성 배우기 완료";
            selectedTraitDescription.text = "";
            selectedTraitCost.text = "";
            applyTraitButton.interactable = false;

            selectedTraitIcon.gameObject.SetActive(false);
            PopulateTraitList();
        }
        else
        { 
        }
    }

    void HandleTraitAdded()
    {
        availableTraitPoints = character.GetTraitPoints();
        UpdateTraitPointsUI();
    }

    void HandleTraitRemoved()
    {
        availableTraitPoints = character.GetTraitPoints();
        UpdateTraitPointsUI();
    }
    #endregion

    #region Drag and Drop Handling

    private void OnInventoryItemDragEnd(ItemInstance itemInstance, Vector3 dropPosition)
    {
        DraggableItem draggable = GetDraggableItemFromItem(itemInstance);
        var original = draggable.originalSlot;
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = dropPosition,
            pointerDrag = draggable != null ? draggable.gameObject : null
        };
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (RaycastResult result in results)
        {
            DroppableSlot droppable = result.gameObject.GetComponent<DroppableSlot>();
            if (droppable != null)
            {
                // 드롭된 슬롯이 장비 슬롯인 경우
                droppable.OnDrop(pointerData);
                return;
            }

            // 드롭된 슬롯이 인벤토리 슬롯인지 확인
            InventorySlot inventorySlot = result.gameObject.GetComponent<InventorySlot>();
            if (inventorySlot != null)
            {
                // 드래그한 아이템의 원래 슬롯이 장비 슬롯인지 확인
                if (original is DroppableSlot equipmentSlot)
                {
                    // 인벤토리 슬롯의 아이템이 같은 부위의 장비 아이템인지 확인
                    if (inventorySlot.itemInstance != null &&
                        inventorySlot.itemInstance.baseItem is EquipmentItem targetEquipment &&
                        targetEquipment.slot == equipmentSlot.itemSlotType)
                    {
                        EquipItem(inventorySlot.itemInstance);
                    }
                    else
                    {
                        UnEquipItem(equipmentSlot);
                    }
                    return;
                }
                else if (original is InventorySlot sourceInventorySlot)
                {
                    SwapItems(sourceInventorySlot, inventorySlot);
                    return;
                }
            }
        }
    }

    private DraggableItem GetDraggableItemFromItem(ItemInstance itemInstance)
    {
        if(itemInstance == null) return null;
        foreach (InventorySlot slot in inventorySlots)
        {
            if (slot.itemInstance != null && slot.itemInstance.instanceId == itemInstance.instanceId)
            {
                return slot.GetComponent<DraggableItem>();
            }
        }

        foreach (var slotUI in equipmentSlots.Values)
        {
            if (slotUI.inventorySlot.itemInstance != null && slotUI.inventorySlot.itemInstance.instanceId == itemInstance.instanceId)
            {
                return slotUI.inventorySlot.GetComponent<DraggableItem>();
            }
        }
        return null;
    }

    private InventorySlot GetInventorySlotFromItem(BaseItem item)
    {
        foreach (InventorySlot slot in inventorySlots)
        {
            if (slot.item == item)
            {
                return slot;
            }
        }
        return null;
    }

    public DroppableSlot GetDroppableSlot(EquipmentSlot slotType)
    {
        if (equipmentSlots.TryGetValue(slotType, out DroppableSlot slotUI))
        {
            return slotUI;
        }
        return null;
    }

    private void SwapItems(InventorySlot fromSlot, InventorySlot toSlot)
    {
        if (fromSlot == toSlot)
            return;
        // 두 슬롯의 아이템 인스턴스를 교환
        ItemInstance tempItemInstance = toSlot.itemInstance;

        toSlot.SetItemInstance(fromSlot.itemInstance);
        fromSlot.SetItemInstance(tempItemInstance);

        Debug.Log($"아이템 교환: {fromSlot.itemInstance?.baseItem.itemName} <-> {toSlot.itemInstance?.baseItem.itemName}");
    }


    #endregion

    #region Inventory Management

    // 인벤토리에 아이템 추가

    public bool AddItemToInventory(ItemInstance itemInstance, bool isLoading = false)
    {

        if (character == null)
        {
            Debug.LogError("character가 null입니다.");
        }
        if (character.playerData == null)
        {
            Debug.LogError("character.playerData가 null입니다.");
        }
        if (character.playerData.InventoryItems == null)
        {
            Debug.LogError("character.playerData.InventoryItems가 null입니다.");
        }
        if (itemInstance == null)
        {
            Debug.LogError("itemInstance 가 null입니다.");
        }
        if (itemInstance.baseItem == null)
        {
            Debug.LogError("baseItem 이 null입니다.");
        }
        BaseItem baseItem = itemInstance.baseItem;
        

        // 소비 아이템의 경우 수량을 합칠 수 있음
        if (baseItem is ConsumableItem)
        {
            // 동일한 아이템이 이미 있는지 확인
            foreach (InventorySlot slot in inventorySlots)
            {
                if (slot.itemInstance != null && slot.itemInstance.baseItem == baseItem)
                {
                    slot.itemInstance.quantity += itemInstance.quantity;
                    slot.UpdateQuantityText();
                    if (!isLoading)
                    {
                        SaveSystem.SavePlayerData(character.playerData);
                    }
                    return true;
                }
            }
        }

        // 빈 슬롯에 추가
        foreach (InventorySlot slot in inventorySlots)
        {
            if (slot.itemInstance == null)
            {
                slot.SetItemInstance(itemInstance);
                character.playerData.InventoryItems.Add(itemInstance);
                if (!isLoading)
                {
                    SaveSystem.SavePlayerData(character.playerData);
                }
                return true;
            }
        }
        return false; // 인벤토리에 빈 슬롯이 없을 경우
    }

    // 인벤토리에서 아이템 제거
    public void RemoveItemFromInventory(ItemInstance itemInstance, bool isLoading = false)
    {
        bool res = false;
        foreach (InventorySlot slot in inventorySlots)
        {
            if (slot.itemInstance != null && slot.itemInstance.instanceId == itemInstance.instanceId)
            {
                slot.ClearSlot();
                res = true;
                break;
            }
        }
        if (res && !isLoading)
        {
            character.playerData.InventoryItems.RemoveAll(i => i.instanceId == itemInstance.instanceId);
            SaveSystem.SavePlayerData(character.playerData);
        }
    }

    #endregion

    #region Equipment Management

    public void EquipItem(ItemInstance itemInstance, bool isLoading = false)
    {
        EquipmentItem equipment = itemInstance.baseItem as EquipmentItem;
        if (equipment == null) return;

        if (equipmentSlots.TryGetValue(equipment.slot, out DroppableSlot slotUI))
        {
            InventorySlot slot = slotUI.inventorySlot;

            // 기존 장비 아이템이 있다면 장비 해제
            if(slot.itemInstance != null)
            {
                if (slot.itemInstance.baseItem != null)
                {
                    UnEquipItem(slotUI, isLoading);
                }
            }
           

            equipment.Equip(character);
            slot.SetItemInstance(itemInstance);

            // 인벤토리에서 아이템 제거
            RemoveItemFromInventory(itemInstance, isLoading);

            if (!isLoading)
            {
                character.playerData.EquippedItems.Add(itemInstance);
                SaveSystem.SavePlayerData(character.playerData);
            }
        }
        else
        {
            Debug.LogError($"해당 장비 슬롯을 찾을 수 없습니다: {equipment.slot}");
        }
    }
    public void UnEquipItem(DroppableSlot equipmentSlot, bool isLoading = false)
    {
        if (equipmentSlot.inventorySlot.itemInstance != null)
        {
            ItemInstance itemInstance = equipmentSlot.inventorySlot.itemInstance;
            EquipmentItem equipmentItem = itemInstance.baseItem as EquipmentItem;
            equipmentItem.Unequip(character);
            AddItemToInventory(itemInstance, isLoading: isLoading);
            equipmentSlot.inventorySlot.SetItemInstance(null);

            if (!isLoading)
            {
                character.playerData.EquippedItems.RemoveAll(i => i.instanceId == itemInstance.instanceId);
                SaveSystem.SavePlayerData(character.playerData);
            }
        }
    }
    #endregion
}
