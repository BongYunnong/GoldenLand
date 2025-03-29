using System;
using System.Collections.Generic;
using DG.Tweening.Plugins.Core.PathCore;
using UnityEngine;
using Path = System.IO.Path;

public enum EBookmarkType
{
    Main,
    Sub,
    Signature,
    Illust,
    MAX
}


[System.Serializable]
public struct ConstRestoreInfo
{
    public string GameEventId;
    public int MaxGoalCount;
    public int GoalValue;
    public ConstRestoreInfo(string value)
    {
        string[] args = value.Split('/');
        GameEventId = args[0];
        MaxGoalCount = int.Parse(args[1]);
        GoalValue = int.Parse(args[2]);
    }
}


[System.Serializable]
public struct ConstBookInfo
{
    public string ID;
    public string CharacterId;
    public List<Sprite> CoverImages;
    public List<ConstRestoreInfo> RestoreInfos;
    public string Tool;
    public string EquipmentPresetId;
    public float AttackRapidCooldownTime;
    public Dictionary<string, List<string>> ActionKeyBinding;
    public string PatternGroupId;
    public List<string> PersonalityIds;

    public ConstBookInfo(BookDataSet.TableData data)
    {
        this.ID = data.ID;
        this.CharacterId = data.CharacterId;

        this.CoverImages = new List<Sprite>();

        List<string> partNames = new List<string>();
        partNames.Add("Front");
        partNames.Add("Left");
        partNames.Add("Back");
        partNames.Add("Right");
        partNames.Add("Up");
        partNames.Add("Down");
        for (int i = 0; i < partNames.Count; i++)
        {
            SpritePath coverPart = new SpritePath(data.CoverImagePath.path,Path.GetFileNameWithoutExtension(data.CoverImagePath.path) + "_" + partNames[i]);
            CoverImages.Add(coverPart.GetSprite());
        }
        
        this.RestoreInfos = data.RestoreInfos;
        this.Tool = data.Tool;
        this.EquipmentPresetId = data.EquipmentPresetId;
        this.PersonalityIds = data.PersonalityIds;
        this.PatternGroupId = data.PatternGroupId;
        this.AttackRapidCooldownTime = data.AttackRapidCooldownTime;
        // KeyBinding 처리
        this.ActionKeyBinding = new Dictionary<string, List<string>>();
        for (int i = 0; i < data.ActionKeyBinding.Count; i++)
        {
            string actionKeyBind = data.ActionKeyBinding[i];
            string[] tokens = actionKeyBind.Split("=");
            
            string actionInputName = tokens[1];
            if (this.ActionKeyBinding.ContainsKey(actionInputName) == false)
            {
                this.ActionKeyBinding.Add(actionInputName, new List<string>());
            }
            this.ActionKeyBinding[actionInputName].Add(tokens[0]);
        }
    }
}

[System.Serializable]
public enum ECompareType
{
    Equal,
    Less,
    LessEqual,
    Greater,
    GreaterEqual
};


[System.Serializable]
public enum EConditionType
{
    True,
    False,
    Base,

    GameEvent,          // 특정 게임이벤트가 발동되었을 때 1회
    Status,             // 특정 상태 값이 변경되었을 때
    Memory,             // MemoryComponent에 특정 값이 적재될 때
    MemoryScore,        // MemoryComponent에 Score가 적재될 때
    GameEnvironment,    // 환경이 변경되었을 때
};


[System.Serializable]
public class ConditionValue
{
    public EConditionType ValueType;
    public string Key;
    public float Value;
    public ConditionValue(ConditionValue conditionValue)
    {
        this.ValueType = conditionValue.ValueType;
        this.Key = conditionValue.Key;
        this.Value = conditionValue.Value;
    }
    public ConditionValue(EConditionType valueType, string key, float value)
    {
        this.ValueType = valueType;
        this.Key = key;
        this.Value = value;
    }
}

[System.Serializable]
public struct ConditionSet
{
    public ECompareType compareType;
    public ConditionValue lValue;
    public ConditionValue rValue;
    public ConditionSet(ConditionSet conditionSet)
    {
        this.compareType = conditionSet.compareType;
        this.lValue = new ConditionValue(conditionSet.lValue);
        this.rValue = new ConditionValue(conditionSet.rValue);
    }
    public ConditionSet(ECompareType compareType, EConditionType lValueType, string lKey, float lValue, EConditionType rValueType, string rKey, float rValue)
    {
        this.compareType = compareType;
        this.lValue = new ConditionValue(lValueType, lKey, lValue);
        this.rValue = new ConditionValue(rValueType, rKey, rValue);
    }
    public ConditionSet(string[] arguments)
    {
        this.compareType = (ECompareType)Enum.Parse(typeof(ECompareType), arguments[0]);
        string[] lValueArgumnets = arguments[1].Split('/');
        this.lValue = new ConditionValue(Enum.Parse<EConditionType>(lValueArgumnets[0]), lValueArgumnets[1], float.Parse(lValueArgumnets[2]));
        string[] rValueArgumnets = arguments[2].Split('/');
        this.rValue = new ConditionValue(Enum.Parse<EConditionType>(rValueArgumnets[0]), rValueArgumnets[1], float.Parse(rValueArgumnets[2]));
    }
}