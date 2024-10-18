using Photon.Pun;
using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class Gunner : Character
{
    #region Fields

    [Header("Skills")]
    public float rollCooldown = 2f;

    [Header("Grenade Settings")]
    public float chargeSpeed = 2f;
    public float maxChargeTime = 5f;

    [Header("Audio")]
    public AudioClip reloadSound;
    public AudioClip[] grenadeSounds;

    private float rollTime;
    private float lastRollTime = -10f;
    private BulletPool pool;

    private bool isChargingGrenade = false;
    private float grenadeChargeTime = 0f;
    private float currentGrenadeForce = 2f;

    #endregion

    #region Properties

    public bool IsChargingGrenade
    {
        get => isChargingGrenade;
        private set => isChargingGrenade = value;
    }

    #endregion

    #region Unity Callbacks

    public override void Start()
    {
        base.Start();
        InitializeGunner();

        LoadCharacterData_FollowUp();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        UnsubscribeFromRollEvents();
    }

    private void Update()
    {
        if (PV.IsMine)
        {
            HandleInput();
        }
    }
    #endregion

    #region Initialization

    private void InitializeGunner()
    {
        AttackDuration = 0.2f;
        AttackCooldown = 0.05f;
        attackDamage = 4;
        basicAttackDamage = attackDamage;
        pool = FindObjectOfType<BulletPool>();
        SubscribeToRollEvents();

        currentMP = maxMP;
        uiManager.SetMp((int)currentMP, (int)maxMP);

        LoadSkills();
        LoadAudioClips();
    }

    private void LoadSkills()
    {
        for (int i = 0; i < 5; i++)
        {
            if (i < skillTreeManager.CharacterClasses[1].Skills.Count)
            {
                skills.Add(skillTreeManager.CharacterClasses[1].Skills[i]);
                Debug.Log($"Gunner Skill[{i}] = {skills[i]?.Name}");
            }
            else
            {
                Debug.LogWarning($"Gunner: Skill index {i} is out of range.");
            }
        }
    }

    private void LoadAudioClips()
    {
        reloadSound = Resources.Load<AudioClip>("Sounds/reload");

        grenadeSounds = new AudioClip[2];
        grenadeSounds[0] = Resources.Load<AudioClip>("Sounds/grenadeThrow");
        grenadeSounds[1] = Resources.Load<AudioClip>("Sounds/grenadeExplode");

        // ���� üũ
        if (reloadSound == null)
        {
            Debug.LogWarning("Gunner: Reload sound failed to load.");
        }

        for (int i = 0; i < grenadeSounds.Length; i++)
        {
            if (grenadeSounds[i] == null)
            {
                Debug.LogWarning($"Gunner: Grenade sound {i} failed to load.");
            }
        }
    }

    private void SubscribeToRollEvents()
    {
        roll.OnRollStarted += HandleRollStart;
        roll.OnRollEnded += HandleRollEnd;
    }

    private void UnsubscribeFromRollEvents()
    {
        roll.OnRollStarted -= HandleRollStart;
        roll.OnRollEnded -= HandleRollEnd;
    }

    #endregion

    #region Input Handling

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            StartChargingGrenade();
        }

        if (Input.GetKey(KeyCode.E))
        {
            ChargeGrenade();
        }

        if (Input.GetKeyUp(KeyCode.E))
        {
            ReleaseGrenade();
        }
    }

    #endregion

    #region Grenade Management

    private void StartChargingGrenade()
    {
        if (IsChargingGrenade)
            return;

        var grenadeToss = skills[2] as GrenadeToss;
        if (grenadeToss == null)
        {
            Debug.LogError("GrenadeToss ��ų�� null�Դϴ�.");
            return;
        }

        if (!grenadeToss.UseSkill(this, true))
        {
            Debug.LogWarning("GrenadeToss ��ų�� ����� �� �����ϴ�.");
            return;
        }

        IsChargingGrenade = true;
        grenadeChargeTime = 0f;
        currentGrenadeForce = 1f; // �⺻ ������ �� �ʱ�ȭ

        // ���� ���� ���� ���
        if (audioSource != null && grenadeSounds.Length > 0)
        {
            audioSource.PlayOneShot(grenadeSounds[0]);
        }
    }

    private void ChargeGrenade()
    {
        if (!IsChargingGrenade)
            return;

        grenadeChargeTime += Time.deltaTime * chargeSpeed;
        currentGrenadeForce += Time.deltaTime * chargeSpeed; // ������ �� ����

        currentGrenadeForce = Mathf.Clamp(currentGrenadeForce, 1f, 8f); // ������ �� ����
    }

    private void ReleaseGrenade()
    {
        if (!IsChargingGrenade)
            return;

        IsChargingGrenade = false;

        // ����ź �߻�
        var grenadeToss = skills[2] as GrenadeToss;
        if (grenadeToss != null)
        {
            grenadeToss.ThrowGrenade(this, currentGrenadeForce);

            // ���� �Ϸ� ���� ���
            if (audioSource != null && grenadeSounds.Length > 1)
            {
                audioSource.PlayOneShot(grenadeSounds[1]);
            }
        }
        else
        {
            Debug.LogError("GrenadeToss ��ų�� null�Դϴ�.");
        }
    }

    #endregion

    #region Skill Management

    public override void StartSkill(int skillIdx)
    {
        if (skillIdx == 2) return; // ����ź ��ų�� ������ ó��
        base.StartSkill(skillIdx);
    }

    public override void StartSpecialSkill(bool isDown)
    {
        if (isRolling || isAttacking) return;
        skillTreeManager.UseSkill(skills[4], this);
    }

    #endregion

    #region Recovery

    protected override async UniTask RecoveryHpMpAsync()
    {
        while (!isDead)
        {
            currentHealth += maxHealth / 400 * healthRecoveryPer;
            if (currentHealth > maxHealth) currentHealth = maxHealth;

            HealthImage.fillAmount = currentHealth / maxHealth;

            if (PV.IsMine)
            {
                uiManager.SetHp((int)currentHealth, (int)maxHealth);
                uiManager.SetMp((int)currentMP, (int)maxMP);
            }
            await UniTask.Delay(1000);
        }
    }

    protected override float CalculateMaxMP(int level)
    {
        return 9 + additionalMP;
    }

    protected override float CalculateMaxHealth(int level)
    {
        return 80 + (level - 1) * 8 + additionalHealth;
    }

    #endregion

    #region Event Handlers

    private void HandleRollStart()
    {
        Debug.Log("Gunner: ������!");
        // ������ ���� �� �ʿ��� ���� �߰�
    }

    private void HandleRollEnd()
    {
        isRolling = false;
        Debug.Log("������ ����!");
        // ������ ���� �� �ʿ��� ���� �߰�
    }

    #endregion

    #region Damage Handling

    [PunRPC]
    public override void ReceiveDamage(int damage, Vector2 attackDirection)
    {
        if (isRolling) return;
        base.ReceiveDamage(damage, attackDirection);
    }

    protected override void Die()
    {
        base.Die();
        var snipeShotSkill = GetComponent<SnipeShotSkill>();
        if (snipeShotSkill != null)
        {
            snipeShotSkill.DeactivateSnipeShot();
        }
        else
        {
            Debug.LogWarning("SnipeShotSkill ������Ʈ�� ã�� �� �����ϴ�.");
        }
    }

    #endregion

    #region Animation Handling

    [PunRPC]
    public override void Respawn()
    {
        
        base.Respawn();
    }

    #endregion
}
