using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterTargeting : MonoBehaviourPunCallbacks
{
    private Transform target;
    private List<Transform> playersInRange = new List<Transform>();
    private Vector2 initialPosition;
    private Dictionary<Transform, Coroutine> removeCoroutines = new Dictionary<Transform, Coroutine>();
    private float targetLostDelay = 0.5f;
    private void Start()
    {
        initialPosition = transform.position;
        if (PhotonNetwork.IsMasterClient)
        {
            PlayerScript.OnPlayerDied += HandlePlayerDied;
            PlayerScript.OnPlayerRespawned += HandlePlayerRespawned;

        }
    }
    public bool IsInRange(Transform target)
    {
        return playersInRange.Contains(target);
    }
    private void OnDestroy()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PlayerScript.OnPlayerDied -= HandlePlayerDied;
            PlayerScript.OnPlayerRespawned -= HandlePlayerRespawned;
        }
    }

    public void RemovePlayerInRange(Transform playerTransform)
    {
        // �̹� �ڷ�ƾ�� ���� ���̸� ����
        if (removeCoroutines.ContainsKey(playerTransform))
        {
            return;
        }

        // Ÿ�� ���� �ڷ�ƾ ����
        Coroutine coroutine = StartCoroutine(RemovePlayerWithDelay(playerTransform));
        removeCoroutines.Add(playerTransform, coroutine);
    }

    private IEnumerator RemovePlayerWithDelay(Transform playerTransform)
    {
        yield return new WaitForSeconds(targetLostDelay);

        playersInRange.Remove(playerTransform);
        removeCoroutines.Remove(playerTransform);

        if (target == playerTransform)
        {
            SetNewTarget();
        }
    }

    public void AddPlayerInRange(Transform playerTransform)
    {
        // �÷��̾ �̹� ����Ʈ�� ������ �߰�
        if (!playersInRange.Contains(playerTransform))
        {
            playersInRange.Add(playerTransform);
        }

        // ���� ������ �ڷ�ƾ�� ������ ����
        if (removeCoroutines.ContainsKey(playerTransform))
        {
            StopCoroutine(removeCoroutines[playerTransform]);
            removeCoroutines.Remove(playerTransform);
        }

        if (target == null)
        {
            SetNewTarget();
        }
    }


    private void SetNewTarget()
    {
        if (playersInRange.Count > 0)
        {
            // ����� �÷��̾ �������� Ÿ�� ����
            target = playersInRange[Random.Range(0, playersInRange.Count)];
            PhotonView pv = GetComponent<PhotonView>();
            pv.RPC("SetTargetRPC", RpcTarget.AllBuffered, target.GetComponent<PhotonView>().ViewID);
        }
        else
        {
            target = null;
            PhotonView pv = GetComponent<PhotonView>();
            pv.RPC("SetTargetRPC", RpcTarget.AllBuffered, -1);
        }
    }
    private void HandlePlayerDied(PlayerScript player)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (playersInRange.Contains(player.transform))
        {
            playersInRange.Remove(player.transform);
            if (target == player.transform)
            {
                SetNewTarget();
            }
        }
    }

    private void HandlePlayerRespawned(PlayerScript player)
    {
        //�Ⱦ�
    }


    [PunRPC]
    public void SetTargetRPC(int targetViewID)
    {
        if (targetViewID != -1)
        {
            PhotonView targetPV = PhotonView.Find(targetViewID);
            if (targetPV != null)
            {
                target = targetPV.transform;
            }
            else
            {
                target = null;
            }
        }
        else
        {
            target = null;
        }

        GetComponent<MonsterMovement>().SetTarget(target);
    }
}
