using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using UnityEditor.Localization.Plugins.XLIFF.V20;
using System;
using System.Linq;


public class StatusModifier
{
    public enum ModifierType { Add, Multiply }
    public float Value { get; private set; }
    public ModifierType Type { get; private set; }
    public StatusEffectBase Source { get; private set; }
    public int Priority { get; private set; }

    public StatusModifier(float value, ModifierType type, StatusEffectBase source, int priority = 0)
    {
        Value = value;
        Type = type;
        Source = source;
        Priority = priority;
    }
}

[System.Serializable]
public class Status
{
    public EStatusType Type { get; private set; }
    public float BaseValue;
    private List<StatusModifier> modifiers = new List<StatusModifier>();
    public IReadOnlyList<StatusModifier> Modifiers => modifiers;
    private float cachedValue;
    private bool isDirty = true;

    public float MinValue { get; set; }
    public float MaxValue { get; set; }

    public UnityAction<float> OnValueChanged;

    public Status(EStatusType type, float baseValue = 0.0f, float minValue = 0.0f, float maxValue = 100.0f)
    {
        Type = type;
        
        MinValue = minValue;
        MaxValue = maxValue;    
        SetBaseValue(baseValue);
    }
        
    public void SetBaseValue(float value)
    {
        BaseValue = ClampValue(value);
        isDirty = true;
    }

    public void Reset()
    {
        modifiers.Clear();
        isDirty = true;
        OnValueChanged.Invoke(GetValue());
    }

    public void AddModifier(StatusModifier modifier)
    {
        modifiers.Add(modifier);
        isDirty = true;
    }

    public void RemoveModifier(StatusModifier modifier)
    {
        modifiers.Remove(modifier);
        isDirty = true;
    }
    public float GetValue()
    {
        if (isDirty)
        {
            RecalculateValue();
        }
        return cachedValue;
    }
    private void RecalculateValue()
    {
        float total = BaseValue;
        float multiplier = 1f;

        // Sort modifiers by priority (descending)
        modifiers.Sort((a, b) => b.Priority.CompareTo(a.Priority));

        foreach (var modifier in modifiers)
        {
            if (modifier.Type == StatusModifier.ModifierType.Add)
            {
                total += modifier.Value;
            }
            else if (modifier.Type == StatusModifier.ModifierType.Multiply)
            {
                multiplier *= modifier.Value;
            }
        }

        cachedValue = ClampValue(total * multiplier);
        isDirty = false;
        OnValueChanged?.Invoke(cachedValue);
    }
    private float ClampValue(float value)
    {
        return Math.Clamp(value, MinValue, MaxValue);
    }

    public string GetCalculationLog()
    {
        List<string> log = new List<string> { $"Base: {BaseValue}" };
        foreach (var modifier in modifiers)
        {
            string type = modifier.Type == StatusModifier.ModifierType.Add ? "Add" : "Multiply";
            log.Add($"{type} {modifier.Value} ({modifier.Source.StatusEffectInfo.id}, Priority {modifier.Priority})");
        }
        return string.Join(" -> ", log) + $" = {GetValue()}";
    }
}



[System.Serializable]
public class StatusBundle
{
    private Dictionary<EStatusType, Status> statuses = new Dictionary<EStatusType, Status>();
    public Dictionary<EBuffType, Status> Resistances = new Dictionary<EBuffType, Status>();

    public StatusBundle(Dictionary<EStatusType, List<float>> initialValues)
    {
        foreach (var pair in initialValues)
        {
            List<float> values = pair.Value;
            if (values.Count <= 1)
            {
                values.Add(0);
            }
            if (values.Count <= 2)
            {
                values.Add(100);
            }
            statuses[pair.Key] = new Status(pair.Key, values[0], values[1], values[2]);
        }
    }

    public Status GetStatus(EStatusType type)
    {
        if (statuses.TryGetValue(type, out var status))
        {
            return status;
        }

        throw new ArgumentException($"Status of type {type} does not exist.");
    }

    public void Clear()
    {
        foreach (var status in statuses.Values)
        {
            status.Reset();
        }
    }

    public bool HasStatus(EStatusType type)
    {
        return statuses.ContainsKey(type);
    }


    /// <summary>
    /// 만약 Status가 없으면 0을 반환
    /// </summary>
    public float GetStatusValue(EStatusType type)
    {
        if (statuses.TryGetValue(type, out var status))
        {
            return status.GetValue();
        }
        return 0;
    }

    public void AddStatus(EStatusType type, float baseValue, float minValue, float maxValue)
    {
        if (!statuses.ContainsKey(type))
        {
            statuses[type] = new Status(type, baseValue, minValue, maxValue);
        }
    }

    public void RemoveStatus(EStatusType type)
    {
        if (statuses.ContainsKey(type))
        {
            statuses.Remove(type);
        }
    }
}


public class StatusComponent : MonoBehaviour
{
    public SpecBundle SpecBundle { get; private set; }
    public StatusBundle StatusBundle { get; private set; }
    private List<StatusEffectBase> activeEffects = new List<StatusEffectBase>();
    public IReadOnlyList<StatusEffectBase> ActiveEffects => activeEffects;

    protected CharacterBase ownerCharacter;
    public CharacterBase OwnerCharacter { get { return ownerCharacter; } }

    private bool dead = false;

    public UnityAction<CharacterBase> CharacterChanged;
    public UnityAction<EStatusType, Status> OnStatusChanged;

    public UnityAction<StatusEffectBase, bool> onStatusEffectAdded;
    public UnityAction<StatusEffectBase> onStatusEffectRemoved;
    
    private Dictionary<int, StatusEffectBase> activatedStatusEffects = new Dictionary<int, StatusEffectBase>();

    private void Awake()
    {
        // Awake에서 하는 것이기에 매우 기본적인 것. 대부분 덮어씌워져야 함
        var initialValues = new Dictionary<EStatusType, List<float>>
        {
            { EStatusType.MaxHealth, new List<float>{100f} },
            { EStatusType.Health, new List<float>{100f} },
            { EStatusType.Vitality, new List<float>{10f} },
            { EStatusType.Defense, new List<float>{10f} },

            { EStatusType.Damage, new List<float>{5f} },
            { EStatusType.AttackCooldownTime, new List<float>{5f} },

            { EStatusType.InteractSpeed, new List<float>{5f} },
            { EStatusType.AttackRapidCooldownTime, new List<float>{5f} },

            { EStatusType.Speed, new List<float>{5f} },
            { EStatusType.JumpHeight, new List<float>{5f} },
            { EStatusType.Evasion, new List<float>{5f} },

            { EStatusType.CriticalProbability, new List<float>{5f} },
            { EStatusType.CriticalRatio, new List<float>{5f} },
            { EStatusType.LearningAbility, new List<float>{5f} },

            { EStatusType.PerceptionFocusRange, new List<float>{5f} },
            { EStatusType.PerceptionCircleRange, new List<float>{5f} },
            { EStatusType.PerceptionAngle, new List<float>{5f} },
        };

        StatusBundle = new StatusBundle(initialValues);

        foreach (var type in initialValues.Keys)
        {
            Status status = StatusBundle.GetStatus(type);
            StatusBundle.GetStatus(type).OnValueChanged += (value => OnStatusChanged.Invoke(type, status));
        }
        OnStatusChanged += HandleStatusChanged;
    }

    public void InitializeStatusComponent(CharacterBase characterBase)
    {
        // Status 초기화
        ResetStatus();

        // 캐릭터 관련
        ownerCharacter = characterBase;
        if (CharacterChanged != null)
        {
            CharacterChanged.Invoke(ownerCharacter);
        }

        Dictionary<ESpecType, int> specDict = new Dictionary<ESpecType, int>();
        Character character = characterBase as Character;
        if (character != null)
        {
            for (int i = 0; i < character.CharacterInfo.SpecInfos.Count; i++)
            {
                specDict.Add(character.CharacterInfo.SpecInfos[i].specType, character.CharacterInfo.SpecInfos[i].level);
            }
        }

        // 스펙은 기본적으로 다 지니고있다고 보기에, 만약 SpecType이 존재하지 않으면 채워준다.
        for (ESpecType specType = ESpecType.HEALTH; specType < ESpecType.MAX; specType++)
        {
            if(specDict.ContainsKey(specType) == false)
            {
                specDict.Add(specType, 1);
            }
        }

        // 스펙
        SpecBundle = new SpecBundle(specDict);
        SpecToStatusLinker.InitializeLinks(SpecBundle, StatusBundle);
    }

    public void SetPersistentStatuses(CharacterInfo characterInfo)
    { 
    }

    public void ResetStatus()
    {
        dead = false;
        activeEffects.Clear();
        StatusBundle.Clear();
    }

    public void AddStatusEffect(StatusEffectBase effect)
    {
        var existingEffect = activeEffects.Find(e => e.StatusEffectInfo.id == effect.StatusEffectInfo.id);
        
        bool canStack = false;
        if (existingEffect != null)
        {
            if (existingEffect.IsStackable())
            {
                canStack = true;
            }
            else
            {
                // Update the duration if not stackable
                StartEffect(existingEffect, false);
            }
        }
        else
        {
            canStack = true;
        }
        if(canStack)
        {
            activeEffects.Add(effect);
            StartEffect(effect, true);
        }
        if(onStatusEffectAdded != null)
        {
            onStatusEffectAdded.Invoke(effect, canStack);
        }
    }

    private void StartEffect(StatusEffectBase effect, bool isNew)
    {
        if (isNew)
        {
            effect.EffectFinished += OnEffectFinished;
            if (effect.StatusEffectInfo.withdrawAtEnd)
            {
                activatedStatusEffects.Add(effect.SequenceId, effect);
            }
        }
        effect.Start();
    }
    
    private void OnEffectFinished(StatusEffectBase statusEffect)
    {
        if (activatedStatusEffects.ContainsKey(statusEffect.SequenceId))
        {
            activatedStatusEffects.Remove(statusEffect.SequenceId);
            statusEffect.EffectFinished -= OnEffectFinished;
        }
    }
    
    public void RemoveStatusEffect(StatusEffectBase effect)
    {
        if (activeEffects.Contains(effect))
        {
            effect.End();
            activeEffects.Remove(effect);
            if(onStatusEffectRemoved != null)
            {
                onStatusEffectRemoved.Invoke(effect);
            }
        }
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            StatusEffectBase effect = activeEffects[i];
            effect.Update(deltaTime);
            if (effect.IsEnded)
            {
                effect.End();
                activeEffects.RemoveAt(i);
                if (onStatusEffectRemoved != null)
                {
                    onStatusEffectRemoved.Invoke(effect);
                }
            }
        }
    }

    public void PrintStatusCalculationLog(EStatusType statusType)
    {
        string log = StatusBundle.GetStatus(statusType).GetCalculationLog();
        Debug.Log(log);
    }


    public bool HasStatusEffectOfId(string effectId)
    {
        List<StatusEffectBase> foundedEffects = activeEffects.FindAll(x => x.StatusEffectInfo.id == effectId);
        return foundedEffects != null && foundedEffects.Count > 0;
    }
    public bool HasStatusEffectOfType(EStatusEffectType effectType)
    {
        List<StatusEffectBase> foundedEffects = GetStatusEffectOfType(effectType);
        return foundedEffects != null && foundedEffects.Count > 0;
    }
    public List<StatusEffectBase> GetStatusEffectOfType(EStatusEffectType effectType)
    {
        return activeEffects.FindAll(x => x.StatusEffectInfo.EffectType == effectType);
    }


    public bool IsDead()
    {
        return dead || StatusBundle.GetStatus(EStatusType.Health).GetValue() <= 0;
    }


    private void HandleStatusChanged(EStatusType type, Status status)
    {

    }

    private void OnStatusEffectAdded(StatusEffectBase statusEffect)
    {
    }

    private void OnStatusEffectRemoved(StatusEffectBase statusEffect)
    {
    }

    // deperecated
    private void OnStatusEffectUpdated(StatusEffectBase statusEffect)
    {
    }
}
