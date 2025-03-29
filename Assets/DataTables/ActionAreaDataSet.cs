using System;
using System.Collections.Generic;
using UnityEngine;

public class ActionAreaDataSet : SingletonScriptableObject<ActionAreaDataSet>
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
		public EActionSpaceType ActionSpaceType;
		public EActionAreaType ActionAreaType;
		public float AngleDeltaSize;
		public float Duration;
		public List<Vector2> Points;
	}

	public void AddData(TableData data)
	{
		datas.Add(data);
	}
}

