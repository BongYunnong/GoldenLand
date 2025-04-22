using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyByTime : MonoBehaviour
{
    [SerializeField] string objectPoolName;
    [SerializeField] float lifeTime = 1f;

    Coroutine destroyCoroutine;

    private void OnEnable()
    {
        destroyCoroutine = StartCoroutine("DestroyWhenOverTimeCoroutine");
    }

    public void InitializeDestroyByTime(float InTIme)
    {
        lifeTime = InTIme;
        if(destroyCoroutine != null)
        {
            StopCoroutine(destroyCoroutine);
        }
        destroyCoroutine = StartCoroutine("DestroyWhenOverTimeCoroutine");
    }

    IEnumerator DestroyWhenOverTimeCoroutine()
    {
        yield return new WaitForSeconds(lifeTime);
        if(ObjectPoolManager.GetIndexOfObject(objectPoolName) >= 0)
        {
            ObjectPoolManager.ReturnObject(objectPoolName, gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
}
