using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillTreeManager : MonoBehaviour
{
    public List<CharacterClass> CharacterClasses { get; private set; }
    public int PlayerSkillPoints { get; private set; } = 15;

    private BulletPool bulletPool;  // BulletPool 참조

    private void Start()
    {
        // BulletPool을 생성하고 스킬들에 전달
        bulletPool = FindObjectOfType<BulletPool>();

        InitializeSkillTrees();
    }

    void InitializeSkillTrees()
    {
        // 전사 스킬 초기화
        var basicAttackSkill = new basicAttack();
        var comboAttackSkill = new comboAttack(new List<Skill> { basicAttackSkill });
        
        
        var rushSkill = new rush(new List<Skill> { comboAttackSkill });
        var powerStrikeSkill = new powerStrike(new List<Skill> { rushSkill });

        var shieldBlockSkill = new shieldBlock(new List<Skill> { comboAttackSkill });

        
        var basicShotSkill = new basicShot();
        var rapidFireSkill = new rapidFire(new List<Skill> { basicShotSkill });

        var grenadeTossSkill = new GrenadeToss(new List<Skill> { rapidFireSkill });
        var snipeShotSkill = new SnipeShot(new List<Skill> { grenadeTossSkill });

        var rollSkill = new roll(new List<Skill> { basicShotSkill });

        /*
        var snipeShot = new Skill("정밀 조준", "저격을 하여 1명의 적에게 매우 큰 데미지를 줍니다.", 5, new List<Skill> { grenadeToss }, 4);
        var evasiveRoll = new Skill("회피 구르기", "이동방향으로 빠르게 구릅니다.", 0);
        */

        var fireballSkill = new FireBall();
        var arcaneShieldSkill = new ArcaneShield(new List<Skill> { fireballSkill });
        var healingWaveSkill = new HealingWave(new List<Skill> { arcaneShieldSkill });
        var MeteorStrikeSkill = new Meteor(new List<Skill> { arcaneShieldSkill, healingWaveSkill });
        var teleportSkill = new TeleportSkill(new List<Skill> { fireballSkill });

        // 스킬 트리 설정
        CharacterClasses = new List<CharacterClass>()
        {
            new CharacterClass("전사", new List<Skill>
            {
                basicAttackSkill,
                comboAttackSkill,
                rushSkill,
                powerStrikeSkill,
                shieldBlockSkill
            }),
            new CharacterClass("거너", new List<Skill>
            {
                basicShotSkill,
                rapidFireSkill,
                grenadeTossSkill,
                snipeShotSkill,
                rollSkill
            }),
            new CharacterClass("마법사", new List<Skill>
            {
                fireballSkill,
                arcaneShieldSkill,
                healingWaveSkill,
                MeteorStrikeSkill,
                teleportSkill
            })
        };

    }
    public Skill GetSkillByName(string skillName)
    {
        foreach(var characterClass in CharacterClasses)
        {
            foreach(var skill in characterClass.Skills)
            {
                if(skill.Name == skillName)
                {
                    return skill;
                }
            }
        }
        return null;
    }
    public bool CanAcquireSkill(Skill skill)
    {
        foreach (var prerequisite in skill.Prerequisites)
        {
            if (!prerequisite.IsAcquired)
            {
                Debug.Log($"{skill.Name} 스킬을 배우기 위해 {prerequisite.Name} 스킬을 먼저 배워야 합니다.");
                return false;
            }
        }

        if (PlayerSkillPoints < skill.Point)
        {
            Debug.Log("스킬 포인트가 부족합니다.");
            return false;
        }

        return true;
    }

    public void AcquireSkill(Skill skill, Character character)
    {
        if (skill.IsAcquired) return;
        if (CanAcquireSkill(skill))
        {
            skill.Acquire(character);
            PlayerSkillPoints -= skill.Point;
            
            if(character != null && character.playerData != null) 
            {
                character.playerData.LearnedSkills.Add(skill.Name);
                SaveSystem.SavePlayerData(character.playerData);
            }
            Debug.Log($"{skill.Name} 스킬을 획득했습니다.");
        }
        
    }

    public bool UseSkill(Skill skill, Character character, bool isToggle = false)
    {
        return skill.UseSkill(character, isToggle);
    }
}
