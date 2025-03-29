using System;
using System.Collections.Generic;
using UnityEngine;

public class VisualEffectDataSet : SingletonScriptableObject<VisualEffectDataSet>
{
	[SerializeField]
	public List<TableData> datas = new List<TableData>();

	public TableData this[string index]
	{
		get
		{
			return datas.Find(x => x.ID == index);
		}
	}

	[Serializable]
	public class TableData
	{
		public string ID;
		public AssetPath<GameObject> EffectPaths;
		public string Description;
	}

	public void AddData(TableData data)
	{
		datas.Add(data);
	}
}

