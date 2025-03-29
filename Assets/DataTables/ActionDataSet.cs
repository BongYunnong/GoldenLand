using System;
using System.Collections.Generic;
using UnityEngine;

public class ActionDataSet : SingletonScriptableObject<ActionDataSet>
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
		public EActionType ActionType;
		public List<string> ActionParameters;
		public EActionProcessType ProcessType;
		public EActionStrategyType StrategyType;
		public int Priority;
		public List<string> RequiredGameplayTags;
		public List<string> TransitionAllowedProgresses;
		public List<string> RequiredActions;
		public int RequireAmmo;
		public bool RequireCast;
		public List<string> AllowedStates;
		public string ActionSequence;
		public float PreDelay;
		public float Duration;
		public float PostDelay;
		public float CooldownTime;
		public float ActionEnterDistance;
		public float Weight;
		public List<string> ModifierIds;
	}

	public void AddData(TableData data)
	{
		datas.Add(data);
	}
}

