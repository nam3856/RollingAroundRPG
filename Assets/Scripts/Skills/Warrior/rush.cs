using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rush : Skill
{
    public static event Action OnRushStarted;
    public static event Action OnRushEnded;
    public rush(List<Skill> prerequisites) : base("돌진", "전방으로 돌진합니다.", 3, prerequisites, 4,7f) { }
    protected override void ExecuteSkill(Character character)
    {
        float rushSpeed = 3f;
        float rushDuration = 0.5f;
        Warrior warrior = character as Warrior;
        warrior.isRushing = true;

        OnRushStarted?.Invoke();
        Vector2 rushDirection = warrior.GetLastMoveDirection();

        if (rushDirection.x > 0) warrior.AN.SetTrigger("rush right");
        else if (rushDirection.x < 0) warrior.AN.SetTrigger("rush left");
        else if (rushDirection.y > 0) warrior.AN.SetTrigger("rush up");
        else if (rushDirection.y < 0) warrior.AN.SetTrigger("rush down");

        warrior.RB.velocity = rushDirection * rushSpeed;
        warrior.StartCoroutine(RollEndTrigger(rushDuration));
        Debug.Log("돌진");
    }

    private IEnumerator RollEndTrigger(float rushDuration)
    {
        yield return new WaitForSeconds(rushDuration);
        OnRushEnded?.Invoke();
    }
}
