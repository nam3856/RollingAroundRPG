using Cysharp.Threading.Tasks;
using Pathfinding;
using Photon.Pun;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Collections;

public abstract class MonsterBase : MonoBehaviourPunCallbacks
{
    #region Protected Fields
    [Header("Base Ability")]
    [SerializeField] protected int maxHealth;
    [SerializeField] protected int experiencePoints;
    [SerializeField] protected float speed;
    [SerializeField] protected float attackCooldown;
    [SerializeField] protected int damage;
    [SerializeField] protected bool IsBoss = false;

    protected int currentHealth;
    public bool IsDead { get; protected set; } = false;
    protected HashSet<int> attackers = new HashSet<int>();
    protected float lastHitTime = 0;
    protected PhotonView PV;

    protected float nextWaypointDistance = 0.2f;
    protected Rigidbody2D rb;


    #endregion

    #region Private Fields
    private Transform target;
    private List<Transform> playersInRange = new List<Transform>();
    private Vector2 initialPosition;
    private Vector2 lastTargetPosition;
    private Dictionary<Transform, UniTask> removeTasks = new Dictionary<Transform, UniTask>();
    private float targetLostDelay = 0.5f;
    private float lastTargetingTime;
    private Seeker seeker;
    public Animator animator;
    private Path path;
    private int currentWaypoint = 0;
    private bool reachedEndOfPath = false;
    private bool isUpdatingPath = false;
    #endregion

    #region References
    public GameObject damageTextPrefab;
    public Transform canvasTransform;
    public List<BaseItem> dropItems;

    #endregion

    #region Unity Callbacks

    protected virtual void Start()
    {
        currentHealth = maxHealth;

        initialPosition = transform.position;
        if (PhotonNetwork.IsMasterClient)
        {
            PlayerScript.OnPlayerDied += HandlePlayerDied;
            PlayerScript.OnPlayerRespawned += HandlePlayerRespawned;
        }

        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        initialPosition = transform.position;
        PV = GetComponent<PhotonView>();

        if (damageTextPrefab == null)
        {
            damageTextPrefab = Resources.Load<GameObject>("damageText");
        }

        if (canvasTransform == null)
        {
            canvasTransform = GetComponentInChildren<Canvas>().transform;
        }
    }

    protected virtual void OnDestroy()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PlayerScript.OnPlayerDied -= HandlePlayerDied;
            PlayerScript.OnPlayerRespawned -= HandlePlayerRespawned;
        }
    }


    /// <summary>
    /// �̵� ����
    /// </summary>
    private void FixedUpdate()
    {
        Move();
    }

    #endregion

    #region Combat

    [PunRPC]
    protected virtual void TriggerAttackAnimation()
    {
        animator.SetTrigger("attack");
    }

    [PunRPC]
    public virtual void TakeDamage(object[] data)
    {
        if (IsDead) return;
        int damage = (int)data[0];
        int attackerViewID = (int)data[1];
        bool isCritical = (bool)data[2];

        currentHealth -= damage;

        if (lastHitTime + 0.5f < Time.time)
        {
            GetComponent<Animator>().SetTrigger("hit");
            lastHitTime = Time.time;
        }

        
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


        if (currentHealth <= 0)//���� ��
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
            if (data.Length > 3)
            {
                PV.RPC("ApplyKnockback", RpcTarget.All, (Vector3)data[3], (float)data[4]);
            }
        }
    }
    protected void DropLoot()
    {
        foreach (BaseItem dropItem in dropItems)
        {
            float randomAngle = UnityEngine.Random.Range(-180, 180);
            float randomValue = UnityEngine.Random.Range(0f, 1f);
            int owner = UnityEngine.Random.Range(1, attackers.Count);
            Vector2 randomDirection = RotateVector(Vector2.up, randomAngle);
            if (randomValue <= dropItem.dropChance)
            {
                Debug.Log($"drop Item : {dropItem}, owner = {owner}");
                // ����� ������ �ν��Ͻ� ����
                ItemInstance itemInstance = new ItemInstance(dropItem);

                // ����� ������ ����
                GameObject droppedItem = PhotonNetwork.Instantiate("ItemPrefab", transform.position, Quaternion.identity);
                ItemObject itemObject = droppedItem.GetComponent<ItemObject>();
                itemObject.SetItem(itemInstance, randomDirection, owner);
            }
            else Debug.Log($"drop Item : {dropItem}, Ȯ�� ����");
        }
    }

    private Vector2 RotateVector(Vector2 direction, float angle)
    {
        float radian = angle * Mathf.Deg2Rad;
        float cos = Mathf.Cos(radian);
        float sin = Mathf.Sin(radian);

        return new Vector2(
            direction.x * cos - direction.y * sin,
            direction.x * sin + direction.y * cos
        );
    }


    /// <summary>
    /// �����鼭 ���� ����鿡�� ����ġ ����
    /// </summary>
    protected virtual void GiveExperienceToAttackers()
    {

        foreach (int attackerViewID in attackers)
        {
            PhotonView attackerPV = PhotonView.Find(attackerViewID);
            if (attackerPV != null)
            {
                Character attackerCharacter = attackerPV.GetComponent<Character>();
                if (attackerCharacter != null)
                {
                    attackerPV.RPC("AddExperience", attackerPV.Owner, experiencePoints);
                }
                else
                {
                    Debug.LogError("Character ������Ʈ�� ã�� �� �����ϴ�.");
                }
            }
            else
            {
                Debug.LogError("�������� PhotonView�� ã�� �� �����ϴ�. attackerViewID: " + attackerViewID);
            }
        }


        //�̰Ÿ� ���� override �� �� ���� ��
        //UniTask.Void(async () =>
        //{
        //    await UniTask.Delay(TimeSpan.FromSeconds(0.7f));//�״� �ִϸ��̼� ���
        //    PhotonNetwork.Destroy(gameObject);
        //});

    }

    protected abstract void Attack();

    [PunRPC]
    public virtual void ApplyKnockback(Vector3 sourcePosition, float force)
    {
        Vector2 knockbackDirection = (transform.position - sourcePosition).normalized;
        if (rb != null)
        {
            rb.AddForce(knockbackDirection * force, ForceMode2D.Impulse);
        }
    }
    #endregion

    #region Targetting Players
    public bool IsInRange(Transform target)
    {
        return playersInRange.Contains(target);
    }
    public void RemovePlayerInRange(Transform playerTransform)
    {
        // �̹� UniTask�� ���� ���̸� ����
        if (removeTasks.ContainsKey(playerTransform))
        {
            return;
        }

        if (gameObject.activeInHierarchy)
        {
            // Ÿ�� ���� UniTask ����
            UniTask task = RemovePlayerWithDelay(playerTransform);
            removeTasks.Add(playerTransform, task);
        }
    }

    private async UniTask RemovePlayerWithDelay(Transform playerTransform)
    {
        await UniTask.Delay((int)(targetLostDelay * 1000)); // ��� �ð� �߰�

        playersInRange.Remove(playerTransform);
        removeTasks.Remove(playerTransform);

        if (target == playerTransform)
        {
            SetNewTarget();
        }
    }

    public void AddPlayerInRange(Transform playerTransform)
    {
        // �÷��̾ �̹� ����Ʈ�� ������ �߰�
        if (!playersInRange.Contains(playerTransform))
        {
            playersInRange.Add(playerTransform);
        }

        // ���� ������ UniTask�� ������ ���
        if (removeTasks.ContainsKey(playerTransform))
        {
            removeTasks.Remove(playerTransform);
        }

        if (target == null)
        {
            SetNewTarget();
        }
    }

    private void SetNewTarget()
    {
        if (playersInRange.Count > 0)
        {
            target = playersInRange[UnityEngine.Random.Range(0, playersInRange.Count)];

            PV.RPC("SetTargetRPC", RpcTarget.AllBuffered, target.GetComponent<PhotonView>().ViewID);
        }
        else
        {
            target = null;
            PV.RPC("SetTargetRPC", RpcTarget.AllBuffered, -1);
        }
    }

    private void HandlePlayerDied(PlayerScript player)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (playersInRange.Contains(player.transform))
        {
            playersInRange.Remove(player.transform);
            if (target == player.transform)
            {
                SetNewTarget();
            }
        }
    }

    private void HandlePlayerRespawned(PlayerScript player)
    {
        // ������� ����
    }

    [PunRPC]
    public void SetTargetRPC(int targetViewID)
    {
        if (targetViewID != -1)
        {
            PhotonView targetPV = PhotonView.Find(targetViewID);
            if (targetPV != null)
            {
                target = targetPV.transform;
            }
            else
            {
                target = null;
            }
        }
        else
        {
            target = null;
        }

        SetTarget(target);
    }

    #endregion

    #region Movement

    protected virtual void Move()
    {
        if (!PhotonNetwork.IsMasterClient || path == null || IsDead) return;

        if (currentWaypoint >= path.vectorPath.Count)
        {
            reachedEndOfPath = true;
            return;
        }
        else
        {
            reachedEndOfPath = false;
        }

        Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
        Vector2 force = direction * speed;// * Time.deltaTime;
        rb.AddForce(force);//, ForceMode2D.Impulse);

        Vector2 currentWaypointPos = path.vectorPath[currentWaypoint];

        if (Mathf.Approximately(rb.position.x, currentWaypointPos.x) &&
            Mathf.Approximately(rb.position.y, currentWaypointPos.y))
        {
            currentWaypoint++;
        }
        else
        {
            float distance = Vector2.Distance(rb.position, currentWaypointPos);

            if (distance < nextWaypointDistance)
            {
                currentWaypoint++;
            }
        }

        Vector2 velocity = rb.velocity;

        animator.SetFloat("moveX", velocity.x);
        animator.SetFloat("moveY", velocity.y);

        GetComponent<SpriteRenderer>().flipX = velocity.x > 0;
    }
    public void SetTarget(Transform targetTransform)
    {
        target = targetTransform;

        if (PhotonNetwork.IsMasterClient)
        {
            if (target != null)
            {
                StopUpdatingPaths();
                StartUpdatingPath().Forget();
            }
            else
            {
                StopUpdatingPaths();
                StartUpdatingPathToInitialPosition().Forget();
            }
        }
    }

    private async UniTaskVoid StartUpdatingPathToInitialPosition()
    {
        if (isUpdatingPath) return;
        isUpdatingPath = true;

        while (isUpdatingPath && !IsDead && PhotonNetwork.IsMasterClient && playersInRange.Count == 0)
        {
            float distanceToInitialPosition = Vector2.Distance(rb.position, initialPosition);

            if (distanceToInitialPosition < 0.3f)
            {
                isUpdatingPath = false;
                path = null;

                animator.SetFloat("moveX", 0);
                animator.SetFloat("moveY", 0);
                return;
            }

            if (seeker.IsDone())
            {
                seeker.StartPath(rb.position, initialPosition, OnPathComplete);
            }

            await UniTask.Delay(500); // 0.1�ʸ��� ��� ����
        }
    }

    private async UniTaskVoid StartUpdatingPath()
    {
        if (isUpdatingPath) return;
        isUpdatingPath = true;

        while (isUpdatingPath && !IsDead && target != null && PhotonNetwork.IsMasterClient && playersInRange.Count > 0)
        {

            if (Vector2.Distance(target.position, lastTargetPosition) > 0.2f || Time.time - lastTargetingTime > 1f || path == null)
            {
                if (seeker.IsDone())
                {
                    seeker.StartPath(rb.position, target.position, OnPathComplete);
                    lastTargetPosition = target.position;
                    lastTargetingTime = Time.time;
                }
            }

            await UniTask.Delay(500);
        }
    }

    private void StopUpdatingPaths()
    {
        isUpdatingPath = false;
    }


    void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }

    #endregion
}

