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

        for (int i = 0; i < 5; i++)
        {
            skills.Add(skillTreeManager.CharacterClasses[2].Skills[i]);
        }
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
        if (attackDirection.x > 0) AN.SetTrigger("mage attack right");
        else if (attackDirection.x < 0) AN.SetTrigger("mage attack left");
        else if (attackDirection.y > 0) AN.SetTrigger("mage attack up");
        else if (attackDirection.y < 0) AN.SetTrigger("mage attack down");

    }

    [PunRPC]
    public override void Respawn()
    {
        base.Respawn();
        AN.SetTrigger("mage init");
    }
}