using System;
using System.Collections.Generic;
using UnityEngine;



[System.Serializable]
public enum ESpecType
{
    HEALTH,         // ü��
    STRENGTH,       // �ٷ�
    DEXTERITY,       // ������
    AGILITY,        // ��ø��
    INTELLIGENCE,   // ����
    SENSE,          // ����(�þ�)
    MENTALITY,      // ���ŷ�

    MAX,
};

[System.Serializable]
public class SpecInfo
{
    public ESpecType specType;
    public int level;
    public int exp;

    public void AddSpecExperience(int _addExp)
    {
        exp += _addExp;
        if (exp >= GetMaxExp())
        {
            level += (int)(exp / GetMaxExp());
            exp = exp % GetMaxExp();
        }
    }

    public int GetMaxExp()
    {
        return 100;
    }
}
public class Spec
{
    public int BaseValue { get; private set; }
    private List<float> modifiers = new List<float>();
    public int MinValue { get; set; }
    public int MaxValue { get; set; } 

    public Spec(int baseValue = 0, int minValue =0, int maxValue =100)
    {
        SetBaseValue(baseValue);
        MinValue = minValue;
        MaxValue = maxValue;
    }
    public void Reset()
    {
        modifiers.Clear();
    }

    public void SetBaseValue(int value)
    {
        BaseValue = ClampValue(value);
    }

    public int GetValue()
    {
        float total = BaseValue;
        foreach (var modifier in modifiers)
        {
            total += modifier;
        }
        return ClampValue(Mathf.FloorToInt(total));
    }

    public void AddModifier(float modifier)
    {
        modifiers.Add(modifier);
    }

    public void RemoveModifier(float modifier)
    {
        modifiers.Remove(modifier);
    }

    private int ClampValue(int value)
    {
        return Math.Clamp(value, MinValue, MaxValue);
    }
}

public class SpecBundle
{
    private Dictionary<ESpecType, Spec> specs = new Dictionary<ESpecType, Spec>();

    public SpecBundle(Dictionary<ESpecType, int> initialValues)
    {
        foreach (var pair in initialValues)
        {
            specs[pair.Key] = new Spec(pair.Value);
        }
    }

    public Spec GetSpec(ESpecType type)
    {
        if (specs.TryGetValue(type, out var spec))
        {
            return spec;
        }
        throw new KeyNotFoundException($"Spec of type {type} not found.");
    }

    public void AddSpec(ESpecType type, int baseValue)
    {
        if (!specs.ContainsKey(type))
        {
            specs[type] = new Spec(baseValue);
        }
    }

    public void Reset()
    {
        foreach (var spec in specs.Values)
        {
            spec.Reset();
        }
    }
}

public static class SpecToStatusLinker
{
    // Links a Spec to a Status with a multiplier
    public static void LinkSpecToStatus(Spec spec, Status status, float multiplier)
    {
        if(status == null)
        {
            return;
        }
        float specValue = spec.GetValue();
        float addedValue = specValue * multiplier;

        // Add a modifier to the Status based on the Spec
        var specModifier = new StatusModifier(addedValue, StatusModifier.ModifierType.Add, null);
        status.AddModifier(specModifier);
    }

    // Example: Linking multiple Specs to their corresponding Statuses
    public static void InitializeLinks(SpecBundle specBundle, StatusBundle statusBundle)
    {
        LinkSpecToStatus(specBundle.GetSpec(ESpecType.HEALTH), statusBundle.GetStatus(EStatusType.MaxHealth), 10f);
        LinkSpecToStatus(specBundle.GetSpec(ESpecType.HEALTH), statusBundle.GetStatus(EStatusType.Vitality), 1f);
        LinkSpecToStatus(specBundle.GetSpec(ESpecType.HEALTH), statusBundle.GetStatus(EStatusType.Defense), 1f);
        // LinkSpecToStatus(specBundle.GetSpec(ESpecType.HEALTH), statusBundle.GetStatus(EStatusType.Health), 1f);


        LinkSpecToStatus(specBundle.GetSpec(ESpecType.STRENGTH), statusBundle.GetStatus(EStatusType.Damage), 5f);
        LinkSpecToStatus(specBundle.GetSpec(ESpecType.STRENGTH), statusBundle.GetStatus(EStatusType.AttackCooldownTime), -0.05f);


        LinkSpecToStatus(specBundle.GetSpec(ESpecType.DEXTERITY), statusBundle.GetStatus(EStatusType.InteractSpeed), 1f);
        LinkSpecToStatus(specBundle.GetSpec(ESpecType.DEXTERITY), statusBundle.GetStatus(EStatusType.AttackRapidCooldownTime), -0.05f);


        LinkSpecToStatus(specBundle.GetSpec(ESpecType.AGILITY), statusBundle.GetStatus(EStatusType.Speed), 0.1f);
        LinkSpecToStatus(specBundle.GetSpec(ESpecType.AGILITY), statusBundle.GetStatus(EStatusType.JumpHeight), 0.1f);
        LinkSpecToStatus(specBundle.GetSpec(ESpecType.AGILITY), statusBundle.GetStatus(EStatusType.Evasion), 1f);


        LinkSpecToStatus(specBundle.GetSpec(ESpecType.INTELLIGENCE), statusBundle.GetStatus(EStatusType.CriticalProbability), 0.05f);
        LinkSpecToStatus(specBundle.GetSpec(ESpecType.INTELLIGENCE), statusBundle.GetStatus(EStatusType.CriticalRatio), 0.02f);
        LinkSpecToStatus(specBundle.GetSpec(ESpecType.INTELLIGENCE), statusBundle.GetStatus(EStatusType.LearningAbility), 0.1f);


        LinkSpecToStatus(specBundle.GetSpec(ESpecType.SENSE), statusBundle.GetStatus(EStatusType.PerceptionFocusRange), 1f);
        LinkSpecToStatus(specBundle.GetSpec(ESpecType.SENSE), statusBundle.GetStatus(EStatusType.PerceptionCircleRange), 1f);
        LinkSpecToStatus(specBundle.GetSpec(ESpecType.SENSE), statusBundle.GetStatus(EStatusType.PerceptionAngle), 2f);


        float specValue = specBundle.GetSpec(ESpecType.MENTALITY).GetValue();
        statusBundle.Resistances.Add(EBuffType.Stun, new Status(EStatusType.StunResistance, specValue));
        statusBundle.Resistances.Add(EBuffType.Fear, new Status(EStatusType.FearResistance, specValue));
        statusBundle.Resistances.Add(EBuffType.Charm, new Status(EStatusType.CharmResistance, specValue));
        statusBundle.Resistances.Add(EBuffType.Insanity, new Status(EStatusType.InsanityResistance, specValue));
    }
}