using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class SnipeShot : Skill
{
    public SnipeShot(List<Skill> prerequisites) : base("저격", "줌인 상태에서 강력한 한발을 날려 큰 데미지를 입히고 적을 기절시킵니다.", 9, prerequisites, 2, 30f) { }

    protected override void ExecuteSkill(Character character)
    {
        character.SetIsAttacking(true);
        character.RB.velocity = Vector2.zero;

        character.RB.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
        if (character.TryGetComponent(out SnipeShotSkill snipeShotSkill))
        {
            snipeShotSkill.SetSnipeShotSkill(this);

            snipeShotSkill.ActiveSnipeScopeAsync().Forget(e => Debug.LogException(e));
        }

    }

}
