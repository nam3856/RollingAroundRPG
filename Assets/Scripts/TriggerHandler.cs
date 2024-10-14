using Photon.Pun;
using UnityEngine;

public class GrenadeChildScript : MonoBehaviour
{
    private GrenadeScript parentGrenadeScript;

    private void Awake()
    {
        parentGrenadeScript = GetComponentInParent<GrenadeScript>();
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        PhotonView targetPhotonView = col.GetComponent<PhotonView>();

        if (targetPhotonView == null || parentGrenadeScript.attackedPlayers.Contains(targetPhotonView.ViewID))
        {
            return;
        }

        if (col.CompareTag("Monster"))
        {
            parentGrenadeScript.attackedPlayers.Add(targetPhotonView.ViewID);
            if (targetPhotonView != null)
            {
                Debug.LogError(parentGrenadeScript.shooterActorNumber + "의 수류탄에 몬스터 타격");
                // 몬스터의 체력 감소 처리
                targetPhotonView.RPC("TakeDamage", RpcTarget.AllBuffered, parentGrenadeScript.damage, parentGrenadeScript.attackerViewId);
            }
        }
        if (col.CompareTag("Removable Obstacle"))
        {
            PhotonView obstaclePV = col.GetComponent<PhotonView>();

            if (obstaclePV != null)
            {
                Vector2 directionToPlayer = (col.transform.position - transform.position).normalized;
                obstaclePV.RPC("DestroyRPC", RpcTarget.AllBuffered);
            }
        }
    }
}
