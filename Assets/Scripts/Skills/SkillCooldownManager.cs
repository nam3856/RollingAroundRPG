using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillCooldownManager : MonoBehaviour
{
    // ��ų ��Ÿ�� �̺�Ʈ ����
    public event Action<string, float> OnSkillUsed;  // ��ų ��� �� �߻��ϴ� �̺�Ʈ (��ų �̸��� ��Ÿ�� ����)
    public event Action<string> OnSkillReady;  // ��ų ��Ÿ�� �Ϸ� �� �߻��ϴ� �̺�Ʈ

    public void UseSkill(Skill skill)
    {
        if (skill.IsAcquired && !skill.IsOnCooldown())
        {
            StartCoroutine(CooldownRoutine(skill));  // Coroutine���� ��Ÿ�� ����
        }
    }

    private IEnumerator CooldownRoutine(Skill skill)
    {
        float cooldown = skill.Cooldown;
        OnSkillUsed?.Invoke(skill.Name, cooldown);  // ��ų ��� �̺�Ʈ �߻�

        yield return new WaitForSeconds(cooldown);  // ��Ÿ�� ��ٸ�

        OnSkillReady?.Invoke(skill.Name);  // ��Ÿ�� �Ϸ� �̺�Ʈ �߻�
    }
}
