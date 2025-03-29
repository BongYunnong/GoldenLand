using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.Events;

[Serializable]
public class GameplayTagContainer
{
    private HashSet<EGameplayTag> tags = new HashSet<EGameplayTag>();
    public UnityAction<EGameplayTag> GameplayTagAdded;
    public UnityAction<EGameplayTag> GameplayTagRemoved;

    // 태그 추가
    public void AddTag(EGameplayTag tag)
    {
        tags.Add(tag); // 중복 방지 자동 처리
        GameplayTagAdded?.Invoke(tag);
    }

    // 태그 제거
    public void RemoveTag(EGameplayTag tag)
    {
        tags.Remove(tag);
        GameplayTagRemoved?.Invoke(tag);
    }

    // 특정 태그 포함 여부 확인
    public bool HasTag(EGameplayTag tag)
    {
        return tags.Contains(tag);
    }

    // 다수 태그 중 하나라도 포함되어 있는지 확인
    public bool MatchesAny(IEnumerable<EGameplayTag> requiredTags)
    {
        foreach (var tag in requiredTags)
        {
            if (tags.Contains(tag))
            {
                return true;
            }
        }
        return false;
    }

    // 다수 태그를 모두 포함하고 있는지 확인
    public bool MatchesAll(IEnumerable<EGameplayTag> requiredTags)
    {
        foreach (var tag in requiredTags)
        {
            if (!tags.Contains(tag))
            {
                return false;
            }
        }
        return true;
    }

    // 현재 저장된 모든 태그 반환
    public IEnumerable<EGameplayTag> GetAllTags()
    {
        return tags;
    }
}

public class GameplayTagQuery
{
    public HashSet<EGameplayTag> RequiredTags { get; }
    public HashSet<EGameplayTag> ExcludedTags { get; }

    public GameplayTagQuery(IEnumerable<EGameplayTag> requiredTags, IEnumerable<EGameplayTag> excludedTags)
    {
        RequiredTags = new HashSet<EGameplayTag>(requiredTags);
        ExcludedTags = new HashSet<EGameplayTag>(excludedTags);
    }

    public bool Evaluate(GameplayTagContainer container)
    {
        // RequiredTags는 모두 포함되어야 하고, ExcludedTags는 포함되지 않아야 함
        return container.MatchesAll(RequiredTags) && !container.MatchesAny(ExcludedTags);
    }
}
