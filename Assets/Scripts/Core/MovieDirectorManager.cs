using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Playables;
using UnityEngine.Rendering;
using UnityEngine.Events;
using System;
using UnityEngine.Serialization;

public struct HighlightCharacterInfo
{
    public string originSortingLayerName;
    public int originOrderInLayer;
}

public class MovieDirectorManager : MonoBehaviour
{
    #region Variables & Initializer
    private static MovieDirectorManager instance = null;
    public static MovieDirectorManager GetInstance()
    {
        if (instance == null)
        {
            instance = FindFirstObjectByType<MovieDirectorManager>();
        }
        return instance;
    }

    [SerializeField] private float TimeChangingSpeed = 0;

    [Range(0, 1)]
    public float GlobalTime;
    
    private Coroutine timeScaleCoroutine = null;

    [SerializeField] private GameObject LoadingCircle = null;
    #endregion

    private void Update()
    {
        GlobalTime += Time.deltaTime * TimeChangingSpeed;
        if (GlobalTime >= 1) GlobalTime -= 1;
    }

    public void SetTimeScale(float scale, float speed, float exitTime)
    {
        if (timeScaleCoroutine != null)
        {
            StopCoroutine(timeScaleCoroutine);
        }

        timeScaleCoroutine = StartCoroutine(SetTimeScaleCoroutine(scale, speed, exitTime));
    }
    
    IEnumerator SetTimeScaleCoroutine(float scale, float speed, float exitTime)
    {
        float timeCount = 0;
        while (Mathf.Abs(Time.timeScale - scale) >= 0.01f)
        {
            yield return new WaitForEndOfFrame();
            Time.timeScale = Mathf.Lerp(Time.timeScale, scale, Time.deltaTime * speed);
            timeCount += Time.deltaTime;
            if (timeCount >= exitTime)
            {
                break;
            }
        }
        Time.timeScale = 1f;
    }

    public void ShowLoadingCircle(bool bShow)
    {
        LoadingCircle.SetActive(bShow);
    }
}
