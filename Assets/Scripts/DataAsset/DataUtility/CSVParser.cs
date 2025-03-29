using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class CSVParser
{
    public static void WriteCSV(string name, List<string[]> strings, string path= "")
    {
        string filePath = path;
        if (string.IsNullOrEmpty(filePath))
        {
            filePath = GetPath(name);
        }

        int length = strings.Count;
        string delimiter = ",";

        StringBuilder sb = new StringBuilder();
        for (int index = 0; index < length; index++)
        {
            sb.Append(string.Join(delimiter, strings[index]));
        }

        StreamWriter outStream = System.IO.File.CreateText(filePath);
        outStream.WriteLine(sb);
        outStream.Close();
    }

    public static void WriteCSV(string name, string[][] strings, string path = "")
    {
        string filePath = path;
        if (string.IsNullOrEmpty(filePath))
        {
            filePath = GetPath(name);
        }

        int length = strings.Length;
        string delimiter = ",";

        StringBuilder sb = new StringBuilder();
        for (int index = 0; index < length; index++)
        {
            sb.AppendLine(string.Join(delimiter, strings[index]));
        }

        StreamWriter outStream = System.IO.File.CreateText(filePath);
        outStream.WriteLine(sb);
        outStream.Close();
    }

    private static string GetPath(string name)
    {
#if UNITY_EDITOR
        return Application.dataPath + "/DataTableFromString/" + name + ".csv";
#elif UNITY_ANDROID
        return Application.persistentDataPath + name + ".csv";
#elif UNITY_IPHONE
        return Application.persistentDataPath+"/" + name + ".csv";
#else
        return Application.dataPath +"/" + name + ".csv";
#endif
    }
}
