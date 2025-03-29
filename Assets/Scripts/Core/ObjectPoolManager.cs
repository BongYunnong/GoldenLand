using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    [System.Serializable]
    public class ObjInfo
    {
        public string objName;

        public GameObject projectilePrefab;
        public Queue<GameObject> poolingProjectileQueue = new Queue<GameObject>();
    }


    private static ObjectPoolManager instance;
    public static ObjectPoolManager GetInstance()
    {
        if (instance == null)
        {
            instance = FindFirstObjectByType<ObjectPoolManager>();
        }
        return instance;
    }

    [SerializeField] public List<ObjInfo> objs = new List<ObjInfo>();

    private void Awake()
    {
        if (GetInstance() == null)
        {
            instance = this;
        }
        else if (GetInstance() != this)
        {
            Destroy(this.gameObject);
        }
        DontDestroyOnLoad(this.gameObject);
    }
    private void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    private GameObject CreateNewObject(string name)
    {
        int index = GetIndexOfObject(name);
        if (index < 0)
        {
            Debug.LogAssertion("[ObjectPoolManager] There is no Pool");
            return null;
        }

        var newObj = Instantiate(GetInstance().objs[index].projectilePrefab);
        newObj.SetActive(false);
        newObj.transform.SetParent(transform);
        return newObj;
    }
    public static GameObject GetObject(string name, Transform parent = null)
    {
        int index = GetIndexOfObject(name);
        if (index < 0)
        {
            Debug.LogAssertion($"[ObjectPoolManager] There is no Pool : {name}");
            return null;
        }

        if (GetInstance().objs[index].poolingProjectileQueue.Count > 0)
        {
            var obj = GetInstance().objs[index].poolingProjectileQueue.Dequeue();
            obj.transform.SetParent(parent);
            obj.gameObject.SetActive(true);
            return obj;
        }
        else
        {
            var newObj = GetInstance().CreateNewObject(name);
            newObj.gameObject.SetActive(true);
            newObj.transform.SetParent(parent);
            return newObj;
        }
    }

    public static void ReturnObject(string name, GameObject obj)
    {
        int index = GetIndexOfObject(name);
        if (index < 0)
        {
            Debug.LogAssertion("[ObjectPoolManager] There is no Pool");
            return;
        }

        obj.gameObject.SetActive(false);
        obj.transform.SetParent(GetInstance().transform);
        GetInstance().objs[index].poolingProjectileQueue.Enqueue(obj);
    }
    
    public static int GetIndexOfObject(string name)
    {
        int index = -1;
        for (int i = 0; i < GetInstance().objs.Count; i++)
        {
            if (GetInstance().objs[i].objName == name)
            {
                index = i;
                break;
            }
        }
        return index;
    }
}
