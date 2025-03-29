using UnityEngine;

public static class Tables
{
	public static ActionAreaDataSet ActionAreaDataSet;
	public static ActionDataSet ActionDataSet;
	public static ActionEffectDataSet ActionEffectDataSet;
	public static ActionInputDataSet ActionInputDataSet;
	public static ActionModifierDataSet ActionModifierDataSet;
	public static AnimationClipDataSet AnimationClipDataSet;
	public static BookDataSet BookDataSet;
	public static EffectGroupDataSet EffectGroupDataSet;
	public static EquipmentDataSet EquipmentDataSet;
	public static EquipmentPresetDataSet EquipmentPresetDataSet;
	public static ExtraGameObjectDataSet ExtraGameObjectDataSet;
	public static ExtraInfoDataSet ExtraInfoDataSet;
	public static SoundDataSet SoundDataSet;
	public static StatusEffectDataSet StatusEffectDataSet;
	public static VisualEffectDataSet VisualEffectDataSet;
	public static WeaponDataSet WeaponDataSet;

	static Tables()
	{
		if (ActionAreaDataSet == null)
			ActionAreaDataSet = Load<ActionAreaDataSet>();

		if (ActionDataSet == null)
			ActionDataSet = Load<ActionDataSet>();

		if (ActionEffectDataSet == null)
			ActionEffectDataSet = Load<ActionEffectDataSet>();

		if (ActionInputDataSet == null)
			ActionInputDataSet = Load<ActionInputDataSet>();

		if (ActionModifierDataSet == null)
			ActionModifierDataSet = Load<ActionModifierDataSet>();

		if (AnimationClipDataSet == null)
			AnimationClipDataSet = Load<AnimationClipDataSet>();

		if (BookDataSet == null)
			BookDataSet = Load<BookDataSet>();

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

		if (StatusEffectDataSet == null)
			StatusEffectDataSet = Load<StatusEffectDataSet>();

		if (VisualEffectDataSet == null)
			VisualEffectDataSet = Load<VisualEffectDataSet>();

		if (WeaponDataSet == null)
			WeaponDataSet = Load<WeaponDataSet>();

	}

	public static T Load<T>() where T : ScriptableObject
	{
		T[] asset = Resources.LoadAll<T>("");

		if (asset == null || asset.Length != 1)
			throw new System.Exception($"{nameof(Tables)} : Tables Load Error");

		return asset[0];
	}
}
