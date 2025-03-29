using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ConstAnimationClipInfo
{
    public string ID;
    public AnimationClip AnimationClip;
    
    public ConstAnimationClipInfo(AnimationClipDataSet.TableData data)
    {
        this.ID = data.ID;
        this.AnimationClip = data.ClipPath.GetValue();
    }
}