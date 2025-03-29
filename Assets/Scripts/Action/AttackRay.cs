using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;


public class AttackRay : MonoBehaviour
{
    private IAttackable attackable;
    
    private ActionBase actionBase;
    public UnityAction AttackRayDestroyed;
    
    private Dictionary<int, IHittable> hitted = new Dictionary<int, IHittable>();
    private AttackRequsetBundle attackRequsetBundle;
    
    public float HitDuration { get; private set; }            // 히트 판정 지속 시간
    
    public Vector3 CachedStartPos { get; private set; }
    public Vector3 CachedTargetPos { get; private set; }
    
    public void SetAttackable(IAttackable attackable, ActionBase actionBase, AttackRequsetBundle attackRequsetBundle, ConstActionAreaInfo actionAreaInfo,
        Vector3 startPos, Vector3 endPos, float angleDeltaSize)
    {
        this.attackable = attackable;
        this.actionBase = actionBase;
        this.attackRequsetBundle = attackRequsetBundle;

        Vector2 dir = endPos - startPos;
        float angle = Random.Range(-angleDeltaSize, angleDeltaSize);
        CachedStartPos = startPos;
        CachedTargetPos = startPos + Quaternion.AngleAxis(angle, Vector3.forward) * dir;
        
        HitDuration = actionAreaInfo.Duration;
        hitted.Clear();
        // 히트 판정 처리
        CoroutineRunner.instance.StartCoroutine(HitDetection());
    }
    
    private IEnumerator HitDetection()
    {
        Debug.Log("Hit Ray enabled.");
        float elapsedTime = 0;
        while (elapsedTime < HitDuration)
        {
            float deltaTime = Time.deltaTime;
            yield return new WaitForSeconds(deltaTime);
            elapsedTime += deltaTime;

            HandleRaycastHits(Physics2D.RaycastAll(CachedStartPos, CachedTargetPos));
        }
        Debug.Log("Hit Ray disabled.");
        
        if (AttackRayDestroyed != null)
        {
            AttackRayDestroyed.Invoke();
        }
        ObjectPoolManager.ReturnObject("AttackRay", this.gameObject);
    }

    
    void HandleRaycastHits(RaycastHit2D[] hits)
    {
        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit2D hit = hits[i];
            if (hit.transform.TryGetComponent(out IHittable hittable))
            {
                if (hitted.ContainsKey(hit.transform.gameObject.GetInstanceID()) == false)
                {
                    hitted.Add(hittable.GetGameObject().GetInstanceID(), hittable);
                    hittable.Hit(attackable.GetAttackInfo(actionBase, hittable, attackRequsetBundle));
                }
            }
        }
    }
}
