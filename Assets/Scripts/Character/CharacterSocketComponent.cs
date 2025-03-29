using System;
using Unity.VisualScripting;
using UnityEngine;

public enum ECharacterSocketType
{
    None,           // 보이지 않음.
    LeftSide,
    RightSide,
    LeftShoulder,
    RightShoulder,
    LeftHand,
    RightHand,
    Back,
    Custom,
}

public enum EOrderInLayerPreset
{
    Back,
    Front,
    InnerBody,
    OuterBody,
}

public struct SocketChildTransformInfo
{
    public SocketChildTransformInfo(string settingString)
    {
        string[] idleSocketSetting = settingString.Split('/');
        SocketType = Enum.Parse<ECharacterSocketType>(idleSocketSetting[0]);
        OrderInLayerPreset = Enum.Parse<EOrderInLayerPreset>(idleSocketSetting[1]);
    }

    public ECharacterSocketType SocketType;
    public EOrderInLayerPreset OrderInLayerPreset;
}

public class CharacterSocketComponent : MonoBehaviour
{
    [SerializeField] private SerializableDictionary<ECharacterSocketType, Transform> sockets = new SerializableDictionary<ECharacterSocketType, Transform>();
    
    public Transform GetSocket(ECharacterSocketType type)
    {
        if (sockets.TryGetValue(type, out Transform socket))
        {
            return socket;
        }
        return sockets[ECharacterSocketType.None];
    }
    
    public void AttachToSocket(Transform target, SocketChildTransformInfo setting)
    {
        Transform targetSocket = GetSocket(setting.SocketType);
        target.SetParent(targetSocket);
        SpriteRenderer spriteRenderer = target.GetComponentInChildren<SpriteRenderer>();
        if(spriteRenderer)
        {
            int layerIndex = 0;
            switch (setting.OrderInLayerPreset)
            {
                case EOrderInLayerPreset.Back:
                    layerIndex = -10;
                    break;
                case EOrderInLayerPreset.Front:
                    layerIndex = 10;
                    break;
                case EOrderInLayerPreset.OuterBody:
                    layerIndex = 4;
                    break;
                case EOrderInLayerPreset.InnerBody:
                    layerIndex = -4;
                    break;
            }
            spriteRenderer.sortingOrder = layerIndex;
        }
    }
}
