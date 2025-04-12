using UnityEngine;
using UnityEngine.Events;

public class TimingManager : SingletonMonoBehavior<TimingManager>
{
    [SerializeField] private int bpm = 120;
    private double elapsedTime = 0d;
    private int beatFrameCount = 0;
    
    public UnityAction<int> beatFrameUpdated;
    
    private void Start()
    {
        beatFrameUpdated += HandleBeatFrameUpdated;
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;
        double bpmTimeUnit = 60d / bpm;
        int currentFrame = (int)(elapsedTime / bpmTimeUnit);
        if (currentFrame != beatFrameCount)
        {
            beatFrameCount = currentFrame;
            beatFrameUpdated.Invoke(beatFrameCount);
        }
    }

    private void HandleBeatFrameUpdated(int frame)
    {
        /*
        foreach (var character in characters)
        {
            for (int i = 0; i < character.Value.Count; i++)
            {
                if (instigator == character.Value[i]) continue;
                character.Value[i].PlayIdleAnim();
            }
        }
        */
    }
}
