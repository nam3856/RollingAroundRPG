using Cysharp.Threading.Tasks;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class shieldBlock : Skill
{
    private bool isParrying;
    private Warrior warrior;
    private int parryingTime = 500;
    public shieldBlock(List<Skill> prerequisites) : 
        base("방패 막기", "방패를 들어 적의 공격을 막습니다. 타이밍을 맞추면 주위 적들이 뒤로 밀려납니다.", 1, prerequisites, 2,2f) 
    {
        icon = Resources.Load<Sprite>("Icons/Warrior_Skill5");
    }
    protected override void ExecuteSkill(Character character)
    {
        if (character is Warrior){
            warrior = character as Warrior;
            isParrying = true;
            warrior.SetIsBlocking(true);
            warrior.RB.velocity = Vector2.zero;
            warrior.RB.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
        }

        warrior.PV.RPC("StartBlockingAnimation", RpcTarget.All);
        // 1초 후 방어 완료 처리
        EndParryingAsync(parryingTime).Forget();
    }

    private async UniTask EndParryingAsync(int parryingTime)
    {
        await UniTask.Delay(parryingTime);
        isParrying = false;
    }

    [PunRPC]
    public void OnReceiveDamage(float damage, Vector2 attackDirection)
    {
        GameObject damageTextInstance = PhotonNetwork.Instantiate("DamageText", warrior.canvasTransform.position, Quaternion.identity);
        DamageText damageTextScript = damageTextInstance.GetComponent<DamageText>();
        GameObject uiCanvas = GameObject.Find("Canvas");
        damageTextInstance.transform.SetParent(uiCanvas.transform, false);
        PhotonView damageTextPV = damageTextInstance.GetComponent<PhotonView>();
        int curHealth = warrior.GetCurrentHealth();

        if (isParrying) // 패링 상태
        {
            KnockbackEnemies(0.6f); // 적 넉백 처리
            damageTextPV.RPC("SetDamageText", RpcTarget.All, "Guard!");
            warrior.PV.RPC("ActivateGuardEffectObject", RpcTarget.All);
        }
        else if (warrior.GetIsBlocking()) // 1초 이후에는 데미지 1/3만 받음
        {
            float reducedDamage = damage / 3;
            damageTextPV.RPC("SetDamageText", RpcTarget.All, ((int)reducedDamage).ToString());
            warrior.SetCurrentHealth(curHealth - (int)reducedDamage);

            KnockbackEnemies(0.3f); // 넉백
        }
        else
        {
            damageTextPV.RPC("SetDamageText", RpcTarget.All, damage.ToString());
            warrior.SetCurrentHealth(curHealth - (int)damage); // 방어하지 않는 경우 일반 데미지
        }
        
        warrior.PV.RPC("UpdateHealthBar", RpcTarget.All);
    }

    private void KnockbackEnemies(float force)
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(warrior.transform.position, 0.6f, warrior.enemyLayer);
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

