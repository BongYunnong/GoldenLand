using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillboardSprite : MonoBehaviour
{
    Camera mainCamera;
    void Start()
    {
        mainCamera = Camera.main;
    }
    void LateUpdate()
    {
        Vector3 newRotation = mainCamera.transform.eulerAngles;
        newRotation.x = 0;
        newRotation.z = 0;
        transform.eulerAngles = newRotation;
    }
}
