using System;
using System.Collections.Generic;
using UnityEngine;

public class NoteManager : MonoBehaviour
{
    [SerializeField] private Transform noteSpawnTransform;
    [SerializeField] private Transform noteEndTransform;
    [SerializeField] private GameObject notePrefab;
    
    private List<Transform> spawnedNotes = new List<Transform>();

    [SerializeField] private Transform center = null;
    [SerializeField] private RectTransform[] timingRect = null;
    private Vector2[] timingBoxes = null;
    
    [SerializeField] private AudioSource audioSource = null;
    [SerializeField] private List<AudioClip> audioClips = new List<AudioClip>();

    [SerializeField] private AudioSource musicSource = null;
    [SerializeField] private Transform lightTransform = null;
    
    private void Start()
    {
        timingBoxes = new Vector2[timingRect.Length];
        for (int i = 0; i < timingRect.Length; i++)
        {
            timingBoxes[i].Set(center.localPosition.x - timingRect[i].rect.width * 0.5f,
                            center.localPosition.x + timingRect[i].rect.width * 0.5f);
        }

        TimingManager.Instance.beatFrameUpdated += HandleBeatFrameUpdated;
        PlayerController.Instance.OnClickedCharacter += HandleClickedCharacter;
    }

    void Update()
    {
        for (int i = spawnedNotes.Count - 1; i >=0 ; i--)
        {
            if (spawnedNotes[i].position.x > noteEndTransform.position.x)
            {
                Destroy(spawnedNotes[i].gameObject);
                spawnedNotes.RemoveAt(i);
            }
        }
    }

    private void HandleBeatFrameUpdated(int beatFrame)
    {
        GameObject spawnedNote = Instantiate(notePrefab, noteSpawnTransform.position, Quaternion.identity);
        spawnedNote.transform.SetParent(transform);
        spawnedNotes.Add(spawnedNote.transform);
    }
    
    private int CheckTiming()
    {
        for (int i = 0; i < spawnedNotes.Count; i++)
        {
            float notePosX = spawnedNotes[i].transform.localPosition.x;
            for (int x = 0; x < timingBoxes.Length; x++)
            {
                if (timingBoxes[x].x <= notePosX && notePosX <= timingBoxes[x].y)
                {
                    Debug.Log("Hit " + x);
                    Destroy(spawnedNotes[i].gameObject);
                    spawnedNotes.RemoveAt(i);
                    
                    audioSource.clip = audioClips[i];
                    audioSource.Play();
                    return x;
                }
            }
        }
        Debug.Log("Miss");
        return -1;
    }
    
    private void HandleClickedCharacter(Character character)
    {
        if (character.TryGetComponent(out IAudioPlayable audioPlayable))
        {
            int level = CheckTiming();
            if (level >= 0)
            {
                audioPlayable.PlayAudio();
                // audioPlayable.PlayAudioWithLevel(level);
                lightTransform.transform.position = character.transform.position + Vector3.up * 6;

                if (musicSource.isPlaying == false)
                {
                    musicSource.Play();
                }
            }
            else
            {
                lightTransform.transform.position = new Vector3(0, 6, -2);
            }
        }
    }
}
