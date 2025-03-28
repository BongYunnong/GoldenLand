using System;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentDataSet : SingletonScriptableObject<EquipmentDataSet>
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
		public EEquipmentType EquipmentType;
		public List<string> Sprites;
		public List<string> Effects;
	}

	public void AddData(TableData data)
	{
		datas.Add(data);
	}
}

