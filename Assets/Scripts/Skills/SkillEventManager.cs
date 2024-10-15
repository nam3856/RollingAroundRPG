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

    // ��ų�� ���õǾ��� ��
    public event Action<Skill> OnSkillSelected;
    public void SkillSelected(Skill skill)
    {
        OnSkillSelected?.Invoke(skill);
    }

    // ��ų�� �н��Ǿ��� ��
    public event Action<Skill> OnSkillLearned;
    public void SkillLearned(Skill skill)
    {
        OnSkillLearned?.Invoke(skill);
    }

    // ��ų�� �ʱ�ȭ�Ǿ��� ��
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

    // ��ų ��Ÿ�� �Ϸ� �� �߻��ϴ� �̺�Ʈ (ViewID, ��ų �̸�)
    public event Action<int, string> OnSkillReady;
    public void SkillReady(Skill skill, int viewID)
    {
        OnSkillReady?.Invoke(viewID, skill.Name);
    }
}
