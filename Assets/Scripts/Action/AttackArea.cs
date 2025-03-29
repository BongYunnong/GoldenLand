using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.Events;


public class AttackArea : MonoBehaviour
{
    private IAttackable attackable;
    private PolygonCollider2D collider2D;
    private ActionBase actionBase;
    public UnityAction AttackAreaDestroyed;
    
    private Dictionary<int, IHittable> hitted = new Dictionary<int, IHittable>();
    private AttackRequsetBundle attackRequsetBundle;
    
    public PolygonCollider2D Collider2D
    {
        get
        {
            if(collider2D == null)
            {
                collider2D = GetComponent<PolygonCollider2D>();
                collider2D.enabled = false;
            }

            return collider2D;
        }
    }
    private float hitDuration;            // 히트 판정 지속 시간
    
    public void SetAttackable(IAttackable attackable, ActionBase actionBase, AttackRequsetBundle attackRequsetBundle, ConstActionAreaInfo actionAreaInfo)
    {
        this.attackable = attackable;
        this.actionBase = actionBase;
        this.attackRequsetBundle = attackRequsetBundle;

        Collider2D.points = actionAreaInfo.Points.ToArray();
        hitDuration = actionAreaInfo.Duration;
        hitted.Clear();
        // 히트 판정 처리
        CoroutineRunner.instance.StartCoroutine(HitDetection());
    }
    
    private IEnumerator HitDetection()
    {
        Collider2D.enabled = true;
        Debug.Log("Hit collider enabled.");

        yield return new WaitForSeconds(hitDuration);

        Collider2D.enabled = false;
        Debug.Log("Hit collider disabled.");
        
        if (AttackAreaDestroyed != null)
        {
            AttackAreaDestroyed.Invoke();
        }
        ObjectPoolManager.ReturnObject("AttackArea", this.gameObject);
    }

    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out IHittable hittable))
        {
            if (hitted.ContainsKey(other.gameObject.GetInstanceID()) == false)
            {
                hitted.Add(hittable.GetGameObject().GetInstanceID(), hittable);
                hittable.Hit(attackable.GetAttackInfo(actionBase, hittable, attackRequsetBundle));
            }
        }
    }
}
