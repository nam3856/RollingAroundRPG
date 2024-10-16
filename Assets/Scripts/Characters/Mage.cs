using Photon.Pun;
using UnityEngine;

public class Mage : Character
{
    public override void Start()
    {
        base.Start();
        maxHealth = 60;
        AttackCooldown = 2f;
        AttackDuration = 0.5f;
        attackDamage = 10;

        basicAttackDamage = attackDamage;
        for (int i = 0; i < 5; i++)
        {
            skills.Add(skillTreeManager.CharacterClasses[2].Skills[i]);
        }


        LoadCharacterData_FollowUp();
    }

    protected override int CalculateMaxHealth(int level)
    {
        return 60 + (level - 1) * 6;
    }

    protected override int CalculateMaxMP(int level)
    {
        return 100 + (level - 1) * 10;
    }

    public override void StartSkill(int skillIdx)
    {
        base.StartSkill(skillIdx);
    }

    public override void StartSpecialSkill(bool isDown)
    {
        if (isAttacking) return;
        skillTreeManager.UseSkill(skills[4], this);
    }

    [PunRPC]
    public override void StartAttackingMotion(Vector2 attackDirection, int motionNum)
    {
        magicTransform.gameObject.SetActive(true);
        magicAnimator.SetTrigger("Fireball");
        base.StartAttackingMotion(attackDirection, motionNum);
    }

    [PunRPC]
    public override void Respawn()
    {
        base.Respawn();
    }
}