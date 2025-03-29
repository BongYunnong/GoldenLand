using System.Collections.Generic;
using UnityEngine;

public class GhostTraceInfo
{
    public float duraiton;
    public GhostTrail ghostTrailRef;
    public float frequency;
    public float accTimeForFrequency;

    public GhostTraceInfo(float duraiton, GhostTrail ghostTrail, float frequency)
    {
        ghostTrailRef = ghostTrail;
        this.duraiton = duraiton;
        this.frequency = frequency;
        accTimeForFrequency = 0;
    }
}


public class GhostTrailController : SingletonMonoBehavior<GhostTrailController>
{
    // key는 instanceId
    Dictionary<int, GhostTraceInfo> ghostTraceStimulusValues = new Dictionary<int, GhostTraceInfo>();
    
    public void AddGhostTrace(GhostTrail targetGhostTrail, float duration, float frequency = 0)
    {
        int instanceID = targetGhostTrail.GetInstanceID();
        if (ghostTraceStimulusValues.ContainsKey(instanceID))
        {
            ghostTraceStimulusValues[instanceID].duraiton = Mathf.Max(ghostTraceStimulusValues[instanceID].duraiton, duration);
        }
        else
        {
            ghostTraceStimulusValues.Add(instanceID, new GhostTraceInfo(duration, targetGhostTrail, frequency));
        }
        targetGhostTrail.CreateNewGhostImage();
    }

    private void Update()
    {
        List<int> expiredInstanceKeys = new List<int>();
        foreach(var ghostTraceStimlusPair in ghostTraceStimulusValues)
        {
            GhostTraceInfo ghostTraceInfo = ghostTraceStimlusPair.Value;
            ghostTraceInfo.accTimeForFrequency += Time.deltaTime;
            ghostTraceInfo.duraiton -= Time.deltaTime;

            if (ghostTraceInfo.duraiton <= 0 || ghostTraceInfo.ghostTrailRef == null)
            {
                expiredInstanceKeys.Add(ghostTraceStimlusPair.Key);
            }
            else if (ghostTraceInfo.frequency > 0 && ghostTraceInfo.accTimeForFrequency > ghostTraceInfo.frequency)
            {
                ghostTraceInfo.accTimeForFrequency -= ghostTraceInfo.frequency;
                ghostTraceInfo.ghostTrailRef.CreateNewGhostImage();
            }
        }
        for(int i=0;i< expiredInstanceKeys.Count;i++)
        {
            ghostTraceStimulusValues.Remove(expiredInstanceKeys[i]);
        }
    }
}
