using System;
using Unity.VisualScripting;
using UnityEngine;

public class SkillEventManager : MonoBehaviour
{
    public static SkillEventManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    // 스킬이 선택되었을 때
    public event Action<Skill> OnSkillSelected;
    public void SkillSelected(Skill skill)
    {
        OnSkillSelected?.Invoke(skill);
    }

    // 스킬이 학습되었을 때
    public event Action<Skill> OnSkillLearned;
    public void SkillLearned(Skill skill)
    {
        OnSkillLearned?.Invoke(skill);
    }

    // 스킬이 초기화되었을 때
    public event Action OnSkillsReset;
    public void SkillsReset()
    {
        OnSkillsReset?.Invoke();
    }

    public event Action<int, string, float> OnSkillUsed;
    public void SkillUsed(Skill skill, int viewID)
    {
        OnSkillUsed?.Invoke(viewID, skill.Name, skill.Cooldown);
    }

    // 스킬 쿨타임 완료 시 발생하는 이벤트 (ViewID, 스킬 이름)
    public event Action<int, string> OnSkillReady;
    public void SkillReady(Skill skill, int viewID)
    {
        OnSkillReady?.Invoke(viewID, skill.Name);
    }
}
