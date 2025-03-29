using System;
using System.Collections.Generic;
using UnityEngine;

public class ActionInputDataSet : SingletonScriptableObject<ActionInputDataSet>
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
		public EInputAction InputAction;
	}

	public void AddData(TableData data)
	{
		datas.Add(data);
	}
}

