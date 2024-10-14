using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillCooldownManager : MonoBehaviour
{
    public static SkillCooldownManager Instance { get; private set; }
    // 스킬 쿨타임 이벤트 정의
    public event Action<string, float> OnSkillUsed;  // 스킬 사용 시 발생하는 이벤트 (스킬 이름과 쿨타임 전달)
    public event Action<string> OnSkillReady;  // 스킬 쿨타임 완료 시 발생하는 이벤트

    public void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 필요에 따라 제거 가능
        }
    }
    public void UseSkill(Skill skill)
    {
        if (skill.IsAcquired && !skill.IsOnCooldown())
        {
            CooldownAsync(skill).Forget();
        }
    }

    /// <summary>
    /// 쿨타임이 다되면 완료이벤트를 보냅니다.
    /// </summary>
    /// <param name="skill">스킬 클래스</param>
    /// <returns></returns>
    private async UniTask CooldownAsync(Skill skill)
    {
        float cooldown = skill.Cooldown;
        OnSkillUsed?.Invoke(skill.Name, cooldown);  // 스킬 사용 이벤트 발생

        await UniTask.Delay(TimeSpan.FromSeconds(cooldown));

        OnSkillReady?.Invoke(skill.Name);  // 쿨타임 완료 이벤트 발생
    }
}
