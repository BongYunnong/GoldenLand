using System;
using UnityEngine;

public class VisualEffectComponent : MonoBehaviour
{
    private Transform innerContainer;

    private void Awake()
    {
        innerContainer = transform.GetChild(0);
    }

    public void FlipX(bool flip)
    {
        transform.localScale = new Vector3(flip ? -1 : 1,1, 1);
    }

    public void SetBaseTransform(Vector3 position, Quaternion rotation)
    {
        transform.position = position;
        transform.rotation = rotation;
    }
    public void SetBaseLocalTransform(Vector3 position, Quaternion rotation)
    {
        transform.localPosition = position;
        transform.localRotation = rotation;
    }
    public void SetInnerTransform(Vector3 position, Quaternion rotation)
    {
        innerContainer.localPosition = position;
        innerContainer.localRotation = rotation;
    }
}
