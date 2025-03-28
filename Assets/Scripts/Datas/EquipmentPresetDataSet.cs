using System;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentPresetDataSet : SingletonScriptableObject<EquipmentPresetDataSet>
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
		public string Hair;
		public string BackHair;
		public string Head;
		public string Face;
		public string Hat;
		public string Accessory;
		public string Robe;
		public string UpperBody;
		public string LowerBody;
		public string BackStuff;
		public string HandStuff;
	}

	public void AddData(TableData data)
	{
		datas.Add(data);
	}
}

