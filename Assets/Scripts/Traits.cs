using System.Collections.Generic;
using UnityEngine;

public class TraitRepository : MonoBehaviour
{
    // �̱��� �ν��Ͻ�
    public static TraitRepository Instance { get; private set; }

    // ��� Trait�� ������ ����Ʈ
    public List<Trait> allTraits { get; private set; } = new List<Trait>();

    // TraitRepository �ʱ�ȭ
    private void Awake()
    {
        // �̱��� ���� ����
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeTraits();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Trait �ʱ�ȭ �޼���
    private void InitializeTraits()
    {
        Sprite attackIcon = Resources.Load<Sprite>("Icons/Attack");
        Sprite speedIcon = Resources.Load<Sprite>("Icons/Speed");
        Sprite armorIcon = Resources.Load<Sprite>("Icons/Armor");
        // Trait �ν��Ͻ� ���� �� �߰�
        allTraits.Add(new IncreaseAttackTrait("���ݷ� ����", "���ݷ��� 1 ������ŵ�ϴ�.", attackIcon, 1, 5));
        allTraits.Add(new IncreaseSpeedTrait("�̵��ӵ� ����", "�̵��ӵ��� 10% ������ŵ�ϴ�.", speedIcon, 0, 5, 1.1f));
        allTraits.Add(new IncreaseArmorTrait("���� ����", "������ 10% ������ŵ�ϴ�.", armorIcon, 0, 5, 1.1f));
        allTraits.Add(new CriticalTrait("ġ��Ÿ Ȯ�� 20% ����", "ġ��Ÿ Ȯ���� 20% ������ŵ�ϴ�.", armorIcon, 0, 1));
    }

    // ��� Trait�� ��ȯ�ϴ� �޼���
    public List<Trait> GetAllTraits()
    {
        return new List<Trait>(allTraits);
    }
}


public abstract class Trait
{
    public int stackCount;
    public int maxStack;

    public virtual bool IsCompletelyLearned()
    {
        return maxStack<=stackCount;
    }
    public string TraitName { get; protected set; }
    public string Description { get; protected set; }
    public abstract void Apply(Character character);
    public abstract void Remove(Character character);

    public Sprite Icon { get; protected set; }
    public int Cost { get; protected set; }
    protected Character character;

    public Trait(string traitName, string description, Sprite icon, int cost,int stackCount = 0)
    {
        maxStack = stackCount;
        this.stackCount = 0;
        TraitName = traitName;
        Description = description;
        Icon = icon;
        Cost = cost;
    }
}
public class IncreaseAttackTrait : Trait
{
    public float AttackMultiplier { get; private set; }

    public IncreaseAttackTrait(string name, string description, Sprite icon, int cost, int stackCount)
        : base(name, description, icon, cost, stackCount)
    {
    }

    public override void Apply(Character character)
    {
        stackCount++;
        character.additionalAttackDamage++;
        character.attackDamage = character.basicAttackDamage + character.additionalAttackDamage;
    }

    public override void Remove(Character character)
    {
        character.additionalAttackDamage -= stackCount;
        character.attackDamage = character.basicAttackDamage + character.additionalAttackDamage;
        stackCount = 0;
    }
}

public class IncreaseSpeedTrait : Trait
{
    public float SpeedMultiPlier { get; private set; }
    public IncreaseSpeedTrait(string name, string description, Sprite icon, int cost, int stackCount, float multiplier)
        : base(name, description, icon, cost, stackCount)
    {
        SpeedMultiPlier = multiplier;
    }

    public override void Apply(Character character)
    {
        character.moveSpeed = character.moveSpeed * SpeedMultiPlier;
        stackCount++;
    }
    public override void Remove(Character character)
    {
        character.moveSpeed = character.moveSpeed / SpeedMultiPlier;
        stackCount--;
    }
}

public class IncreaseArmorTrait : Trait
{
    public float ArmorMultiPlier { get; private set; }

    public IncreaseArmorTrait(string name, string description, Sprite icon, int cost, int stackCount, float multiplier)
        : base(name, description, icon, cost, stackCount)
    {
        ArmorMultiPlier = multiplier;
    }
    public override void Apply(Character character)
    {
        if (stackCount == 0) character.armor = 1;
        else character.armor *= ArmorMultiPlier;
        stackCount++;
    }
    public override void Remove(Character character)
    {
        if (stackCount > 0) character.armor /= ArmorMultiPlier;
        else character.armor = 0;
        stackCount--;
    }
}

public class CriticalTrait : Trait
{

    public CriticalTrait(string name, string description, Sprite icon, int cost, int stackCount)
        : base(name, description, icon, cost, stackCount)
    {
        maxStack = 1;
    }
    public override void Apply(Character character)
    {
        stackCount++;
        character.criticalProbability = 0.2f;
    }
    public override void Remove(Character character)
    {
        stackCount--;
        character.criticalProbability = 0f;
    }
}