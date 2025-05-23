using UnityEngine;

public static class Tables
{
	public static EquipmentPresetDataSet EquipmentPresetDataSet;

	static Tables()
	{
		if (EquipmentPresetDataSet == null)
			EquipmentPresetDataSet = Load<EquipmentPresetDataSet>();

	}

	public static T Load<T>() where T : ScriptableObject
	{
		T[] asset = Resources.LoadAll<T>("");

		if (asset == null || asset.Length != 1)
			throw new System.Exception($"{nameof(Tables)} : Tables Load Error");

		return asset[0];
	}
}
