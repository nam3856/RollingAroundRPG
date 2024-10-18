using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public class MonsterAttack : MonoBehaviourPunCallbacks
{
    public int damage = 10;

    private List<PhotonView> playersInRange = new List<PhotonView>();
    private Dictionary<PhotonView, float> lastAttackTimes = new Dictionary<PhotonView, float>();
    private PhotonView PV;
    MonsterBase health;
    public float attackCooldown = 1.0f;

    void Start()
    {
        PV = GetComponentInParent<PhotonView>();
        health = GetComponentInParent<MonsterBase>();
        PlayerScript.OnPlayerDied += HandlePlayerDied;
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PhotonView playerPV = other.GetComponent<PhotonView>();
            if (playerPV != null && !playersInRange.Contains(playerPV))
            {
                playersInRange.Add(playerPV);
                if (!lastAttackTimes.ContainsKey(playerPV))
                {
                    lastAttackTimes[playerPV] = Time.time - attackCooldown;
                }
                Attack(playerPV).Forget();
            }
        }
    }
    private void OnDestroy()
    {
        // OnPlayerDied 이벤트에서 구독 해제
        PlayerScript.OnPlayerDied -= HandlePlayerDied;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PhotonView playerPV = other.GetComponent<PhotonView>();
            if (playerPV != null)
            {
                playersInRange.Remove(playerPV);
            }
        }
    }

    private void HandlePlayerDied(PlayerScript player)
    {
        PhotonView playerPV = player.GetComponent<PhotonView>();
        if (playerPV != null && playersInRange.Contains(playerPV))
        {
            playersInRange.Remove(playerPV);
        }
    }

    private async UniTaskVoid Attack(PhotonView playerPV)
    {
        while (!health.isDead && playersInRange.Contains(playerPV))
        {
            if (Time.time >= lastAttackTimes[playerPV] + attackCooldown)
            {
                PV.RPC("TriggerAttackAnimation", RpcTarget.All);

                if (playerPV != null && playerPV.Owner != null && playerPV.gameObject != null && playerPV.gameObject.activeInHierarchy)
                {
                    Vector2 attackDirection = (playerPV.transform.position - transform.position).normalized;
                    playerPV.RPC("ReceiveDamage", RpcTarget.All, Random.Range(damage - damage / 3, damage + damage / 3), attackDirection);
                    lastAttackTimes[playerPV] = Time.time;
                }

                await UniTask.Delay((int)(attackCooldown * 1000));
            }
            else
            {
                await UniTask.Yield();
            }
        }
    }

}
