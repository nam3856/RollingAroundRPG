using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillCooldownManager : MonoBehaviour
{
    public static SkillCooldownManager Instance { get; private set; }
    // ��ų ��Ÿ�� �̺�Ʈ ����
    public event Action<string, float> OnSkillUsed;  // ��ų ��� �� �߻��ϴ� �̺�Ʈ (��ų �̸��� ��Ÿ�� ����)
    public event Action<string> OnSkillReady;  // ��ų ��Ÿ�� �Ϸ� �� �߻��ϴ� �̺�Ʈ

    public void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // �ʿ信 ���� ���� ����
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
    /// ��Ÿ���� �ٵǸ� �Ϸ��̺�Ʈ�� �����ϴ�.
    /// </summary>
    /// <param name="skill">��ų Ŭ����</param>
    /// <returns></returns>
    private async UniTask CooldownAsync(Skill skill)
    {
        float cooldown = skill.Cooldown;
        OnSkillUsed?.Invoke(skill.Name, cooldown);  // ��ų ��� �̺�Ʈ �߻�

        await UniTask.Delay(TimeSpan.FromSeconds(cooldown));

        OnSkillReady?.Invoke(skill.Name);  // ��Ÿ�� �Ϸ� �̺�Ʈ �߻�
    }
}
