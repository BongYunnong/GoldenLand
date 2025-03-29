using System;
using System.Collections.Generic;
using UnityEngine;

public class ActionModifierDataSet : SingletonScriptableObject<ActionModifierDataSet>
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
		public EActionModifierType ActionModifierType;
		public EActionStartPositionType StartPositionType;
		public EActionTargetPositionType TargetPositionType;
		public EActionFlipType ActionFlipType;
		public List<string> Parameters;
	}

	public void AddData(TableData data)
	{
		datas.Add(data);
	}
}

