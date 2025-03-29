using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ConstEquipmentPresetInfo
{
    public string id;
    public string Hair;
    public string BackHair;
    public string Head;
    public string Face;
    public string Hat;
    public string Accessory;
    public string Robe;
    public string UpperBody;
    public string LowerBody;
    public string BackStuff;
    public string HandStuff;
/*
    public ConstEquipmentPresetInfo(EquipmentPresetDataSet.TableData data)
    {
        this.id = data.ID;
        this.Hair = data.Hair;
        this.BackHair = data.BackHair;
        this.Head = data.Head;
        this.Face = data.Face;
        this.Hat = data.Hat;
        this.Accessory = data.Accessory;
        this.Robe = data.Robe;
        this.UpperBody = data.UpperBody;
        this.LowerBody = data.LowerBody;
        this.BackStuff = data.BackStuff;
        this.HandStuff = data.HandStuff;
    }
    */
}


[System.Serializable]
public class ConstEquipmentInfo
{
    public string id;
    public EEquipmentType type;

    public List<Sprite> sprites = new List<Sprite>();
    public List<string> effects = new List<string>();
/*
    public ConstEquipmentInfo(EquipmentDataSet.TableData data)
    {
        this.id = data.ID;
        this.type = data.EquipmentType;

        for(int i=0;i< data.Sprites.Count;i++)
        {
            if (string.IsNullOrWhiteSpace(data.Sprites[i]) || string.IsNullOrEmpty(data.Sprites[i]))
            {
                this.sprites.Add(null);
                continue;
            }
            SpritePath spritePath = new SpritePath(data.Sprites[i].Split('='));
            this.sprites.Add(spritePath.GetSprite());
        }
        this.effects = data.Effects;
    }
    */
}

