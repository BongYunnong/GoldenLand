using UnityEngine;

[System.Serializable]
public struct ConstCharacterInfo
{
    public string equipmentPresetId;

    public SerializableDictionary<EEquipmentType, string> defaultEquipmentIds;

    public ConstCharacterInfo(CharacterDataSet.TableData data)
    {
        equipmentPresetId = data.EquipmentPresetId;
        
        defaultEquipmentIds = new SerializableDictionary<EEquipmentType, string>();
        if (DataManager.Instance.equipmentPresetDict.TryGetValue(equipmentPresetId,
                out ConstEquipmentPresetInfo equipmentPresetInfo))
        {
            defaultEquipmentIds[EEquipmentType.Hair] = equipmentPresetInfo.Hair;
            defaultEquipmentIds[EEquipmentType.BackHair] = equipmentPresetInfo.BackHair;
            defaultEquipmentIds[EEquipmentType.Head] = equipmentPresetInfo.Head;
            defaultEquipmentIds[EEquipmentType.Face] = equipmentPresetInfo.Face;
            defaultEquipmentIds[EEquipmentType.Hat] = equipmentPresetInfo.Hat;
            defaultEquipmentIds[EEquipmentType.Accessory] = equipmentPresetInfo.Accessory;
            defaultEquipmentIds[EEquipmentType.Robe] = equipmentPresetInfo.Robe;
            defaultEquipmentIds[EEquipmentType.UpperBody] = equipmentPresetInfo.UpperBody;
            defaultEquipmentIds[EEquipmentType.LowerBody] = equipmentPresetInfo.LowerBody;
            defaultEquipmentIds[EEquipmentType.BackStuff] = equipmentPresetInfo.BackStuff;
            defaultEquipmentIds[EEquipmentType.HandStuff] = equipmentPresetInfo.HandStuff;
        }
    }
}

public class CharacterBase : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
