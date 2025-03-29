using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ConstEffectGroupInfo
{
    public string id;
    public List<string> VisualEffectIds = new List<string>();
    public List<string> SoundEffectIds = new List<string>();

    public ConstEffectGroupInfo(EffectGroupDataSet.TableData data)
    {
        this.id = data.ID;
        this.VisualEffectIds = data.VisualEffectIds;
        this.SoundEffectIds = data.SoundEffectIds;
    }
}