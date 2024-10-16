using Photon.Pun;
using UnityEngine;
using System.Collections.Generic;
using System.Threading;
using System.Collections;

public class FireballScript : MonoBehaviourPunCallbacks
{
    public Vector2 dir;
    public int damage;
    public int shooterActorNumber;
    public int attackerViewId = 0;
    private Rigidbody2D rb;
    public BoxCollider2D boxCollider;
    public PhotonView targetPhotonView;
    private HashSet<int> attackedPlayers = new HashSet<int>();
    double critical;

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
            if (dir.x < 0) transform.rotation = Quaternion.Euler(0, 0, 180);
            else transform.rotation = Quaternion.identity;
        }
        else
        {
            if(dir.y<0) transform.rotation = Quaternion.Euler(0, 0, 270);
            else transform.rotation = Quaternion.Euler(0, 0, 90);
        }

        if (photonView.IsMine)
        {
            rb.velocity = dir * 3f;
        }

        if (data[3] != null)
        {
            Physics2D.IgnoreCollision(boxCollider, (CapsuleCollider2D)data[3]);
        }

        attackerViewId = (int)data[5];

        critical = (double)data[6];
        boxCollider.enabled = true;
        StartCoroutine(DelayedDestroyBullet());
    }
    private IEnumerator DelayedDestroyBullet()
    {
        yield return new WaitForSeconds(2f);
        PhotonView PV = GetComponent<PhotonView>();
        PV.RPC("DestroyBullet", RpcTarget.AllBuffered);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        targetPhotonView = col.GetComponent<PhotonView>();
        
        if (targetPhotonView == null)
        {
            return; // PhotonView가 없으면 처리하지 않음
        }
        if (attackedPlayers.Contains(targetPhotonView.ViewID)) return;

        if (col.CompareTag("Monster"))
        {
            attackedPlayers.Add(targetPhotonView.ViewID);
            
            if (targetPhotonView != null)
            {
                double p = UnityEngine.Random.value;
                bool isCriticalHit = false;
                if (p <= critical)
                {
                    damage *= 2;
                    isCriticalHit = true;
                }
                targetPhotonView.RPC("TakeDamage", RpcTarget.AllBuffered, damage, attackerViewId, isCriticalHit);
            }
        }
    }

    [PunRPC]
    private void DestroyBullet()
    {
        dir = Vector2.zero;
        attackedPlayers.Clear();
        gameObject.SetActive(false);
        if (photonView.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}
