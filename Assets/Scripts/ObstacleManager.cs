using Photon.Pun;
using System;
using UnityEngine;

public class ObstacleManager : MonoBehaviourPun
{
    public static event Action OnObstacleDestroyed;
    public AudioSource audio;
    [PunRPC]
    void DestroyRPC() {
        audio.Play();
        gameObject.SetActive(false);
        OnObstacleDestroyed?.Invoke();
    } 
}

