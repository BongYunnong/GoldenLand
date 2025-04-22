using UnityEngine;

public static class Tables
{
	public static AnimationClipDataSet AnimationClipDataSet;
	public static EffectGroupDataSet EffectGroupDataSet;
	public static EquipmentDataSet EquipmentDataSet;
	public static EquipmentPresetDataSet EquipmentPresetDataSet;
	public static ExtraGameObjectDataSet ExtraGameObjectDataSet;
	public static ExtraInfoDataSet ExtraInfoDataSet;
	public static SoundDataSet SoundDataSet;
	public static VisualEffectDataSet VisualEffectDataSet;

	static Tables()
	{
		if (AnimationClipDataSet == null)
			AnimationClipDataSet = Load<AnimationClipDataSet>();

		if (EffectGroupDataSet == null)
			EffectGroupDataSet = Load<EffectGroupDataSet>();

		if (EquipmentDataSet == null)
			EquipmentDataSet = Load<EquipmentDataSet>();

		if (EquipmentPresetDataSet == null)
			EquipmentPresetDataSet = Load<EquipmentPresetDataSet>();

		if (ExtraGameObjectDataSet == null)
			ExtraGameObjectDataSet = Load<ExtraGameObjectDataSet>();

		if (ExtraInfoDataSet == null)
			ExtraInfoDataSet = Load<ExtraInfoDataSet>();

		if (SoundDataSet == null)
			SoundDataSet = Load<SoundDataSet>();

		if (VisualEffectDataSet == null)
			VisualEffectDataSet = Load<VisualEffectDataSet>();
	}

	public static T Load<T>() where T : ScriptableObject
	{
		T[] asset = Resources.LoadAll<T>("");

		if (asset == null || asset.Length != 1)
			throw new System.Exception($"{nameof(Tables)} : Tables Load Error");

		return asset[0];
	}
}
