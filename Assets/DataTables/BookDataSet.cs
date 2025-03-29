using System;
using System.Collections.Generic;
using UnityEngine;

public class BookDataSet : SingletonScriptableObject<BookDataSet>
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
		public string CharacterId;
		public string EquipmentPresetId;
		public SpritePath CoverImagePath;
		public string Tool;
		public float AttackRapidCooldownTime;
		public List<string> ActionKeyBinding;
		public string PatternGroupId;
		public List<string> PersonalityIds;
	}

	public void AddData(TableData data)
	{
		datas.Add(data);
	}
}

