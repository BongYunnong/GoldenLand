using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class ConstVisualEffefctInfo
{
    public string id;
    public VisualEffectComponent particlePrefab;


    public ConstVisualEffefctInfo(VisualEffectDataSet.TableData data)
    {
        this.id = data.ID;
        if (data.EffectPaths.GetValue())
        {
            this.particlePrefab = data.EffectPaths.GetValue().GetComponent<VisualEffectComponent>();
        }
    }
}

[System.Serializable]
public class VisualEffectInfo
{
    public string id;
    public VisualEffectComponent source;

    public VisualEffectInfo(string id, VisualEffectComponent source)
    {
        this.id = id;
        this.source = source;
    }
}

public class VisualEffectController : PersistentSingletonMonoBehavior<VisualEffectController>
{
    private Dictionary<int, VisualEffectInfo> activatedParticles = new Dictionary<int, VisualEffectInfo>();
    private Dictionary<string, Queue<VisualEffectComponent>> effectPools = new Dictionary<string, Queue<VisualEffectComponent>>();
    public static int Seq = 0;

    private void Update()
    {
        CheckInvalidVisualEffects();
    }

    private void CheckInvalidVisualEffects()
    {
        int removeUID = -1;
        foreach (var activatedParticle in activatedParticles)
        {
            if (activatedParticle.Value.source.gameObject.activeSelf == false)
            {
                removeUID = activatedParticle.Key;
                break;
            }
        }

        if (removeUID >= 0)
        {
            StopVisualEffect(removeUID);
        }
    }

    public VisualEffectComponent PlayVisualEffect(string id)
    {
        VisualEffectComponent pooledVisualVisualEffect = GetOrCreateVisualEffect(id);
        activatedParticles.Add(Seq, new VisualEffectInfo(id, pooledVisualVisualEffect));
        Seq++;
        return pooledVisualVisualEffect;
    }


    public void StopVisualEffect(int uid)
    {
        if (activatedParticles.ContainsKey(uid) == false)
        {
            return;
        }
        VisualEffectComponent visualEffectComponent = activatedParticles[uid].source;
        visualEffectComponent.gameObject.SetActive(false);
        visualEffectComponent.transform.SetParent(null);
        string visualEffectID = activatedParticles[uid].id;
        activatedParticles.Remove(uid);

        if (effectPools.ContainsKey(visualEffectID) == false)
        {
            effectPools.Add(visualEffectID, new Queue<VisualEffectComponent>());
        }
        effectPools[visualEffectID].Enqueue(visualEffectComponent);
    }

    private VisualEffectComponent GetOrCreateVisualEffect(string id)
    {
        if (effectPools.TryGetValue(id, out Queue<VisualEffectComponent> pool))
        {
            if (pool.Count > 0)
            {
                VisualEffectComponent currentVisualEffect = pool.Dequeue();
                currentVisualEffect.gameObject.SetActive(true); // 활성화
                currentVisualEffect.transform.localScale = Vector3.one;
                currentVisualEffect.transform.localPosition = Vector3.zero;
                currentVisualEffect.transform.localRotation = Quaternion.identity;
                return currentVisualEffect;
            }
        }
        
        // 풀 크기 초과 시 새로 생성
        DataManager dataManager = DataManager.Instance;
        ConstVisualEffefctInfo visualEffectInfo = dataManager.visualEffectDict[id];
        if (visualEffectInfo == null)
        {
            Debug.LogWarning("VisualEffect Info is null " + id);
            return null;
        }

        VisualEffectComponent newVisualVisualEffect = Instantiate(visualEffectInfo.particlePrefab);
        newVisualVisualEffect.transform.localScale = Vector3.one;
        newVisualVisualEffect.transform.localPosition = Vector3.zero;
        newVisualVisualEffect.transform.localRotation = Quaternion.identity;
        return newVisualVisualEffect;
    }
}
