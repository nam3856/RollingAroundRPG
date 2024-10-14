using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

public class PlayerDetector : MonoBehaviourPunCallbacks
{
    public MonsterTargeting monsterTargeting;

    private void Start()
    {
        if (monsterTargeting == null)
        {
            monsterTargeting = GetComponentInParent<MonsterTargeting>();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (other.CompareTag("Player"))
        {
            monsterTargeting.AddPlayerInRange(other.transform);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (other.CompareTag("Player"))
        {
            monsterTargeting.RemovePlayerInRange(other.transform);
        }
    }
}
