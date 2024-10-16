using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillTreeManager : MonoBehaviour
{
    public List<CharacterClass> CharacterClasses { get; private set; }
    public int PlayerSkillPoints { get; private set; } = 0;


    private void Start()
    {
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
        Debug.Log(skill);
        foreach (var prerequisite in skill.Prerequisites)
        {
            Debug.Log($"요구 스킬 {prerequisite}");
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

    public bool AcquireSkill(Skill skill, Character character)
    {
        if (skill.IsAcquired) return false;
        if (CanAcquireSkill(skill))
        {
            if (character.PV.IsMine)
            {
                PlayerSkillPoints -= skill.Point;
                skill.IsAcquired = true;
                if (character != null && character.playerData != null)
                {
                    character.playerData.LearnedSkills.Add(skill.Name);
                    character.playerData.SkillPoint = PlayerSkillPoints;
                    SaveSystem.SavePlayerData(character.playerData);
                }
                Debug.Log($"{skill.Name} 스킬을 획득했습니다.");
                if (SkillEventManager.Instance != null)
                {
                    SkillEventManager.Instance.SkillLearned(skill);
                }
            }
            else
            {
                Debug.LogError($"Character.PV.IsMine이 False입니다. character: {character.PV.ViewID} {character.PV.name}");
            }

            return true;
        }
        return false;
    }


    public bool UseSkill(Skill skill, Character character, bool isToggle = false)
    {
        return skill.UseSkill(character, isToggle);
    }

    public CharacterClass PlayerClass;
    public void ResetSkills(Character character)
    {
        if (PlayerClass.Skills.Count > 0)
        {
            // 맨 처음 스킬만 남기고 나머지 스킬 초기화
            for (int i = 1; i < PlayerClass.Skills.Count; i++)
            {
                if(PlayerClass.Skills[i].IsAcquired)
                {
                    PlayerClass.Skills[i].IsAcquired = false;
                    PlayerSkillPoints += PlayerClass.Skills[i].Point;
                    character.playerData.LearnedSkills.Remove(PlayerClass.Skills[i].Name);
                }
            }
            character.playerData.SkillPoint = PlayerSkillPoints;
            SaveSystem.SavePlayerData(character.playerData);
            Debug.Log("모든 스킬이 초기화되었습니다.");
            if (SkillEventManager.Instance != null)
            {
                SkillEventManager.Instance.SkillsReset();
            }
        }
    }

    public void AddSkillPoint(int num)
    {
        PlayerSkillPoints += num;

    }

    public void SetPlayerClass(CharacterClass characterClass)
    {
        PlayerClass = characterClass;
    }
}
