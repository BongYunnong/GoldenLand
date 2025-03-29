using System;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffectDataSet : SingletonScriptableObject<StatusEffectDataSet>
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
		public EStatusEffectType EffectType;
		public SpritePath IconPath;
		public AssetPath<GameObject> StatusEffectPrefab;
		public float Lifetime;
		public bool WithdrawAtEnd;
		public bool Stackable;
		public bool Show;
		public List<string> EffectParams;
	}

	public void AddData(TableData data)
	{
		datas.Add(data);
	}
}

