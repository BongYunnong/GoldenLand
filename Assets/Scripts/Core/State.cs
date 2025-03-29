using System;

public abstract class State<T, TEnum> where T : class  where TEnum : struct, Enum
{
    public abstract void Enter(T entity);

    public abstract void Execute(T entity);

    public abstract void Exit(T entity);
    
    public abstract bool CanTransitionTo(T entity, TEnum newState);
}