using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAnimationOnce : MonoBehaviour
{
    public void OnAnimationEnd()
    {
        gameObject.SetActive(false);
    }
}