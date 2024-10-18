using Cysharp.Threading.Tasks;
using Pathfinding;
using Photon.Pun;
using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class MonsterBase : MonoBehaviourPunCallbacks
{
    #region Protected Fields
    [Header("Base Ability")]
    [SerializeField] protected int maxHealth;
    [SerializeField] protected int experiencePoints;
    [SerializeField] protected float speed;
    [SerializeField] protected float attackCooldown;
    [SerializeField] protected int damage;
    
    protected int currentHealth;
    public bool isDead { get; protected set; } = false;
    protected HashSet<int> attackers = new HashSet<int>();
    protected float lastHitTime = 0;
    protected PhotonView PV;

    protected float nextWaypointDistance = 1f;


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
    private Rigidbody2D rb;
    private Animator animator;
    private Path path;
    private int currentWaypoint = 0;
    private bool reachedEndOfPath = false;
    private bool isUpdatingPath = false;
    #endregion

    #region References
    public GameObject damageTextPrefab;
    public Transform canvasTransform;

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

        if(damageTextPrefab == null)
        {
            damageTextPrefab = Resources.Load<GameObject>("damageText");
        }

        if(canvasTransform == null)
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
    /// 이동 로직
    /// </summary>
    private void FixedUpdate()
    {
        if (!PhotonNetwork.IsMasterClient || path == null || isDead) return;

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
        Vector2 force = direction * speed * Time.deltaTime;
        rb.AddForce(force, ForceMode2D.Impulse);

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
        if (Mathf.Abs(velocity.x) > Mathf.Abs(velocity.y))
        {
            if (velocity.x > 0)
            {
                GetComponent<SpriteRenderer>().flipX = true;
            }
            else
            {
                GetComponent<SpriteRenderer>().flipX = false;
            }
        }
    }

    #endregion

    #region Combat

    [PunRPC]
    private void TriggerAttackAnimation()
    {
        GetComponent<Animator>().SetTrigger("attack");
    }

    [PunRPC]
    public virtual void TakeDamage(object[] data)
    {
        if (isDead) return;
        int damage = (int)data[0];
        int attackerViewID = (int)data[1];
        bool isCritical = (bool)data[2];

        currentHealth -= damage;

        if (lastHitTime + 0.5f < Time.time)
        {
            GetComponent<Animator>().SetTrigger("hit");
        }

        lastHitTime = Time.time;
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
            isDead = true;
            GetComponent<CircleCollider2D>().enabled = false;
            GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
            GetComponent<Animator>().SetTrigger("die");
            if (PhotonNetwork.IsMasterClient)
            {
                GiveExperienceToAttackers();
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

    /// <summary>
    /// 죽으면서 때린 사람들에게 경험치 지급
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
                    Debug.LogError("Character 컴포넌트를 찾을 수 없습니다.");
                }
            }
            else
            {
                Debug.LogError("공격자의 PhotonView를 찾을 수 없습니다. attackerViewID: " + attackerViewID);
            }
        }


        //이거만 떼서 override 할 때 쓰면 됨
        //UniTask.Void(async () =>
        //{
        //    await UniTask.Delay(TimeSpan.FromSeconds(0.7f));//죽는 애니메이션 대기
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
        ForceUpdatePath();
    }
    #endregion

    #region Targetting Players
    public bool IsInRange(Transform target)
    {
        return playersInRange.Contains(target);
    }
    public void RemovePlayerInRange(Transform playerTransform)
    {
        // 이미 UniTask가 실행 중이면 무시
        if (removeTasks.ContainsKey(playerTransform))
        {
            return;
        }

        if (gameObject.activeInHierarchy)
        {
            // 타겟 유지 UniTask 시작
            UniTask task = RemovePlayerWithDelay(playerTransform);
            removeTasks.Add(playerTransform, task);
        }
    }

    private async UniTask RemovePlayerWithDelay(Transform playerTransform)
    {
        await UniTask.Delay((int)(targetLostDelay * 1000)); // 대기 시간 추가

        playersInRange.Remove(playerTransform);
        removeTasks.Remove(playerTransform);

        if (target == playerTransform)
        {
            SetNewTarget();
        }
    }

    public void AddPlayerInRange(Transform playerTransform)
    {
        // 플레이어가 이미 리스트에 없으면 추가
        if (!playersInRange.Contains(playerTransform))
        {
            playersInRange.Add(playerTransform);
        }

        // 제거 예정인 UniTask가 있으면 취소
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
        // 사용하지 않음
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
        isUpdatingPath = true;

        while (isUpdatingPath && !isDead && PhotonNetwork.IsMasterClient)
        {
            float distanceToInitialPosition = Vector2.Distance(rb.position, initialPosition);

            if (distanceToInitialPosition < 0.1f)
            {
                isUpdatingPath = false;
                path = null;
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
                return;
            }

            if (seeker.IsDone())
            {
                seeker.StartPath(rb.position, initialPosition, OnPathComplete);
            }

            await UniTask.Delay(500); // 0.5초마다 경로 갱신
        }
    }

    private async UniTaskVoid StartUpdatingPath()
    {
        isUpdatingPath = true;

        while (isUpdatingPath && !isDead && target != null && PhotonNetwork.IsMasterClient)
        {
            if (target == null || !target.gameObject.activeInHierarchy)
            {
                Debug.Log("타겟이 비활성화됨");
                target = null;
                path = null;
                isUpdatingPath = false;
                return;
            }

            if (Vector2.Distance(target.position, lastTargetPosition) > 0.2f || Time.time - lastTargetingTime > 1f || path == null)
            {
                if (seeker.IsDone())
                {
                    seeker.StartPath(rb.position, target.position, OnPathComplete);
                    lastTargetPosition = target.position;
                    lastTargetingTime = Time.time;
                }
            }

            await UniTask.Delay(500); // 0.5초마다 경로 갱신
        }
    }

    private void StopUpdatingPaths()
    {
        isUpdatingPath = false;
    }

    public void ForceUpdatePath()
    {
        if (isDead) return;
        if (seeker.IsDone())
        {
            seeker.StartPath(rb.position, target.position, OnPathComplete);
            lastTargetPosition = target.position;
        }
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

