using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class roll : Skill
{
    public static event Action OnRollStarted;
    public static event Action OnRollEnded;
    public roll(List<Skill> prerequisites) : base("구르기", "이동 방향으로 빠르게 구릅니다.", 0, prerequisites, 1, 3f)
    {

        icon = Resources.Load<Sprite>("Icons/Gunner_Skill5");
    }

    protected override void ExecuteSkill(Character character)
    {
        float rollSpeed = 2f;
        float rollDuration = 0.5f;
        Gunner gunner = character as Gunner;
        gunner.isRolling = true;
        OnRollStarted?.Invoke();

        Vector2 rollDirection = gunner.GetLastMoveDirection();

        if (rollDirection.x > 0) gunner.AN.SetTrigger("roll right");
        else if (rollDirection.x < 0) gunner.AN.SetTrigger("roll left");
        else if (rollDirection.y > 0) gunner.AN.SetTrigger("roll up");
        else if (rollDirection.y < 0) gunner.AN.SetTrigger("roll down");

        gunner.RB.velocity = rollDirection * rollSpeed;
        gunner.StartCoroutine(RollEndTrigger(rollDuration));


    }

    private IEnumerator RollEndTrigger(float rollDuration)
    {
        yield return new WaitForSeconds(rollDuration);
        OnRollEnded?.Invoke();
    }
}
