using UnityEngine;
using Photon.Pun;
using Cysharp.Threading.Tasks;

public class MeteorMovement : MonoBehaviourPun
{
    public Transform target;
    public float speed = 5f;
    private int damage = 20;
    private int attackerViewID;
    private bool isMoving = false;
    private double critical;

    public void Initialize(Transform targetTransform, int attackerID, double critical)
    {
        target = targetTransform;
        attackerViewID = attackerID;
        this.critical = critical;
        // 이동 코루틴 시작
        if (photonView.IsMine)
        {
            MoveTowardsTargetAsync().Forget();
        }
    }

    private async UniTaskVoid MoveTowardsTargetAsync()
    {
        isMoving = true;

        while (isMoving)
        {
            if (target == null)
            {
                PhotonNetwork.Destroy(gameObject);
                return;
            }

            Vector3 direction = (target.position - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;

            await UniTask.Yield();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!photonView.IsMine) return;

        if (collision.CompareTag("Monster") && collision.transform == target)
        {
            
            PhotonView monsterPV = collision.GetComponent<PhotonView>();
            if (monsterPV != null)
            {
                double p = UnityEngine.Random.value;
                bool isCriticalHit = false;
                if (p <= critical)
                {
                    damage *= 2;
                    isCriticalHit = true;
                }

                monsterPV.RPC("TakeDamage", RpcTarget.All, damage, attackerViewID, isCriticalHit);
            }

            // 이동 중지 및 Meteor 파괴
            isMoving = false;
            PhotonNetwork.Destroy(gameObject);
        }
    }
}
