using System.Collections.Generic;
using UnityEngine;

public abstract class Skill
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public int Cost { get; private set; }
    public List<Skill> Prerequisites { get; private set; }
    public bool IsAcquired { get; private set; }
    public int Point { get; private set; }
    public float Cooldown { get; private set; }
    private float lastUsedTime;

    public void SetLastUsedTime(float time)
    {
        lastUsedTime = time;
    }
    public Skill(string name, string description, int cost, List<Skill> prerequisites = null, int point = 1, float cooldown = 0.5f)
    {
        Name = name;
        Description = description;
        Cost = cost;
        Point = point;
        Prerequisites = prerequisites ?? new List<Skill>();
        IsAcquired = false;
        Cooldown = cooldown;
        lastUsedTime = -cooldown;
    }
    public void Acquire(Character character)
    {
        IsAcquired = true;
        UpdateSkillUI(character);
    }

    public bool IsOnCooldown()
    {
        return Time.time - lastUsedTime < Cooldown;
    }

    public bool IsEnoughMp(Character character)
    {
        return Cost <= character.GetCurrentMP();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="character"></param>
    /// <param name="isToggle">����� ��ų�� ��� True</param>
    /// <returns>��ų ��� �Ϸ� ����</returns>
    public bool UseSkill(Character character, bool isToggle = false)
    {
        if (IsAcquired && !IsOnCooldown() && IsEnoughMp(character))
        {
            if (!isToggle)
            {
                SkillCooldownManager.Instance.UseSkill(this);
                lastUsedTime = Time.time;
            }
            character.AdjustCurrentMP(Cost);
            ExecuteSkill(character);
            UpdateSkillUI(character);
            return true;
        }
        else
        {
            return false;
        }
    }

    public void StartCoolDown(Character character)
    {
        SkillCooldownManager.Instance.UseSkill(this);
        lastUsedTime = Time.time;
        UpdateSkillUI(character);
    }

    protected abstract void ExecuteSkill(Character character);

    public void UpdateSkillUI(Character character)
    {
        if (IsAcquired && IsEnoughMp(character))
        {
            UIManager.Instance.SetSkillIconToColor(Name);  // ����� ������ ����ϸ� �÷��� ����
        }
        else
        {
            UIManager.Instance.SetSkillIconToGrayscale(Name);  // ����� �ʾҰų� ������ ������� ������ ������� ����
        }
    }

}

public class CharacterClass
{
    public string Name { get; private set; }
    public List<Skill> Skills { get; private set; }
    public CharacterClass(string name, List<Skill> skills)
    {
        Name = name;
        Skills = skills;
    }
}