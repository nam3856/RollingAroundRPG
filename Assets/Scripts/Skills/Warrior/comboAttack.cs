using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class comboAttack : Skill
{
    private Warrior warrior;
    public comboAttack(List<Skill> prerequisites) : base("��� �˼�", "���� �߰��� �ֵѷ� ������ ������ �� ū �������� �ݴϴ�.", 1, prerequisites, 2,0.4f) { }

    protected override void ExecuteSkill(Character character)
    {
        if (character == null) return;

        if (character is Warrior)
        {
            warrior = character as Warrior;
        }
        warrior.PlayRandomSwordSound();
        warrior.SetIsAttacking(true);
        warrior.SetLastAttackTime(Time.time);
        // ���� ���� ��������
        Vector2 attackDirection = warrior.GetLastMoveDirection();
        warrior.PV.RPC("StartAttackingMotion", RpcTarget.All, attackDirection, 1);
        // ��ä�� ���� ����
        GameObject bullet = PhotonNetwork.Instantiate("FanBullet", warrior.transform.position, Quaternion.identity);
        FanBulletScript bulletScript = bullet.GetComponent<FanBulletScript>();
        bullet.GetComponent<PhotonView>().RPC("DirRPC", RpcTarget.All, attackDirection, character.attackDamage * 2, character.PV.ViewID);


        warrior.StartCoroutine(warrior.ResetAttackState(0.3f));
    }

    
}
