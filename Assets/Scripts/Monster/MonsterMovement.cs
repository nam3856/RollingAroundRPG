using UnityEngine;
using Pathfinding;
using Photon.Pun;
using Unity.VisualScripting;

public class MonsterMovement : MonoBehaviourPunCallbacks
{
    public float speed = 5f;
    public float nextWaypointDistance = 1f;
    private Transform target;
    private float lastTargetingTime;
    private Path path;
    private int currentWaypoint = 0;
    private bool reachedEndOfPath = false;
    private Vector2 lastTargetPosition;
    private Vector2 initialPosition;

    private Seeker seeker;
    private Rigidbody2D rb;

    private Animator animator;
    public MonsterTargeting monsterTargeting;


    private void Awake()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        initialPosition = transform.position;
        if (monsterTargeting == null)
        {
            monsterTargeting = GetComponentInParent<MonsterTargeting>();
        }
    }

    public void SetTarget(Transform targetTransform)
    {
        target = targetTransform;

        if (PhotonNetwork.IsMasterClient)
        {
            if (target != null)
            {
                CancelInvoke("UpdatePathToInitialPosition");
                InvokeRepeating("UpdatePath", 0f, 0.5f);
                CancelInvoke("UpdatePathToInitialPosition");
                target = targetTransform;
            }
            else
            {
                CancelInvoke("UpdatePath");
                InvokeRepeating("UpdatePathToInitialPosition", 0f, 0.5f);
                CancelInvoke("UpdatePath");
                target = null;
            }
        }
    }

    void UpdatePathToInitialPosition()
    {

        if (seeker.IsDone())
        {
            float distanceToInitialPosition = Vector2.Distance(rb.position, initialPosition);

            // ���� �Ÿ� �̳��� �����ߴ��� Ȯ��
            if (distanceToInitialPosition < 0.1f)
            {
                // �ʱ� ��ġ�� ���������Ƿ� �̵� ���� �� ��� ������Ʈ ����
                CancelInvoke("UpdatePathToInitialPosition");
                path = null;
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;


                return;
            }

            seeker.StartPath(rb.position, initialPosition, OnPathComplete);
        }
    }

    void UpdatePath()
    {
        if (target == null || !PhotonNetwork.IsMasterClient) return;
        if (!target.gameObject.activeInHierarchy || target==null)
        {
            Debug.Log("Ÿ���� ��Ȱ��ȭ��");
            target = null;
            CancelInvoke("UpdatePath");
            path = null;
            return;
        }


        // �÷��̾��� ��ġ ��ȭ�� ����� ��ȿ���� Ȯ���Ͽ� ��� ����
        if (Vector2.Distance(target.position, lastTargetPosition) > 0.2f || Time.time - lastTargetingTime > 1f || path == null)
        {
            if (seeker.IsDone())
            {
                seeker.StartPath(rb.position, target.position, OnPathComplete);
                lastTargetPosition = target.position;
                lastTargetingTime = Time.time;
            }
        }
    }

    public void ForceUpdatePath()
    {
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

    private void FixedUpdate()
    {
        if (!PhotonNetwork.IsMasterClient || path == null) return;

        if (currentWaypoint >= path.vectorPath.Count)
        {
            reachedEndOfPath = true;
            return;
        }
        else
        {
            reachedEndOfPath = false;
        }

        // ���� ��ġ�� ���� ��������Ʈ ������ ���� ���
        //Vector2 currentPosition = rb.position;
        //Vector2 targetPosition = (Vector2)path.vectorPath[currentWaypoint];
        Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
        Vector2 force = direction * speed * Time.deltaTime;
        rb.AddForce(force, ForceMode2D.Impulse);

        Vector2 currentWaypointPos = path.vectorPath[currentWaypoint];

        // ��������Ʈ�� ���� �����ߴ��� Ȯ�� (��Ȯ�� ��ġ ��)
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

        if (Mathf.Abs(velocity.x) > Mathf.Abs(velocity.y))
        {
            // X�� �̵��� �� Ŭ ��
            if (velocity.x > 0)
            {
                // ���������� �̵�
                animator.SetBool("move hori", true);
                animator.SetBool("move down", false);
                animator.SetBool("move up", false);
                GetComponent<SpriteRenderer>().flipX = true; // ���������� �̵� �� ��������Ʈ ������
            }
            else
            {
                // �������� �̵�
                animator.SetBool("move hori", true);
                animator.SetBool("move down", false);
                animator.SetBool("move up", false);
                GetComponent<SpriteRenderer>().flipX = false;  // �������� �̵� �� ��������Ʈ ������ ����
            }
        }
        else
        {
            // Y�� �̵��� �� Ŭ ��
            if (velocity.y > 0)
            {
                // ���� �̵�
                animator.SetBool("move hori", false);
                animator.SetBool("move down", false);
                animator.SetBool("move up", true);
            }
            else
            {
                // �Ʒ��� �̵�
                animator.SetBool("move hori", false);
                animator.SetBool("move up", false);
                animator.SetBool("move down", true);
            }
        }
    }


    [PunRPC]
    public void ApplyKnockback(Vector3 sourcePosition, float force)
    {
        Vector2 knockbackDirection = (transform.position - sourcePosition).normalized;
        if (rb != null)
        {
            rb.AddForce(knockbackDirection * force, ForceMode2D.Impulse);
        }
        ForceUpdatePath();
    }

}