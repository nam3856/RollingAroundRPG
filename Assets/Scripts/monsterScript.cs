using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using static UnityEngine.GraphicsBuffer;
using System.Threading;

public class MonsterScript : MonoBehaviourPunCallbacks
{
    public Transform targetPlayer;      // ������ �÷��̾��� Transform
    public float speed = 2f;            // ���� �̵� �ӵ�
    public int damage = 10;             // �÷��̾�� ���� ������
    public float attackCooldown = 1f;   // ���� ��Ÿ��
    private float lastAttackTime = 0f;  // ������ ���� �ð�
    private Rigidbody2D rb;
    private Animator animator;
    public int maxHealth = 20;           // ���� �ִ� ü��
    private int currentHealth;
    public GameObject damageTextPrefab; // ������ �ؽ�Ʈ ������
    public Transform canvasTransform;
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>(); 
        currentHealth = maxHealth;
        if (PhotonNetwork.IsMasterClient)
        {
            // ���� ���� �� �÷��̾� ��Ͽ��� ������ �÷��̾� ����
            FindTargetPlayer();
        }
    }

    private void Update()
    {
        // ������ Ŭ���̾�Ʈ������ ���� �̵� ���� ����
        if (PhotonNetwork.IsMasterClient)
        {
            if (targetPlayer != null)
            {
                MoveAndAnimate();
            }
            else
            {
                // ������ �÷��̾ ã��
                FindTargetPlayer();
            }
        }
    }

    void MoveAndAnimate()
    {
        if (targetPlayer == null) return;

        // �÷��̾���� ���� ���
        Vector2 direction = (targetPlayer.position - transform.position).normalized;
        rb.velocity = rb.velocity * 0.9f;

        rb.AddForce(direction * speed);

        // X��� Y���� ���밪�� ���Ͽ� �� ū ���� �������� ������ ����
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            // X�� �̵��� �� Ŭ ��
            if (direction.x > 0)
            {
                // ���������� �̵�
                animator.SetBool("move hori", true);
                animator.SetBool("move down", false);
                animator.SetBool("move up", false);
                GetComponent<SpriteRenderer>().flipX = true; // ��������Ʈ�� ������ ����
            }
            else
            {
                // �������� �̵�
                animator.SetBool("move hori", true);
                animator.SetBool("move down", false);
                animator.SetBool("move up", false);
                GetComponent<SpriteRenderer>().flipX = false;  // �������� �̵��� ��������Ʈ�� ������
            }
        }
        else
        {
            // Y�� �̵��� �� Ŭ ��
            if (direction.y > 0)
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
    void FindTargetPlayer()
    {
        // �÷��̾� ��Ͽ��� ������ �÷��̾ ���� ������� ����
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length > 0)
        {
            targetPlayer = players[Random.Range(0, players.Length)].transform;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // ������ Ŭ���̾�Ʈ������ ������ ó��
        if (!PhotonNetwork.IsMasterClient) return;

        if (collision.gameObject.CompareTag("Player") && Time.time > lastAttackTime + attackCooldown)
        {
            PhotonView targetPV = collision.gameObject.GetComponent<PhotonView>();
            if (targetPV != null)
            {
                // ������ ������ RPC�� ȣ��
                Vector2 attackDirection = (collision.transform.position - transform.position).normalized;
                targetPV.RPC("ReceiveDamage", RpcTarget.All, damage, attackDirection);
                lastAttackTime = Time.time;
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // ������ Ŭ���̾�Ʈ������ ������ ó��
        if (!PhotonNetwork.IsMasterClient) return;

        if (collision.gameObject.CompareTag("Player") && Time.time > lastAttackTime + attackCooldown)
        {
            PhotonView targetPV = collision.gameObject.GetComponent<PhotonView>();
            if (targetPV != null)
            {
                // ������ ������ RPC�� ȣ��
                Vector2 attackDirection = (collision.transform.position - transform.position).normalized;
                targetPV.RPC("ReceiveDamage", RpcTarget.All, damage, attackDirection);
                lastAttackTime = Time.time;
            }
        }
    }

    [PunRPC]
    public void TakeDamage(int damage)
    {
        GameObject damageTextInstance = Instantiate(damageTextPrefab, canvasTransform);
        DamageText damageTextScript = damageTextInstance.GetComponent<DamageText>();
        damageTextScript.SetDamageText(damage.ToString());
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.Destroy(gameObject);
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
    }
    void Die()
    {
        Debug.Log("���� ���!");
        Destroy(gameObject);
    }
}
