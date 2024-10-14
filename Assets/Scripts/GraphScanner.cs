using System.Collections;
using UnityEngine;
using Photon.Pun;
using Pathfinding;

public class GraphScanner : MonoBehaviourPunCallbacks
{
    private float scanDelay = 1.0f;
    private bool scanScheduled = false;

    public override void OnEnable()
    {
        base.OnEnable();
       ObstacleManager.OnObstacleDestroyed += ScheduleGraphScan;
    }

    public override void OnDisable()
    {
        ObstacleManager.OnObstacleDestroyed -= ScheduleGraphScan;
        base.OnDisable();
        
    }

    private void ScheduleGraphScan()
    {
        if (!scanScheduled)
        {
            scanScheduled = true; // ½ºÄµÀÌ ¿¹¾àµÊ
            StartCoroutine(ScanGraphAfterDelay(scanDelay));
        }
    }

    private IEnumerator ScanGraphAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("ScanGraphRPC", RpcTarget.All);
        }

        scanScheduled = false;
    }

    [PunRPC]
    public void ScanGraphRPC()
    {
        AstarPath.active.Scan();
        Debug.Log("Graph Scanned");
    }
}
