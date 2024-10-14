using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private SkillTreeManager skillTreeManager;
    // Start is called before the first frame update
    void Start()
    {
        skillTreeManager = FindObjectOfType<SkillTreeManager>();
        //skillTreeManager.AcquireSkill(new basicAttack());
        //skillTreeManager.AcquireSkill(new comboAttack());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
