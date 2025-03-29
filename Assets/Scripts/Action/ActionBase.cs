using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Localization.Plugins.XLIFF.V20;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public enum EActionCancelReason
{
    Base,
    NoStrategy,
    CannotExecute,
    CannotDoAction,
    CannotPostAction,
}

public enum EActionProcessType
{
    Sequential,
    Parallel
}

public enum EActionSpaceType
{
    Local,
    World,
}

public enum EActionStartPositionType
{
    None,
    OwnerPos,
    OwnerCenter,
    ViewDirection,
    ViewDirectionCenter,
    TargetPosDirection,
    TargetPosOwnerCenterDirection,
    TargetPos,
}

public enum EActionTargetPositionType
{
    None,
    OwnerPos,
    OwnerCenter,
    ViewDirection,
    ViewDirectionCenter,
    TargetPosDirection,
    TargetPosOwnerCenterDirection,
    TargetPos
}

public enum EActionFlipType
{
    None,
    ViewDirection,
    TargetDirection,
    PreViewDirection,
    PreTargetDirection,
}

public enum EActionType
{
    Base,
    Continuous,
}

public enum EActionStrategyType
{
    Base,
    Hold,
    Cast,
    Toggle
}

public enum EActionProgressType
{
    PreAction,
    Action,
    PostAction,
}

[System.Serializable]
public enum EActionEffectTriggerType
{
    None = 0,
    Start = 1,
    Complete = 2,
    Cancel = 3,
    PreAction = 4,
    DoAction = 5,
    PostAction = 6,
    Affected = 7,   // Affected 이외에는 모두 Self가 타겟
    Canceled = 8,
    Finished = 9,
};

[System.Serializable]
public enum EEffectPositionType
{
    OwnerOffset,    // OwnerCharacter의 위치 + offset
    StartOffset,    // Action의 위치 + Offset
    TargetOffset,   // Target의 위치(보통 center) + Offset
    OwnerCenterOffset    // OwnerCharacter의 Center 위치 + offset 
}

[System.Serializable]
public enum EEffectRotationType
{
    LookDirection,
    EulerAngle,
    StartToEnd,
    StartToEndFlip,
    StartToEndOnlyX,
    PreStartToEnd,
    PreStartToEndFlip,
    PreStartToEndOnlyX
}

[System.Serializable]
public struct EffectInfo
{
    public string Id;
    public EEffectPositionType PositionType;
    public EEffectRotationType RotationType;
    public string[] positionTokens;
    public string[] rotationTokens;
    
    public EffectInfo(string id, string transformOriginString)
    {
        Id = id;
        
        PositionType = EEffectPositionType.OwnerOffset;
        RotationType = EEffectRotationType.LookDirection;
        positionTokens = null;
        rotationTokens = null;
        if (string.IsNullOrEmpty(transformOriginString) == false)
        {
            string[] transformTokens = transformOriginString.Split('_');
            positionTokens = transformTokens[0].Split('/');
            PositionType = (EEffectPositionType)Enum.Parse(typeof(EEffectPositionType), positionTokens[0]);
            
            if (transformTokens.Length > 1)
            {
                rotationTokens = transformTokens[1].Split('/');
                RotationType = (EEffectRotationType)Enum.Parse(typeof(EEffectRotationType), rotationTokens[0]);
            }
        }

    }
}

[System.Serializable]
public class ConstActionInfo
{
    public string id;
    public EActionType ActionType;
    public List<string> ActionParameters = new List<string>();
    public EActionProcessType ProcessType;
    public EActionStrategyType StrategyType;
    public int Priority;
    public List<EGameplayTag> RequiredGameplayTags;
    public List<EActionProgressType> TransitionAllowedProgresses;
    public List<string> RequiredActions;
    public int RequireAmmo;
    public bool RequireCast;
    public float PreDelay;
    public float Duration;
    public float PostDelay;
    public float CooldownTime;
    public float ActionEnterDistance;
    public float Weight;
    public string ActionSequence;
    public List<ECharacterState> AllowedStates = new List<ECharacterState>();
    public List<string> ModifierIds = new List<string>();

    public ConstActionInfo(ActionDataSet.TableData data)
    {
        this.id = data.ID;
        this.ActionType = data.ActionType;
        this.ActionParameters = data.ActionParameters;
        this.ProcessType = data.ProcessType;
        this.StrategyType = data.StrategyType;
        this.Priority = data.Priority;
        this.RequiredGameplayTags = new List<EGameplayTag>();
        for (int i = 0; i < data.RequiredGameplayTags.Count; i++)
        {
            EGameplayTag gameplayTag = Enum.Parse<EGameplayTag>(data.RequiredGameplayTags[i]);
            this.RequiredGameplayTags.Add(gameplayTag);
        }
        this.TransitionAllowedProgresses = new List<EActionProgressType>();
        for (int i = 0; i < data.TransitionAllowedProgresses.Count; i++)
        {
            EActionProgressType actionProgress = Enum.Parse<EActionProgressType>(data.TransitionAllowedProgresses[i]);
            this.TransitionAllowedProgresses.Add(actionProgress);
        }
        this.RequiredActions = data.RequiredActions;
        this.PreDelay = data.PreDelay;
        this.Duration = data.Duration;
        this.PostDelay = data.PostDelay;
        this.CooldownTime = data.CooldownTime;
        this.RequireAmmo = data.RequireAmmo;
        this.RequireCast = data.RequireCast;
        this.ActionEnterDistance = data.ActionEnterDistance;
        this.Weight = data.Weight;
        this.AllowedStates.Clear();
        
        for (int i = 0; i < data.AllowedStates.Count; i++)
        {
            if (ECharacterState.TryParse(data.AllowedStates[i], out ECharacterState state))
            {
                this.AllowedStates.Add(state);
            }
        }
        this.ActionSequence = data.ActionSequence;
        this.ModifierIds = data.ModifierIds;
    }
}


public class ActionBase
{
    public static int Seq = 0;
    public int SequenceId{ get; private set; }
    public ConstActionInfo ActionInfo { get; private set;}
    
    public ActionComponent ActionComponent;
    public Weapon weapon;
    public string bookId;
    
    protected BasicActionStrategy currentStrategy;
    private HashSet<ECharacterState> allowedStates = new HashSet<ECharacterState>();
    public EInputAction RequirekeyInputAction { get; protected set; }
    
    public List<ActionModifier> Modifiers { get; private set; } = new List<ActionModifier>();
    
    public EActionProgressType CurrentActionProgressState { get; private set; }
    protected bool isCanceled = false;
    protected bool isFinished = false;
    
    private Vector2 dashDirection = Vector2.zero;
    protected float totalElapsedTime = 0f;
    public float TotalElapsedTime {get{return totalElapsedTime;}}
    protected float actionElapsedTime = 0f;
    public float ActionElapsedTime {get{return actionElapsedTime;}}

    public Action ActionCompleted;
    public Action ActionCanceled;
    public Action ActionFinished;
    
    private float cachedVelocityMultiplier = 1.0f;

    private bool forcedExecute = false;

    public ActionBase(ConstActionInfo actionInfo, ActionComponent actionComponent, Weapon weapon, EInputAction keyInput, string bookId)
    {
        SequenceId = Seq++;
        
        ActionInfo = actionInfo;
        this.ActionComponent = actionComponent;
        this.weapon = weapon;
        this.bookId = bookId;
        
        switch (actionInfo.StrategyType)
        {
            case EActionStrategyType.Cast:
                currentStrategy = new CastActionStrategy(this, 0.25f);
                RequirekeyInputAction = keyInput;
                break;
            case EActionStrategyType.Toggle:
                currentStrategy = new ToggleActionStrategy(this);
                RequirekeyInputAction = EInputAction.None;
                break;
            case EActionStrategyType.Hold:
                currentStrategy = new BasicActionStrategy(this);
                RequirekeyInputAction = keyInput;
                break;
            case EActionStrategyType.Base:
            default:
                currentStrategy = new BasicActionStrategy(this);
                RequirekeyInputAction = EInputAction.None;
                break;
        }
        for (int i = 0; i < actionInfo.AllowedStates.Count; i++)
        {
            allowedStates.Add(actionInfo.AllowedStates[i]);
        }
        
        DataManager dataManager = DataManager.Instance;
        for (int i = 0; i < actionInfo.ModifierIds.Count; i++)
        {
            string modifierId = actionInfo.ModifierIds[i];
            if (dataManager.actionModifierDict.TryGetValue(modifierId, out ConstActionModifierInfo modifierInfo))
            {
                Modifiers.Add(ActionModifierFactory.CreateStruct(modifierInfo, this));
            }
        }
    }

    #region Condition

    public void SetForceExecute(bool value)
    {
        forcedExecute = value;
    }
    
    public bool CanStartAction()
    {
        if (forcedExecute)
        {
            return true;
        }
        return IsAllowedInState(ActionComponent.OwnerCharacter.CurrentCharacterState) && currentStrategy.CanPreAction();
    }

    public bool CanTransitionTo(ConstActionInfo newActionInfo)
    {
        bool canTransition = newActionInfo.TransitionAllowedProgresses.Count == 3;
        bool actionConnected = newActionInfo.RequiredActions.Count == 0;

        // 이전 Action이 있을 경우 PreAction일 때만 가드로 전환 가능
        // 보통은 PostAction에 연결 가능
        if (canTransition == false)
        {
            for (int i = 0; i < newActionInfo.TransitionAllowedProgresses.Count; i++)
            {
                EActionProgressType progressType = newActionInfo.TransitionAllowedProgresses[i];
                if (CurrentActionProgressState == progressType)
                {
                    canTransition = true;
                    break;
                }
            }
        }

        string actionId = ActionComponent.CurrentAction.ActionInfo.id;
        for (int i = 0; i < newActionInfo.RequiredActions.Count; i++)
        {
            if (newActionInfo.RequiredActions[i] == actionId)
            {
                actionConnected = true;
                break;
            }
        }
        return canTransition && actionConnected;
    }
    
    
    /// <summary>
    /// attackAction.AllowInState(EActionState.Idle);
    /// </summary>
    public void AllowInState(ECharacterState state)
    {
        allowedStates.Add(state);
    }
    
    public bool IsAllowedInState(ECharacterState currentState)
    {
        return allowedStates.Contains(currentState);
    }
    

    #endregion

    #region Execute

    protected virtual void Reset()
    {
        CurrentActionProgressState = EActionProgressType.PreAction;
        totalElapsedTime = 0;
        actionElapsedTime = 0;
        isCanceled = false;
        isFinished = false;
        if (currentStrategy != null)
        {
            currentStrategy.Reset();
        }
    }
    
    
    public virtual IEnumerator Execute(Action onComplete = null,
        Action onCanceled = null, Action onFinished = null)
    {
        Debug.Log($"{ActionInfo.id} Sart Execute");
        this.ActionCompleted += onComplete;
        this.ActionCanceled += onCanceled;
        this.ActionFinished += onFinished;
        Reset();
        
        if (currentStrategy == null)
        {
            HandleCanceled(EActionCancelReason.NoStrategy);
            yield break;
        }
        
        // 조건 확인
        if (!CanStartAction())
        {
            HandleCanceled(EActionCancelReason.CannotExecute);
            yield break;
        }

        // 일단 액션 시작 시 Wait 조건에 따라 기다린다.
        while (currentStrategy.Wait())
        {
            yield return new WaitForSeconds(Time.deltaTime);
            if (!CanStartAction())
            {
                HandleCanceled(EActionCancelReason.CannotExecute);
                yield break;
            }
        }

        StartAction();
        
        float preEndTime = ActionInfo.PreDelay;
        float doEndTime = ActionInfo.PreDelay + ActionInfo.Duration;
        float postEndTime = ActionInfo.PreDelay + ActionInfo.Duration + ActionInfo.PostDelay;
        
        // PreAction 단계
        yield return PerformPhase(0, preEndTime, PreAction);

        // DoAction 단계
        if (currentStrategy.CanDoAction() == false)
        {
            HandleCanceled(EActionCancelReason.CannotDoAction);
            yield break;
        }
        yield return PerformPhase(preEndTime, doEndTime, DoAction);

        // PostAction 단계
        if (currentStrategy.CanPostAction() == false)
        {
            HandleCanceled(EActionCancelReason.CannotPostAction);
            yield break;
        }
        yield return PerformPhase(doEndTime, postEndTime, PostAction);

        Complete();
    }
    
    protected IEnumerator PerformPhase(float phaseStartTime, float phaseEndTime, System.Action phaseAction)
    {
        bool phaseExecuted = false;
        
        while (totalElapsedTime < phaseEndTime)
        {
            if (!phaseExecuted && totalElapsedTime >= phaseStartTime)
            {
                phaseAction?.Invoke();
                phaseExecuted = true;
            }
            
            // 캔슬 플래그가 처리될 떄 event도 같이 처리되었어야 함
            if (isCanceled)
            {
                yield break;
            }
            totalElapsedTime += Time.deltaTime;
            if (CurrentActionProgressState == EActionProgressType.Action)
            {
                actionElapsedTime += Time.deltaTime;
            }
            yield return null;
        }
        if (!phaseExecuted && totalElapsedTime >= phaseStartTime)
        {
            phaseAction?.Invoke();
        }
    }

    /// <summary>
    /// RequireKeyInput을 충족하지 못할 경우 호출
    /// </summary>
    public void Stop()
    {
        if (CurrentActionProgressState != EActionProgressType.PostAction)
        {
            CoroutineRunner.instance.StartCoroutine(StopCoroutine());
        }
    }
    
    public virtual IEnumerator StopCoroutine()
    {
        yield break;
    }


    public virtual void StartAction()
    {
        // Modifier보다 먼저 값을 캐시해둔다.
        cachedVelocityMultiplier = ActionComponent.OwnerCharacter.inputMultiplier;
        ActionComponent.OwnerCharacter.inputMultiplier = 0;
        ActionComponent.OwnerCharacter.BlockView(true, "Action");
        ActionComponent.OwnerCharacter.BlockJump(true, "Action");
        
        foreach (var modifier in Modifiers)
        {
            modifier.StartAction();
        }
    }

    // 행동 전 로직
    public virtual void PreAction()
    {
        Debug.Log($"{ActionInfo.id}: PreAction started.");
        CurrentActionProgressState = EActionProgressType.PreAction;
        ActionComponent.HandleActionProgressChanged(EActionProgressType.PreAction);
        
        foreach (var modifier in Modifiers)
        {
            modifier.PreAction();
        }
    }

    // 실제 행동 수행
    public virtual void DoAction()
    {
        Debug.Log($"{ActionInfo.id}: DoAction started");
        CurrentActionProgressState = EActionProgressType.Action;
        ActionComponent.HandleActionProgressChanged(EActionProgressType.Action);
        ApplyModifiers();
        
        GhostTrail currGhostTrail = ActionComponent.OwnerCharacter.GetComponentInChildren<GhostTrail>();
        if (currGhostTrail)
        {
            GhostTrailController.Instance.AddGhostTrace(currGhostTrail, 0.5f);
        }

        foreach (var modifier in Modifiers)
        {
            modifier.DoAction();
        }
    }

    // 행동 후 로직
    public virtual void PostAction()
    {
        Debug.Log($"{ActionInfo.id}: PostAction started");
        CurrentActionProgressState = EActionProgressType.PostAction;
        ActionComponent.HandleActionProgressChanged(EActionProgressType.PostAction);
        ActionComponent.OwnerCharacter.SetDodgeInfo(null);
        
        foreach (var modifier in Modifiers)
        {
            modifier.PostAction();
        }

        ActionComponent.ExecuteSequenceAction(bookId, ActionInfo.ActionSequence);
    }
    
    public virtual void FinishAction()
    {
        Debug.Log($"{ActionInfo.id}: FinishAction started");
        StartCooldown(); // 캔슬 시에도 쿨다운 시작
        
    }
    #endregion


    #region Cooldown & Cancel

    
    private void StartCooldown()
    {
        ActionComponent.StartCooldown(ActionInfo.CooldownTime);
    }

    public virtual void Cancel()
    {
        if(isFinished)
        {
            return;
        }
        isCanceled = true;
        HandleCanceled(EActionCancelReason.Base);
    }

    public virtual void Complete()
    {
        if(isFinished)
        {
            return;
        }
        HandleCompleted();
    }

    protected void HandleCanceled(EActionCancelReason reason)
    {
        switch (reason)
        {
            case EActionCancelReason.Base:
                break;
            case EActionCancelReason.NoStrategy:
                Debug.LogError("ActionBase: No strategy set.");
                break;
            case EActionCancelReason.CannotExecute:
                Debug.Log($"{ActionInfo.id} cannot be executed in the current state.");
                break;
            case EActionCancelReason.CannotDoAction:
                Debug.Log("ActionBase: DoAction conditions not met.");
                break;
            case EActionCancelReason.CannotPostAction:
                Debug.Log("ActionBase: PostAction conditions not met.");
                break;
        }
        Debug.Log($"{ActionInfo.id} cancelled.");
        
        foreach (var modifier in Modifiers)
        {
            modifier.CancelAction();
        }
        ActionCanceled?.Invoke();
        HandleFinished();
    }

    protected void HandleCompleted()
    {
        foreach (var modifier in Modifiers)
        {
            modifier.CompleteAction();
        }
        ActionCompleted?.Invoke();
        HandleFinished();
    }

    private void HandleFinished()
    {
        isFinished = true;
        ActionFinished?.Invoke();
        
        foreach (var modifier in Modifiers)
        {
            modifier.FinishAction();
        }
        
        // MovementModifier가 있으면 Finish에서 되돌릴 수 있으므로 이후에 처리
        ActionComponent.OwnerCharacter.BlockView(false, "Action");
        ActionComponent.OwnerCharacter.BlockJump(false, "Action");
        ActionComponent.OwnerCharacter.SetDodgeInfo(null);
        ActionComponent.OwnerCharacter.inputMultiplier = cachedVelocityMultiplier;
        RollbackAction();
    }
    
    public void HandleAffected(IHittable hittable)
    {
        foreach (var modifier in Modifiers)
        {
            modifier.AffectTarget(hittable);
        }
    }
    
    
    /// <summary>
    /// 남아있는 Effect나 상태 등을 처리함
    /// </summary>
    public virtual void RollbackAction()
    {
        Debug.Log($"{ActionInfo.id} has been rolled back.");
    }


    #endregion
    #region Modifier

    // Modifier 추가
    public void AddModifier(ActionModifier modifier)
    {
        Modifiers.Add(modifier);
    }

    // Modifier 제거
    public void RemoveModifier(ActionModifier modifier)
    {
        Modifiers.Remove(modifier);
    }

    // Modifier 적용
    public void ApplyModifiers()
    {
        foreach (var modifier in Modifiers)
        {
            modifier.ModifyActionProperties(this);
        }
    }

    public bool HasModifierOfType(EActionModifierType modifierType)
    {
        foreach (var modifier in Modifiers)
        {
            if (modifier.ActionModifierInfo.actionModifierType == modifierType)
            {
                return true;
            }
        }
        return false;
    }
    #endregion
    
}


public class ContinuousAction : ActionBase
{
    private bool isRunning = false;
    private int maxExecutionCount = 0;
    private int executionCount = 0;
    private float executionInterval = 0.1f;
    public ContinuousAction(ConstActionInfo actionInfo, ActionComponent actionComponent, Weapon weapon, EInputAction keyInput, string bookId)
        : base(actionInfo, actionComponent, weapon, keyInput, bookId)
    {
        if (actionInfo.ActionParameters.Count > 0)
        {
            float.TryParse(actionInfo.ActionParameters[0], out executionInterval);
        }
        if (actionInfo.ActionParameters.Count > 1)
        {
            int.TryParse(actionInfo.ActionParameters[1], out maxExecutionCount);
        }
    }

    protected override void Reset()
    {
        executionCount = 0;
        base.Reset();
    }
    
    public override IEnumerator Execute(Action onComplete = null, Action onCanceled = null, Action onFinished = null)
    {
        Debug.Log($"{ActionInfo.id} Sart Execute");
        onFinished += () => { isRunning = false; };
        this.ActionCompleted += onComplete;
        this.ActionCanceled += onCanceled;
        this.ActionFinished += onFinished;
        Reset();
        
        if (currentStrategy == null)
        {
            Debug.LogError("ActionBase: No strategy set.");
            yield break;
        }

        currentStrategy.Reset();
        // 이미 동작하고있는가
        if (isRunning)
        {
            Debug.LogError("ActionBase: Action Is Already Running.");
            yield break;
        }

        // 조건 확인
        if (!CanStartAction())
        {
            Debug.Log($"{ActionInfo.id} cannot be executed in the current state.");
            yield break;
        }

        // 일단 액션 시작 시 Wait 조건에 따라 기다린다.
        while (currentStrategy.Wait())
        {
            yield return new WaitForSeconds(Time.deltaTime);
            totalElapsedTime += Time.deltaTime;
            if (!CanStartAction())
            {
                Debug.Log($"{ActionInfo.id} cannot be executed in the current state.");
                yield break;
            }
        }
        
        isRunning = true;
        
        StartAction();
        
        float preEndTime = ActionInfo.PreDelay;
        // PreAction 단계
        yield return PerformPhase(0, preEndTime, PreAction);
        
        // Action 단계
        yield return ExecuteContinuously();
        // ContinuousAction은 Stop이나 Cancel을 통해서 PostAciton, Complete가 불린다.
        
        Complete();
    }

    
    private IEnumerator ExecuteContinuously()
    {
        while (isRunning)
        {
            if (isCanceled)
            {
                yield break;
            }
            if (currentStrategy.CanDoAction() == false)
            {
                yield break;
            }
            executionCount++;
            DoAction();
            if (maxExecutionCount > 0 && executionCount >= maxExecutionCount)
            {
                Stop();
                yield break;
            }
            float actionInterval = GetActionInterval();
            yield return new WaitForSeconds(actionInterval);
            totalElapsedTime += actionInterval;
            actionElapsedTime += actionInterval;
        }
    }
    
    public override IEnumerator StopCoroutine()
    {
        Debug.LogWarning("StopCoroutine");
        isRunning = false;
        // PostAction 단계
        if (currentStrategy.CanPostAction() == false)
        {
            HandleCanceled(EActionCancelReason.CannotPostAction);
            Debug.LogWarning("StopCoroutine Break");
            yield break;
        }
        yield return PerformPhase(TotalElapsedTime, TotalElapsedTime + ActionInfo.PostDelay, PostAction);

        Complete();
    }
    
    public override void Cancel()
    {
        base.Cancel();
        isRunning = false;
    }
    
    protected virtual float GetActionInterval()
    {
        return executionInterval;
    }
}
