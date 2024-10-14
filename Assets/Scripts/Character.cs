using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;
using Cinemachine;
using System;
using ExitGames.Client.Photon;
using Cysharp.Threading.Tasks;


public abstract class Character : MonoBehaviourPunCallbacks, IPunObservable
{
    public float maxHealth = 100;
    protected float maxMP = 100;
    protected float currentMP = 100;
    public int attackDamage = 1;
    public int experience = 0;
    public int level = 1;
    public List<int> levelPerExperience;
    protected float currentHealth;
    public PhotonView PV;
    public Animator AN;
    public Rigidbody2D RB;
    public float moveSpeed = 3f;
    protected Vector2 lastMoveDirection;
    public Image HealthImage;
    public SkillTreeManager skillTreeManager;
    public PlayerScript player;
    public LayerMask enemyLayer;
    public LayerMask playerLayer;
    public GameObject guardEffectObject;
    public GameObject shieldEffectObject;
    public GameObject damageTextPrefab;
    public Transform canvasTransform;
    protected UIManager uiManager;
    protected List<Skill> skills = new List<Skill>();
    public AudioSource audioSource;

    protected AudioClip receivedArcaneShieldSound;
    Vector3 curPos;

    protected float lastAttackTime = -10f;
    protected float attackTime = 0;
    protected bool isAttacking = false;
    protected bool isUsingSkill = false;
    public bool isBlocking = false;
    protected Transform levelTransform;
    protected Transform magicTransform;
    protected Animator levelAnimator;
    protected Animator magicAnimator;
    
    public enum SkillNum { Q,W,E,R,F }
    public enum CharacterNum { Warrior, Gunner, Mage}

    protected int arcaneShieldStack = 0;
    public float AttackDuration = 0.25f;
    public float AttackCooldown = 0.05f;

    public bool isRolling = false;
    public bool isRushing = false;

    public PlayerData playerData;
    public bool isDead { get; private set; } = false;

    public virtual void Start()
    {
        currentHealth = maxHealth;
        AN = GetComponent<Animator>();
        PV = GetComponent<PhotonView>();
        RB = GetComponent<Rigidbody2D>();
        skillTreeManager = FindObjectOfType<SkillTreeManager>();
        enemyLayer = LayerMask.GetMask("Enemy");
        playerLayer = LayerMask.GetMask("Player");
        GameObject soundEffectObject = transform.Find("SoundEffect").gameObject;
        soundEffectObject.SetActive(true);
        audioSource = soundEffectObject.GetComponent<AudioSource>();
        uiManager = FindObjectOfType<UIManager>();
        levelPerExperience = new List<int>() { 10,20,30,40,50,60,70,80,90,100,110,5000000 };
        HealthImage = transform.Find("Canvas/Hp").GetComponent<Image>();
        player = GetComponent<PlayerScript>();
        shieldEffectObject = transform.Find("ShieldEffect").gameObject;
        guardEffectObject = transform.Find("GuardEffect").gameObject;
        guardEffectObject.SetActive(false);
        shieldEffectObject.SetActive(false);
        magicTransform = transform.Find("MagicEffect");
        magicAnimator = magicTransform.GetComponent<Animator>();
        levelTransform = transform.Find("LevelEffect");
        levelAnimator = levelTransform.GetComponent<Animator>();
        damageTextPrefab = Resources.Load<GameObject>("DamageText");
        canvasTransform = transform.Find("Canvas");
        lastMoveDirection = Vector2.left;
        receivedArcaneShieldSound = Resources.Load<AudioClip>("Sounds/ReceivedArcaneShieldSound");

        if (PV.IsMine)
        {
            playerData = FindObjectOfType<PlayerScript>().playerData;
            LoadCharacterData();
            var CM = GameObject.Find("CMCamera").GetComponent<CinemachineVirtualCamera>();
            CM.Follow = transform;
            CM.LookAt = transform;
        }

        InvokeRepeating("RecoveryHPMP", 1f, 1f);
    }

    public int GetCurrentMP()
    {
        return (int)currentMP;
    }

    public int GetMaxMP()
    {
        return (int)maxMP;
    }

    public void AdjustCurrentMP(int mp)
    {
        currentMP -= mp;
        if(currentMP > maxMP) currentMP = maxMP;

        if (currentMP < 0) currentMP = 0;
        uiManager.SetMp((int)currentMP, (int)maxMP);
    }
    public int GetShieldStack()
    {
        return arcaneShieldStack;
    }


    [PunRPC]
    public void SetArcaneShield()
    {
        arcaneShieldStack = 2;
        magicTransform.gameObject.SetActive(true);
        audioSource.PlayOneShot(receivedArcaneShieldSound);
        magicAnimator.SetTrigger("Arcane Shield");
    }

    [PunRPC]
    public void GetHealing()
    {
        currentHealth += maxHealth / 20;
        magicTransform.gameObject.SetActive(true);
        magicAnimator.SetTrigger("Healing Wave");
    }
    protected virtual void RecoveryHPMP()
    {
        if (!PV.IsMine) return;
        currentHealth += maxHealth / 400;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        currentMP += maxMP / 300;
        if (currentMP > maxMP) currentMP = maxMP;
        uiManager.SetHp((int)currentHealth, (int)maxHealth, this);
        uiManager.SetMp((int)currentMP, (int)maxMP);

        foreach(Skill skill in skills)
        {
            skill.UpdateSkillUI(this);
        }
    }
    void LoadCharacterData()
    {
        if(playerData != null)
        {
            maxHealth = CalculateMaxHealth(playerData.Level);
            maxMP = CalculateMaxMP(playerData.Level);
            currentHealth = maxHealth;
            uiManager.SetHp((int)currentHealth, (int)maxHealth, this);
            uiManager.SetMp((int)currentMP, (int)maxMP);
            
            foreach (var skillName in playerData.LearnedSkills)
            {
                var skill = skillTreeManager.GetSkillByName(skillName);
                if(skill != null)
                {
                    skill.Acquire(this);
                }
            }

            experience = playerData.Experience;
            Debug.Log("EXP: " + experience + " Level: " + level);
            if (level > 0)
            {
                uiManager.SetExp(experience, levelPerExperience[level - 1]);
                uiManager.SetLevel(level);
            }
            else uiManager.SetExp(0, 1);
            //����������, �κ��丮 ����
            //TBD

        }
    }
    protected virtual int CalculateMaxHealth(int level)
    {
        return 0;
    }
    protected virtual int CalculateMaxMP(int level)
    {
        return 0;
    }
    public virtual IEnumerator ResetAttackState(float sec)
    {
        yield return new WaitForSeconds(sec);
        isAttacking = false;
    }

    protected virtual void Update()
    {
        if (PV.IsMine)
        {
            
        }
        else
        {
            //Debug.Log(curPos);
            float distance = (transform.position - curPos).sqrMagnitude;

            if (distance > 1.0f)
            {
                transform.position = curPos;
            }
            else
            {
                transform.position = Vector3.Lerp(transform.position, curPos, Time.deltaTime * 10);
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // ������ ���� �ۼ��ϴ� ��� (����)
            //player.logger.AddDebugLog("���� ��ġ ���� ��: " + transform.position);
            stream.SendNext(transform.position);  // ��ġ ����ȭ
            stream.SendNext(isBlocking);          // ��� ���� ����ȭ
            stream.SendNext(isAttacking);         // ���� ���� ����ȭ
            stream.SendNext(isRolling);           // ������ ���� ����ȭ
            stream.SendNext(isUsingSkill);
            stream.SendNext(currentHealth);
            stream.SendNext(HealthImage.fillAmount);
            stream.SendNext(isDead);
        }
        else
        {
            // �ٸ� Ŭ���̾�Ʈ�κ��� ������ ���� (�б�)
            curPos = (Vector3)stream.ReceiveNext();  // ��ġ ����
            isBlocking = (bool)stream.ReceiveNext(); // ��� ���� ����
            isAttacking = (bool)stream.ReceiveNext(); // ���� ���� ����
            isRolling = (bool)stream.ReceiveNext(); // ������ ���� ����
            isUsingSkill = (bool)stream.ReceiveNext();
            currentHealth = (float)stream.ReceiveNext();
            HealthImage.fillAmount = (float)stream.ReceiveNext();
            isDead = (bool)stream.ReceiveNext();
        }
    }

    public virtual void OnDestroy()
    {
    }
    public bool GetIsAttacking()
    {
        return isAttacking;
    }
    public void SetIsAttacking(bool attacking)
    {
        isAttacking = attacking;
    }
    public float GetLastAttackTime()
    {
        return lastAttackTime;
    }
    public void SetLastAttackTime(float LAT)
    {
        lastAttackTime = LAT;
    }
    public void SetAttackTime(float AT)
    {
        attackTime = AT;
    }
    public virtual void Attack()
    {
        if (PV.IsMine)
        {
            uiManager.SetMp((int)currentMP, (int)maxMP);
        }
    }
    public virtual void Block(bool p) {

        if (PV.IsMine)
        {
            uiManager.SetMp((int)currentMP, (int)maxMP);
        }
    }
    public virtual void Roll() {
        if (PV.IsMine)
        {
            uiManager.SetMp((int)currentMP, (int)maxMP);
        }
    }

    public int GetCurrentHealth()
    {
        return (int)currentHealth;
    }
    public void SetCurrentHealth(int health)
    {
        currentHealth = health;
        HealthImage.fillAmount = (float)currentHealth / maxHealth;
        if (PV.IsMine)
        {
            uiManager.SetHp((int)currentHealth , (int)maxHealth, this);
        }
        PV.RPC("UpdateHealthBar", RpcTarget.All);
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public virtual void Move(Vector2 direction)
    {
        if (!PV.IsMine) return;
        if (isAttacking || isBlocking || isUsingSkill)
        {
            RB.velocity = Vector2.zero;
            return;
        }
        if (isRolling||isRushing) return;
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
    protected virtual void HandleMovementAnimation(Vector2 direction)
    {
        if (AN != null)
        {
            // �¿� �̵�
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

            // ���� �̵�
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
    }

    [PunRPC]
    public virtual void StartAttackingMotion(Vector2 attackDirection, int motionNum)
    {
        if (attackDirection.x > 0) AN.SetTrigger("attack right");
        else if (attackDirection.x < 0) AN.SetTrigger("attack left");
        else if (attackDirection.y > 0) AN.SetTrigger("attack up");
        else if (attackDirection.y < 0) AN.SetTrigger("attack down");

    }
    public Vector2 GetLastMoveDirection()
    {
        return lastMoveDirection;
    }
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
        //GameObject damageTextInstance = Instantiate(damageTextPrefab, canvasTransform);
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
    [PunRPC]
    protected void UpdateHealthBar()
    {
        if (HealthImage != null)
        {
            HealthImage.fillAmount = (float)currentHealth / maxHealth;
        }
    }
    protected virtual void Die()
    {
        // ������ �÷��̾ ���� ó��
        if (!PV.IsMine) return;
        isDead = true;
        CancelInvoke("RecoveryHPMP");

        PhotonNetwork.Instantiate("Death", transform.position, Quaternion.identity);
        GameObject.Find("UI").transform.Find("RespawnPanel").gameObject.SetActive(true);
        isAttacking = false;
        PV.RPC("DeactivateGameObject", RpcTarget.AllBuffered);
    }

    [PunRPC]
    public void AddExperience(int amount)
    {
        experience += amount;
        if(experience >= levelPerExperience[level - 1])
        {
            PV.RPC("LevelUp", RpcTarget.All);
        }
        if (PV.IsMine) uiManager.SetExp(experience, levelPerExperience[level - 1]);
    }

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
        if (PV.IsMine)
        {
            uiManager.SetHp((int)currentHealth, (int)maxHealth, this);
            uiManager.SetMp((int)currentMP, (int)maxMP);
            uiManager.SetExp(experience, levelPerExperience[level - 1]);
            uiManager.SetLevel(level);


            //Save
            playerData.Level = level;
            playerData.Experience = experience;
            playerData.LastPosition = transform.position;
            SaveSystem.SavePlayerData(playerData);
        }
        UpdateHealthBar();
    }
    public virtual void StartSkill(int skillIdx)
    {
        skillTreeManager.UseSkill(skills[skillIdx], this);
        if (PV.IsMine)
        {
            uiManager.SetMp((int)currentMP, (int)maxMP);
        }
    }
    public virtual void StartSpecialSkill(bool isDown)
    {
        throw new NotImplementedException();
    }


    [PunRPC]
    public virtual void Respawn()
    {
        isDead = false;
        InvokeRepeating("RecoveryHPMP",1f,1f);
        currentHealth = maxHealth;
        currentMP = maxMP;
        UpdateHealthBar();
        if (PV.IsMine)
        {
            uiManager.SetHp((int)currentHealth, (int)maxHealth, this);
            uiManager.SetMp((int)currentMP, (int)maxMP);
        }
    }

    
}

public class Warrior : Character
{
    public shieldBlock shieldBlockSkill;

    public AudioClip[] swordSounds;

    private int attackStep = 0;
    public float comboResetTime;
    public float comboDelay = 3f;

    public override void Start()
    {
        base.Start();
        AttackDuration = 0.3f;
        AttackCooldown = 0.05f;
        attackDamage = 2;
        swordSounds = new AudioClip[20];
        for (int i = 0; i < swordSounds.Length; i++)
        {
            string clipName = $"Sounds/sword_{(i + 1).ToString("D2")}";  // sword_01, sword_02 ... sword_20
            swordSounds[i] = Resources.Load<AudioClip>(clipName);
        }

        rush.OnRushStarted += HandleRushStart;
        rush.OnRushEnded += HandleRushEnd;
        
        for(int i = 0; i < 5; i++)
        {
            skills.Add(skillTreeManager.CharacterClasses[0].Skills[i]);
        }
    }
    protected override int CalculateMaxMP(int level)
    {
        return 50;
    }
    public override IEnumerator ResetAttackState(float sec)
    {
        yield return new WaitForSeconds(sec);
        isAttacking = false;

        comboResetTime = Time.time + comboDelay;
        attackStep = (attackStep + 1) % 2;
    }
    protected override int CalculateMaxHealth(int level)
    {
        return 100 + (level - 1) * 10;
    }

    [PunRPC]
    public override void Respawn()
    {
        base.Respawn();
        AN.SetTrigger("warrior init");
    }
    public int GetAttackStep()
    {
        return attackStep;
    }
    public void SetAttackStep(int step)
    {
        attackStep = step;
    }
    public void SetComboResetTime(float time)
    {
        comboResetTime = time;
    }


    public override void StartSkill(int skillIdx)
    {
        if(skillIdx == 0 || skillIdx == 1)
        {
            Attack();
            return;
        }

        base.StartSkill(skillIdx);
    }

    public override void Attack()
    {
        if (isAttacking) return;
        if (comboResetTime < Time.time) attackStep = 0;

        StartAttack(attackStep);
    }

    private void StartAttack(int step)
    {
        if (skillTreeManager != null && player != null)
        {
            skillTreeManager.UseSkill(skills[step], this);

            Vector2 attackDirection = GetLastMoveDirection();

        }
    }

    [PunRPC]
    public override void StartAttackingMotion(Vector2 attackDirection, int motionNum)
    {
        //Debug.Log(motionNum);
        if (motionNum == 0)
        {
            if (attackDirection.x > 0) AN.SetTrigger("attack right");
            else if (attackDirection.x < 0) AN.SetTrigger("attack left");
            else if (attackDirection.y > 0) AN.SetTrigger("attack up");
            else if (attackDirection.y < 0) AN.SetTrigger("attack down");
        }
        else if (motionNum == 1)
        {
            if (attackDirection.x > 0) AN.SetTrigger("attack2 right");
            else if (attackDirection.x < 0) AN.SetTrigger("attack2 left");
            else if (attackDirection.y > 0) AN.SetTrigger("attack2 up");
            else if (attackDirection.y < 0) AN.SetTrigger("attack2 down");
        }
    }

    private void HandleRushStart()
    {
        Debug.Log("Warrior: ����!");
    }

    private void HandleRushEnd()
    {
        isRushing = false;
        Debug.Log("���� ����!");
    }

    

    

    public void ResetAttack()
    {
        if (Time.time > comboResetTime)
        {
            attackStep = 0;
            //Debug.Log("���� ����: �޺��� �ʱ�ȭ�Ǿ����ϴ�.");
        }
    }
    public void PlayRandomSwordSound()
    {
        int randomIndex = UnityEngine.Random.Range(0, swordSounds.Length);

        audioSource.clip = swordSounds[randomIndex];
        audioSource.Play();
    }
    public override void StartSpecialSkill(bool isDown)
    {
        if (isDown)
        {
            if(!isBlocking)
                StartBlock();  // ��� ����
        }
        else
        {
            if(isBlocking)
                EndBlock();  // ��� ���̸� ����
        }
    }

    //public override void Block(bool p)
    //{
    //    if (p)
    //    {
    //        StartBlock();  // ��� ����
    //        base.Block(p);
    //    }
    //    else
    //    {
    //        EndBlock();  // ��� ���̸� ����
    //    }
    //}

    public void StartBlock()
    {
        shieldBlockSkill = skills[4] as shieldBlock;  // ���� ��� ��ų
        //skillTreeManager.UseSkill(shieldBlockSkill, this);

        skillTreeManager.UseSkill(skills[4], this);
    }

    [PunRPC]
    public void ActivateGuardEffectObject()
    {
        if (guardEffectObject != null)
        {
            guardEffectObject.SetActive(true); // ���� ���� ���� ������Ʈ Ȱ��ȭ
        }
    }
    [PunRPC]
    void StartPowerStrikeMotion(Vector2 attackDirection)
    {
        if (attackDirection.x > 0) AN.SetTrigger("powerstrike right");
        else if (attackDirection.x < 0) AN.SetTrigger("powerstrike left");
        else if (attackDirection.y > 0) AN.SetTrigger("powerstrike up");
        else if (attackDirection.y < 0) AN.SetTrigger("powerstrike down");
    }


    // ��� ����
    public void EndBlock()
    {
        isBlocking = false;
        RB.constraints = RigidbodyConstraints2D.FreezeRotation;
        //Debug.Log("Warrior: ��� ����");

        skills[4].SetLastUsedTime(Time.time);  // ��� ���� ������ ��Ÿ�� ����
        Debug.Log($"{PV.Owner.NickName}�� ���� ���� ����, ��Ÿ�� ����");

        PV.RPC("EndBlockingAnimation", RpcTarget.All);
    }

    [PunRPC]
    void EndBlockingAnimation()
    {
        if (shieldEffectObject != null)
        {
            shieldEffectObject.SetActive(false);
        }
        AN.SetBool("block right", false);
        AN.SetBool("block left", false);
        AN.SetBool("block up", false);
        AN.SetBool("block down", false);
    }

    [PunRPC]
    void StartBlockingAnimation()
    {
        Vector2 blockDirection = lastMoveDirection;

        if (blockDirection.x > 0) AN.SetBool("block right", true);
        else if (blockDirection.x < 0) AN.SetBool("block left", true);
        else if (blockDirection.y > 0) AN.SetBool("block up", true);
        else if (blockDirection.y < 0) AN.SetBool("block down", true);

        shieldEffectObject.SetActive(true);
    }


    [PunRPC]
    public override void ReceiveDamage(int damage, Vector2 attackDirection)
    {
        if (arcaneShieldStack > 0)
        {
            arcaneShieldStack--;
            damage /= 2;
            magicTransform.gameObject.SetActive(true);
            audioSource.PlayOneShot(receivedArcaneShieldSound);
            magicAnimator.SetTrigger("Arcane Shield");
        }
        if (isRushing) { return; }
        if (isBlocking)
        {
            shieldBlockSkill.OnReceiveDamage(damage, attackDirection);
        }
        else
        {
            
            GameObject damageTextInstance = PhotonNetwork.Instantiate("DamageText", canvasTransform.position, Quaternion.identity);
            DamageText damageTextScript = damageTextInstance.GetComponent<DamageText>();
            PhotonView damageTextPV = damageTextInstance.GetComponent<PhotonView>();

            GameObject uiCanvas = GameObject.Find("Canvas");
            damageTextInstance.transform.SetParent(uiCanvas.transform, false);

            damageTextPV.RPC("SetDamageText", RpcTarget.All, damage.ToString());
            currentHealth -= damage;
            Debug.Log(PV.Owner + "�� ���� ü��: " + currentHealth);

            PV.RPC("UpdateHealthBar", RpcTarget.All);
        }
        if (PV.IsMine)
        {
            uiManager.SetHp((int)currentHealth, (int)maxHealth, this);
        }
        if (currentHealth <= 0)
        {
            Die();
        }

    }
    protected override void Die()
    {
        base.Die();
    }

    public override void OnDestroy()
    {
        // �̺�Ʈ ���� ����
        rush.OnRushStarted -= HandleRushStart;
        rush.OnRushEnded -= HandleRushEnd;
    }
}

public class Gunner : Character
{
    public float rollCooldown = 2f;
    private float rollTime;
    private float lastRollTime = -10f;
    private BulletPool pool;
    private AudioClip reloadSound;
    public override void Start()
    {
        base.Start();
        AttackDuration = 0.2f;
        AttackCooldown = 0.05f;
        attackDamage = 4;
        pool = FindObjectOfType<BulletPool>();
        roll.OnRollStarted += HandleRollStart;
        roll.OnRollEnded += HandleRollEnd;
        reloadSound = Resources.Load<AudioClip>("Sounds/reload");
        for (int i = 0; i < 5; i++)
        {
            skills.Add(skillTreeManager.CharacterClasses[1].Skills[i]);
        }
    }

    protected override void RecoveryHPMP()
    {
        if (!PV.IsMine) return;
        currentHealth += maxHealth / 400;
        if (currentHealth > maxHealth) currentHealth = maxHealth;

        uiManager.SetHp((int)currentHealth, (int)maxHealth, this);
        uiManager.SetMp((int)currentMP, (int)maxMP);

        foreach (Skill skill in skills)
        {
            skill.UpdateSkillUI(this);
        }
    }
    protected override int CalculateMaxMP(int level)
    {
        return 9;
    }
    protected override int CalculateMaxHealth(int level)
    {
        return 80 + (level - 1) * 8;
    }
    public override void Attack()
    {
        Debug.Log("Gunner: �� �߻�!");

        skillTreeManager.UseSkill(skills[0], this);
        UniTask.Void(async () =>
        {

            await UniTask.Delay(200);
            isAttacking = false;
        });
        base.Attack();
    }
    [PunRPC]
    public override void Respawn()
    {
        base.Respawn();
        AN.SetTrigger("gunner init");
    }

    public override void StartSkill(int skillIdx)
    {
        base.StartSkill(skillIdx);
    }

    

    public override void StartSpecialSkill(bool isDown)
    {
        if (isRolling || isAttacking) return;
        skillTreeManager.UseSkill(skills[4], this);
    }
    //public override void Roll()
    //{
    //    if (isRolling || isAttacking) return;

    //    //var roll = skillTreeManager.CharacterClasses[1].Skills[4];
    //    //skillTreeManager.UseSkill(roll, this);
    //    skillTreeManager.UseSkill(skills[4], this);

    //    base.Roll();
    //}

    private void HandleRollStart()
    {
        Debug.Log("Gunner: ������!");
    }

    private void HandleRollEnd()
    {
        isRolling = false;
        Debug.Log("������ ����!");
    }

    [PunRPC]
    public override void ReceiveDamage(int damage, Vector2 attackDirection)
    {
        if (isRolling) return;
        base.ReceiveDamage(damage, attackDirection);
    }
    protected override void Die()
    {
        base.Die();
    }

    public override void OnDestroy()
    {
        // �̺�Ʈ ���� ����
        roll.OnRollStarted -= HandleRollStart;
        roll.OnRollEnded -= HandleRollEnd;
    }
}

public class Mage : Character
{
    public override void Start()
    {
        base.Start();
        maxHealth = 60;
        AttackCooldown = 2f;
        AttackDuration = 0.5f;
        attackDamage = 7;

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