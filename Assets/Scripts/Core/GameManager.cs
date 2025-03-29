using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    public static GameManager GetInstance()
    {
        if (instance == null)
        {
            instance = FindObjectOfType<GameManager>();
        }
        return instance;
    }
    public bool initialized = false;

    protected MapBase map;
    public MapBase Map { get { return map; } }

    protected PlayerController playerController = null;
    public PlayerController PlayerController { get { return playerController; } }

    [SerializeField] private float camMoveSpeed = 15.0f;
    [SerializeField] private float camScaleSpeed = 20.0f;
    [SerializeField] private Vector2 camScaleMinMax = new Vector2(5.0f, 15.0f);

    protected float temporalTargetTimeScale = 1;
    protected float temporalTargetTimeScaleRecoveryMultiplier = 1.0f;

    public virtual void InitializeGame()
    {
        playerController = FindObjectOfType<PlayerController>();
        playerController.InitializePlayerController();

        map = FindObjectOfType<MapBase>();

        initialized = true;

        temporalTargetTimeScale = 1;
        temporalTargetTimeScaleRecoveryMultiplier = 1;
        Time.timeScale = 1;
    }

    protected virtual void Update()
    {
        if (initialized == false) return;

        RecoverTargetTimeScale();
    }

    private void RecoverTargetTimeScale()
    {
        if (Mathf.Approximately(temporalTargetTimeScale, 1.0f) == false)
        {
            float sign = Mathf.Sign(temporalTargetTimeScale - 1.0f);
            temporalTargetTimeScale += Time.unscaledDeltaTime * -sign * temporalTargetTimeScaleRecoveryMultiplier;
            Time.timeScale = temporalTargetTimeScale;
        }
    }

    public PlayerController GetPlayerController()
    {
        return playerController;
    }
}
