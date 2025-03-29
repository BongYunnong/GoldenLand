using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public enum ETargetFilterType
{
    All,
    Instigator,
    Ally,      
    Enemy,
};

[Serializable]
public enum EKnockbackDirectionType
{
    None,
    ActionDiff,
    ActionHorizontal,
    ActionVertical,
    OwnerDiff,
    OwnerHorizontal,    
    OwnerVertical,  
};

[Serializable]
public enum EActionAreaType
{
    Ray,        // 출발점이 존재하고, Collide된 point까지 Ray를 그렸을 때 충돌이 막히지 않아야 실행
                // 만약 그것이 벽 같은 것이라면, 벽에 IHittable을 구현하고 AttackInfo에 Blocked를 
    Area,       // 명확한 방향 벡터 없이, Collide된 모든 오브젝트에 대해 공격을 실행
    Target,     // PreAction에서 타겟팅 한 것을 바탕으로 공격을 실행
};


[System.Serializable]
public class ConstActionEffectInfo
{
    public string ID;
    public ETargetFilterType TargetFilterType;
    public bool IsGuardBreak;
    public bool IsStun;
    public bool IsStrike;
    public bool IsBound;
    public float StaggerTime;
    public EKnockbackDirectionType KnockbackDirectionType;
    public Vector2 KnockbackForce;
    public EActionSpaceType KnockbackSpaceType;
    public Vector2 KnockbackOffset;
    
    public ConstActionEffectInfo(ActionEffectDataSet.TableData data)
    {
        this.ID = data.ID;
        this.TargetFilterType = data.TargetFilterType;
        this.IsGuardBreak = data.IsGuardBreak;
        this.IsStun = data.IsStun;
        this.IsStrike = data.IsStrike;
        this.IsBound = data.IsBound;
        this.StaggerTime = data.StaggerTime;
        this.KnockbackDirectionType = data.KnockbackDirectionType;
        this.KnockbackForce = data.KnockbackForce;
        this.KnockbackSpaceType = data.KnockbackSpaceType;
        this.KnockbackOffset = data.KnockbackOffset;
    }
}


[System.Serializable]
public class ConstActionAreaInfo
{
    public string Id;
    public EActionSpaceType ActionSpaceType;
    public EActionAreaType ActionAreaType;
    public float AngleDeltaSize;
    public float Duration;
    public List<Vector2> Points;
    
    public ConstActionAreaInfo(ActionAreaDataSet.TableData data)
    {
        this.Id = data.ID;
        this.ActionSpaceType = data.ActionSpaceType;
        this.ActionAreaType = data.ActionAreaType;
        this.AngleDeltaSize = data.AngleDeltaSize;
        this.Duration = data.Duration;
        this.Points = data.Points;
    }
}


public class AttackRequsetBundle
{
    public float BaseDamage = 1;
    public EKnockbackDirectionType KnockbackDirectionType = EKnockbackDirectionType.ActionDiff;
    public Vector2 KnockbackForce = Vector2.one;
    public Vector2 KnockbackOffset = Vector2.zero;
    public EActionSpaceType KnockbackSpaceType = EActionSpaceType.World;
    public bool IsStrike = false;
    public bool IsGuardBreak = false;
    public bool IsStun = false;
    public bool IsBound = false;
    public float StaggerTime = 0.25f;
    public ETargetFilterType TargetFilterType = ETargetFilterType.Enemy;
    public EActionStartPositionType StartPositionType = EActionStartPositionType.None;
    public EActionTargetPositionType TargetPositionType = EActionTargetPositionType.None;

    public Dictionary<IHittable, bool> AimedTargets = new Dictionary<IHittable, bool>();
    public List<ConstActionAreaInfo> AreaInfos = new List<ConstActionAreaInfo>();
    
    public Vector2 GetKnockbackForceVector(IHittable hittable, Vector3 ownerPosition, Vector3 basePosition)
    {
        Vector2 attackDiff = hittable.GetGameObject().transform.position - basePosition;
        Vector2 ownerDiff = hittable.GetGameObject().transform.position - ownerPosition;

        Vector2 baseKnockbackVector = Vector2.zero;
        switch (KnockbackDirectionType)
        {
            case EKnockbackDirectionType.ActionDiff:
                baseKnockbackVector = attackDiff.normalized * KnockbackForce;
                break;
            case EKnockbackDirectionType.ActionHorizontal:
                baseKnockbackVector = Vector2.right * Mathf.Sign(attackDiff.x) * KnockbackForce;
                break;
            case EKnockbackDirectionType.ActionVertical:
                baseKnockbackVector = Vector2.up * Mathf.Sign(attackDiff.y) * KnockbackForce;
                break;
            case EKnockbackDirectionType.OwnerDiff:
                baseKnockbackVector = ownerDiff.normalized * KnockbackForce;
                break;
            case EKnockbackDirectionType.OwnerHorizontal:
                baseKnockbackVector = Vector2.right * Mathf.Sign(ownerDiff.x) * KnockbackForce;
                break;
            case EKnockbackDirectionType.OwnerVertical:
                baseKnockbackVector = Vector2.up * Mathf.Sign(ownerDiff.y) * KnockbackForce;
                break;
        }

        if (KnockbackSpaceType == EActionSpaceType.Local)
        {   
            return baseKnockbackVector + TransformWithUpVector(baseKnockbackVector, KnockbackOffset);
        }
        return baseKnockbackVector + KnockbackOffset;
    }
    public Vector2 TransformWithUpVector(Vector2 upVectorA, Vector2 localVectorB)
    {
        // 1. 기준 벡터 정규화
        Vector2 up = upVectorA.normalized;

        // 2. 왼손 법칙에 따른 rightVector 계산
        Vector2 right = new Vector2(up.y, -up.x);

        // 3. B 벡터를 새로운 좌표계로 변환
        float newX = localVectorB.x * right.x + localVectorB.y * up.x;
        float newY = localVectorB.x * right.y + localVectorB.y * up.y;

        return new Vector2(newX, newY);
    }

    public void ApplyActionEffectInfo(ConstActionEffectInfo actionEffectInfo)
    {
        // 타겟 필터 타입. 누구에게 영향을 줄 것인가?
        // ex. All, Instigator, Ally, Enemy
        TargetFilterType = actionEffectInfo.TargetFilterType;
            
        // 적의 방어상태를 파괴하는 공격인가?
        IsGuardBreak = actionEffectInfo.IsGuardBreak;
        // 적을 Airborne상태로 전이시키는 공격인가?
        IsStrike = actionEffectInfo.IsStrike;
        // 적이 땅에 닿았을 때 바운딩 되는가?
        IsBound = actionEffectInfo.IsBound;
        // 적을 Stun상태로 전이시키는 공격인가?
        IsStun = actionEffectInfo.IsStun;
            
        // StaggerTime
        StaggerTime = actionEffectInfo.StaggerTime;

        // 적을 날리는 힘
        KnockbackDirectionType = actionEffectInfo.KnockbackDirectionType;
        KnockbackSpaceType = actionEffectInfo.KnockbackSpaceType;
        KnockbackForce = actionEffectInfo.KnockbackForce;
        KnockbackOffset = actionEffectInfo.KnockbackOffset;
    }
}

public class AttackActionModifier : ActionModifier
{
    public List<ConstActionAimInfo> aimInfos = new List<ConstActionAimInfo>();
    private List<ActionAim> actionAims = new List<ActionAim>();
    public AttackRequsetBundle attackRequsetBundle;

    public AttackActionModifier(ConstActionModifierInfo actionModifierInfo, ActionBase actionBase) 
        :  base(actionModifierInfo, actionBase)
    {
        attackRequsetBundle = new AttackRequsetBundle();
        
        // 적에게 가할 데미지
        if (float.TryParse(actionModifierInfo.parameters[0], out float damage))
        {
            attackRequsetBundle.BaseDamage = damage;
        }
        
        
        DataManager dataManager = DataManager.Instance;
        string actionEffectId = actionModifierInfo.parameters[1];
        if (dataManager.actionEffectDict.TryGetValue(actionEffectId, out ConstActionEffectInfo actionEffectInfo))
        {
            attackRequsetBundle.ApplyActionEffectInfo(actionEffectInfo);
        }
        
        // Area도 여러개일 수 있다.
        if (actionModifierInfo.parameters.Count > 2)
        {
            string[] areaTokens = actionModifierInfo.parameters[2].Split('&');
            for (int i = 0; i < areaTokens.Length; i++)
            {
                string areaDataId = areaTokens[i];
                if (dataManager.actionAreaDict.TryGetValue(areaDataId, out ConstActionAreaInfo areaInfo))
                {
                    attackRequsetBundle.AreaInfos.Add(areaInfo);
                }
            }
        }

        attackRequsetBundle.StartPositionType = ActionModifierInfo.StartPositionType;
        attackRequsetBundle.TargetPositionType = ActionModifierInfo.TargetPositionType;
        
        // Aim도 여러개일 수 있다.
        // ex. OverlapCircle/10/10/0/0/Character/1/1
        if (actionModifierInfo.parameters.Count > 3)
        {
            string[] aimTokens = actionModifierInfo.parameters[3].Split('&');
            for (int i = 0; i < aimTokens.Length; i++)
            {
                string aimId = aimTokens[i];
                if (dataManager.actionAimDict.TryGetValue(aimId, out ConstActionAimInfo aimInfo))
                {
                    aimInfos.Add(aimInfo);
                }
            }
        }
    }

    public override bool CanExeuteAction()
    {
        return true;
    }

    public override void StartAction()
    {
    }

    public override void PreAction()
    {
        for (int i = 0; i < aimInfos.Count; i++)
        {
            ConstActionAimInfo aimInfo = aimInfos[i];
            ActionAim aim = ObjectPoolManager.GetObject("ActionAim").GetComponent<ActionAim>();
            aim.StartSearch(action.ActionComponent.OwnerCharacter.GetComponent<IHittable>(), aimInfo);
            actionAims.Add(aim);
        }
    }

    public override void DoAction()
    {
        attackRequsetBundle.AimedTargets.Clear();
        for (int i = 0; i < actionAims.Count; i++)
        {
            for (int j = 0; j < actionAims[i].hittables.Count; j++)
            {
                attackRequsetBundle.AimedTargets.TryAdd(actionAims[i].hittables[j], true);
            }
        }
        
        action.weapon.StartAttack(action, attackRequsetBundle);
        
#if UNITY_EDITOR
        DrawLine();
#endif
    }

    public override void PostAction()
    {
    }

    public override void CancelAction()
    {
    }

    public override void CompleteAction()
    {
    }

    public override void FinishAction()
    {
        for (int i = 0; i < actionAims.Count; i++)
        {
            actionAims[i].CancelSearch();
        }
    }

    public override void AffectTarget(IHittable hittable)
    {
    }
}
