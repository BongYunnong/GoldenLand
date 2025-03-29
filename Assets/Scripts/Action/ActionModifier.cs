using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public enum EActionModifierType
{
    Attack,
    Projectile,
    Dodge,
    Reload,
    Guard,
    Effect,
    Animation,
    WeaponAnimation,
    Movement,
    Camera,
    StatusEffect,
    GameplayTag,
    IK,
}

[System.Serializable]
public class ConstActionModifierInfo
{
    public string id;
    public EActionModifierType actionModifierType;
    public EActionStartPositionType StartPositionType;
    public EActionTargetPositionType TargetPositionType;
    public EActionFlipType ActionFlipType;
    public List<string> parameters;
    
    public ConstActionModifierInfo(ActionModifierDataSet.TableData data)
    {
        this.id = data.ID;
        this.actionModifierType = data.ActionModifierType;
        this.StartPositionType = data.StartPositionType;
        this.TargetPositionType = data.TargetPositionType;
        this.ActionFlipType = data.ActionFlipType;
        this.parameters = data.Parameters;
    }
}

public abstract class ActionModifier 
{
    public ActionBase action;
    public ConstActionModifierInfo ActionModifierInfo { get; private set; }

    public ActionModifier(ConstActionModifierInfo actionModifierInfo, ActionBase actionBase)
    {
        ActionModifierInfo = actionModifierInfo;
        action = actionBase;
    }

    // 액션 속성 변경
    public virtual void ModifyActionProperties(ActionBase action)
    {
    }
    
    public abstract bool CanExeuteAction();
    public abstract void StartAction();
    public abstract void PreAction();
    public abstract void DoAction();
    public abstract void PostAction();
    public abstract void CancelAction();
    public abstract void CompleteAction();
    public abstract void FinishAction();
    public abstract void AffectTarget(IHittable hittable);
}