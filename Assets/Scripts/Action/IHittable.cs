using System.Collections.Generic;
using UnityEngine;

public interface IHittable
{
    public bool CanHit(AttackInfo attackInfo);

    public bool Hit(AttackInfo attackInfo);

    public string GetHitSoundId();
    public string GetVisualEffectId();

    public GameObject GetGameObject();
}
