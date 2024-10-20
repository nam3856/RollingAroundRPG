using Photon.Pun;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Collections;
using Cysharp.Threading.Tasks;

public class SlimeBoss : MonsterBase
{
    private enum BossPhase { Phase1, Phase2, Phase3 }
    private BossPhase currentPhase = BossPhase.Phase1;
    public float splitStartTime;

    [Header("Jump Attack")]
    [SerializeField] private GameObject jumpAttackIndicatorPrefab;
    private GameObject jumpAttackIndicatorInstance;
    private float jumpAttackCooldown = 12f;
    private float lastJumpAttackTime = -Mathf.Infinity;
    private bool isPreparingJumpAttack = false;

    [Header("Spawn Mini Slimes")]
    [SerializeField] private float spawnCooldown = 5f;
    private float lastSpawnTime = -Mathf.Infinity;

    [Header("Split Into Medium Slimes")]
    [SerializeField] private float splitInterval = 40f;
    public float recombineDelay = 20f;
    private float lastSplitTime = -Mathf.Infinity;
    public bool isSplit = false;
    public List<GameObject> mediumSlimes = new List<GameObject>();

    private int phase3StartHealth;


    public event Action<int> OnPhaseChanged;

    #region Unity Callbacks
    protected override void Start()
    {
        base.Start();
        currentPhase = BossPhase.Phase1;
        // Initialize timers and health checkpoints
    }
    private void Update()
    {
        if (PhotonNetwork.IsMasterClient && !IsDead)
        {
            if (currentPhase >= BossPhase.Phase1 && !isSplit)
                HandleJumpAttack();
            if (currentPhase == BossPhase.Phase3 && !isPreparingJumpAttack)
                HandleSplitting();
        }
    }

    protected override void Move()
    {
        if (isPreparingJumpAttack || isSplit) return;
        base.Move();
    }

    #endregion
    private void HandlePhaseTransitions()
    {
        if (currentPhase == BossPhase.Phase1 && currentHealth <= maxHealth * 0.7f)
        {
            currentPhase = BossPhase.Phase2;
            Debug.Log("Transitioned to Phase 2");
        }
        else if (currentPhase == BossPhase.Phase2 && currentHealth <= maxHealth * 0.4f)
        {
            currentPhase = BossPhase.Phase3;
            phase3StartHealth = currentHealth;
            Debug.Log("Transitioned to Phase 3");
        }
    }

    protected override void Attack()
    {
    }

    

    private void HandleJumpAttack()
    {
        if (Time.time >= lastJumpAttackTime + jumpAttackCooldown && !isPreparingJumpAttack)
        {
            isPreparingJumpAttack = true;
            StartJumpAttack();
            lastJumpAttackTime = Time.time;
        }
    }

    private IEnumerator PrepareJumpAttack()
    {
        float preparationTime = 2f;
        float elapsedTime = 0f;
        rb.velocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezePosition | RigidbodyConstraints2D.FreezeRotation;
        if (jumpAttackIndicatorPrefab != null)
        {
            jumpAttackIndicatorInstance = Instantiate(jumpAttackIndicatorPrefab, transform.position, Quaternion.identity);
            jumpAttackIndicatorInstance.transform.SetParent(transform);
            jumpAttackIndicatorInstance.transform.localScale = Vector3.zero;
        }

        while (elapsedTime < preparationTime)
        {
            elapsedTime += Time.deltaTime;
            float scale = Mathf.Lerp(0f, 2f, elapsedTime / preparationTime); // 최대 크기 조정
            if (jumpAttackIndicatorInstance != null)
            {
                jumpAttackIndicatorInstance.transform.localScale = Vector3.one * scale;
            }
            yield return null;
        }

        PerformJumpAttack();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        animator.SetBool("isBouncing", false);
        if (jumpAttackIndicatorInstance != null)
        {
            Destroy(jumpAttackIndicatorInstance);
        }
        isPreparingJumpAttack = false;
    }


    private void PerformJumpAttack()
    {
        float attackRadius = 3f;
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(transform.position, attackRadius, LayerMask.GetMask("Player"));

        foreach (Collider2D playerCollider in hitPlayers)
        {
            PhotonView playerPV = playerCollider.GetComponent<PhotonView>();
            if (playerPV != null)
            {
                int damageAmount = 20;
                playerPV.RPC("ReceiveDamage", RpcTarget.All, damageAmount, Vector2.zero);
            }
        }

    }


    [PunRPC]
    public override void TakeDamage(object[] data)
    {
        if (isSplit) return;
        int damage = (int)data[0];
        int attackerViewID = (int)data[1];
        bool isCritical = (bool)data[2];

        currentHealth -= damage;

        GameObject damageTextInstance = PhotonNetwork.Instantiate("DamageText", canvasTransform.position, Quaternion.identity);
        DamageText damageTextScript = damageTextInstance.GetComponent<DamageText>();
        PhotonView damageTextPV = damageTextInstance.GetComponent<PhotonView>();
        GameObject uiCanvas = GameObject.Find("Canvas");
        damageTextInstance.transform.SetParent(uiCanvas.transform, false);
        damageTextPV.RPC("SetDamageText", RpcTarget.All, damage.ToString(), isCritical);

        if (!attackers.Contains(attackerViewID))
        {
            attackers.Add(attackerViewID);
        }


        if (currentHealth <= 0)//죽을 때
        {
            IsDead = true;
            GetComponent<CircleCollider2D>().enabled = false;
            GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
            GetComponent<Animator>().SetTrigger("die");
            if (PhotonNetwork.IsMasterClient)
            {
                GiveExperienceToAttackers();
                DropLoot();
            }
        }
        else
        {
            HandlePhaseTransitions();
        }

        

        if ((currentPhase == BossPhase.Phase2 || currentPhase == BossPhase.Phase3) && PhotonNetwork.IsMasterClient)
        {
            if (Time.time >= lastSpawnTime + spawnCooldown)
            {
                SpawnSmallSlimes();
                lastSpawnTime = Time.time;
            }
        }
    }

    [PunRPC]
    public void DieInstant()
    {
        isSplit = IsDead = true;
        
        GetComponent<CircleCollider2D>().enabled = false;
        GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
        GetComponent<Animator>().SetTrigger("die");
        if (PhotonNetwork.IsMasterClient)
        {
            GiveExperienceToAttackers();
            DropLoot();
        }
    }

    private void SpawnSmallSlimes()
    {
        int numberOfSlimes = UnityEngine.Random.Range(2, 4);

        for (int i = 0; i < numberOfSlimes; i++)
        {
            Vector2 spawnPosition = (Vector2)transform.position + UnityEngine.Random.insideUnitCircle * 1f;
            GameObject miniSlime = PhotonNetwork.Instantiate("Slime", spawnPosition, Quaternion.identity);
            Slime miniSlimeScript = miniSlime.GetComponent<Slime>();
            miniSlimeScript.dropItems.Clear();
            miniSlimeScript.photonView.RPC("SetForBoss", RpcTarget.AllBuffered);
        }
    }
    private void HandleSplitting()
    {
        if (!isSplit && Time.time >= lastSplitTime + splitInterval)
        {
            isSplit = true;
            splitStartTime = Time.time;
            SplitIntoMediumSlimes();
        }

        //if (isSplit && Time.time >= splitStartTime + recombineDelay)
        //{
        //    RecombineFromMediumSlimes();
            
        //}
    }

    private void SplitIntoMediumSlimes()
    {
        int mediumSlimeCount = 5;
        int slimeHealth = currentHealth / mediumSlimeCount;
        rb.constraints = RigidbodyConstraints2D.FreezePosition | RigidbodyConstraints2D.FreezeRotation;
        for (int i = 0; i < mediumSlimeCount; i++)
        {
            Vector2 spawnPosition = (Vector2)transform.position + UnityEngine.Random.insideUnitCircle * 1f;
            GameObject slimeObject = PhotonNetwork.Instantiate("MediumSlimePrefab", spawnPosition, Quaternion.identity);
            BossSlimeMedium mediumSlime = slimeObject.GetComponent<BossSlimeMedium>();
            mediumSlime.Initialize(slimeHealth, this);
            mediumSlimes.Add(slimeObject);
        }
        PV.RPC("SetActiveRPC", RpcTarget.All, false);
    }

    public void RecombineFromMediumSlimes()
    {
        int totalHealth = 0;
        isSplit = false;
        lastSplitTime = Time.time;
        foreach (GameObject slimeObject in mediumSlimes)
        {
            if (slimeObject != null)
            {
                BossSlimeMedium mediumSlime = slimeObject.GetComponent<BossSlimeMedium>();
                if (mediumSlime != null)
                {
                    totalHealth += mediumSlime.GetCurrentHealth();
                    PhotonNetwork.Destroy(slimeObject);
                }
            }
        }

        mediumSlimes.Clear();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        PV.RPC("SetActiveRPC", RpcTarget.All, true);

        currentHealth = Mathf.Min(phase3StartHealth, totalHealth);
        IsDead = false;
    }

    [PunRPC]
    private void SetActiveRPC(bool active)
    {
        gameObject.SetActive(active);
    }

    [PunRPC]
    private void StartJumpAttackRPC()
    {
        animator.SetBool("isBouncing", true);
        animator.SetTrigger("bounce");
        StartCoroutine(PrepareJumpAttack());
    }

    private void StartJumpAttack()
    {
        PV.RPC("StartJumpAttackRPC", RpcTarget.All);
    }

    protected override void GiveExperienceToAttackers()
    {
        base.GiveExperienceToAttackers();
        if (jumpAttackIndicatorInstance != null)
        {
            Destroy(jumpAttackIndicatorInstance);
        }
        UniTask.Void(async () =>
        {
            await UniTask.Delay(TimeSpan.FromSeconds(0.7f));
            PhotonNetwork.Destroy(gameObject);
        });

    }
}


