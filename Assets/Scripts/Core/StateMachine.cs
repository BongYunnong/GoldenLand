using System;
using System.Collections.Generic;

public class EnumHandler<TEnum> where TEnum : struct, Enum
{
    public void PrintEnumValues()
    {
        foreach (var value in Enum.GetValues(typeof(TEnum)))
        {
            Console.WriteLine(value);
        }
    }
    public bool IsValidEnumValue(TEnum value)
    {
        return Enum.IsDefined(typeof(TEnum), value);
    }
}

public class StateMachine<T, TEnum> where T : class  where TEnum : struct, Enum
{
    private T ownerEntity;
    private State<T, TEnum> currentState;
    private State<T, TEnum> previousState;
    private State<T, TEnum> globalState;

    private Dictionary<State<T, TEnum>, List<System.Func<bool>>> transitionRules = new Dictionary<State<T, TEnum>, List<System.Func<bool>>>();
    
    public void Setup(T owner, State<T, TEnum> entryState)
    {
        ownerEntity = owner;
        currentState = null;
        previousState = null;
        globalState = null;

        ChangeState(entryState);
    }
    public void Execute()
    {
        if (globalState != null)
        {
            globalState.Execute(ownerEntity);
        }

        if (currentState != null)
        {
            currentState.Execute(ownerEntity);
        }
        
        UpdateTransitions();
    }
    public void ChangeState(State<T, TEnum> newState)
    {
        if (newState == null) return;
        if (currentState != null)
        {
            previousState = currentState;

            currentState.Exit(ownerEntity);
        }
        currentState = newState;
        currentState.Enter(ownerEntity);
    }
    public void SetGlobalState(State<T, TEnum> newState)
    {
        globalState = newState;
    }

    /// <summary>
    /// ChangeState를 할 때 force가 아닌 경우 이 condition을 따른다.
    /// </summary>
    public bool CanTransitionTo(TEnum newState)
    {
        return currentState.CanTransitionTo(ownerEntity, newState);
    }
    
    /// <summary>
    /// 이 Transition은 자동이다.
    /// </summary>
    public void AddTransition(State<T, TEnum> fromState, State<T, TEnum> toState, System.Func<bool> condition)
    {
        if (!transitionRules.ContainsKey(fromState))
        {
            transitionRules[fromState] = new List<System.Func<bool>>();
        }
        transitionRules[fromState].Add(() =>
        {
            if (condition())
            {
                ChangeState(toState);
                return true;
            }
            return false;
        });
    }
    
    public void UpdateTransitions()
    {
        // Update에서 불러줘야함.
        // 현재 State에 연결된 Rule들을 다 순회하면서 실행. Rule은 Condition과 로직을 포함
        if (transitionRules.TryGetValue(currentState, out var rules))
        {
            foreach (var rule in rules)
            {
                if (rule())
                {
                    break;
                }
            }
        }
    }
}