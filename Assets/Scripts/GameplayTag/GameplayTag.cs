using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEditor;

/// <summary>
/// index로 들어가기 때문에 DataSet에서 값을 설정하고 실 사용때 Parse하는 것이 좋음
/// </summary>
public enum EGameplayTag
{
    None,
    // 상위 태그
    Dashing,
    Dodging,
    Jumping,
    Crounching,
    Guarding,
    Invincible,
    Stunned,
    Airborne,
    Downed,
    CanBound,
    // 하위 태그
    Jumping_WallJumping,
    Damage_Fire,
    Damage_Water,
    Healing_Light,
    Healing_Dark,
}

public static class GameplayTagExtensions
{
    // 특정 태그가 다른 태그의 자식인지 확인
    public static bool IsChildOf(this EGameplayTag childTag, EGameplayTag parentTag)
    {
        string childName = childTag.ToString();
        string parentName = parentTag.ToString();
        return childName.StartsWith(parentName + "_");
    }

    // 특정 태그가 다른 태그의 부모인지 확인
    public static bool IsParentOf(this EGameplayTag parentTag, EGameplayTag childTag)
    {
        return childTag.IsChildOf(parentTag);
    }
}