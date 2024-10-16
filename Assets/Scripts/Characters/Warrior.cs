using Cysharp.Threading.Tasks;
using Photon.Pun;
using System;
using UnityEngine;

public class Warrior : Character
{
    #region Fields

    [Header("Skills")]
    public shieldBlock shieldBlockSkill;

    [Header("Audio")]
    public AudioClip[] swordSounds;

    private int attackStep = 0;
    private float comboResetTime;
    private float comboDelay = 3f;

    #endregion

    #region Properties

    public bool IsBlocking {
        get => isBlocking; 
        private set => isBlocking = value; 
    }
    public int AttackStep
    {
        get => attackStep;
        private set => attackStep = value;
    }

    #endregion

    #region Unity Callbacks

    public override void Start()
    {
        base.Start();
        InitializeWarrior();
        LoadCharacterData_FollowUp();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        UnsubscribeFromRushEvents();
    }

    #endregion

    #region Initialization

    private void InitializeWarrior()
    {
        attackDamage = 3f;
        basicAttackDamage = attackDamage;
        LoadSwordSounds();
        SubscribeToRushEvents();
        InitializeSkills();
    }

    private void LoadSwordSounds()
    {
        swordSounds = new AudioClip[20];
        for (int i = 0; i < swordSounds.Length; i++)
        {
            string clipName = $"Sounds/sword_{(i + 1).ToString("D2")}";
            swordSounds[i] = Resources.Load<AudioClip>(clipName);
            if (swordSounds[i] == null)
            {
                Debug.LogWarning($"Warrior: Failed to load sound {clipName}");
            }
        }
    }

    private void SubscribeToRushEvents()
    {
        rush.OnRushStarted += HandleRushStart;
        rush.OnRushEnded += HandleRushEnd;
    }

    private void UnsubscribeFromRushEvents()
    {
        rush.OnRushStarted -= HandleRushStart;
        rush.OnRushEnded -= HandleRushEnd;
    }

    private void InitializeSkills()
    {
        for (int i = 0; i < 5; i++)
        {
            if (i < skillTreeManager.CharacterClasses[0].Skills.Count)
            {
                skills.Add(skillTreeManager.CharacterClasses[0].Skills[i]);
            }
            else
            {
                Debug.LogWarning($"Warrior: Skill index {i} is out of range.");
            }
        }
    }

    protected override int CalculateMaxHealth(int level)
    {
        return 100 + (level - 1) * 10;
    }

    protected override int CalculateMaxMP(int level)
    {
        return 50 + (level - 1);
    }
    #endregion

    #region Skill Management

    public override void StartSkill(int skillIdx)
    {
        if (skillIdx == 0 || skillIdx == 1)
        {
            Attack();
            Debug.Log($"Warrior: StartSkill: {skillIdx}");
            return;
        }

        base.StartSkill(skillIdx);
    }

    public void Attack()
    {
        Debug.Log($"Warrior: Attack: {isAttacking}");
        if (isAttacking) return;

        ResetAttack();

        StartAttack(attackStep);
    }

    private void StartAttack(int step)
    {
        if (skillTreeManager != null)
        {
            Debug.Log($"Warrior: StartAttack: UseSkill");
            skillTreeManager.UseSkill(skills[step], this);
            Vector2 attackDirection = GetLastMoveDirection();

            if (skills[1].IsAcquired)
            {
                comboResetTime = Time.time + comboDelay;
                attackStep = (attackStep + 1) % 2;
            }
        }
    }

    public override async UniTask ResetAttackState(float sec, bool lockPosition = true)
    {
        await base.ResetAttackState(sec, lockPosition);
    }

    public void ResetAttack()
    {
        if (Time.time > comboResetTime)
        {
            attackStep = 0;
        }
    }

    #endregion

    #region Blocking

    public override void StartSpecialSkill(bool isDown)
    {
        if (isDown)
        {
            if (!IsBlocking)
                StartBlock();  // Start blocking
        }
        else
        {
            if (IsBlocking)
                EndBlock();  // End blocking
        }
    }

    private void StartBlock()
    {
        shieldBlockSkill = skills[4] as shieldBlock;  // Warrior's shield block skill
        skillTreeManager.UseSkill(skills[4], this, true);
    }

    private void EndBlock()
    {
        IsBlocking = false;
        RB.constraints = RigidbodyConstraints2D.FreezeRotation;
        skills[4].StartCoolDown(this);  // Start cooldown when blocking ends

        PV.RPC("EndBlockingAnimation", RpcTarget.All);
    }

    [PunRPC]
    private void ActivateGuardEffectObject()
    {
        if (guardEffectObject != null)
        {
            guardEffectObject.SetActive(true);
        }
    }

    public bool GetIsBlocking()
    {
        return isBlocking;
    }
    public void SetIsBlocking(bool isBlocking)
    {
        this.isBlocking = isBlocking;
    }
    public void SetCurrentHealth(int v)
    {
        currentHealth = v;
    }

    [PunRPC]
    private void StartBlockingAnimation()
    {
        Vector2 blockDirection = lastMoveDirection;

        if (blockDirection.x > 0)
            AN.SetBool("block right", true);
        else if (blockDirection.x < 0)
            AN.SetBool("block left", true);
        else if (blockDirection.y > 0)
            AN.SetBool("block up", true);
        else if (blockDirection.y < 0)
            AN.SetBool("block down", true);

        shieldEffectObject.SetActive(true);
    }

    [PunRPC]
    private void EndBlockingAnimation()
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

    #endregion

    #region Rush Handling

    private void HandleRushStart()
    {
        Debug.Log("Warrior: Rush started!");
        // Enable collider or other rush-related features here
    }

    private void HandleRushEnd()
    {
        isRushing = false;
        Debug.Log("Warrior: Rush ended!");
        // Disable collider or other rush-related features here
    }

    [PunRPC]
    private void StartPowerStrikeMotion(Vector2 attackDirection)
    {
        if (attackDirection.x > 0)
            AN.SetTrigger("powerstrike right");
        else if (attackDirection.x < 0)
            AN.SetTrigger("powerstrike left");
        else if (attackDirection.y > 0)
            AN.SetTrigger("powerstrike up");
        else if (attackDirection.y < 0)
            AN.SetTrigger("powerstrike down");
    }

    #endregion

    #region Damage Handling

    [PunRPC]
    public override void ReceiveDamage(int damage, Vector2 attackDirection)
    {
        if (isRushing) return;

        if (arcaneShieldStack > 0)
        {
            arcaneShieldStack--;
            damage /= 2;
            magicTransform.gameObject.SetActive(true);
            audioSource.PlayOneShot(receivedArcaneShieldSound);
            magicAnimator.SetTrigger("Arcane Shield");
        }

        float armoredDamage = (float)Math.Ceiling(damage - damage * armor * 0.1);

        if (IsBlocking)
        {
            shieldBlockSkill.OnReceiveDamage(armoredDamage, attackDirection);
        }
        else
        {
            DisplayDamage((int)armoredDamage);
            currentHealth -= armoredDamage;
            PV.RPC("UpdateHealthBar", RpcTarget.All);
        }

        if (PV.IsMine)
        {
            uiManager.SetHp((int)currentHealth, (int)maxHealth);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void DisplayDamage(int damage)
    {
        GameObject damageTextInstance = PhotonNetwork.Instantiate("DamageText", canvasTransform.position, Quaternion.identity);
        DamageText damageTextScript = damageTextInstance.GetComponent<DamageText>();
        PhotonView damageTextPV = damageTextInstance.GetComponent<PhotonView>();

        GameObject uiCanvas = GameObject.Find("Canvas");
        damageTextInstance.transform.SetParent(uiCanvas.transform, false);

        damageTextPV.RPC("SetDamageText", RpcTarget.All, damage.ToString());
    }

    protected override void Die()
    {
        base.Die();
    }

    #endregion

    #region Animation Handling

    [PunRPC]
    public override void Respawn()
    {
        base.Respawn();
        AN.SetTrigger("warrior init");
        isBlocking = false;
        isRushing = false;
    }

    [PunRPC]
    public override void StartAttackingMotion(Vector2 attackDirection, int motionNum)
    {
        if (motionNum == 0)
        {
            TriggerAttackAnimation(attackDirection, "attack");
        }
        else if (motionNum == 1)
        {
            TriggerAttackAnimation(attackDirection, "attack2");
        }
    }

    private void TriggerAttackAnimation(Vector2 direction, string attackType)
    {
        if (direction.x > 0)
            AN.SetTrigger($"{attackType} right");
        else if (direction.x < 0)
            AN.SetTrigger($"{attackType} left");
        else if (direction.y > 0)
            AN.SetTrigger($"{attackType} up");
        else if (direction.y < 0)
            AN.SetTrigger($"{attackType} down");
    }

    #endregion

    #region Audio Handling

    public void PlayRandomSwordSound()
    {
        if (swordSounds.Length == 0)
        {
            Debug.LogWarning("Warrior: No sword sounds available to play.");
            return;
        }

        int randomIndex = UnityEngine.Random.Range(0, swordSounds.Length);
        audioSource.clip = swordSounds[randomIndex];
        audioSource.Play();
    }




    #endregion
}
