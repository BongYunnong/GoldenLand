using System;
using System.Collections.Generic;
using UnityEngine;

public class WeaponDataSet : SingletonScriptableObject<WeaponDataSet>
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
		public EAttackObjectType AttackObjectType;
		public AssetPath<GameObject> WeaponPrefab;
		public string Collider;
		public List<SpritePath> Sprites;
		public List<string> AnimStrings;
		public List<string> StoredSocketSettings;
		public List<string> IdleSocketSettings;
		public List<string> ActionSocketSettings;
	}

	public void AddData(TableData data)
	{
		datas.Add(data);
	}
}

