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

    public event Action<int> OnPhaseChanged;
    protected override void Start()
    {
        base.Start();
        currentPhase = BossPhase.Phase1;
        // Initialize timers and health checkpoints
    }

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

    private float jumpAttackCooldown = 10f;
    private float lastJumpAttackTime = -Mathf.Infinity;
    private bool isPreparingJumpAttack = false;

    private void HandleJumpAttack()
    {
        if (Time.time >= lastJumpAttackTime + jumpAttackCooldown && !isPreparingJumpAttack)
        {
            isPreparingJumpAttack = true;
            StartCoroutine(PrepareJumpAttack());
            lastJumpAttackTime = Time.time;
        }
    }

    private IEnumerator PrepareJumpAttack()
    {
        float preparationTime = 2f;
        float elapsedTime = 0f;

        // Show and grow the circle indicator here

        while (elapsedTime < preparationTime)
        {
            elapsedTime += Time.deltaTime;
            // Update indicator size
            yield return null;
        }

        PerformJumpAttack();
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

    private float spawnCooldown = 5f;
    private float lastSpawnTime = -Mathf.Infinity;

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


        if (currentHealth <= 0)//Á×À» ¶§
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

    private void SpawnSmallSlimes()
    {
        int numberOfSlimes = UnityEngine.Random.Range(2, 4);

        for (int i = 0; i < numberOfSlimes; i++)
        {
            Vector2 spawnPosition = (Vector2)transform.position + UnityEngine.Random.insideUnitCircle * 1f;
            PhotonNetwork.Instantiate("Monster1", spawnPosition, Quaternion.identity);
        }
    }

    private float splitInterval = 40f;
    public float recombineDelay = 20f;
    private float lastSplitTime = -Mathf.Infinity;
    public bool isSplit = false;
    public List<GameObject> mediumSlimes = new List<GameObject>();
    private int phase3StartHealth;

    private void Update()
    {
        if (PhotonNetwork.IsMasterClient && !IsDead)
        {
            if (currentPhase >= BossPhase.Phase1 && !isSplit)
                HandleJumpAttack();
            if (currentPhase == BossPhase.Phase3)
                HandleSplitting();
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

        PV.RPC("SetActiveRPC", RpcTarget.All, true);

        currentHealth = Mathf.Min(phase3StartHealth, totalHealth);
        IsDead = false;
    }

    public void OnAllMediumSlimeDead()
    {
        IsDead = true;
        isSplit = false;

        GetComponent<CircleCollider2D>().enabled = false;
        GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
        GetComponent<Animator>().SetTrigger("die");
        if (PhotonNetwork.IsMasterClient)
        {
            GiveExperienceToAttackers();
            DropLoot();
        }

    }


    [PunRPC]
    private void SetActiveRPC(bool active)
    {
        gameObject.SetActive(active);
    }

    [PunRPC]
    private void StartJumpAttackRPC()
    {
        StartCoroutine(PrepareJumpAttack());
    }

    // Call this method on the master client
    private void StartJumpAttack()
    {
        PV.RPC("StartJumpAttackRPC", RpcTarget.All);
    }

    protected override void GiveExperienceToAttackers()
    {
        base.GiveExperienceToAttackers();
        UniTask.Void(async () =>
        {
            await UniTask.Delay(TimeSpan.FromSeconds(0.7f));
            PhotonNetwork.Destroy(gameObject);
        });
    }
}


