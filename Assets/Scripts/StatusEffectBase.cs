using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public enum EEffectTargetType
{
    None = 0,
    Mine = 1,
    Ally = 2,
    Enemy = 3,
    Destructible = 4,
    Interactable = 5,
};


[System.Serializable]
public enum EBuffType
{
    Stun = 0,
    Fear = 1,
    Charm = 2,
    Insanity = 3,
    Slow = 4,
    Knockback = 5,
    Silence = 6,
    Blind = 7,
    Poison = 8,
    Burn = 9,
    Freeze = 10,
    Weak = 11,
};

[System.Serializable]
public enum EStatusType
{
    None = 0,
    Health = 1,
    MaxHealth = 2,
    Speed = 3,
    JumpHeight = 4,
    Damage = 5,
    Defense = 6,
    AttackRapid = 7,
    AttackCooldownTime = 8,
    InteractSpeed = 9,
    LearningAbility = 10,
    CriticalProbability = 13,
    CriticalRatio = 14,
    Vitality = 15,
    Evasion = 16,
    // Perception
    PerceptionCircleRange = 101,
    PerceptionAngle = 102,
    PerceptionFocusRange = 103,
    // PerceptionValue         = 104,
    // Perception              = 105,
    RecognizeCutLine = 106,
    SuspicionCutLine = 107,
    PathCalcCooldownTime = 108,
    StimulusSensitivity = 109,
    // Weapon
    StartAttackCost = 301,
    UpdateAttackCost = 302,
    WeaponDamage = 303,
    // AttackRange             = 304,
    LeastAttackRange = 305,
    AttackRapidCooldownTime = 306,
    ReloadCooldownTime = 307,
    MaxAmmo = 308,
    ReloadAmmoAtOnce = 309,
    CancelReloadWhenAttack = 310,
    AngleDeltaSize = 311,
    KnockbackForce = 312,
    // Shield
    ShieldHealth = 401,
    ShieldMaxHealth = 402,
    MaintenanceTime = 403,
    GuardAngle = 404,
    BlockPenetrateForce = 405,
    GuardKnockbackForce = 406,
    GuardKnockbackForceRatio = 407,
    // Skill
    SkillRange = 501,
    SkillDamage = 502,
    SkillKnockbackForce = 503,
    SkillHealAmount = 504,
    SkillAttackStimulus = 505,
    SkillSpawnCount = 506,
    SkillSpawnSpacingValue = 507,
    // Resistance
    StunResistance = 601,
    FearResistance = 602,
    CharmResistance = 603,
    InsanityResistance = 604,
    SlowResistantoughnessce = 605,
    KnockbackReshardinessistance = 606,
    SilenceResistance = 607,
    BlindResistance = 608,
    PoisonResistance = 609,
    BurnResistance = 610,
    FreezeResistance = 611,
    WeakResistance = 612,

    // Situation
    Sanctioned = 800,
    Cover = 801,


    // Action
    UseSkill = 1000,
};


public interface IStatusEffectSource
{
    string Name { get; }
    GameObject GameObject { get; }
}

public enum EStatusEffectType
{
    UniqueStatusEffect,
    MultiStatusEffect,
    DamageOverTime,
    MaxHealthBuff,
    SimpleDamage,
}

/// <summary>
/// id,type,icon,statusEffectPrefab,lifetime,period,withdrawAtEnd,stackable,show,effectParams
/// HealthBoost, Multiple, Icons/health, Prefabs/HealthEffect,10,0,true,true,true, Health:50; Speed: 1.5
/// </summary>
[System.Serializable]
public class ConstStatusEffectInfo
{
    public string id;
    public EStatusEffectType EffectType { get; set; }
    public Sprite icon;
    public GameObject statusEffectPrefab;
    public float lifetime;
    public bool withdrawAtEnd;
    public bool stackable;
    public bool show;
    public List<string> EffectParams;

    public ConstStatusEffectInfo(string id)
    {
        this.id = id;
    }

    public ConstStatusEffectInfo(StatusEffectDataSet.TableData data)
    {
        this.id = data.ID;
        this.icon = data.IconPath.GetSprite();
        this.statusEffectPrefab = data.StatusEffectPrefab.GetValue();
        this.lifetime = data.Lifetime;
        this.withdrawAtEnd = data.WithdrawAtEnd;
        this.stackable = data.Stackable;
        this.show = data.Show;
        this.EffectParams = data.EffectParams;
    }
}

[System.Serializable]
public abstract class StatusEffectBase
{
    public static int Seq = 0;
    public int SequenceId = -1;
    public ConstStatusEffectInfo StatusEffectInfo { get; private set; }

    public StatusComponent Target { get; private set; }
    public IStatusEffectSource Source { get; private set; }

    public float Duration { get; private set; }  
    public float ElapsedTime { get; private set; } = 0f;
    private bool isEnded = false;

    public bool IsEnded => isEnded;
    public virtual bool IsEffectPermanent => false;



    protected Dictionary<EStatusType, StatusModifier> AffectedStatuses = new Dictionary<EStatusType, StatusModifier>();

    public UnityAction<float,float> OnStatusEffectProcessed;
    public UnityAction<StatusEffectBase> EffectFinished;

    public StatusEffectBase(StatusComponent target, ConstStatusEffectInfo info, IStatusEffectSource source)
    {
        Target = target;
        StatusEffectInfo = info;
        Duration = info.lifetime;
        Source = source;
        SequenceId = Seq++;
    }

    protected void InitializeAffectedStatuses(Dictionary<EStatusType, float> effects)
    {
        foreach (var effect in effects)
        {
            var status = Target.StatusBundle.GetStatus(effect.Key);
            var modifier = new StatusModifier(effect.Value, StatusModifier.ModifierType.Add, this);
            AffectedStatuses[effect.Key] = modifier;
            status.AddModifier(modifier);
        }
    }

    public void UpdateElapsedTime(float elapsedTime)
    {
        ElapsedTime = elapsedTime;
    }


    public virtual bool IsStackable()
    {
        return false; // Default implementation: not stackable
    }

    public virtual void Start()
    {
        UpdateElapsedTime(0);
        if(Source != null)
        {
            Debug.Log($"{GetType().Name} started on {Target.name} by {Source.Name} for {Duration} seconds.");
        }
        else
        {
            Debug.Log($"{GetType().Name} started on {Target.name} by System for {Duration} seconds.");
        }
    }

    public virtual void Update(float deltaTime)
    {
        if (isEnded) return;

        ElapsedTime += deltaTime;
        if (ElapsedTime >= Duration)
        {
            End();
        }
        if(OnStatusEffectProcessed != null)
        {
            OnStatusEffectProcessed.Invoke(ElapsedTime, deltaTime);
        }
    }

    public virtual void End()
    {
        if (isEnded) return;

        isEnded = true;
        foreach (var entry in AffectedStatuses)
        {
            var status = Target.StatusBundle.GetStatus(entry.Key);
            status.RemoveModifier(entry.Value);
        }
        EffectFinished?.Invoke(this);
        Debug.Log($"{GetType().Name} ended on {Target.name}.");
    }
}

public class UniqueStatusEffect : StatusEffectBase
{
    public UniqueStatusEffect(StatusComponent target, ConstStatusEffectInfo info, IStatusEffectSource source, Dictionary<EStatusType, float> effects)
        : base(target, info, source)
    {
        InitializeAffectedStatuses(effects);
    }
}

public class MultiStatusEffect : StatusEffectBase
{
    public MultiStatusEffect(StatusComponent target, ConstStatusEffectInfo info, IStatusEffectSource source, Dictionary<EStatusType, float> effects)
        : base(target, info, source)
    {
        InitializeAffectedStatuses(effects);
    }

    public override bool IsStackable()
    {
        return true; // Example: This effect is stackable
    }
}

// Example: A status effect that increases maximum health
public class MaxHealthBuff : StatusEffectBase
{
    private StatusModifier healthIncreaseModifier;

    public MaxHealthBuff(StatusComponent target, ConstStatusEffectInfo info, IStatusEffectSource source)
        : base(target, info, source)
    {
        float increaseAmount = float.Parse(info.EffectParams[0]); // Example: first param is health increase
        healthIncreaseModifier = new StatusModifier(increaseAmount, StatusModifier.ModifierType.Add, this);
    }
    public override bool IsEffectPermanent => true;

    public override void Start()
    {
        base.Start();
        var healthStatus = Target.StatusBundle.GetStatus(EStatusType.Health);
        healthStatus.AddModifier(healthIncreaseModifier);
        Debug.Log($"{Target.name}'s max health increased by {healthIncreaseModifier.Value} due to {StatusEffectInfo.id}. Current Health: {healthStatus.GetValue()}");
    }
    public override void End()
    {
        Status healthStatus = Target.StatusBundle.GetStatus(EStatusType.MaxHealth);
        if (!IsEffectPermanent)
        {
            healthStatus.RemoveModifier(healthIncreaseModifier);
            base.End();
        }
        base.End();
        Debug.Log($"{Target.name}'s max health buff ended. Current Health: {healthStatus.GetValue()}");
    }
}

public class SimpleDamage : StatusEffectBase
{
    private float damage;

    public SimpleDamage(StatusComponent target, ConstStatusEffectInfo info, IStatusEffectSource source)
        : base(target, info, source)
    {
        float damage = float.Parse(info.EffectParams[0]); // Example: first param is health increase
        this.damage = damage;
    }

    public override bool IsStackable()
    {
        return true; // DamageOverTime is stackable
    }

    public override void Start()
    {
        base.Start();
        Target.StatusBundle.GetStatus(EStatusType.Health).AddModifier(new StatusModifier(-damage, StatusModifier.ModifierType.Add, this));
    }
}

public class DamageOverTime : StatusEffectBase
{
    private float damagePerSecond;

    public DamageOverTime(StatusComponent target, ConstStatusEffectInfo info, IStatusEffectSource source)
        : base(target, info, source)
    {
        this.damagePerSecond = float.Parse(info.EffectParams[0]); // Example: first param is damage
    }

    public override bool IsStackable()
    {
        return true; // DamageOverTime is stackable
    }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);
        Target.StatusBundle.GetStatus(EStatusType.Health).AddModifier(new StatusModifier(-damagePerSecond * deltaTime, StatusModifier.ModifierType.Add, this));
    }
}
