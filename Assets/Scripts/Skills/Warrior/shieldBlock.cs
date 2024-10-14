using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class shieldBlock : Skill
{
    private bool isBlocking;
    private Warrior warrior;
    public shieldBlock(List<Skill> prerequisites) : base("방패 막기", "방패를 들어 적의 공격을 막습니다. 타이밍을 맞추면 주위 적들이 뒤로 밀려납니다.", 1, prerequisites, 2,3f) { }
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
        // 1초 후 방어 완료 처리
        warrior.StartCoroutine(EndBlock());
    }

    private IEnumerator EndBlock()
    {
        yield return new WaitForSeconds(1f); // 1초 동안 패링 상태

        isBlocking = false; // 1초 후 일반 블록 상태로 전환
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
        if (isBlocking) // 패링 상태
        {
            KnockbackEnemies(1f); // 적 넉백 처리
            damageTextPV.RPC("SetDamageText", RpcTarget.All, "Guard!");
            warrior.PV.RPC("ActivateGuardEffectObject", RpcTarget.All);
        }
        else if (warrior.isBlocking) // 1초 이후에는 데미지 1/3만 받음
        {
            float reducedDamage = damage / 3;
            damageTextPV.RPC("SetDamageText", RpcTarget.All, ((int)reducedDamage).ToString());
            warrior.SetCurrentHealth(curHealth - (int)reducedDamage);

            KnockbackEnemies(1f); // 넉백
        }
        else
        {
            damageTextPV.RPC("SetDamageText", RpcTarget.All, damage.ToString());
            warrior.SetCurrentHealth(curHealth - (int)damage); // 방어하지 않는 경우 일반 데미지
        }
        
        
        warrior.PV.RPC("UpdateHealthBar", RpcTarget.All);
    }

    // 적을 넉백시키는 로직
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

