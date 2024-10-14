using UnityEngine;
using Photon.Pun;
public class Portal : MonoBehaviourPunCallbacks
{
    public Transform destination;
    public bool isNetworkedPortal = false;
    public PolygonCollider2D confiner;
    private int confinerID;

    private void Start()
    {
        if (confiner != null)
        {
            ConfinerID confinerIDComponent = confiner.GetComponent<ConfinerID>();
            if (confinerIDComponent != null)
            {
                confinerID = confinerIDComponent.confinerID;
            }
            else
            {
                Debug.LogError("Confiner에 ConfinerID 스크립트가 없습니다.");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PhotonView playerPV = other.GetComponent<PhotonView>();
            if (playerPV != null && playerPV.IsMine)
            {
                if (isNetworkedPortal)
                {
                    playerPV.RPC("Teleport", RpcTarget.All, destination.position, confinerID);
                }
                else
                {
                    other.transform.position = destination.position;
                }
            }
        }
    }
}
