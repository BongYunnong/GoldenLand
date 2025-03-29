using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterDataSet : SingletonScriptableObject<CharacterDataSet>
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
		public SpritePath ProfileImage;
		public SpritePath StandingImage;
		public string PersonalColor;
		public string BookSeriesId;
		public string EquipmentPresetId;
		public AssetPath<UnityEngine.Timeline.TimelineAsset> IntroTimeline;
	}

	public void AddData(TableData data)
	{
		datas.Add(data);
	}
}

