using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class GameplayTagEventArgs
{
    public EGameplayTag Tag { get; private set; }
    public object Sender { get; private set; }
    public GameplayTagEventArgs(EGameplayTag tag, object sender)
    {
        Tag = tag;
        Sender = sender;
    }
}

public class GameplayTagEventManager
{
    // 등록
    // GameplayTagEventManager.Instance.RegisterListener(GameplayTag.Damage_Fire, OnFireDamageEvent);
    // 이벤트 트리거
    // GameplayTagEventManager.Instance.TriggerEvent(GameplayTag.Damage_Fire);

    private static GameplayTagEventManager instance;
    public static GameplayTagEventManager Instance => instance ??= new GameplayTagEventManager();

    private Dictionary<EGameplayTag, Action<GameplayTagEventArgs>> tagEvents = new();

    // 이벤트 등록
    public void RegisterListener(EGameplayTag tag, Action<GameplayTagEventArgs> callback)
    {
        if (!tagEvents.ContainsKey(tag))
        {
            tagEvents[tag] = null;
        }
        tagEvents[tag] += callback;
    }

    // 이벤트 해제
    public void UnregisterListener(EGameplayTag tag, Action<GameplayTagEventArgs> callback)
    {
        if (tagEvents.ContainsKey(tag))
        {
            tagEvents[tag] -= callback;
        }
    }

    // 이벤트 트리거
    public void TriggerEvent(EGameplayTag tag, object sender)
    {
        if (tagEvents.ContainsKey(tag) && tagEvents[tag] != null)
        {
            tagEvents[tag](new GameplayTagEventArgs(tag, sender));
        }

        // 부모 태그도 이벤트 트리거
        foreach (var parentTag in GetParentTags(tag))
        {
            if (tagEvents.ContainsKey(parentTag) && tagEvents[parentTag] != null)
            {
                tagEvents[parentTag](new GameplayTagEventArgs(tag, sender));
            }
        }
    }

    // 부모 태그 검색
    private IEnumerable<EGameplayTag> GetParentTags(EGameplayTag childTag)
    {
        string[] segments = childTag.ToString().Split('_');
        for (int i = 0; i < segments.Length - 1; i++)
        {
            string parentName = string.Join("_", segments.Take(i + 1));
            if (Enum.TryParse(parentName, out EGameplayTag parentTag))
            {
                yield return parentTag;
            }
        }
    }
}
