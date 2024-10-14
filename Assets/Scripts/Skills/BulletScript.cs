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
    private bool flag = false;

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

        if (targetPhotonView == null || flag)
        {
            return;
        }

        if (col.CompareTag("Monster"))
        {
            flag = true;
            if (targetPhotonView != null)
            {
                Debug.LogError(shooterActorNumber + "가 쏜 총알에 몬스터 충돌");
                // 몬스터의 체력 감소 처리
                targetPhotonView.RPC("TakeDamage", RpcTarget.AllBuffered, damage, attackerViewId);
                // 총알 파괴 동기화
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
        
        dir = Vector2.zero;  // 방향 초기화
        flag = false;  // 공격 대상 초기화
        rb.velocity = Vector2.zero;  // 속도 초기화
        boxCollider.enabled = false; // 콜라이더 비활성화 (재사용할 때 다시 활성화 필요)
        
        gameObject.SetActive(false);  // 오브젝트 비활성화
        // 풀링 시스템을 사용해 오브젝트 관리
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
