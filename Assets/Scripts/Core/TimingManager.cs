using UnityEngine;
using UnityEngine.Events;

public class TimingManager : SingletonMonoBehavior<TimingManager>
{
    [SerializeField] private int bpm = 120;
    private double currentTime = 0d;
    private int beatFrameCount = 0;
    
    public UnityAction<int> beatFrameUpdated;
    
    private void Start()
    {
        beatFrameUpdated += HandleBeatFrameUpdated;
    }

    private void Update()
    {
        currentTime += Time.deltaTime;
        if (currentTime >= 60d / bpm)
        {
            currentTime -= 60d / bpm;
            beatFrameCount++;
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
