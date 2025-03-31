using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioPeer : MonoBehaviour
{
    private AudioSource audio;
    public static float[] samples = new float[512];
    public static float[] freqBand  = new float[8];

    [SerializeField] private GameObject sampleObjectPrefab;
    private GameObject[] sampleObjects = new GameObject[512];
    [SerializeField] private float maxScale = 10;
    [SerializeField] private float startScale = 2;
    
    [SerializeField] private List<GameObject> paramObjects = new List<GameObject>();

    private void Start()
    {
        audio = GetComponent<AudioSource>();

        for (int i = 0; i < 512; i++)
        {
            GameObject instantiatedObject = Instantiate(sampleObjectPrefab, transform);
            instantiatedObject.transform.position = transform.position;;
            instantiatedObject.transform.SetParent(transform);
            instantiatedObject.name = "SampleCube_" + i;
            transform.eulerAngles = new Vector3(0, -0.703125f * i, 0);
            instantiatedObject.transform.position = Vector3.forward * 100;
            sampleObjects[i] = instantiatedObject;
        }
    }

    private void Update()
    {
        GetSpectrumAudioSource();
        MakeFrequencyBands();

        for (int i = 0; i < 512; i++)
        {
            if (sampleObjects[i] != null)
            {
                sampleObjects[i].transform.localScale = new Vector3(10, samples[i] * maxScale + startScale ,10);
            }
        }

        for (int i = 0; i < paramObjects.Count; i++)
        {
            paramObjects[i].transform.localScale = new Vector3(paramObjects[i].transform.localScale.x, freqBand[i] * maxScale + startScale , paramObjects[i].transform.localScale.z);
        }
    }

    void GetSpectrumAudioSource()
    {
        audio.GetSpectrumData(samples, 0, FFTWindow.Blackman);
    }

    void MakeFrequencyBands()
    {
        /*
         * 22050 / 512 = 43 hertz per sample
         * 20 - 60 hertz
         * 60 - 250 hertz
         * 250 - 500 hertz
         * 500 - 2000 hertz
         * 2000 - 4000 hertz
         * 4000 - 6000 hertz
         * 6000 - 20000 hertz
         *
         * 0 - 2 = 86hertz
         * 1 - 4 = 172 hertz - 87-258
         * 2 - 8 = 344 hertz - 259-602
         * 3 - 16 = 688 hertz - 603-1290 
         * 4 - 32 = 1376 hertz - 1291-2666
         * 5 - 64 = 2752 hertz - 2667-5418
         * 6 - 128 = 5504 hertz - 5419-10922
         * 7 - 256 = 11800 hertz - 10923-21930
         * 510
         */
        int count = 0;
        for (int i = 0; i < 8; i++)
        {
            float average = 0;
            int sampleCount = (int)Mathf.Pow(2, i) * 2;
            if (i == 7)
            {
                sampleCount += 2;
            }

            for (int j = 0; j < sampleCount; j++)
            {
                average += samples[count] * (count + 1);
                count++;
            }
            average /= count;
            freqBand[i] = average * 10;
        }
    }
}
