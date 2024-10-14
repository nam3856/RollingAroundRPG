using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using static UnityEngine.GraphicsBuffer;
using System.Threading;

public class MonsterScript : MonoBehaviourPunCallbacks
{
    public Transform targetPlayer;      // 추적할 플레이어의 Transform
    public float speed = 2f;            // 몬스터 이동 속도
    public int damage = 10;             // 플레이어에게 입힐 데미지
    public float attackCooldown = 1f;   // 공격 쿨타임
    private float lastAttackTime = 0f;  // 마지막 공격 시간
    private Rigidbody2D rb;
    private Animator animator;
    public int maxHealth = 20;           // 몬스터 최대 체력
    private int currentHealth;
    public GameObject damageTextPrefab; // 데미지 텍스트 프리팹
    public Transform canvasTransform;
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>(); 
        currentHealth = maxHealth;
        if (PhotonNetwork.IsMasterClient)
        {
            // 게임 시작 시 플레이어 목록에서 추적할 플레이어 선택
            FindTargetPlayer();
        }
    }

    private void Update()
    {
        // 마스터 클라이언트에서만 몬스터 이동 로직 수행
        if (PhotonNetwork.IsMasterClient)
        {
            if (targetPlayer != null)
            {
                MoveAndAnimate();
            }
            else
            {
                // 추적할 플레이어를 찾음
                FindTargetPlayer();
            }
        }
    }

    void MoveAndAnimate()
    {
        if (targetPlayer == null) return;

        // 플레이어와의 방향 계산
        Vector2 direction = (targetPlayer.position - transform.position).normalized;
        rb.velocity = rb.velocity * 0.9f;

        rb.AddForce(direction * speed);

        // X축과 Y축의 절대값을 비교하여 더 큰 쪽을 기준으로 방향을 설정
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            // X축 이동이 더 클 때
            if (direction.x > 0)
            {
                // 오른쪽으로 이동
                animator.SetBool("move hori", true);
                animator.SetBool("move down", false);
                animator.SetBool("move up", false);
                GetComponent<SpriteRenderer>().flipX = true; // 스프라이트를 뒤집지 않음
            }
            else
            {
                // 왼쪽으로 이동
                animator.SetBool("move hori", true);
                animator.SetBool("move down", false);
                animator.SetBool("move up", false);
                GetComponent<SpriteRenderer>().flipX = false;  // 왼쪽으로 이동시 스프라이트를 뒤집음
            }
        }
        else
        {
            // Y축 이동이 더 클 때
            if (direction.y > 0)
            {
                // 위로 이동
                animator.SetBool("move hori", false);
                animator.SetBool("move down", false);
                animator.SetBool("move up", true);
            }
            else
            {
                // 아래로 이동
                animator.SetBool("move hori", false);
                animator.SetBool("move up", false);
                animator.SetBool("move down", true);
            }
        }
    }
    void FindTargetPlayer()
    {
        // 플레이어 목록에서 임의의 플레이어를 추적 대상으로 설정
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length > 0)
        {
            targetPlayer = players[Random.Range(0, players.Length)].transform;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 마스터 클라이언트에서만 데미지 처리
        if (!PhotonNetwork.IsMasterClient) return;

        if (collision.gameObject.CompareTag("Player") && Time.time > lastAttackTime + attackCooldown)
        {
            PhotonView targetPV = collision.gameObject.GetComponent<PhotonView>();
            if (targetPV != null)
            {
                // 데미지 적용을 RPC로 호출
                Vector2 attackDirection = (collision.transform.position - transform.position).normalized;
                targetPV.RPC("ReceiveDamage", RpcTarget.All, damage, attackDirection);
                lastAttackTime = Time.time;
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // 마스터 클라이언트에서만 데미지 처리
        if (!PhotonNetwork.IsMasterClient) return;

        if (collision.gameObject.CompareTag("Player") && Time.time > lastAttackTime + attackCooldown)
        {
            PhotonView targetPV = collision.gameObject.GetComponent<PhotonView>();
            if (targetPV != null)
            {
                // 데미지 적용을 RPC로 호출
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
        Debug.Log("몬스터 사망!");
        Destroy(gameObject);
    }
}
