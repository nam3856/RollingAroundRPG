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
    /// 캐릭터의 기본 설정을 초기화합니다.
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
        // PhotonView가 할당될 때까지 대기
        while (PV == null && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        // PhotonView가 초기화될 때까지 추가로 대기 (필요 시)
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
    /// 레벨과 경험치 관련 설정을 초기화합니다.
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
    /// 로컬 플레이어의 추가 설정을 초기화합니다.
    /// </summary>
    private void InitializeLocalPlayer()
    {
        LoadCharacterData();
        SetupCamera();

        uiManager.UpdateSkillPointsUI();
        RecoveryHpMpAsync().Forget();
    }

    
    /// <summary>
    /// 원격 플레이어의 위치를 업데이트하기 위한 비동기 작업을 시작합니다.
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
    /// 카메라 설정을 초기화합니다.
    /// </summary>
    private void SetupCamera()
    {
        var CM = GameObject.Find("CMCamera").GetComponent<Cinemachine.CinemachineVirtualCamera>();
        CM.Follow = transform;
        CM.LookAt = transform;
    }

    /// <summary>
    /// 캐릭터 데이터를 로드하고 적용합니다.
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

            // 장착 아이템 및 인벤토리 적용 (추가 구현 필요)
        }
    }

    #endregion

    #region Photon Callbacks

    /// <summary>
    /// 포톤 네트워크를 통해 데이터를 송수신합니다.
    /// </summary>
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // 본인이 직접 작성하는 경우 (전송)
            stream.SendNext(transform.position);       // 위치 동기화
            stream.SendNext(isBlocking);               // 방어 상태 동기화
            stream.SendNext(isAttacking);              // 공격 상태 동기화
            stream.SendNext(isRolling);                // 구르기 상태 동기화
            stream.SendNext(isUsingSkill);             // 스킬 사용 상태 동기화
            stream.SendNext(currentHealth);            // 현재 체력 동기화
            stream.SendNext(maxHealth);
            stream.SendNext(HealthImage.fillAmount);   // 체력바 채우기량 동기화
            stream.SendNext(isDead);                   // 사망 상태 동기화
        }
        else
        {
            // 다른 클라이언트로부터 데이터 수신 (읽기)
            curPos = (Vector3)stream.ReceiveNext();      // 위치 수신
            isBlocking = (bool)stream.ReceiveNext();     // 방어 상태 수신
            isAttacking = (bool)stream.ReceiveNext();    // 공격 상태 수신
            isRolling = (bool)stream.ReceiveNext();      // 구르기 상태 수신
            isUsingSkill = (bool)stream.ReceiveNext();   // 스킬 사용 상태 수신
            currentHealth = (float)stream.ReceiveNext(); // 현재 체력 수신
            maxHealth = (float)stream.ReceiveNext();
            HealthImage.fillAmount = (float)stream.ReceiveNext(); // 체력바 채우기량 수신
            isDead = (bool)stream.ReceiveNext();         // 사망 상태 수신
        }
    }

    #endregion

    #region Recovery

    /// <summary>
    /// 체력과 마나를 지속적으로 회복합니다.
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
    /// 스킬 UI를 업데이트합니다.
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
    /// 스킬을 시작합니다.
    /// </summary>
    /// <param name="skillIdx">스킬 인덱스</param>
    public virtual void StartSkill(int skillIdx)
    {
        skillTreeManager.UseSkill(skills[skillIdx], this);
        if (PV.IsMine)
        {
            uiManager.SetMp((int)currentMP, (int)maxMP);
        }
    }

    /// <summary>
    /// 특수 스킬을 시작합니다.
    /// </summary>
    /// <param name="isDown">스킬키가 눌려있는지 여부</param>
    public virtual void StartSpecialSkill(bool isDown)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region Damage Handling

    /// <summary>
    /// 데미지를 수신합니다.
    /// </summary>
    /// <param name="damage">받은 데미지</param>
    /// <param name="attackDirection">공격 방향</param>
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
    /// 체력바를 업데이트합니다.
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
    /// 캐릭터가 사망했을 때 호출됩니다.
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
    /// 경험치를 추가합니다.
    /// </summary>
    /// <param name="amount">추가할 경험치 양</param>
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
    /// 레벨업을 처리합니다.
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

            // 데이터 저장
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
    /// 캐릭터를 이동시킵니다.
    /// </summary>
    /// <param name="direction">이동 방향</param>
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

        // 마지막 이동 방향 저장
        if (movement != Vector2.zero)
        {
            lastMoveDirection = movement;
        }

        // 애니메이션 처리
        HandleMovementAnimation(direction);
    }

    /// <summary>
    /// 이동 방향에 따라 애니메이션을 처리합니다.
    /// </summary>
    /// <param name="direction">이동 방향</param>
    protected virtual void HandleMovementAnimation(Vector2 direction)
    {
        if (AN == null) return;

        // 좌우 이동 애니메이션
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

        // 상하 이동 애니메이션
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
    /// 공격 모션을 시작합니다.
    /// </summary>
    /// <param name="attackDirection">공격 방향</param>
    /// <param name="motionNum">모션 번호</param>
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
    /// 리스폰
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
    /// 이동 방향을 반환합니다.
    /// </summary>
    /// <returns>마지막 이동 방향</returns>
    public Vector2 GetLastMoveDirection()
    {
        return lastMoveDirection;
    }

    

    /// <summary>
    /// 공격 상태를 반환합니다.
    /// </summary>
    /// <returns>공격 상태 여부</returns>
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
    /// 현재 체력을 반환합니다.
    /// </summary>
    /// <returns>현재 체력</returns>
    public int GetCurrentHealth()
    {
        return (int)currentHealth;
    }

    /// <summary>
    /// 현재 마나를 반환합니다.
    /// </summary>
    /// <returns>현재 마나</returns>
    public int GetCurrentMP()
    {
        return (int)currentMP;
    }

    /// <summary>
    /// 최대 마나를 반환합니다.
    /// </summary>
    /// <returns>최대 마나</returns>
    public int GetMaxMP()
    {
        return (int)maxMP;
    }

    /// <summary>
    /// 현재 마나를 조정합니다.
    /// </summary>
    /// <param name="mp">조정할 마나 양</param>
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
    /// 최대 체력을 계산합니다.
    /// </summary>
    /// <param name="level">현재 레벨</param>
    /// <returns>최대 체력</returns>
    protected abstract int CalculateMaxHealth(int level);

    /// <summary>
    /// 최대 마나를 계산합니다.
    /// </summary>
    /// <param name="level">현재 레벨</param>
    /// <returns>최대 마나</returns>
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