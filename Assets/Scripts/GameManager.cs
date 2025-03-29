using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    public static GameManager GetInstance()
    {
        if (instance == null)
        {
            instance = FindObjectOfType<GameManager>();
        }
        return instance;
    }
    public bool initialized = false;

    [SerializeField]
    private CinemachineConfiner cameraConfiner;
    [SerializeField]
    private CinemachineBrain cinemachineBrain;
    public CinemachineBrain CinemachineBrain { get { return cinemachineBrain; } }
    [SerializeField]
    private MainCameraComponent mainCameraComp;
    public MainCameraComponent MainCameraComp { get { return mainCameraComp; } }

    [SerializeField] protected Transform mainCanvas;
    public Transform MainCanvas { get { return mainCanvas; } }

    protected MapBase map;
    public MapBase Map { get { return map; } }

    protected PlayerController playerController = null;
    public PlayerController PlayerController { get { return playerController; } }

    public bool CanAttackAlly = false;

    [SerializeField] private float camMoveSpeed = 15.0f;
    [SerializeField] private float camScaleSpeed = 20.0f;
    [SerializeField] private Vector2 camScaleMinMax = new Vector2(5.0f, 15.0f);

    protected float temporalTargetTimeScale = 1;
    protected float temporalTargetTimeScaleRecoveryMultiplier = 1.0f;

    public virtual void InitializeGame()
    {
        playerController = FindObjectOfType<PlayerController>();
        playerController.InitializePlayerController();

        map = FindObjectOfType<MapBase>();

        initialized = true;

        temporalTargetTimeScale = 1;
        temporalTargetTimeScaleRecoveryMultiplier = 1;
        Time.timeScale = 1;
    }

    protected virtual void Update()
    {
        if (initialized == false) return;

        RecoverTargetTimeScale();
        UpdateCamera();

        if(Input.GetMouseButtonDown(0))
        {
            HandleMouseClickInput(Input.mousePosition);
        }
    }

    private void RecoverTargetTimeScale()
    {
        if (Mathf.Approximately(temporalTargetTimeScale, 1.0f) == false)
        {
            float sign = Mathf.Sign(temporalTargetTimeScale - 1.0f);
            temporalTargetTimeScale += Time.unscaledDeltaTime * -sign * temporalTargetTimeScaleRecoveryMultiplier;
            Time.timeScale = temporalTargetTimeScale;
        }
    }

    public void SetTemporalTargetTimeScale(float InTargetTimeScale, float InRecoveryMultiplier)
    {
        temporalTargetTimeScale = InTargetTimeScale;
        temporalTargetTimeScaleRecoveryMultiplier = InRecoveryMultiplier;
    }

    private void UpdateCamera()
    {
#if UNITY_ANDROID && !UNITY_EDITOR_WIN
        if (Input.touchCount == 1)
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
        if (Input.touchCount == 2)
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

        Vector3 input = MainCameraComp.VirtualCamera.transform.right * Input.GetAxis("Horizontal") +
            Vector3.ProjectOnPlane(MainCameraComp.VirtualCamera.transform.forward, Vector3.up).normalized * Input.GetAxis("Vertical");
        if (MainCameraComp.VirtualCamera.LookAt == null)
        {
            MainCameraComp.VirtualCamera.transform.position += (camMoveSpeed * input.normalized * Time.deltaTime);
            if (cameraConfiner)
            {
                float tmpX = Mathf.Clamp(MainCameraComp.VirtualCamera.transform.position.x, cameraConfiner.m_BoundingShape2D.bounds.min.x, cameraConfiner.m_BoundingShape2D.bounds.max.x);
                float tmpZ = Mathf.Clamp(MainCameraComp.VirtualCamera.transform.position.z, cameraConfiner.m_BoundingShape2D.bounds.min.y, cameraConfiner.m_BoundingShape2D.bounds.max.y);
                MainCameraComp.VirtualCamera.transform.position = new Vector3(tmpX, MainCameraComp.VirtualCamera.transform.position.y, tmpZ);
            }
        }
        float mouseWheel = Input.GetAxis("Mouse ScrollWheel");
        Zoom(mouseWheel);
#endif
    }

    private void Zoom(float zoomValue)
    {
        if (Mathf.Abs(zoomValue) >= 0.1f)
        {
            if(Camera.main.orthographic)
            {
                MainCameraComp.AddOrthographicSize(camScaleSpeed * -Mathf.Sign(zoomValue) * Time.deltaTime);
                MainCameraComp.SetOrthographicSize(Mathf.Clamp(MainCameraComp.VirtualCamera.m_Lens.OrthographicSize, camScaleMinMax.x, camScaleMinMax.y));
            }
            else
            {
                Vector3 targetPos = MainCameraComp.VirtualCamera.transform.position + (MainCameraComp.VirtualCamera.transform.forward * zoomValue * 6);
                targetPos.y = Mathf.Clamp(targetPos.y, 1, 50);

                if (cameraConfiner)
                {
                    targetPos.x = Mathf.Clamp(targetPos.x, cameraConfiner.m_BoundingShape2D.bounds.min.x, cameraConfiner.m_BoundingShape2D.bounds.max.x);
                    targetPos.z = Mathf.Clamp(targetPos.z, cameraConfiner.m_BoundingShape2D.bounds.min.y, cameraConfiner.m_BoundingShape2D.bounds.max.y);
                }
                MainCameraComp.VirtualCamera.transform.position = targetPos;
            }
        }
    }

    public virtual void HandleMouseClickInput(Vector2 mousePos)
    {
    }


    public PlayerController GetPlayerController()
    {
        return playerController;
    }

    public virtual void HandleCharacterSpawned(Character character)
    {

    }

    public virtual bool IsCharacterControllable()
    {
        return true;
    }
}
