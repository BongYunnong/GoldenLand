using UnityEngine;

public interface IAttackable
{
    public AttackInfo GetAttackInfo(ActionBase actionBase, IHittable hittable, AttackRequsetBundle attackRequsetBundle);
    
    public GameObject GetGameObject();
    public string GetFireEffectGroup();
    public string GetHitEffectGroup();
}
