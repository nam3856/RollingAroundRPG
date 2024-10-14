using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

public class MonsterAttack : MonoBehaviourPunCallbacks
{
    public int damage = 10;
    public float attackCooldown = 1f;

    private List<PhotonView> playersInRange = new List<PhotonView>();
    private Dictionary<PhotonView, float> lastAttackTimes = new Dictionary<PhotonView, float>();

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PhotonView playerPV = other.GetComponent<PhotonView>();
            if (playerPV != null && !playersInRange.Contains(playerPV))
            {
                playersInRange.Add(playerPV);
                lastAttackTimes[playerPV] = 0f; // 마지막 공격 시간을 초기화
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PhotonView playerPV = other.GetComponent<PhotonView>();
            if (playerPV != null)
            {
                playersInRange.Remove(playerPV);
                lastAttackTimes.Remove(playerPV);
            }
        }
    }

    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        foreach (var playerPV in playersInRange)
        {
            if (playerPV != null && playerPV.Owner != null && playerPV.gameObject != null && playerPV.gameObject.activeInHierarchy)
            {
                float lastAttackTime;
                if (!lastAttackTimes.TryGetValue(playerPV, out lastAttackTime))
                {
                    lastAttackTime = 0f;
                }

                if (Time.time > lastAttackTime + attackCooldown)
                {
                    Vector2 attackDirection = (playerPV.transform.position - transform.position).normalized;
                    playerPV.RPC("ReceiveDamage", RpcTarget.All, Random.Range(damage-damage/3,damage+damage/3), attackDirection);
                    lastAttackTimes[playerPV] = Time.time;
                }
            }
        }
    }
}
