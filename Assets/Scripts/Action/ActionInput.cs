using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;



[System.Serializable]
public class ConstActionInputInfo
{
    public string id;
    public EInputAction InputAction;
    public ConstActionInputInfo(ActionInputDataSet.TableData data)
    {
        this.id = data.ID;
        this.InputAction = data.InputAction;
    }
}

public class ActionInput
{
    public ConstActionInputInfo actionInputInfo;

    public ActionInput(ConstActionInputInfo actionInputInfo)
    {
        this.actionInputInfo = actionInputInfo;
    }
}