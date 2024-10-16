using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class FanBulletScript : MonoBehaviourPunCallbacks
{
    public DebugLogger logger;
    public Collider2D attackTrigger;
    public string targetTag = "Player";
    public PhotonView PV;
    Vector2 dir;
    private float attackRange = 0.65f;
    private float attackAngle = 90f;
    private int attackDamage = 1;
    private float knockbackForce = 0.4f;
    public LayerMask monsterLayer;
    private HashSet<int> attackedPlayers = new HashSet<int>();
    public CircleCollider2D basicAndComboRange;
    public BoxCollider2D powerStrikeRange;
    private bool isPowerStrike = false;
    private Vector2 horizontalOffset = new Vector2(1.22f, 0.002f);
    private Vector2 horizontalSize = new Vector2(2.36f, 1.1f);
    private Vector2 verticalSize = new Vector2(2.36f, 1.7f);
    double critical;
    private Vector2 verticalOffset = new Vector2(1.2f, -0.02f);
    public int playerNum;

    [PunRPC]
    void DirRPC(Vector2 dir, int damage, int actorNum, double critical)
    {
        this.dir = dir;
        isPowerStrike = false;
        basicAndComboRange.enabled = true;
        powerStrikeRange.enabled = false;

        playerNum = actorNum;
        attackDamage = damage;

        this.critical = critical;
    }

    private void Start()
    {
        Destroy(gameObject, 0.3f);
    }
    [PunRPC]
    void SetPowerStrike(Vector2 dir, int damage, int actorNum, double critical)
    {
        basicAndComboRange.enabled = false;
        powerStrikeRange.enabled = true;
        attackDamage = damage;
        isPowerStrike = true;
        float angle = Vector2.SignedAngle(Vector2.right, dir);
        transform.rotation = Quaternion.Euler(0, 0, angle);
        this.dir = dir;
        AdjustColliderSize();
        knockbackForce = 0.8f;
        playerNum = actorNum;
        this.critical = critical;
    }


    private void OnTriggerEnter2D(UnityEngine.Collider2D other)
    {
        double p = UnityEngine.Random.value;
        bool isCriticalHit = false;
        if (p <= critical)
        {
            attackDamage *= 2;
            isCriticalHit = true;
        }
        //Debug.Log(other.tag);
        if (isPowerStrike)
        {
            if (other.CompareTag("Monster"))
            {
                PhotonView monsterPV = other.GetComponent<PhotonView>();
                if (attackedPlayers.Contains(monsterPV.ViewID)) return;

                if (monsterPV != null)
                {
                    monsterPV.RPC("TakeDamage", RpcTarget.AllBuffered, attackDamage, playerNum, isCriticalHit);
                    monsterPV.RPC("ApplyKnockback", RpcTarget.All, transform.position, knockbackForce);

                    attackedPlayers.Add(monsterPV.ViewID);
                }
            }
        }
        else
        {
            if (other.CompareTag("Monster"))
            {
                PhotonView monsterPV = other.GetComponent<PhotonView>();
                if (attackedPlayers.Contains(monsterPV.ViewID)) return;

                if (monsterPV != null)
                {
                    Vector2 directionToPlayer = (other.transform.position - transform.position).normalized;
                    float angleToPlayer = Vector2.Angle(dir, directionToPlayer);
                    if (angleToPlayer <= attackAngle / 2)
                    {
                        // �������� �˹��� ��� Ŭ���̾�Ʈ���� ����
                        monsterPV.RPC("TakeDamage", RpcTarget.AllBuffered, attackDamage, playerNum, isCriticalHit);

                        if (monsterPV != null)
                        {
                            monsterPV.RPC("ApplyKnockback", RpcTarget.All, transform.position, knockbackForce);
                            attackedPlayers.Add(monsterPV.ViewID);
                        }
                    }
                }
            }
            if (other.CompareTag("Removable Obstacle"))
            {
                PhotonView obstaclePV = other.GetComponent<PhotonView>();
                if (attackedPlayers.Contains(obstaclePV.ViewID)) return;

                if (obstaclePV != null)
                {
                    Vector2 directionToPlayer = (other.transform.position - transform.position).normalized;
                    float angleToPlayer = Vector2.Angle(dir, directionToPlayer);
                    if (angleToPlayer <= attackAngle / 2)
                    {
                        attackedPlayers.Add(obstaclePV.ViewID);
                        obstaclePV.RPC("DestroyRPC", RpcTarget.AllBuffered);
                        Destroy(gameObject);
                    }
                }
            }
        }
    }
    void AdjustColliderSize()
    {
        if (powerStrikeRange != null)
        {
            // ��/�Ϸ� ������ �� (Y���� �� ũ�ٸ�)
            if (Mathf.Abs(dir.y) > Mathf.Abs(dir.x))
            {
                powerStrikeRange.size = verticalSize; // ���� �ݶ��̴� ũ�� ����
                powerStrikeRange.offset = verticalOffset;

            }
            // ��/��� ������ �� (X���� �� ũ�ٸ�)
            else
            {
                powerStrikeRange.size = horizontalSize; // ���� �ݶ��̴� ũ�� ����
                powerStrikeRange.offset = horizontalOffset;

            }
        }
    }

}