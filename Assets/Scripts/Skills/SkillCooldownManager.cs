using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillCooldownManager : MonoBehaviour
{
    // 스킬 쿨타임 이벤트 정의
    public event Action<string, float> OnSkillUsed;  // 스킬 사용 시 발생하는 이벤트 (스킬 이름과 쿨타임 전달)
    public event Action<string> OnSkillReady;  // 스킬 쿨타임 완료 시 발생하는 이벤트

    public void UseSkill(Skill skill)
    {
        if (skill.IsAcquired && !skill.IsOnCooldown())
        {
            StartCoroutine(CooldownRoutine(skill));  // Coroutine으로 쿨타임 시작
        }
    }

    private IEnumerator CooldownRoutine(Skill skill)
    {
        float cooldown = skill.Cooldown;
        OnSkillUsed?.Invoke(skill.Name, cooldown);  // 스킬 사용 이벤트 발생

        yield return new WaitForSeconds(cooldown);  // 쿨타임 기다림

        OnSkillReady?.Invoke(skill.Name);  // 쿨타임 완료 이벤트 발생
    }
}
