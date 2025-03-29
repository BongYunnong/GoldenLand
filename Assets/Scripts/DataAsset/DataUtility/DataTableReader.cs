using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class DataTableReader : MonoBehaviour
{
    static string SPLIT_RE = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
    static string LINE_SPLIT_RE = @"\r\n|\n\r|\n|\r";
    static char[] TRIM_CHARS = { '\"' };


    public static List<Dictionary<string, object>> Read(TextAsset data, out string pirmaryKeyType)
    {
        return ReadInternal(data, out string[] header, out string[] types, out pirmaryKeyType);
    }

    public static List<Dictionary<string, object>> Read(TextAsset data, out string[] header, out string[] types, out string pirmaryKeyType)
    {
        return ReadInternal(data, out header, out types, out pirmaryKeyType);
    }

    public static List<Dictionary<string, object>> ReadInternal(TextAsset data, out string[] header, out string[] types, out string pirmaryKeyType)
    {
        var list = new List<Dictionary<string, object>>();

        header = new string[] { };
        types = new string[] { };
        pirmaryKeyType = "int";

        if (data == null)
        {
            Debug.LogError($"{nameof(DataTableReader)} : TextAsset is Null");
            return null;
        }

        var lines = Regex.Split(data.text, LINE_SPLIT_RE);

        if (lines.Length <= 1)
            return list;

        header = Regex.Split(lines[0], SPLIT_RE);
        types = Regex.Split(lines[1], SPLIT_RE);

        pirmaryKeyType = types[0];

        for (var i = 2; i < lines.Length; i++)
        {
            var values = Regex.Split(lines[i], SPLIT_RE);
            if (values.Length == 0 || values[0] == "")
                continue;

            var entry = new Dictionary<string, object>();

            for (var j = 0; j < header.Length && j < values.Length; j++)
            {
                string typeString = types[j];
                Type type = Type.GetType(typeString);
                string value = values[j];
                value = value.TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "");

                object finalvalue = value;
                
                if (typeString.Contains("List"))
                {
                    string pattern = @"\<(.+)\>";
                    Regex regex = new Regex(pattern);
                    MatchCollection mc = regex.Matches(typeString);

                    string[] valueParams = value.Split(';',StringSplitOptions.RemoveEmptyEntries);
                    foreach (Match m in mc)
                    {
                        Group group = m.Groups[1];
                        string innerType = group.Value;
                        if (innerType == nameof(SpritePath))
                        {
                            finalvalue = ParseCellValueList<SpritePath>(innerType, value);
                        }
                        else if (innerType == "Vector2")
                        {
                            finalvalue = ParseCellValueList<Vector2>(innerType, value);
                        }
                        else if (innerType == "string")
                        {
                            finalvalue = ParseCellValueList<string>(innerType, value);
                        }
                        else if (innerType == "bool")
                        {
                            finalvalue = ParseCellValueList<bool>(innerType, value);
                        }
                        else if (innerType == "int")
                        {
                            finalvalue = ParseCellValueList<int>(innerType, value);
                        }
                        else if (innerType == "float")
                        {
                            finalvalue = ParseCellValueList<float>(innerType, value);
                        }
                        else
                        {
                            finalvalue = ParseCellValueList<object>(innerType, value);
                        }
                        break;
                    }
                }
                else
                {
                    finalvalue = ParseCellValue<object>(typeString, value);
                }

                entry[header[j]] = finalvalue;
            }

            list.Add(entry);
        }

        return list;
    }
    T GetValue<T>(string name)
    {
        return (T)Convert.ChangeType(name, typeof(T));
    }

    public static List<T> ParseCellValueList<T>(string typeString, string value)
    {
        string[] valueParams = value.Split(';', StringSplitOptions.RemoveEmptyEntries);
        List<T> result = new List<T>();
        for (int k = 0; k < valueParams.Length; k++)
        {
            result.Add(ParseCellValue<T>(typeString, valueParams[k].Trim()));
        }
        return result;

    }
    public static T ParseCellValue<T>(string typeString, string value)
    {
        object finalvalue = null;
        // Debug.Log(typeString + " / "+ value);
        if (typeString == nameof(Vector2))
        {
            string[] splitted = value.Split('/', StringSplitOptions.RemoveEmptyEntries);
            finalvalue = new Vector2(float.Parse(splitted[0]), float.Parse(splitted[1]));
        }
        else if (typeString == nameof(SpritePath))
        {
            finalvalue = new SpritePath(value.Split('='));
        }
        else if (typeString.Contains("AssetPath"))
        {
            string pattern = @"\<(.+)\>";
            Regex regex = new Regex(pattern);
            MatchCollection mc = regex.Matches(typeString);
            foreach (Match m in mc)
            {
                Group a = m.Groups[1];
                if (a.Value == "UnityEngine.Tilemaps.TileBase")
                {
                    finalvalue = new AssetPath<TileBase>(value);
                    break;
                }
                else if (a.Value == "GameObject")
                {
                    finalvalue = new AssetPath<GameObject>(value);
                    break;
                }
                else if (a.Value == "UnityEngine.Timeline.TimelineAsset")
                {
                    finalvalue = new AssetPath<UnityEngine.Timeline.TimelineAsset>(value);
                    break;
                }
                else if (a.Value == "AnimationClip")
                {
                    finalvalue = new AssetPath<AnimationClip>(value);
                    break;
                }
            }
        }
        else if (typeString == "bool")
        {
            finalvalue = value == "TRUE" || value.ToUpper() == "1";
        }
        else if (typeString == "string")
        {
            finalvalue = value;
        }
        else if (typeString == "int")
        {
            if (int.TryParse(value, out int result))
            {
                finalvalue = result;
            }
            else
            {
                finalvalue = 0;
            }
        }
        else if (typeString == "float")
        {
            if (float.TryParse(value, out float result))
            {
                finalvalue = result;
            }
            else
            {
                finalvalue = 0;
            }
        }
        else if (typeString.StartsWith("E") && TryParseEnum(Type.GetType(typeString), value, out object enumValue))
        {
            finalvalue = enumValue;
        }
        return (T)finalvalue;
    }


    private static bool TryParseEnum(Type type, string value, out object result)
    {
        if (type != null && Enum.TryParse(type, value, out object parseResult))
        {
            result = parseResult;
        }
        else
        {
            result = null;
        }

        return result != null;
    }
}