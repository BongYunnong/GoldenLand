using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization.SmartFormat.PersistentVariables;

[System.Serializable]
public struct LocalizeVariableInfo
{
    public string key;
    public LocalizeVariableType variableType;
    public object value;

    public LocalizeVariableInfo(string key, LocalizeVariableType type, object value)
    {
        this.key = key;
        variableType = type;
        this.value = value;
    }
}

public enum LocalizeVariableType
{
    BoolVariable,
    IntVariable,
    LongVariable,
    FloatVariable,
    StringVariable,
    ObjectVariable,
}


[RequireComponent(typeof(LocalizeStringEvent))]
public class LocalizeText : MonoBehaviour
{
    public static string StringTableGameplay = "ST_Gameplay";
    public static string StringTableLog = "ST_Log";
    public static string StringTableMission = "ST_Mission";
    public static string StringTableLobby = "ST_Lobby";
    public static string StringTableItem = "ST_Item";
    public static string StringTableAgent = "ST_Agent";
    public static string StringTableAction = "ST_Action";
    public static string StringTableBook = "ST_Book";
    public static string StringTableSurvey = "ST_Survey";
    public static string StringTableBuildling = "ST_Building";
    public static string StringTableChallenge = "ST_Challenge";
    public static string StringTableUI = "ST_UI";
    public static string StringTableToast = "ST_Toast";
    public static string StringTableWorldView = "ST_WorldView";
    public static string StringTableStatus = "ST_Status";
    public static string StringTablePersonality = "ST_Personality";
    public static string StringTableDialogue = "ST_Dialogue";
    public static string StringTableContextDialogue = "ST_ContextDialogue";
    public static string StringTableLetter = "ST_Letter";


    LocalizedString localizeString = new LocalizedString() { TableReference = "ST_Empty", TableEntryReference = "Empty" };
    /*
    private void OnEnable()
    {
        localizeString.StringChanged += UpdateString;
    }

    private void OnDisable()
    {
        localizeString.StringChanged -= UpdateString;
    }

    void UpdateString(string translatedValue)
    {
        // Do something here
    }
    */

    public void LocalizeTextString(string tableName, string stringName, params LocalizeVariableInfo[] arguments)
    {
        localizeString.TableReference = tableName;
        localizeString.TableEntryReference = stringName;

        if (arguments != null && arguments.Length > 0)
        {
            for (int i = 0; i < arguments.Length; i++)
            {
                switch (arguments[i].variableType)
                {
                    case LocalizeVariableType.BoolVariable:
                        {
                            var argumentValue = new BoolVariable { Value = (bool)arguments[i].value };
                            localizeString.Add(arguments[i].key, argumentValue);
                        }
                        break;
                    case LocalizeVariableType.IntVariable:
                        {
                            var argumentValue = new IntVariable { Value = (int)arguments[i].value };
                            localizeString.Add(arguments[i].key, argumentValue);
                        }
                        break;
                    case LocalizeVariableType.LongVariable:
                        {
                            var argumentValue = new LongVariable { Value = (long)arguments[i].value };
                            localizeString.Add(arguments[i].key, argumentValue);
                        }
                        break;
                    case LocalizeVariableType.FloatVariable:
                        {
                            var argumentValue = new FloatVariable { Value = (float)arguments[i].value };
                            localizeString.Add(arguments[i].key, argumentValue);
                        }
                        break;
                    case LocalizeVariableType.StringVariable:
                        {
                            var argumentValue = new StringVariable { Value = (string)arguments[i].value };
                            localizeString.Add(arguments[i].key, argumentValue);
                        }
                        break;
                    case LocalizeVariableType.ObjectVariable:
                        {
                            var argumentValue = new ObjectVariable { Value = (Object)arguments[i].value };
                            localizeString.Add(arguments[i].key, argumentValue);
                        }
                        break;
                }
            }
        }
        GetComponent<LocalizeStringEvent>().StringReference = localizeString;
    }

    public void SetText(string text)
    {
        if(TryGetComponent(out TMP_Text tmpText))
        {
            tmpText.text = text;
        }
        else if(TryGetComponent(out Text Text))
        {
            Text.text = text;
        }
        else
        {
            Debug.LogWarning("[LocalizeText] Failed to SetText in "+ name + " with "+ text);
        }
    }

    public string GetText()
    {
        if (TryGetComponent(out TMP_Text tmpText))
        {
            return tmpText.text;
        }
        else if (TryGetComponent(out Text Text))
        {
            return Text.text;
        }
        return "";
    }
}