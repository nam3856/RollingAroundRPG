using Cysharp.Threading.Tasks;
using Photon.Pun;
using System;
using UnityEngine;

public class basicAttack : Skill
{
    private Warrior warrior;
    public basicAttack() : 
        base("기본 검술", "검을 휘둘러 전방의 적에게 데미지를 줍니다.", 0,null,0,0.4f) 
    {
        icon = Resources.Load<Sprite>("Icons/Warrior_Skill1");
    }
    protected override void ExecuteSkill(Character character)
    {
        if (character is Warrior)
        {
            warrior = character as Warrior;
        }
        warrior.ResetAttackState(0.3f).Forget();
        warrior.PlayRandomSwordSound();

        int damage = (int)Math.Ceiling(warrior.attackDamage);
        Vector2 attackDirection = warrior.GetLastMoveDirection();
        if (Mathf.Abs(attackDirection.x) > Mathf.Abs(attackDirection.y))
        {
            attackDirection = new Vector2(Mathf.Sign(attackDirection.x), 0);
        }
        else
        {
            attackDirection = new Vector2(0, Mathf.Sign(attackDirection.y));
        }

        warrior.PV.RPC("StartAttackingMotion", RpcTarget.All, attackDirection, 0);
        PhotonNetwork.Instantiate("FanBullet", warrior.transform.position, Quaternion.identity)
            .GetComponent<PhotonView>().RPC("DirRPC", RpcTarget.All, attackDirection, damage, warrior.PV.ViewID, critical);
    }


}