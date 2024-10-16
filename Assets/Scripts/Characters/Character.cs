using Photon.Pun;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Collections;
using UnityEngine.TextCore.Text;

public abstract class Character : MonoBehaviourPunCallbacks, IPunObservable
{
    #region Fields

    [Header("Health and Mana")]
    public float maxHealth = 100;
    protected float maxMP = 100;
    protected float currentMP = 100;
    protected float currentHealth;

    [Header("Combat")]
    public int attackDamage = 1;
    public int experience = 0;
    public int level = 1;
    public List<int> levelPerExperience;
    protected List<Skill> skills = new List<Skill>();

    [Header("Components")]
    public PhotonView PV;
    public Animator AN;
    public Rigidbody2D RB;
    public AudioSource audioSource;
    public Image HealthImage;
    public SkillTreeManager skillTreeManager;
    public PlayerScript player;
    public Transform canvasTransform;

    [Header("Layers")]
    public LayerMask enemyLayer;

    [Header("Effects")]
    public GameObject guardEffectObject;
    public GameObject shieldEffectObject;
    public GameObject damageTextPrefab;
    protected Transform levelTransform;
    protected Transform magicTransform;
    protected Animator levelAnimator;
    protected Animator magicAnimator;

    [Header("Audio Clips")]
    protected AudioClip receivedArcaneShieldSound;

    [Header("Movement")]
    public float moveSpeed = 3f;
    protected Vector2 lastMoveDirection;

    [Header("Attack Parameters")]
    public float AttackDuration = 0.25f;
    public float AttackCooldown = 0.05f;

    [Header("States")]
    public bool isRolling = false;
    public bool isRushing = false;
    public bool isDead { get; private set; } = false;
    protected bool isAttacking = false;
    protected bool isUsingSkill = false;
    protected bool isBlocking = false;

    protected float lastAttackTime = -10f;
    protected float attackTime = 0f;
    protected int arcaneShieldStack = 0;

    protected UIManager uiManager;
    public PlayerData playerData;

    private Vector3 curPos;

    #endregion


    #region Events

    public event Action<Character> OnPhotonViewInitialized;

    #endregion

    #region Unity Callbacks

    public virtual void Start()
    {
        InitializeCharacter();
        if (PV.IsMine)
        {
            InitializeLocalPlayer();
        }
        else
        {
            //InitializeRemotePlayer();
        }

    }

    public virtual void OnDestroy()
    {

    }

    #endregion

    #region Initialization

    /// <summary>
    /// ĳ������ �⺻ ������ �ʱ�ȭ�մϴ�.
    /// </summary>
    private void InitializeCharacter()
    {
        currentHealth = maxHealth;
        AN = GetComponent<Animator>();
        PV = GetComponent<PhotonView>();
        RB = GetComponent<Rigidbody2D>();
        skillTreeManager = FindObjectOfType<SkillTreeManager>();

        InitializeAudio();
        InitializeEffects();
        InitializeUI();
        InitializeLevelExperience();
        InitializeDamageText();
        InitializeMoveDirection();
        LoadResources();

        StartCoroutine(WaitForPhotonView());
    }
    private IEnumerator WaitForPhotonView()
    {
        float timeout = 10f;
        float elapsed = 0f;
        // PhotonView�� �Ҵ�� ������ ���
        while (PV == null && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        // PhotonView�� �ʱ�ȭ�� ������ �߰��� ��� (�ʿ� ��)
        while (!PV.IsMine && PV.Owner == null)
        {
            yield return null;
        }


        if (!PV.IsMine) UpdatePositionAsync().Forget();

        OnPhotonViewInitialized?.Invoke(this);

        playerData = FindObjectOfType<PlayerScript>().playerData;
        UpdateSkillUI().Forget();
    }
    private void InitializeAudio()
    {
        GameObject soundEffectObject = transform.Find("SoundEffect").gameObject;
        soundEffectObject.SetActive(true);
        audioSource = soundEffectObject.GetComponent<AudioSource>();
    }

    private void InitializeEffects()
    {
        shieldEffectObject = transform.Find("ShieldEffect").gameObject;
        guardEffectObject = transform.Find("GuardEffect").gameObject;
        guardEffectObject.SetActive(false);
        shieldEffectObject.SetActive(false);

        magicTransform = transform.Find("MagicEffect");
        magicAnimator = magicTransform.GetComponent<Animator>();

        levelTransform = transform.Find("LevelEffect");
        levelAnimator = levelTransform.GetComponent<Animator>();
    }

    private void InitializeUI()
    {
        uiManager = FindObjectOfType<UIManager>();
        HealthImage = transform.Find("Canvas/Hp").GetComponent<Image>();
        canvasTransform = transform.Find("Canvas");
    }

    /// <summary>
    /// ������ ����ġ ���� ������ �ʱ�ȭ�մϴ�.
    /// </summary>
    private void InitializeLevelExperience()
    {
        levelPerExperience = new List<int>() { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110, 5000000 };
    }

    private void InitializeDamageText()
    {
        damageTextPrefab = Resources.Load<GameObject>("DamageText");
    }

    private void InitializeMoveDirection()
    {
        lastMoveDirection = Vector2.left;
    }

    private void LoadResources()
    {
        receivedArcaneShieldSound = Resources.Load<AudioClip>("Sounds/ReceivedArcaneShieldSound");
    }

    /// <summary>
    /// ���� �÷��̾��� �߰� ������ �ʱ�ȭ�մϴ�.
    /// </summary>
    private void InitializeLocalPlayer()
    {
        LoadCharacterData();
        SetupCamera();

        uiManager.UpdateSkillPointsUI();
        RecoveryHpMpAsync().Forget();
    }

    
    /// <summary>
    /// ���� �÷��̾��� ��ġ�� ������Ʈ�ϱ� ���� �񵿱� �۾��� �����մϴ�.
    /// </summary>
    private async UniTaskVoid UpdatePositionAsync()
    {
        while (true)
        {
            float distance = (transform.position - curPos).sqrMagnitude;

            if (distance > 1.0f)
            {
                transform.position = curPos;
            }
            else
            {
                transform.position = Vector3.Lerp(transform.position, curPos, Time.deltaTime * 10);
            }

            await UniTask.Yield();
        }
    }

    /// <summary>
    /// ī�޶� ������ �ʱ�ȭ�մϴ�.
    /// </summary>
    private void SetupCamera()
    {
        var CM = GameObject.Find("CMCamera").GetComponent<Cinemachine.CinemachineVirtualCamera>();
        CM.Follow = transform;
        CM.LookAt = transform;
    }

    /// <summary>
    /// ĳ���� �����͸� �ε��ϰ� �����մϴ�.
    /// </summary>
    private void LoadCharacterData()
    {
        if (playerData != null)
        {
            level = playerData.Level;
            maxHealth = CalculateMaxHealth(level);
            maxMP = CalculateMaxMP(level);
            currentHealth = maxHealth;
            currentMP = maxMP;

            uiManager.SetHp((int)currentHealth, (int)maxHealth, this);
            uiManager.SetMp((int)currentMP, (int)maxMP);

            foreach (var skillName in playerData.LearnedSkills)
            {
                var skill = skillTreeManager.GetSkillByName(skillName);
                if (skill != null)
                {
                    skill.Acquire(this);
                }
            }
            skillTreeManager.AddSkillPoint(playerData.SkillPoint);
            experience = playerData.Experience;

            if (level > 0)
            {
                uiManager.SetExp(experience, levelPerExperience[level - 1]);
                uiManager.SetLevel(level);
            }
            else
            {
                uiManager.SetExp(0, 1);
            }

            // ���� ������ �� �κ��丮 ���� (�߰� ���� �ʿ�)
        }
    }

    #endregion

    #region Photon Callbacks

    /// <summary>
    /// ���� ��Ʈ��ũ�� ���� �����͸� �ۼ����մϴ�.
    /// </summary>
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // ������ ���� �ۼ��ϴ� ��� (����)
            stream.SendNext(transform.position);       // ��ġ ����ȭ
            stream.SendNext(isBlocking);               // ��� ���� ����ȭ
            stream.SendNext(isAttacking);              // ���� ���� ����ȭ
            stream.SendNext(isRolling);                // ������ ���� ����ȭ
            stream.SendNext(isUsingSkill);             // ��ų ��� ���� ����ȭ
            stream.SendNext(currentHealth);            // ���� ü�� ����ȭ
            stream.SendNext(maxHealth);
            stream.SendNext(HealthImage.fillAmount);   // ü�¹� ä��ⷮ ����ȭ
            stream.SendNext(isDead);                   // ��� ���� ����ȭ
        }
        else
        {
            // �ٸ� Ŭ���̾�Ʈ�κ��� ������ ���� (�б�)
            curPos = (Vector3)stream.ReceiveNext();      // ��ġ ����
            isBlocking = (bool)stream.ReceiveNext();     // ��� ���� ����
            isAttacking = (bool)stream.ReceiveNext();    // ���� ���� ����
            isRolling = (bool)stream.ReceiveNext();      // ������ ���� ����
            isUsingSkill = (bool)stream.ReceiveNext();   // ��ų ��� ���� ����
            currentHealth = (float)stream.ReceiveNext(); // ���� ü�� ����
            maxHealth = (float)stream.ReceiveNext();
            HealthImage.fillAmount = (float)stream.ReceiveNext(); // ü�¹� ä��ⷮ ����
            isDead = (bool)stream.ReceiveNext();         // ��� ���� ����
        }
    }

    #endregion

    #region Recovery

    /// <summary>
    /// ü�°� ������ ���������� ȸ���մϴ�.
    /// </summary>
    protected virtual async UniTask RecoveryHpMpAsync()
    {
        while (!isDead)
        {
            currentHealth += maxHealth / 400;
            if (currentHealth > maxHealth) currentHealth = maxHealth;

            currentMP += maxMP / 300;
            if (currentMP > maxMP) currentMP = maxMP;

            HealthImage.fillAmount = currentHealth / maxHealth;

            if (PV.IsMine)
            {
                uiManager.SetHp((int)currentHealth, (int)maxHealth, this);
                uiManager.SetMp((int)currentMP, (int)maxMP);
            }

            await UniTask.Delay(1000);
        }
    }

    #endregion

    #region Skill Management

    /// <summary>
    /// ��ų UI�� ������Ʈ�մϴ�.
    /// </summary>
    private async UniTaskVoid UpdateSkillUI()
    {
        while (true)
        {
            foreach (Skill skill in skills)
            {
                skill.UpdateSkillUI(this);
            }
            await UniTask.Yield();
        }
    }

    /// <summary>
    /// ��ų�� �����մϴ�.
    /// </summary>
    /// <param name="skillIdx">��ų �ε���</param>
    public virtual void StartSkill(int skillIdx)
    {
        skillTreeManager.UseSkill(skills[skillIdx], this);
        if (PV.IsMine)
        {
            uiManager.SetMp((int)currentMP, (int)maxMP);
        }
    }

    /// <summary>
    /// Ư�� ��ų�� �����մϴ�.
    /// </summary>
    /// <param name="isDown">��ųŰ�� �����ִ��� ����</param>
    public virtual void StartSpecialSkill(bool isDown)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region Damage Handling

    /// <summary>
    /// �������� �����մϴ�.
    /// </summary>
    /// <param name="damage">���� ������</param>
    /// <param name="attackDirection">���� ����</param>
    [PunRPC]
    public virtual void ReceiveDamage(int damage, Vector2 attackDirection)
    {
        if (arcaneShieldStack > 0)
        {
            arcaneShieldStack--;
            damage /= 2;
            magicTransform.gameObject.SetActive(true);
            audioSource.PlayOneShot(receivedArcaneShieldSound);
            magicAnimator.SetTrigger("Arcane Shield");
        }

        GameObject damageTextInstance = PhotonNetwork.Instantiate("DamageText", canvasTransform.position, Quaternion.identity);
        DamageText damageTextScript = damageTextInstance.GetComponent<DamageText>();
        PhotonView damageTextPV = damageTextInstance.GetComponent<PhotonView>();

        GameObject uiCanvas = GameObject.Find("Canvas");
        damageTextInstance.transform.SetParent(uiCanvas.transform, false);

        damageTextPV.RPC("SetDamageText", RpcTarget.All, damage.ToString());
        currentHealth -= damage;

        if (PV.IsMine)
        {
            uiManager.SetHp((int)currentHealth, (int)maxHealth, this);
        }

        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// ü�¹ٸ� ������Ʈ�մϴ�.
    /// </summary>
    [PunRPC]
    protected void UpdateHealthBar()
    {
        if (HealthImage != null)
        {
            HealthImage.fillAmount = currentHealth / maxHealth;
        }
    }

    /// <summary>
    /// ĳ���Ͱ� ������� �� ȣ��˴ϴ�.
    /// </summary>
    protected virtual void Die()
    {
        if (!PV.IsMine) return;

        isDead = true;
        currentHealth = 0;
        uiManager.SetHp((int)currentHealth, (int)maxHealth, this);
        PhotonNetwork.Instantiate("Death", transform.position, Quaternion.identity);
        GameObject.Find("UI").transform.Find("RespawnPanel").gameObject.SetActive(true);
        isAttacking = false;
        PV.RPC("DeactivateGameObject", RpcTarget.AllBuffered);
    }

    /// <summary>
    /// ����ġ�� �߰��մϴ�.
    /// </summary>
    /// <param name="amount">�߰��� ����ġ ��</param>
    [PunRPC]
    public void AddExperience(int amount)
    {
        experience += amount;
        if (experience >= levelPerExperience[level - 1])
        {
            PV.RPC("LevelUp", RpcTarget.All);
        }

        if (PV.IsMine)
        {
            uiManager.SetExp(experience, levelPerExperience[level - 1]);
        }
    }

    /// <summary>
    /// �������� ó���մϴ�.
    /// </summary>
    [PunRPC]
    protected void LevelUp()
    {
        experience -= levelPerExperience[level - 1];
        level++;
        levelTransform.gameObject.SetActive(true);
        levelAnimator.SetTrigger("LevelUp");
        maxHealth = CalculateMaxHealth(level);
        maxMP = CalculateMaxMP(level);
        currentHealth = maxHealth;
        currentMP = maxMP;
        skillTreeManager.AddSkillPoint(3);
        uiManager.UpdateSkillPointsUI();
        if (PV.IsMine)
        {
            uiManager.SetHp((int)currentHealth, (int)maxHealth, this);
            uiManager.SetMp((int)currentMP, (int)maxMP);
            uiManager.SetExp(experience, levelPerExperience[level - 1]);
            uiManager.SetLevel(level);

            // ������ ����
            playerData.Level = level;
            playerData.Experience = experience;
            playerData.LastPosition = transform.position;
            playerData.SkillPoint = skillTreeManager.PlayerSkillPoints;

            SaveSystem.SavePlayerData(playerData);
        }

        UpdateHealthBar();
    }

    #endregion

    #region Movement

    /// <summary>
    /// ĳ���͸� �̵���ŵ�ϴ�.
    /// </summary>
    /// <param name="direction">�̵� ����</param>
    public virtual void Move(Vector2 direction)
    {
        if (!PV.IsMine) return;
        if (isAttacking || isBlocking || isUsingSkill || isRolling || isRushing)
        {
            RB.velocity = Vector2.zero;
            return;
        }

        Vector2 movement = direction.normalized * moveSpeed;
        RB.velocity = movement;

        // ������ �̵� ���� ����
        if (movement != Vector2.zero)
        {
            lastMoveDirection = movement;
        }

        // �ִϸ��̼� ó��
        HandleMovementAnimation(direction);
    }

    /// <summary>
    /// �̵� ���⿡ ���� �ִϸ��̼��� ó���մϴ�.
    /// </summary>
    /// <param name="direction">�̵� ����</param>
    protected virtual void HandleMovementAnimation(Vector2 direction)
    {
        if (AN == null) return;

        // �¿� �̵� �ִϸ��̼�
        if (direction.x > 0)
        {
            AN.SetBool("walk right", true);
            AN.SetBool("walk left", false);
        }
        else if (direction.x < 0)
        {
            AN.SetBool("walk right", false);
            AN.SetBool("walk left", true);
        }
        else
        {
            AN.SetBool("walk left", false);
            AN.SetBool("walk right", false);
        }

        // ���� �̵� �ִϸ��̼�
        if (direction.y > 0)
        {
            AN.SetBool("walk up", true);
            AN.SetBool("walk down", false);
        }
        else if (direction.y < 0)
        {
            AN.SetBool("walk up", false);
            AN.SetBool("walk down", true);
        }
        else
        {
            AN.SetBool("walk up", false);
            AN.SetBool("walk down", false);
        }
    }

    #endregion

    #region Animation Handling

    /// <summary>
    /// ���� ����� �����մϴ�.
    /// </summary>
    /// <param name="attackDirection">���� ����</param>
    /// <param name="motionNum">��� ��ȣ</param>
    [PunRPC]
    public virtual void StartAttackingMotion(Vector2 attackDirection, int motionNum)
    {
        if (attackDirection.x > 0)
            AN.SetTrigger("attack right");
        else if (attackDirection.x < 0)
            AN.SetTrigger("attack left");
        else if (attackDirection.y > 0)
            AN.SetTrigger("attack up");
        else if (attackDirection.y < 0)
            AN.SetTrigger("attack down");
    }

    /// <summary>
    /// ������
    /// </summary>
    [PunRPC]
    public virtual void Respawn()
    {
        currentHealth = maxHealth;
        currentMP = maxMP;
        isAttacking = false;
        isDead = false;

        RecoveryHpMpAsync().Forget();
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// �̵� ������ ��ȯ�մϴ�.
    /// </summary>
    /// <returns>������ �̵� ����</returns>
    public Vector2 GetLastMoveDirection()
    {
        return lastMoveDirection;
    }

    

    /// <summary>
    /// ���� ���¸� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>���� ���� ����</returns>
    public bool GetIsAttacking()
    {
        return isAttacking;
    }
    public void SetIsAttacking(bool val)
    {
        isAttacking = val;
    }

    public virtual async UniTask ResetAttackState(float sec, bool lockPosition = true)
    {
        isAttacking = true;
        if (lockPosition)
        {
            RB.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
            RB.velocity = Vector2.zero;
        }
        await UniTask.Delay(TimeSpan.FromSeconds(sec));

        isAttacking = false;
        
        RB.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    /// <summary>
    /// ���� ü���� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>���� ü��</returns>
    public int GetCurrentHealth()
    {
        return (int)currentHealth;
    }

    /// <summary>
    /// ���� ������ ��ȯ�մϴ�.
    /// </summary>
    /// <returns>���� ����</returns>
    public int GetCurrentMP()
    {
        return (int)currentMP;
    }

    /// <summary>
    /// �ִ� ������ ��ȯ�մϴ�.
    /// </summary>
    /// <returns>�ִ� ����</returns>
    public int GetMaxMP()
    {
        return (int)maxMP;
    }

    /// <summary>
    /// ���� ������ �����մϴ�.
    /// </summary>
    /// <param name="mp">������ ���� ��</param>
    public void AdjustCurrentMP(int mp)
    {
        currentMP -= mp;
        currentMP = Mathf.Clamp(currentMP, 0, maxMP);

        uiManager.SetMp((int)currentMP, (int)maxMP);
    }

    #endregion
    #region Save and Load

    #endregion

    #region Abstract Methods

    /// <summary>
    /// �ִ� ü���� ����մϴ�.
    /// </summary>
    /// <param name="level">���� ����</param>
    /// <returns>�ִ� ü��</returns>
    protected abstract int CalculateMaxHealth(int level);

    /// <summary>
    /// �ִ� ������ ����մϴ�.
    /// </summary>
    /// <param name="level">���� ����</param>
    /// <returns>�ִ� ����</returns>
    protected abstract int CalculateMaxMP(int level);

    #endregion
}


public class Mage : Character
{
    public override void Start()
    {
        base.Start();
        maxHealth = 60;
        AttackCooldown = 2f;
        AttackDuration = 0.5f;
        attackDamage = 10;

        for (int i = 0; i < 5; i++)
        {
            skills.Add(skillTreeManager.CharacterClasses[2].Skills[i]);
        }
    }

    protected override int CalculateMaxHealth(int level)
    {
        return 60 + (level - 1) * 6;
    }

    protected override int CalculateMaxMP(int level)
    {
        return 100 + (level - 1) * 10;
    }

    public override void StartSkill(int skillIdx)
    {
        base.StartSkill(skillIdx);
    }

    public override void StartSpecialSkill(bool isDown)
    {
        if (isAttacking) return;
        skillTreeManager.UseSkill(skills[4], this);
    }

    [PunRPC]
    public override void StartAttackingMotion(Vector2 attackDirection, int motionNum)
    {
        magicTransform.gameObject.SetActive(true);
        magicAnimator.SetTrigger("Fireball");
        if (attackDirection.x > 0) AN.SetTrigger("mage attack right");
        else if (attackDirection.x < 0) AN.SetTrigger("mage attack left");
        else if (attackDirection.y > 0) AN.SetTrigger("mage attack up");
        else if (attackDirection.y < 0) AN.SetTrigger("mage attack down");

    }

    [PunRPC]
    public override void Respawn()
    {
        base.Respawn();
        AN.SetTrigger("mage init");
    }
}