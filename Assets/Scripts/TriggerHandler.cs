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
                Debug.LogError(parentGrenadeScript.shooterActorNumber + "�� ����ź�� ���� Ÿ��");
                // ������ ü�� ���� ó��
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
