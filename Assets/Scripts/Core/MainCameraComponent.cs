using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class MainCameraComponent : MonoBehaviour
{
    [SerializeField]
    private CinemachineVirtualCamera virtualCamera;
    public CinemachineVirtualCamera VirtualCamera { get { return virtualCamera; } }

    [SerializeField] Camera effectCamera;

    public void SetOrthographicSize(float InSize)
    {
        virtualCamera.m_Lens.OrthographicSize = InSize;
        effectCamera.orthographicSize = InSize;
    }
    public void AddOrthographicSize(float InSize)
    {
        virtualCamera.m_Lens.OrthographicSize += InSize;
        effectCamera.orthographicSize = virtualCamera.m_Lens.OrthographicSize;
    }

    public void SetFieldOfView(float InSize)
    {
        virtualCamera.m_Lens.FieldOfView += InSize;
        effectCamera.fieldOfView = virtualCamera.m_Lens.FieldOfView;
    }
    public void AddFieldOfView(float InSize)
    {
        virtualCamera.m_Lens.FieldOfView += InSize;
        effectCamera.fieldOfView = virtualCamera.m_Lens.FieldOfView;
    }
}
