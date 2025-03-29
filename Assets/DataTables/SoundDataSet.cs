using System;
using System.Collections.Generic;
using UnityEngine;

public class SoundDataSet : SingletonScriptableObject<SoundDataSet>
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
		public List<string> SoundPaths;
		public float Volume;
		public float Pitch;
		public float MaxDistance;
		public bool Loop;
		public string Description;
	}

	public void AddData(TableData data)
	{
		datas.Add(data);
	}
}

