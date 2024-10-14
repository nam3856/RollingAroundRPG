using System.Collections.Generic;
using UnityEngine;

public class SnipeShot : Skill
{
    public SnipeShot(List<Skill> prerequisites) : base("����", "���� ���¿��� ������ �ѹ��� ���� ū �������� ������ ���� ������ŵ�ϴ�.", 9, prerequisites, 2, 30f) { }

    protected override void ExecuteSkill(Character character)
    {
        character.SetIsAttacking(true);
        character.RB.velocity = Vector2.zero;

        character.RB.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
        if (character.TryGetComponent(out SnipeShotSkill snipeShotSkill))
        {
            character.StartCoroutine(snipeShotSkill.ActivateSnipeShot());
        }
    }
}
