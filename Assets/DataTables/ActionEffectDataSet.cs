using System;
using System.Collections.Generic;
using UnityEngine;

public class ActionEffectDataSet : SingletonScriptableObject<ActionEffectDataSet>
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
		public ETargetFilterType TargetFilterType;
		public bool IsGuardBreak;
		public bool IsStun;
		public bool IsStrike;
		public bool IsBound;
		public float StaggerTime;
		public EKnockbackDirectionType KnockbackDirectionType;
		public Vector2 KnockbackForce;
		public EActionSpaceType KnockbackSpaceType;
		public Vector2 KnockbackOffset;
	}

	public void AddData(TableData data)
	{
		datas.Add(data);
	}
}

