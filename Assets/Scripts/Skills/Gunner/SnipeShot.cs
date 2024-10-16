using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class SnipeShot : Skill
{

    public int damage;
    public SnipeShot(List<Skill> prerequisites) : base("����", "���� ���¿��� ������ �ѹ��� ���� ū �������� ������ ���� ������ŵ�ϴ�.", 9, prerequisites, 2, 30f) 
    {
        icon = Resources.Load<Sprite>("Icons/Gunner_Skill4");
    }

    protected override void ExecuteSkill(Character character)
    {
        character.SetIsAttacking(true);
        character.RB.velocity = Vector2.zero;

        damage = (int)Math.Ceiling(character.attackDamage * 10);
        character.RB.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
        if (character.TryGetComponent(out SnipeShotSkill snipeShotSkill))
        {
            snipeShotSkill.SetSnipeShotSkill(this);

            snipeShotSkill.ActiveSnipeScopeAsync().Forget(e => Debug.LogException(e));
        }

    }

}
