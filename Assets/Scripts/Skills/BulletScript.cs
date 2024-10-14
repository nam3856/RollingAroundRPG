using Photon.Pun;
using UnityEngine;
using System.Collections.Generic;
using System.Threading;
using System.Collections;

public class BulletScript : MonoBehaviourPunCallbacks
{
    public Vector2 dir;
    public int damage;
    public int shooterActorNumber;
    public int attackerViewId = 0;
    private Rigidbody2D rb;
    public BoxCollider2D boxCollider;
    public PhotonView targetPhotonView;
    private HashSet<int> attackedPlayers = new HashSet<int>();

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
        boxCollider = rb.GetComponent<BoxCollider2D>();
    }

    public void SetDirectionAndDamage(object[] data)
    {
        this.dir = (Vector2)data[0];
        this.damage = (int)data[1];
        this.shooterActorNumber = (int)data[2];
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            transform.rotation = Quaternion.identity;
        }
        else
        {
            transform.rotation = Quaternion.Euler(0, 0, 90);
        }

        if (photonView.IsMine)
        {
            if ((bool)data[4] == false)
            {
                rb.velocity = dir * 3f;
            }
            else rb.velocity = dir * 7f;

        }

        if (data[3] != null)
        {
            Physics2D.IgnoreCollision(boxCollider, (CapsuleCollider2D)data[3]);
        }

        attackerViewId = (int)data[5];
        boxCollider.enabled = true;
        StartCoroutine(DelayedDestroyBullet(3f));
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        targetPhotonView = col.GetComponent<PhotonView>();

        if (targetPhotonView == null || attackedPlayers.Contains(targetPhotonView.ViewID))
        {
            return;
        }

        if (col.CompareTag("Monster"))
        {
            attackedPlayers.Add(targetPhotonView.ViewID);
            if (targetPhotonView != null)
            {
                boxCollider.enabled = false;
                Debug.LogError(shooterActorNumber + "�� �� �Ѿ˿� ���� �浹");
                // ������ ü�� ���� ó��
                targetPhotonView.RPC("TakeDamage", RpcTarget.AllBuffered, damage, attackerViewId);
                // �Ѿ� �ı� ����ȭ
                StartCoroutine(DelayedDestroyBullet(0.05f));
            }
        }
    }

    private IEnumerator DelayedDestroyBullet(float timer)
    {
        if (!gameObject.activeSelf)
        {
            yield break;
        }
        yield return new WaitForSeconds(timer);
        PhotonView PV = GetComponent<PhotonView>();
        PV.RPC("DestroyBullet", RpcTarget.AllBuffered);
    }

    [PunRPC]
    private void DestroyBullet()
    {
        
        dir = Vector2.zero;  // ���� �ʱ�ȭ
        attackedPlayers.Clear();  // ���� ��� �ʱ�ȭ
        rb.velocity = Vector2.zero;  // �ӵ� �ʱ�ȭ
        boxCollider.enabled = false; // �ݶ��̴� ��Ȱ��ȭ (������ �� �ٽ� Ȱ��ȭ �ʿ�)
        
        gameObject.SetActive(false);  // ������Ʈ ��Ȱ��ȭ
        Debug.Log("�Ѿ� �ı�");
        // Ǯ�� �ý����� ����� ������Ʈ ����
        if (photonView.IsMine)
        {
            CustomPrefabPool pool = PhotonNetwork.PrefabPool as CustomPrefabPool;
            if (pool != null)
            {
                pool.Destroy(gameObject);
            }
        }
    }
}
