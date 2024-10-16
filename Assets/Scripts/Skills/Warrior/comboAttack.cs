using Photon.Pun;
using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

public class comboAttack : Skill
{
    private Warrior warrior;
    public comboAttack(List<Skill> prerequisites) : 
        base("고급 검술", "검을 추가로 휘둘러 전방의 적에게 더 큰 데미지를 줍니다.", 1, prerequisites, 2,0.4f) 
    {
        icon = Resources.Load<Sprite>("Icons/Warrior_Skill2");
    }

    protected override void ExecuteSkill(Character character)
    {
        if (character == null) return;

        if (character is Warrior)
        {
            warrior = character as Warrior;
        }
        int damage = (int)Math.Ceiling(warrior.attackDamage * 2);
        warrior.ResetAttackState(0.3f).Forget();
        warrior.PlayRandomSwordSound();
        // 공격 방향 가져오기
        Vector2 attackDirection = warrior.GetLastMoveDirection();
        if (Mathf.Abs(attackDirection.x) > Mathf.Abs(attackDirection.y))
        {
            attackDirection = new Vector2(Mathf.Sign(attackDirection.x), 0);
        }
        else
        {
            attackDirection = new Vector2(0, Mathf.Sign(attackDirection.y));
        }

        warrior.PV.RPC("StartAttackingMotion", RpcTarget.All, attackDirection, 1);
        // 부채꼴 공격 생성
        GameObject bullet = PhotonNetwork.Instantiate("FanBullet", warrior.transform.position, Quaternion.identity);
        FanBulletScript bulletScript = bullet.GetComponent<FanBulletScript>();
        bullet.GetComponent<PhotonView>().RPC("DirRPC", RpcTarget.All, attackDirection, damage, character.PV.ViewID, critical);
    }

    
}
