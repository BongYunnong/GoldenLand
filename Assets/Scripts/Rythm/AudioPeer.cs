using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioPeer : MonoBehaviour
{
    private AudioSource audio;
    private float[] samplesLeft = new float[512];
    private float[] samplesRight = new float[512];
    
    private float[] freqBand  = new float[8];
    private float[] bandBuffer  = new float[8];
    private float[] bufferDecrease = new float[8];
    
    private float[] freqBandHighest = new float[8];
    public static float[] audioBand = new float[8];
    public static float[] audioBandBuffer = new float[8];

    public static float amplitude, amplitudeBuffer;
    private float amplitudeHighest;

    public float audioProfile;

    public enum EChannel
    {
        Stereo,
        Left,
        Right
    };
    public EChannel channel = EChannel.Stereo;
    
    [SerializeField] private bool useBuffer;
    
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

        AudioProfile(audioProfile);
    }

    private void Update()
    {
        GetSpectrumAudioSource();
        MakeFrequencyBands();
        BandBuffer();
        CreateAudioBands();
        GetAmplitude();
        
        for (int i = 0; i < 512; i++)
        {
            if (sampleObjects[i] != null)
            {
                sampleObjects[i].transform.localScale = new Vector3(10, (samplesLeft[i]+samplesRight[i]) * 0.5f * maxScale + startScale ,10);
            }
        }

        for (int i = 0; i < paramObjects.Count; i++)
        {
            if (useBuffer)
            {
                paramObjects[i].transform.localScale = new Vector3(paramObjects[i].transform.localScale.x, bandBuffer[i] * maxScale + startScale , paramObjects[i].transform.localScale.z);
            }
            else
            {
                paramObjects[i].transform.localScale = new Vector3(paramObjects[i].transform.localScale.x, freqBand[i] * maxScale + startScale , paramObjects[i].transform.localScale.z);
            }
            // Color color = new Color(audioBandBuffer[i], audioBandBuffer[i], audioBandBuffer[i]);
        }
    }

    /// <summary>
    /// highest가 8개의 band 간 차이가 너무 크기 때문에 audioProfile로 최소값을 정해줌
    /// </summary>
    /// <param name="audioProfile"></param>
    void AudioProfile(float audioProfile)
    {
        for (int i = 0; i < 8; i++)
        {
            freqBandHighest[i] = audioProfile;
        }
    }

    /// <summary>
    /// 평균적인 음
    /// </summary>
    void GetAmplitude()
    {
        float currentAmplitude = 0;
        float currentAmplitudeBuffer = 0;
        for (int i = 0; i < 8; i++)
        {
            currentAmplitude += audioBand[i];
            currentAmplitudeBuffer += audioBandBuffer[i];
        }

        if (currentAmplitude > amplitudeHighest)
        {
            amplitudeHighest = currentAmplitude;
        }

        amplitude = currentAmplitude / amplitudeHighest;
        amplitudeBuffer = currentAmplitudeBuffer / amplitudeHighest;
    }

    /// <summary>
    /// 가장 높은 주파수를 가진 음을 찾을 수 있음
    /// </summary>
    void CreateAudioBands()
    {
        for (int i = 0; i < 8; i++)
        {
            if (freqBand[i] > freqBandHighest[i])
            {
                freqBandHighest[i] = freqBand[i];
            }

            audioBand[i] = (freqBand[i] / freqBandHighest[i]);
            audioBandBuffer[i] = (bandBuffer[i] / freqBandHighest[i]);
        }
    }

    void GetSpectrumAudioSource()
    {
        audio.GetSpectrumData(samplesLeft, 0, FFTWindow.Blackman);
        audio.GetSpectrumData(samplesRight, 1, FFTWindow.Blackman);
    }

    void BandBuffer()
    {
        for (int i = 0; i < 8; i++)
        {
            if (freqBand[i] > bandBuffer[i])
            {
                bandBuffer[i] = freqBand[i];
                bufferDecrease[i] = 0.005f;
            }
            if (freqBand[i] < bandBuffer[i])
            {
                bandBuffer[i] -= bufferDecrease[i];
                bufferDecrease[i] *= 1.2f;
            }
        }
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
                if (channel == EChannel.Stereo)
                {
                    average += (samplesLeft[count]+samplesRight[count]) * 0.5f * (count + 1);
                }
                else if (channel == EChannel.Left)
                {
                    average += samplesLeft[count] * (count + 1);
                }
                else if (channel == EChannel.Right)
                {
                    average += samplesRight[count] * (count + 1);
                }
                count++;
            }
            average /= count;
            freqBand[i] = average * 10;
        }
    }
}
