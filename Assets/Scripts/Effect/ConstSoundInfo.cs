using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class ConstSoundInfo
{
    public string id;
    public List<AudioClip> clips = new List<AudioClip>();

    [Range(0f, 1f)]
    public float volume = 1;
    [Range(0.1f, 3f)]
    public float pitch = 1;
    [Range(5f, 50f)]
    public float maxDistance = 5;

    public bool loop;

    public ConstSoundInfo(SoundDataSet.TableData data)
    {
        this.id = data.ID;
        clips.Clear();
        for (int i = 0; i < data.SoundPaths.Count; i++)
        {
            AssetPath<AudioClip> soundPath = new AssetPath<AudioClip>(data.SoundPaths[i]);
            AudioClip audioClip = soundPath.GetValue();
            if(audioClip == null)
            {
                continue;
            }
            clips.Add(audioClip);
        }
        this.volume = data.Volume;
        this.pitch = data.Pitch;
        this.maxDistance = data.MaxDistance;
        this.loop = data.Loop;
    }
}

[System.Serializable]
public class SoundInfo
{
    public string id;
    public AudioSource source;

    public SoundInfo(string id, AudioSource source)
    {
        this.id = id;
        this.source = source;
    }
}