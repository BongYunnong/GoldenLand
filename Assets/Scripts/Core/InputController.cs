using System;
using System.Collections.Generic;
using UnityEditor.Localization.Plugins.XLIFF.V20;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public enum EInputAction
{
    None = 0,
    Left,
    Right,
    Down,
    Up,
    Jump,
    JumpRelease,
    Dodge,
    BaseAction,
    BaseActionHold,
    BaseActionTap,
    BaseActionSlowTap,
    SubAction,
    SubActionHold,
    SubActionTap,
    SubActionSlowTap,
    CastAction0,
    CastAction1,
    CastAction2,
    Interact,
    Reload,
    Cancel,
    Guard,
    MAX
}

public enum EInputActionContext
{
    Waiting = 0,
    Started,
    Performed,
    Canceled,
}

public class InputController : MonoBehaviour
{
    private InputSystem_Actions playerInputSystem;
    
    public UnityAction<EInputAction, EInputActionContext> InputActionTriggered;

    private void OnEnable()
    {
        playerInputSystem = new InputSystem_Actions();
        playerInputSystem.Enable();
    }

    public void HandleActionInput(InputAction.CallbackContext context)
    {
        string actionName =  context.action.name;

        if (Enum.TryParse<EInputAction>(actionName, out EInputAction inputAction) == false)
        {
            return;
        }
        if (context.started)
        {
            InputActionTriggered?.Invoke(inputAction, EInputActionContext.Started);
        }
        else if (context.performed)
        {
            InputActionTriggered?.Invoke(inputAction, EInputActionContext.Performed);
        }
        else if (context.canceled)
        {
            InputActionTriggered?.Invoke(inputAction, EInputActionContext.Canceled);
        }
        else
        {
            InputActionTriggered?.Invoke(inputAction, EInputActionContext.Waiting);
        }
    }
}