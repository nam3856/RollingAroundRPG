using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class shieldBlock : Skill
{
    private bool isBlocking;
    private Warrior warrior;
    public shieldBlock(List<Skill> prerequisites) : base("���� ����", "���и� ��� ���� ������ �����ϴ�. Ÿ�̹��� ���߸� ���� ������ �ڷ� �з����ϴ�.", 1, prerequisites, 2,3f) { }
    protected override void ExecuteSkill(Character character)
    {
        if (character is Warrior){
            warrior = character as Warrior;
            isBlocking = true;
            warrior.isBlocking = true;
            warrior.RB.velocity = Vector2.zero;
            warrior.RB.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
        }

        warrior.PV.RPC("StartBlockingAnimation", RpcTarget.All);
        // 1�� �� ��� �Ϸ� ó��
        warrior.StartCoroutine(EndBlock());
    }

    private IEnumerator EndBlock()
    {
        yield return new WaitForSeconds(1f); // 1�� ���� �и� ����

        isBlocking = false; // 1�� �� �Ϲ� ��� ���·� ��ȯ
    }

    [PunRPC]
    public void OnReceiveDamage(float damage, Vector2 attackDirection)
    {
        GameObject damageTextInstance = PhotonNetwork.Instantiate("DamageText", warrior.canvasTransform.position, Quaternion.identity);
            //Instantiate(warrior.damageTextPrefab, warrior.canvasTransform);
        DamageText damageTextScript = damageTextInstance.GetComponent<DamageText>();
        GameObject uiCanvas = GameObject.Find("Canvas");
        damageTextInstance.transform.SetParent(uiCanvas.transform, false);
        PhotonView damageTextPV = damageTextInstance.GetComponent<PhotonView>();
        int curHealth = warrior.GetCurrentHealth();
        if (isBlocking) // �и� ����
        {
            KnockbackEnemies(1f); // �� �˹� ó��
            damageTextPV.RPC("SetDamageText", RpcTarget.All, "Guard!");
            warrior.PV.RPC("ActivateGuardEffectObject", RpcTarget.All);
        }
        else if (warrior.isBlocking) // 1�� ���Ŀ��� ������ 1/3�� ����
        {
            float reducedDamage = damage / 3;
            damageTextPV.RPC("SetDamageText", RpcTarget.All, ((int)reducedDamage).ToString());
            warrior.SetCurrentHealth(curHealth - (int)reducedDamage);

            KnockbackEnemies(1f); // �˹�
        }
        else
        {
            damageTextPV.RPC("SetDamageText", RpcTarget.All, damage.ToString());
            warrior.SetCurrentHealth(curHealth - (int)damage); // ������� �ʴ� ��� �Ϲ� ������
        }
        
        
        warrior.PV.RPC("UpdateHealthBar", RpcTarget.All);
    }

    // ���� �˹��Ű�� ����
    private void KnockbackEnemies(float force)
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(warrior.transform.position, 1f, warrior.enemyLayer);
        foreach (Collider2D enemy in enemies)
        {
            PhotonView monsterPV = enemy.GetComponent<PhotonView>();
            
            if (monsterPV != null)
            {
                monsterPV.RPC("ApplyKnockback", RpcTarget.All, warrior.transform.position, force);
            }
        }
    }
}

