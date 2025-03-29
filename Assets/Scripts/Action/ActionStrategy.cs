using UnityEngine;
using Unity.VisualScripting;
using System.Collections;
using System.Collections.Generic;

public class BasicActionStrategy
{
    protected ActionBase action;
    public BasicActionStrategy(ActionBase action)
    {
        this.action = action;
    }

    public virtual bool Wait() => false;

    public virtual void Reset()
    {
        
    }
    public virtual bool CanPreAction() => true;  // PreAction 항상 가능
    public virtual bool CanDoAction() => true;  // DoAction 항상 가능
    public virtual bool CanPostAction() => true; // PostAction 항상 가능
}

public class CastActionStrategy : BasicActionStrategy
{
     private float requiredCastTime; // 필요한 캐스트 시간
     private float currentCastTime;  // 현재 캐스트된 시간
 
     public CastActionStrategy(ActionBase action, float requiredCastTime):base(action)
     {
         this.requiredCastTime = requiredCastTime;
         this.currentCastTime = 0f;
     }
     
     public override bool Wait()
     {
         UpdateCastTime(Time.deltaTime);
         return currentCastTime < requiredCastTime;
     }
     public override void Reset()
     {
         this.currentCastTime = 0f;
     }

     public override bool CanPreAction()
     {
         if (action.RequirekeyInputAction == EInputAction.None)
         {
             return true;
         }
         return action.ActionComponent.OwnerCharacter.GetControlInput(action.RequirekeyInputAction);
     }
 
     public override bool CanDoAction() => true; // 캐스트가 완료되면 실행 가능
 
     public override bool CanPostAction() => true; // 항상 PostAction 가능
 
     // 캐스트 시간 업데이트 메서드
     public void UpdateCastTime(float deltaTime)
     {
         currentCastTime += deltaTime;
         Debug.Log($"CastAction: Current cast time = {currentCastTime}/{requiredCastTime}");
     }
}

public class ToggleActionStrategy : BasicActionStrategy
{
    public ToggleActionStrategy(ActionBase action):base(action)
    {
    }

    public override bool Wait() => false;
    
    public override bool CanPreAction() => true; // PreAction 항상 가능

    public override bool CanDoAction() => true; // 켜져 있을 때만 실행 가능

    public override bool CanPostAction() => true; // 항상 PostAction 가능
}