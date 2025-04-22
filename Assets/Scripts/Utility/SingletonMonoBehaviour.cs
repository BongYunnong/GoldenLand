#if UNITY_EDITOR
#define DEBUG_ON
#define GATHER_INTO_SAME_PARENT // 하나의 공통 부모 게임오브젝트에 모아놓기
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public abstract class SingletonMonoBehavior<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<T>();
                if (instance == null)
                {
                    instance = ContainerObject.GetComponent<T>();
                }
            }
            return instance;
        }
    }

    public static GameObject ContainerObject
    {
        get
        {
            if (containerObject == null)
            {
                CreateContainerObject();
            }
            return containerObject;
        }
    }

    private static T instance;
    private static GameObject containerObject;

    private static void CreateContainerObject()
    {
        if (containerObject != null) return;
        containerObject = new GameObject($"[Singleton] {typeof(T)}");
        if (instance == null)
        {
            instance = ContainerObject.AddComponent<T>();
        }
    }

    protected virtual void Awake()
    {
        if (instance == null)
        {
            instance = this as T;
            containerObject = gameObject;
        }
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
    }
}
public class PersistentSingletonMonoBehavior<T> : SingletonMonoBehavior<T> where T : MonoBehaviour
{
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(ContainerObject);
    }
}