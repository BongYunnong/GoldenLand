using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;


[RequireComponent(typeof(InputController))]
public class PlayerController : SingletonMonoBehavior<PlayerController>
{
    private InputController inputController;
    
    private bool initialized = false;
    public bool Initialized { get { return initialized; } }
    
    public Player defaultPlayerCharacter;
    public Character SelectedCharacter { get; set; }
    public Character PossessedCharacter { get; set; }

    [SerializeField] private CinemachineVirtualCamera mainVirtualCamera;
    public CinemachineVirtualCamera MainVirtualCamera { get { return mainVirtualCamera; } }
    [SerializeField]
    private CinemachineBrain cinemachineBrain;
    public CinemachineBrain CinemachineBrain { get { return cinemachineBrain; } }
    
    private CinemachineConfiner cameraConfiner;

    [SerializeField]
    private MainCameraComponent mainCameraComp;
    public MainCameraComponent MainCameraComp { get { return mainCameraComp; } }
    
    private Transform pendingCamTarget;
    
    private Transform cursor;
    [SerializeField] private GameObject cursorPrefab;
    [SerializeField] private Canvas effectCanvas;

    [SerializeField] protected LayerMask clickTargetLayer;
    private float doubleTapMaxTimeWait = 1;
    private float doubleTapVariancePosition = 1;

    private bool syncViewWithInput = false;

    [Header("[Camera]")]
    [SerializeField] private float camMoveSpeed = 20.0f;
    [SerializeField] private float zoomSpeed = 2f; 
    [SerializeField] private Vector2 camScaleMinMax = new Vector2(5.0f, 15.0f);

    private Dictionary<string, int> cameraMoveBlockReasons = new Dictionary<string, int>();
    private Dictionary<string, int> cameraZoomBlockReasons = new Dictionary<string, int>();
    private Dictionary<string, int> cameraFocusBlockReasons = new Dictionary<string, int>();

    public UnityAction<Vector3> OnMouseClicked;
    public UnityAction<Character> OnClickedCharacter;
    public UnityAction<Character, Character> OnSelectedCharacterChanged;
    public UnityAction<Character, Character> OnPossessedCharacterChanged;
    
    private void Start()
    {
        inputController = GetComponent<InputController>();
        inputController.InputActionTriggered += OnInputActionTriggered;
    }
    
    public void InitializePlayerController()
    {
        cameraConfiner = MainVirtualCamera.GetComponent<CinemachineConfiner>();
        
        if(defaultPlayerCharacter)
        {
            defaultPlayerCharacter.InitializeCharacter();
            SetCamTarget(defaultPlayerCharacter.transform, Vector3.zero);
        }

        cursor = Instantiate(cursorPrefab, transform).transform;

        initialized = true;
    }

    private void Update()
    {
        if (Initialized == false)
        {
            return;
        }
        
        Vector3 currMousePos = GetMousePos();
        if(cursor)
        {
            cursor.position = currMousePos;
            if(defaultPlayerCharacter)
            {
                cursor.LookAt(defaultPlayerCharacter.transform.position);
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
        
        TickClick();
        if (PossessedCharacter != null)
        {
            UpdateInputs();
        }
        else
        {
            UpdateCamera();
        }
    }

    private void TickClick()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            ClickMouse(Input.mousePosition, 0);
        }
        else if (Input.GetMouseButtonDown(1))
        {
            ClickMouse(Input.mousePosition, 1);
        }
#if UNITY_EDITOR
        else if (Input.GetMouseButtonDown(2))
        {
            ClickMouse(Input.mousePosition, 2);
        }
#endif
    }

    private void UpdateInputs()
    {
        Character character = PossessedCharacter;
        character.SetTargetPos(GetMousePos());
        character.SetMoveInput(new Vector2(Input.GetAxisRaw("Horizontal"), 0), syncViewWithInput);
    }
    
    private void OnInputActionTriggered(EInputAction inputAction, EInputActionContext inputActionContext)
    {
        if (PossessedCharacter != null)
        {
            if (inputAction == EInputAction.JumpRelease)
            {
                // Performed 뒤에 바로 Cancel이 불려버려서 JumpRelease를 GetControlInput으로 제대로 받을 수 없으므로 Cancel은 제외
                if (inputActionContext == EInputActionContext.Performed)
                {
                    PossessedCharacter.SetControlInput(inputAction, true);
                }
                else if (inputActionContext == EInputActionContext.Started)
                {
                    PossessedCharacter.SetControlInput(inputAction, false);
                }
            }
            else
            {
                PossessedCharacter.SetControlInput(inputAction, inputActionContext == EInputActionContext.Performed && PossessedCharacter.IsControllable());
            }
        }
    }

    private void UpdateCamera()
    {
#if UNITY_ANDROID && !UNITY_EDITOR_WIN
        if (Input.touchCount == 1 && IsCameraMoveBlocked() == false)
        {
            Touch firstTouch = Input.GetTouch(0);
            if (VirtualCamera.LookAt == null && firstTouch.phase == TouchPhase.Moved)
            {
                VirtualCamera.transform.position += (Vector3)(camMoveSpeed * 0.02f * firstTouch.deltaPosition * Time.deltaTime);
                float tmpX = Mathf.Clamp(VirtualCamera.transform.position.x, cameraConfiner.m_BoundingShape2D.bounds.min.x, cameraConfiner.m_BoundingShape2D.bounds.max.x);
                float tmpY = Mathf.Clamp(VirtualCamera.transform.position.y, cameraConfiner.m_BoundingShape2D.bounds.min.y, cameraConfiner.m_BoundingShape2D.bounds.max.y);
                VirtualCamera.transform.position = new Vector3(tmpX, tmpY, VirtualCamera.transform.position.z);
            }
        }
        if (Input.touchCount == 2 && IsCameraZoomBlocked() == false)
        {
            Touch firstTouch = Input.GetTouch(0);
            Touch secondTouch = Input.GetTouch(1);

            Vector2 firstTouchPrevPos = firstTouch.position - firstTouch.deltaPosition;
            Vector2 secondTouchPrevPos = secondTouch.position - secondTouch.deltaPosition;

            float prevMagnitude = (firstTouchPrevPos - secondTouchPrevPos).magnitude;
            float currentMagnitude = (secondTouch.position - firstTouch.position).magnitude;

            float diff = currentMagnitude - prevMagnitude;
            Zoom(diff * 0.2f);
        }
#else
        if (IsCameraMoveBlocked() == false)
        {
            /*
            Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            if(input.sqrMagnitude > 0)
            {
                MoveCamera((Vector3)(camMoveSpeed * input * Time.deltaTime));
            }
            */
            
            Vector3 input = MainCameraComp.VirtualCamera.transform.right * Input.GetAxis("Horizontal") +
                            Vector3.ProjectOnPlane(MainCameraComp.VirtualCamera.transform.forward, Vector3.up).normalized * Input.GetAxis("Vertical");
            if (MainCameraComp.VirtualCamera.LookAt == null)
            {
                MainCameraComp.VirtualCamera.transform.position += (camMoveSpeed * input.normalized * Time.deltaTime);
                if (cameraConfiner)
                {
                    float tmpX = Mathf.Clamp(MainCameraComp.VirtualCamera.transform.position.x, cameraConfiner.m_BoundingVolume.bounds.min.x, cameraConfiner.m_BoundingVolume.bounds.max.x);
                    float tmpY = Mathf.Clamp(MainCameraComp.VirtualCamera.transform.position.y, cameraConfiner.m_BoundingVolume.bounds.min.y, cameraConfiner.m_BoundingVolume.bounds.max.y);
                    float tmpZ = Mathf.Clamp(MainCameraComp.VirtualCamera.transform.position.z, cameraConfiner.m_BoundingVolume.bounds.min.z, cameraConfiner.m_BoundingVolume.bounds.max.z);
                    MainCameraComp.VirtualCamera.transform.position = new Vector3(tmpX, tmpY, tmpZ);
                }
            }
        }
        if (IsCameraZoomBlocked() == false)
        {
            float mouseWheel = Input.GetAxis("Mouse ScrollWheel");
            Zoom(mouseWheel);
        }
#endif
    }

    private void MoveCamera(Vector3 delta)
    {
        MoveCameraTo(mainVirtualCamera.transform.position + delta);
    }
    private void MoveCameraTo(Vector3 pos)
    {
        Camera camera = Camera.main;
        float halfHeight = camera.orthographicSize;
        float halfWidth = camera.aspect * halfHeight;

        mainVirtualCamera.transform.position = pos;
        float tmpX = Mathf.Clamp(mainVirtualCamera.transform.position.x, cameraConfiner.m_BoundingShape2D.bounds.min.x + halfWidth, cameraConfiner.m_BoundingShape2D.bounds.max.x - halfWidth);
        float tmpY = Mathf.Clamp(mainVirtualCamera.transform.position.y, cameraConfiner.m_BoundingShape2D.bounds.min.y + halfHeight, cameraConfiner.m_BoundingShape2D.bounds.max.y - halfHeight);
        mainVirtualCamera.transform.position = new Vector3(tmpX, tmpY, mainVirtualCamera.transform.position.z);
    }



    private void Zoom(float value)
    {
        if (value == 0) return;
        float currentSize = mainVirtualCamera.m_Lens.OrthographicSize;

        Vector3 mouseScreenPosition = Input.mousePosition;
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);

        Vector3 cameraPosition = mainVirtualCamera.transform.position;

        float newSize = Mathf.Clamp(currentSize - value * zoomSpeed, camScaleMinMax.x, camScaleMinMax.y);
        float zoomFactor = newSize / currentSize;

        Vector3 newCameraPosition = cameraPosition + (mouseWorldPosition - cameraPosition) * (1 - zoomFactor);

        mainVirtualCamera.transform.position = new Vector3(newCameraPosition.x, newCameraPosition.y, cameraPosition.z);
        mainVirtualCamera.m_Lens.OrthographicSize = newSize;
    }



    public virtual void SelectCharacter(Character targetBot)
    {
        SelectCharacter(targetBot, Vector2.zero);
    }

    public virtual void SelectCharacter(Character targetBot, Vector2 offset, bool bUsePendingTarget = false, float duration = 0.0f)
    {
        if (PossessedCharacter)
        {
            return;
        }

        Character prevCharacter = SelectedCharacter;
        if (prevCharacter != null)
        {
            prevCharacter.CharacterCustomize.EnableOutline(false);
        }

        if (targetBot != null && prevCharacter != targetBot)
        {
            SelectedCharacter = targetBot;
            SetCamTarget(SelectedCharacter.transform, offset, bUsePendingTarget, duration);
        }
        else
        {
            SelectedCharacter = null;
            SetCamTarget(null, offset, bUsePendingTarget, duration);
        }

        if (SelectedCharacter != null)
        {
            SelectedCharacter.CharacterCustomize.SetOutlineColor(Color.red);
            SelectedCharacter.CharacterCustomize.EnableOutline(true);
        }

        UpdateSelectedCharacter(prevCharacter);
    }

    private void UpdateSelectedCharacter(Character prevCharacter)
    {
        if (OnSelectedCharacterChanged != null)
        {
            OnSelectedCharacterChanged.Invoke(prevCharacter, SelectedCharacter);
        }
    }

    public virtual void PossessCharacter(Character targetBot)
    {
        Character prevCharacter = PossessedCharacter;
        if (prevCharacter != null)
        {
            prevCharacter.CharacterCustomize.EnableOutline(false);
        }

        if (targetBot != null && prevCharacter != targetBot)
        {
            SelectCharacter(null);
            PossessedCharacter = targetBot;

            SetCamTarget(PossessedCharacter.transform);
        }
        else
        {
            PossessedCharacter = null;
            SetCamTarget(null);
        }

        if (PossessedCharacter != null)
        {
            PossessedCharacter.CharacterCustomize.SetOutlineColor(Color.green);
            PossessedCharacter.CharacterCustomize.EnableOutline(true);
        }

        UpdatePossessedCharacter(prevCharacter);
    }
    private void UpdatePossessedCharacter(Character prevCharacter)
    {
        if (OnPossessedCharacterChanged != null)
        {
            OnPossessedCharacterChanged.Invoke(prevCharacter, PossessedCharacter);
        }
    }

    
    public void SetCamTarget(Transform transform)
    {
        SetCamTarget(transform, Vector2.zero);
    }

    public void SetCamTarget(Transform transform, Vector2 offset, bool bUsePendingTarget = false, float duration = 0.0f)
    {
        CameraController cameraController = CameraController.Instance;
        if (transform == null)
        {
            cameraController.CameraToTarget(bUsePendingTarget ? pendingCamTarget : null, offset, duration);
            pendingCamTarget = null;
        }
        else
        {
            pendingCamTarget = bUsePendingTarget ? MainVirtualCamera.Follow : null;
            cameraController.CameraToTarget(transform, offset, duration);
        }
        var transposer = MainVirtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        transposer.m_TrackedObjectOffset = new Vector3(offset.x, offset.y, -10);
    }


    public void SetCameraOrthographic(float value)
    {
        MainVirtualCamera.m_Lens.OrthographicSize = value;
    }



    public void BlockCameraMove(string reason)
    {
        if (cameraMoveBlockReasons.ContainsKey(reason))
        {
            cameraMoveBlockReasons[reason]++;
        }
        else
        {
            cameraMoveBlockReasons.Add(reason, 1);
        }
    }
    public void BlockCameraZoom(string reason)
    {
        if (cameraZoomBlockReasons.ContainsKey(reason))
        {
            cameraZoomBlockReasons[reason]++;
        }
        else
        {
            cameraZoomBlockReasons.Add(reason, 1);
        }
    }
    public void UnBlockCameraMove(string reason, bool clear = false)
    {
        if (cameraMoveBlockReasons.ContainsKey(reason))
        {
            if (clear)
            {
                cameraMoveBlockReasons[reason] = 0;
            }
            else
            {
                cameraMoveBlockReasons[reason]--;
            }
        }
    }
    public void UnBlockCameraZoom(string reason)
    {
        if (cameraZoomBlockReasons.ContainsKey(reason))
        {
            cameraZoomBlockReasons[reason]--;
        }
    }
    public void BlockCameraFocus(string reason)
    {
        if (cameraFocusBlockReasons.ContainsKey(reason))
        {
            cameraFocusBlockReasons[reason]++;
        }
        else
        {
            cameraFocusBlockReasons.Add(reason, 1);
        }

        bool blocked = IsCameraFocusingBlocked();
        if (blocked)
        {
            SelectCharacter(null);
        }
    }
    public void UnBlockCameraFocus(string _reason)
    {
        if (cameraFocusBlockReasons.ContainsKey(_reason))
        {
            cameraFocusBlockReasons[_reason]--;
        }
    }


    public virtual void ClickMouse(Vector2 mousePos, int index)
    {
        if (IsPointerOverGameObject()) return;

        if(OnMouseClicked != null)
        {
            OnMouseClicked.Invoke(mousePos);
        }

        if (IsCameraFocusingBlocked()) return;

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Physics.Raycast(ray, out RaycastHit hit, 100000, clickTargetLayer);
        
        if (hit.collider != null)
        {
            Character hittedCharacter = hit.transform.GetComponent<Character>();
            if (index == 0)
            {
                OnClickedCharacter.Invoke(hittedCharacter);
            }
            if (index == 1)
            {
                SelectCharacter(hittedCharacter);
            }
            else if(index == 2)
            {
                PossessCharacter(hittedCharacter);
            }
#if UNITY_EDITOR

            if (SelectedCharacter != null &&
                Input.GetKey(KeyCode.LeftShift) &&
                hittedCharacter.TryGetComponent(out GoalNode goalNode))
            {
                SelectedCharacter.Perception.SetTarget(goalNode);
            }
#endif
        }
        
        GameObject ClickEffectObj = ObjectPoolManager.GetObject("ClickEffect");
        if (ClickEffectObj)
        {
            ParticleSystem currEffect = ClickEffectObj.GetComponent<ParticleSystem>();
            Vector3 pos = GetMouseRayHitPos();
            if (!float.IsNaN(pos.x) && !float.IsNaN(pos.y) && !float.IsNaN(pos.z))
            {
                currEffect.transform.position = pos;
            }
            else
            {
                currEffect.transform.SetParent(FindObjectOfType<Canvas>().transform);
                currEffect.transform.position = MouseWorldPosition();
            }
            currEffect.transform.localScale = Vector3.one;
            currEffect.Play();
        }
    }
    
    public Vector3 MouseWorldPosition()
    {
        var mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = Camera.main.WorldToScreenPoint(transform.position).z;
        return Camera.main.ScreenToWorldPoint(mouseScreenPos);
    }

    public Vector3 GetMouseRayHitPos()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float distance = Vector3.Dot((Vector3.zero - ray.origin), Vector3.up) / Vector3.Dot(ray.direction, Vector3.up); // distance = (p0 - l0) . n / l*n
        return ray.GetPoint(distance); // ray projection with distance
    }

    private Camera GetCurrentCamera()
    {
        return Camera.main;
    }

    public Vector3 GetMousePos()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float distance = Vector3.Dot((Vector3.zero - ray.origin), Vector3.up) / Vector3.Dot(ray.direction, Vector3.up); // distance = (p0 - l0) . n / l*n
        Vector3 resultPos = ray.GetPoint(distance); // ray projection with distance

        return resultPos;
    }

    public Vector3 GetRelativeMousePos(Transform transform)
    {
        return GetMousePos() - transform.position;
    }

    public Vector2 GetRelativeMousePos(Transform transform, float maxDist)
    {
        return Vector2.ClampMagnitude(GetRelativeMousePos(transform), maxDist);
    }

    public Vector2 GetScreenToWorldMousePos()
    {
        return Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }
    
    public RaycastHit GetMouseRayHit()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1000, clickTargetLayer))
        {
            return hit;
        }
        return new RaycastHit();
    }

    public void SetCamTarget(Transform InTransform, Vector3 InOffset, bool bUsePendingTarget = true, float InDuration = 0.0f)
    {
        GameManager gameManager = GameManager.GetInstance();
        if (gameManager == null) return;
        MainCameraComponent mainCamComp = MainCameraComp;
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
    
    public bool IsPointerOverGameObject()
    {
        int pointerId = 0;
#if UNITY_EDITOR || UNITY_STANDALONE
        pointerId = -1;
#endif
        return EventSystem.current.IsPointerOverGameObject(pointerId);
    }

    public bool IsCameraMoveBlocked()
    {
        // ī�޶� Focus�� �ִٸ� ������ �� ����
        if (MainVirtualCamera.LookAt != null || MainVirtualCamera.Follow != null)
        {
            return false;
        }

        foreach (var a in cameraMoveBlockReasons)
        {
            if (a.Value > 0)
            {
                return true;
            }
        }
        return false;
    }
    public bool IsCameraZoomBlocked()
    {
        foreach (var a in cameraZoomBlockReasons)
        {
            if (a.Value > 0)
            {
                return true;
            }
        }
        return false;
    }

    public bool IsCameraFocusingBlocked()
    {
        foreach (var a in cameraFocusBlockReasons)
        {
            if (a.Value > 0)
            {
                return true;
            }
        }
        return false;
    }
}
