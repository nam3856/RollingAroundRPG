using System.Collections.Generic;
using UnityEngine;

public abstract class Skill : MonoBehaviour 
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public int Cost { get; private set; }
    public List<Skill> Prerequisites { get; private set; }
    public bool IsAcquired { get; private set; }
    public int Point { get; private set; }
    public float Cooldown { get; private set; }
    private float lastUsedTime;
    private UIManager ui;
    private SkillCooldownManager cooldownManager;

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
        ui = FindObjectOfType<UIManager>();
        cooldownManager = FindObjectOfType<SkillCooldownManager>();
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
    public bool UseSkill(Character character)
    {
        if (IsAcquired && !IsOnCooldown() && IsEnoughMp(character))
        {
            cooldownManager.UseSkill(this);
            lastUsedTime = Time.time;
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

    protected abstract void ExecuteSkill(Character character);

    public void UpdateSkillUI(Character character)
    {
        if (ui == null)
        {
            ui = FindObjectOfType<UIManager>();
        }
        if (IsAcquired && IsEnoughMp(character))
        {
            ui.SetSkillIconToColor(Name);  // 배웠고 마나가 충분하면 컬러로 설정
        }
        else
        {
            ui.SetSkillIconToGrayscale(Name);  // 배우지 않았거나 마나가 충분하지 않으면 흑백으로 설정
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