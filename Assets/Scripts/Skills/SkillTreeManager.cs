using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillTreeManager : MonoBehaviour
{
    public List<CharacterClass> CharacterClasses { get; private set; }
    public int PlayerSkillPoints { get; private set; } = 15;

    private BulletPool bulletPool;  // BulletPool ����

    private void Start()
    {
        // BulletPool�� �����ϰ� ��ų�鿡 ����
        bulletPool = FindObjectOfType<BulletPool>();

        InitializeSkillTrees();
    }

    void InitializeSkillTrees()
    {
        // ���� ��ų �ʱ�ȭ
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
        var snipeShot = new Skill("���� ����", "������ �Ͽ� 1���� ������ �ſ� ū �������� �ݴϴ�.", 5, new List<Skill> { grenadeToss }, 4);
        var evasiveRoll = new Skill("ȸ�� ������", "�̵��������� ������ �����ϴ�.", 0);
        */

        var fireballSkill = new FireBall();
        var arcaneShieldSkill = new ArcaneShield(new List<Skill> { fireballSkill });
        var healingWaveSkill = new HealingWave(new List<Skill> { arcaneShieldSkill });
        var MeteorStrikeSkill = new Meteor(new List<Skill> { arcaneShieldSkill, healingWaveSkill });
        var teleportSkill = new TeleportSkill(new List<Skill> { fireballSkill });

        // ��ų Ʈ�� ����
        CharacterClasses = new List<CharacterClass>()
        {
            new CharacterClass("����", new List<Skill>
            {
                basicAttackSkill,
                comboAttackSkill,
                rushSkill,
                powerStrikeSkill,
                shieldBlockSkill
            }),
            new CharacterClass("�ų�", new List<Skill>
            {
                basicShotSkill,
                rapidFireSkill,
                grenadeTossSkill,
                snipeShotSkill,
                rollSkill
            }),
            new CharacterClass("������", new List<Skill>
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
                Debug.Log($"{skill.Name} ��ų�� ���� ���� {prerequisite.Name} ��ų�� ���� ����� �մϴ�.");
                return false;
            }
        }

        if (PlayerSkillPoints < skill.Point)
        {
            Debug.Log("��ų ����Ʈ�� �����մϴ�.");
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
            Debug.Log($"{skill.Name} ��ų�� ȹ���߽��ϴ�.");
        }
        
    }

    public bool UseSkill(Skill skill, Character character, bool isToggle = false)
    {
        return skill.UseSkill(character, isToggle);
    }
}
