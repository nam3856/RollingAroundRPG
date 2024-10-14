using UnityEngine;
using Photon.Pun;
using Cysharp.Threading.Tasks;

public class MeteorMovement : MonoBehaviourPun
{
    public Transform target;
    public float speed = 5f;
    private int damage = 999;
    private int attackerViewID;
    private bool isMoving = false;

    public void Initialize(Transform targetTransform, int attackerID)
    {
        target = targetTransform;
        attackerViewID = attackerID;

        // �̵� �ڷ�ƾ ����
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
            // ������ ����
            PhotonView monsterPV = collision.GetComponent<PhotonView>();
            if (monsterPV != null)
            {
                monsterPV.RPC("TakeDamage", RpcTarget.All, damage, attackerViewID);
            }

            // �̵� ���� �� Meteor �ı�
            isMoving = false;
            PhotonNetwork.Destroy(gameObject);
        }
    }
}
