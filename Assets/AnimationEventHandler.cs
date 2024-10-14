using UnityEngine;

public class AnimationEventHandler : MonoBehaviour
{
    private Warrior warrior;

    private void Start()
    {
        warrior = GetComponent<Character>() as Warrior;
    }

    
}