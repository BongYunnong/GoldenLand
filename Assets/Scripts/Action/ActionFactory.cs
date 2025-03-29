using System;
using System.Collections.Generic;
using UnityEngine;

public static class ActionFactory
{
    private static readonly Dictionary<EActionType, Func<ActionComponent, ConstActionInfo, Weapon, EInputAction, string, ActionBase>> structGenerators = 
        new Dictionary<EActionType, Func<ActionComponent, ConstActionInfo, Weapon, EInputAction, string, ActionBase>>
    {
        { EActionType.Base, (actionComponent, constActionInfo, weapon, keyInput, bookId) => new ActionBase(constActionInfo, actionComponent, weapon, keyInput, bookId) },
        { EActionType.Continuous, (actionComponent, constActionInfo, weapon, keyInput, bookId) => new ContinuousAction(constActionInfo, actionComponent, weapon, keyInput, bookId) },
    };


    public static ActionBase CreateStruct(ConstActionInfo param, ActionComponent actionComponent, Weapon weapon, EInputAction inputAction, string bookId)
    {
        if (structGenerators.TryGetValue(param.ActionType, out var generator))
        {
            return generator(actionComponent, param, weapon, inputAction, bookId);
        }
        throw new ArgumentException($"GameEventConditionFactory: {param.id} is not supported");
    }
}

public static class ActionModifierFactory
{
    private static readonly Dictionary<EActionModifierType, Func<ConstActionModifierInfo, ActionBase, ActionModifier>> structGenerators = 
        new Dictionary<EActionModifierType, Func<ConstActionModifierInfo, ActionBase, ActionModifier>>
    {
        { EActionModifierType.Attack, (constActionModifierInfo, action) => new AttackActionModifier(constActionModifierInfo, action) },
        { EActionModifierType.Projectile, (constActionModifierInfo, action) => new ProjectileActionModifier(constActionModifierInfo, action) },
        { EActionModifierType.Dodge, (constActionModifierInfo, action) => new DodgeActionModifier(constActionModifierInfo, action) },
        { EActionModifierType.Reload, (constActionModifierInfo, action) => new ReloadActionModifier(constActionModifierInfo, action) },
        { EActionModifierType.Guard, (constActionModifierInfo, action) => new GuardActionModifier(constActionModifierInfo, action) },
        { EActionModifierType.Effect, (constActionModifierInfo, action) => new EffectActionModifier(constActionModifierInfo, action) },
        { EActionModifierType.Animation, (constActionModifierInfo, action) => new CharacterAnimationActionModifier(constActionModifierInfo, action) },
        { EActionModifierType.WeaponAnimation, (constActionModifierInfo, action) => new WeaponAnimationActionModifier(constActionModifierInfo, action) },
        { EActionModifierType.Movement, (constActionModifierInfo, action) => new MovementActionModifier(constActionModifierInfo, action) },
        { EActionModifierType.Camera, (constActionModifierInfo, action) => new CameraActionModifier(constActionModifierInfo, action) },
        { EActionModifierType.StatusEffect, (constActionModifierInfo, action) => new StatusEffectActionModifier(constActionModifierInfo, action) },
        { EActionModifierType.GameplayTag, (constActionModifierInfo, action) => new GameplayTagActionModifier(constActionModifierInfo, action) },
        { EActionModifierType.IK, (constActionModifierInfo, action) => new IKActionModifier(constActionModifierInfo, action) },
    };


    public static ActionModifier CreateStruct(ConstActionModifierInfo param, ActionBase actionBase)
    {
        if (structGenerators.TryGetValue(param.actionModifierType, out var generator))
        {
            return generator(param, actionBase);
        }
        throw new ArgumentException($"GameEventConditionFactory: {param.id} is not supported");
    }
}