using Photon.Pun;
using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

public class comboAttack : Skill
{
    private Warrior warrior;
    public comboAttack(List<Skill> prerequisites) : 
        base("��� �˼�", "���� �߰��� �ֵѷ� ������ ������ �� ū �������� �ݴϴ�.", 1, prerequisites, 2,0.4f) 
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
        // ���� ���� ��������
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
        // ��ä�� ���� ����
        GameObject bullet = PhotonNetwork.Instantiate("FanBullet", warrior.transform.position, Quaternion.identity);
        FanBulletScript bulletScript = bullet.GetComponent<FanBulletScript>();
        bullet.GetComponent<PhotonView>().RPC("DirRPC", RpcTarget.All, attackDirection, damage, character.PV.ViewID, critical);
    }

    
}
