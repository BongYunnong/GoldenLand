using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    public Player DefaultPlayerCharacter;

    private Transform cursor;
    [SerializeField] private GameObject cursorPrefab;
    [SerializeField] private Canvas effectCanvas;

    [SerializeField] protected LayerMask clickTargetLayer;

    private Transform pendingTargetTR;

    float doubleTapMaxTimeWait = 1;
    float doubleTapVariancePosition = 1;

    public void InitializePlayerController()
    {
        if(DefaultPlayerCharacter)
        {
            DefaultPlayerCharacter.InitializeCharacter();
            SetCamTarget(DefaultPlayerCharacter.transform, Vector3.zero);
        }

        cursor = Instantiate(cursorPrefab, transform).transform;

    }

    public LayerMask GetClickTargetLayer()
    {
        return clickTargetLayer;
    }


    private void Update()
    {
        Vector3 currMousePos = GetMousePos();
        if(cursor)
        {
            cursor.position = currMousePos;
            if(DefaultPlayerCharacter)
            {
                cursor.LookAt(DefaultPlayerCharacter.transform.position);
            }
            else
            {
                Vector3 forward = Camera.main.transform.forward;
                forward.y = 0;
                cursor.LookAt(cursor.transform.position + forward.normalized);
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            /* TODO
            GameObject ClickEffectObj = ObjectPoolManager.GetObject("ClickEffect");
            if(ClickEffectObj)
            {
                ParticleSystem currEffect = ClickEffectObj.GetComponent<ParticleSystem>();
                currEffect.transform.SetParent(effectCanvas.transform);
                currEffect.transform.localScale = Vector3.one;
                currEffect.transform.position = currMousePos;
                currEffect.Play();
            }
            */
        }
        else if(Input.GetMouseButton(0))
        {
        }
        else if(Input.GetMouseButtonUp(0))
        {
        }
    }

    public Vector3 GetMousePos()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float distance = Vector3.Dot((Vector3.zero - ray.origin), Vector3.up) / Vector3.Dot(ray.direction, Vector3.up); // distance = (p0 - l0) . n / l*n
        Vector3 resultPos = ray.GetPoint(distance); // ray projection with distance

        return resultPos;
        // return GetMouseRayHit().point;
    }

    public Vector2 GetScreenToWorldMousePos()
    {
        return Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }
    public RaycastHit GetMouseRayHit()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1000, GetClickTargetLayer()))
        {
            return hit;
        }
        return new RaycastHit();
    }

    public Vector3 GetRelativeMousePos(Transform InTransform)
    {
        return GetMousePos() - InTransform.position;
    }

    public Vector2 GetRelativeMousePos(Transform InTransform, float InMaxDist)
    {
        return Vector2.ClampMagnitude(GetRelativeMousePos(InTransform), InMaxDist);
    }

    public void SetCamTarget(Transform InTransform, Vector3 InOffset, bool bUsePendingTarget = true, float InDuration = 0.0f)
    {
        GameManager gameManager = GameManager.GetInstance();
        if (gameManager == null) return;
        MainCameraComponent mainCamComp = gameManager.MainCameraComp;
        /* TODO
        MovieDirectorManager movieDirector = MovieDirectorManager.GetInstance();
        if (InTransform == null)
        {
            if(bUsePendingTarget)
            {
                movieDirector?.CameraToTarget(pendingTargetTR, InOffset, InDuration);
            }
            else
            {
                movieDirector?.CameraToTarget(null, InOffset, InDuration);
            }
        }
        else
        {
            pendingTargetTR = mainCamComp?.VirtualCamera?.Follow;
            movieDirector?.CameraToTarget(InTransform, InOffset, InDuration);
        }
        */
        var transposer = mainCamComp?.VirtualCamera?.GetCinemachineComponent<CinemachineTransposer>();
        if(transposer != null)
        {
            if(gameManager is LobbyManager)
            {
                transposer.m_FollowOffset = InOffset + new Vector3(-12, 17, -12);
            }
            else
            {
                transposer.m_FollowOffset = InOffset + new Vector3(-7, 10, -7);
            }
            //transposer.m_FollowOffset = InOffset + new Vector3(0, 10, -10);
        }
    }

    public bool IsDoubleTap()
    {
        bool result = false;
        if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            float DeltaTime = Input.GetTouch(0).deltaTime;
            float DeltaPositionLenght = Input.GetTouch(0).deltaPosition.magnitude;

            if (DeltaTime > 0 && DeltaTime < doubleTapMaxTimeWait && DeltaPositionLenght < doubleTapVariancePosition)
                result = true;
        }
        return result;
    }

    private bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }
}
