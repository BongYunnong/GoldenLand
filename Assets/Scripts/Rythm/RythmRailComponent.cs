using System.Collections.Generic;
using UnityEngine;

public class RythmRailComponent : MonoBehaviour
{
    [SerializeField] private int maxRailLength = 5;
    private List<IRythmPlayable> RythmPlayables = new List<IRythmPlayable>();
    
    void Start()
    {
        for (int i = 0; i < maxRailLength; i++)
        {
            RythmPlayables.Add(null);
        }
        TimingManager.Instance.beatFrameUpdated += HandleBeatFrameUpdated;
    }
    
    private void HandleBeatFrameUpdated(int beatFrame)
    {
        for (int i = 0; i < RythmPlayables.Count; i++)
        {
            IRythmPlayable rythmPlayable = RythmPlayables[i];
            if (rythmPlayable != null)
            {
                rythmPlayable.PlayAudio();
            }
        }
    }

    public void AddRythmPlayable(int index, IRythmPlayable rythmPlayable)
    {
        if (index < 0 || RythmPlayables.Count <= index)
        {
            return;
        }
        RythmPlayables[index] = rythmPlayable;
    }
}
