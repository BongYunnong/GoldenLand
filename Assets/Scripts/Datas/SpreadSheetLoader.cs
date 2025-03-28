using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using static UnityEngine.Rendering.DebugUI;

public class SpreadSheetLoader : MonoBehaviour
{
    [System.Serializable]
    public struct SheetInfo
    {
        public string SheetName;
        public string Range;
        public long SheetID;
    }
    [SerializeField] private string SpreadSheetAddress = "https://docs.google.com/spreadsheets/d/1DcQNWDN4sXsmogywtkdz2L-PdPa5Wki7s91Gu4Jhujc";
    [SerializeField] private List<SheetInfo> SheetInfos = new List<SheetInfo>();
    [SerializeField] private string Path = "/DataTables/";

    public static string GetTSVAddress(string address, string range, long sheetID)
    {
        return $"{address}/export?format=tsv&range={range}&gid={sheetID}";
    }

    public void LoadSpreadSheet()
    {
        StartCoroutine(LoadData());
    }

    private IEnumerator LoadData()
    {
        Debug.Log("[SpreadSheetLoad] Started");
        for (int i = 0; i < SheetInfos.Count; i++)
        {
            Debug.Log($"[SpreadSheetLoad] Loading {SheetInfos[i].SheetName}");
            using (UnityWebRequest www = UnityWebRequest.Get(GetTSVAddress(SpreadSheetAddress, SheetInfos[i].Range, SheetInfos[i].SheetID)))
            {
                yield return www.SendWebRequest();
                if(www.isDone)
                {
                    Debug.Log($"[SpreadSheetLoad] Loaded {SheetInfos[i].SheetName}");
                    WriteCSV(SheetInfos[i].SheetName, www.downloadHandler.text);
                    Debug.Log($"[SpreadSheetLoad] CSV Written {SheetInfos[i].SheetName}");
                }
            }
        }
        yield return new WaitForSeconds(0.1f);
        Debug.Log("[SpreadSheetLoad] Finished");
    }

    private void WriteCSV(string fileName, string value)
    {
        List<string[]> datas = new List<string[]>();

        string[] rows = value.Split('\n');
        for (int i = 0; i < rows.Length; i++)
        {
            datas.Add(rows[i].Split('\t'));
        }
        CSVParser.WriteCSV(fileName, datas, GetPath(fileName));
    }
    private string GetPath(string name)
    {
        return Application.dataPath + Path + name + ".csv";
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(SpreadSheetLoader))]
public class SpreadSheetLoaderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        SpreadSheetLoader spreadSheetLoader = (SpreadSheetLoader)target;

        base.OnInspectorGUI();

        EditorGUILayout.BeginHorizontal(); GUILayout.FlexibleSpace();
        if (GUILayout.Button("Load SpreadSheet", GUILayout.Width(160), GUILayout.Height(30)))
        {
            spreadSheetLoader.LoadSpreadSheet();
        }
        GUILayout.FlexibleSpace(); EditorGUILayout.EndHorizontal();
    }
}
#endif