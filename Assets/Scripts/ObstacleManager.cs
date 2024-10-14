using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Unity.VisualScripting;
using System;

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

