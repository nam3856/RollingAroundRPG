using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class basicAttack : Skill
{
    private Warrior warrior;
    public basicAttack() : base("�⺻ �˼�", "���� �ֵѷ� ������ ������ �������� �ݴϴ�.", 0,null,0,0.4f) { }
    protected override void ExecuteSkill(Character character)
    {
        if (character is Warrior)
        {
            warrior = character as Warrior;
        }
        warrior.PlayRandomSwordSound();
        warrior.SetIsAttacking(true);
        warrior.SetLastAttackTime(Time.time);
        warrior.RB.velocity = Vector2.zero;

        Vector2 attackDirection = warrior.GetLastMoveDirection();

        warrior.PV.RPC("StartAttackingMotion", RpcTarget.All, attackDirection, 0);
        PhotonNetwork.Instantiate("FanBullet", warrior.transform.position, Quaternion.identity)
            .GetComponent<PhotonView>().RPC("DirRPC", RpcTarget.All, attackDirection, warrior.attackDamage, warrior.PV.ViewID);


        SetLastUsedTime(Time.time); // ��Ÿ�� ����

        warrior.ResetAttackState(0.3f, true).Forget();
    }


}