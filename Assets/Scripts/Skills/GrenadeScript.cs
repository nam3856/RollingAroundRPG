using Photon.Pun;
using UnityEngine;
using System.Collections.Generic;
using System.Threading;
using System.Collections;

public class GrenadeScript : MonoBehaviourPunCallbacks
{
    public Vector2 dir;
    public int damage;
    public int shooterActorNumber;
    public int attackerViewId = 0;
    public Vector2 velocity = Vector2.zero;

    private Rigidbody2D rb;
    public BoxCollider2D boxCollider;
    public CircleCollider2D circleCollider;
    public PhotonView targetPhotonView;
    public HashSet<int> attackedPlayers = new HashSet<int>();
    public double critical;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
        boxCollider = rb.GetComponent<BoxCollider2D>();
        circleCollider = GetComponentInChildren<CircleCollider2D>();
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
            rb.velocity = (Vector2)data[6];
            rb.angularVelocity = 300f;
        }

        if (data[3] != null)
        {
            Physics2D.IgnoreCollision(boxCollider, (CapsuleCollider2D)data[3]);
        }

        attackerViewId = (int)data[5];
        critical = (double)data[7];
        circleCollider.enabled = false;
        boxCollider.enabled = true;
        StartCoroutine(ExplodeGrenade());
    }

    private IEnumerator ExplodeGrenade()
    {
        yield return new WaitForSeconds(1f);
        boxCollider.enabled = false;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezePosition;
        circleCollider.enabled = true;
        yield return new WaitForSeconds(0.5f);
        PhotonView PV = GetComponent<PhotonView>();
        PV.RPC("DestroyBullet", RpcTarget.AllBuffered);
    }

    [PunRPC]
    private void DestroyBullet()
    {
        dir = Vector2.zero;
        attackedPlayers.Clear();
        rb.velocity = Vector2.zero;
        boxCollider.enabled = false;
        circleCollider.enabled = false;
        rb.constraints = RigidbodyConstraints2D.None;
        gameObject.SetActive(false);

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
