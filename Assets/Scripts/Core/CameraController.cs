using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.Events;

[Flags]
public enum VisibilityType
{
    Default             = 0,
    UI                  = 1 << 0,
    CharacterCanvas     = 1 << 1,
    ActionCanvas        = 1 << 2,
    ShieldCanvas        = 1 << 3,

    All = int.MaxValue
};


public class CameraController : SingletonMonoBehavior<CameraController>
{
    private CinemachineVirtualCamera mainVC;
    public CinemachineVirtualCamera MainVC { get {
        if(mainVC == null)
        {
            mainVC = GameObject.Find("VirtualCamera").GetComponent<CinemachineVirtualCamera>();
        }
        return mainVC;
    } }
    
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Camera impactCamera;
    [SerializeField] private SpriteRenderer impactBackgroundRenderer;
    private Coroutine impactCoroutine;

    [SerializeField] private float startTransitionTime = 0.2f;
    [SerializeField] private float endTransitionTime = 2f;
    
    public VisibilityType visibility = VisibilityType.All;
    private Dictionary<VisibilityType, int> visibilityHideStack = new Dictionary<VisibilityType, int>();
    public UnityAction VisibilityChangeAction = null;

    private Dictionary<Character, HighlightCharacterInfo> characterHighlightDict = new Dictionary<Character, HighlightCharacterInfo>();

    private Coroutine cameraPositionCoroutine = null;
    private Coroutine cameraSizeCoroutine = null;
    
    private void Start()
    {
        visibilityHideStack.Add(VisibilityType.Default, 0);
        visibilityHideStack.Add(VisibilityType.UI, 0);
        visibilityHideStack.Add(VisibilityType.CharacterCanvas, 0);
        visibilityHideStack.Add(VisibilityType.ActionCanvas, 0);
        visibilityHideStack.Add(VisibilityType.ShieldCanvas, 0);
        visibilityHideStack.Add(VisibilityType.All, 0);
    }

    public void HighlightBot(Character taretBot)
    {
        if(characterHighlightDict.ContainsKey(taretBot) == false)
        {
            HighlightCharacterInfo highlightCharacterInfo = new HighlightCharacterInfo();
            CharacterCustomize BotCharacterCustomize = taretBot.GetComponentInChildren<CharacterCustomize>();
            highlightCharacterInfo.originSortingLayerName = BotCharacterCustomize.SortingGroup.sortingLayerName;
            highlightCharacterInfo.originOrderInLayer = BotCharacterCustomize.SortingGroup.sortingOrder;
            characterHighlightDict.Add(taretBot, highlightCharacterInfo);

            BotCharacterCustomize.SortingGroup.sortingLayerName = "AfterUI";
            BotCharacterCustomize.SortingGroup.sortingOrder = 0;

            // visibilityHideStack[VisibilityType.CharacterCanvas]++;
            visibilityHideStack[VisibilityType.ShieldCanvas]++;
            visibilityHideStack[VisibilityType.ActionCanvas]++;
            ChangeVisibility();
        }
    }

    public void ClearHighlightEffect()
    {
        foreach(var highlightCharacterInfo in characterHighlightDict)
        {
            if(highlightCharacterInfo.Key != null)
            {
                CharacterCustomize BotCharacterCustomize = highlightCharacterInfo.Key.GetComponentInChildren<CharacterCustomize>();
                BotCharacterCustomize.SortingGroup.sortingLayerName = highlightCharacterInfo.Value.originSortingLayerName;
                BotCharacterCustomize.SortingGroup.sortingOrder = highlightCharacterInfo.Value.originOrderInLayer;

                // visibilityHideStack[VisibilityType.CharacterCanvas]--;
                visibilityHideStack[VisibilityType.ShieldCanvas]--;
                visibilityHideStack[VisibilityType.ActionCanvas]--;
                ChangeVisibility();
            }
        }
        characterHighlightDict.Clear();
    }


    
    public void ChangeVisibility()
    {
        VisibilityType newVisibilityType = VisibilityType.All;

        if (HasValidVisibilityHideStack(VisibilityType.UI)) newVisibilityType &= ~VisibilityType.UI;
        if (HasValidVisibilityHideStack(VisibilityType.CharacterCanvas)) newVisibilityType &= ~VisibilityType.CharacterCanvas;
        if (HasValidVisibilityHideStack(VisibilityType.ActionCanvas)) newVisibilityType &= ~VisibilityType.ActionCanvas;
        if (HasValidVisibilityHideStack(VisibilityType.ShieldCanvas)) newVisibilityType &= ~VisibilityType.ShieldCanvas;
        visibility = newVisibilityType;
        if(VisibilityChangeAction != null)
        {
            VisibilityChangeAction.Invoke();
        }
    }
    
    public void EnableStrategyMode(bool bEnable)
    {
        if (bEnable)
        {
            visibilityHideStack[VisibilityType.ShieldCanvas]--;
            visibilityHideStack[VisibilityType.ActionCanvas]--;
        }
        else
        {
            visibilityHideStack[VisibilityType.ShieldCanvas]++;
            visibilityHideStack[VisibilityType.ActionCanvas]++;
        }
        ChangeVisibility();
    }

    public void EnableCutSceneMode(bool bEnable)
    {
        if (bEnable)
        {
            visibilityHideStack[VisibilityType.CharacterCanvas]++;
            visibilityHideStack[VisibilityType.ShieldCanvas]++;
            visibilityHideStack[VisibilityType.ActionCanvas]++;
        }
        else
        {
            visibilityHideStack[VisibilityType.CharacterCanvas]--;
            visibilityHideStack[VisibilityType.ShieldCanvas]--;
            visibilityHideStack[VisibilityType.ActionCanvas]--;
        }
        ChangeVisibility();
    }
    
    private bool HasValidVisibilityHideStack(VisibilityType visibility)
    {
        return visibilityHideStack.ContainsKey(visibility) && visibilityHideStack[visibility] > 0;
    }

    public void CameraToTarget(Transform target, Vector2 offset, float duration = 0.0f)
    {
        PlayerController playerController = PlayerController.Instance;
        if (cameraPositionCoroutine != null)
        {
            StopCoroutine(cameraPositionCoroutine);
            playerController.UnBlockCameraMove("CamToTarget",true);
        }

        if(target == null)
        {
            MainVC.Follow = null;
        }
        else if (Mathf.Approximately(duration, 0.0f))
        {
            MainVC.Follow = target;
        }
        else
        {
            cameraPositionCoroutine = StartCoroutine(CameraToTargetCoroutine(target, offset, duration));
        }
    }

    IEnumerator CameraToTargetCoroutine(Transform taret, Vector2 offset, float duration)
    {
        PlayerController playerController = PlayerController.Instance;
        playerController.BlockCameraMove("CamToTarget");
        Vector3 originPos = MainVC.transform.position;
        mainVC.Follow = null;
        float elapsedTime = 0;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            Vector3 currPos = Vector2.Lerp(originPos, taret.position + (Vector3)offset, elapsedTime/duration);
            currPos.z = MainVC.transform.position.z;
            MainVC.transform.position = currPos;
            yield return new WaitForEndOfFrame();
        }
        // Transposer ������ Follow�� �ٲٸ� ���� Follow�� ��ġ�������� �ڵ����� ������ �ǹǷ�, Follow�� �����ϰ� Position�� �����Ѵ�.
        if(taret == null)
        {
            MainVC.Follow = null;
        }
        else
        {
            MainVC.Follow = taret;
        }
        MainVC.ForceCameraPosition(MainVC.transform.position + (Vector3)offset, Quaternion.identity);
        playerController.UnBlockCameraMove("CamToTarget");
    }

    public void SetCameraSize(float size)
    {
        if (cameraSizeCoroutine != null)
        {
            StopCoroutine(cameraSizeCoroutine);
        }

        cameraSizeCoroutine = StartCoroutine(SetCameraSizeCoroutine(size));
    }

    IEnumerator SetCameraSizeCoroutine(float size)
    {
        while (Mathf.Abs(MainVC.m_Lens.OrthographicSize - size) >= 0.1f)
        {
            yield return new WaitForEndOfFrame();
            MainVC.m_Lens.OrthographicSize = Mathf.Lerp(MainVC.m_Lens.OrthographicSize, size, Time.deltaTime);
        }
        MainVC.m_Lens.OrthographicSize = size;
    }
    
    public void StartHighlight(float duration, Color impactColor)
    {
        if (impactCoroutine != null)
        {
            StopCoroutine(impactCoroutine);
        }
        impactCoroutine = StartCoroutine(ImpactCoroutine(duration, impactColor));
    }

    IEnumerator ImpactCoroutine(float duration, Color impactColor)
    {
        float elapsedTime = 0;
        impactCamera.orthographicSize = mainCamera.orthographicSize;
        impactCamera.gameObject.SetActive(true);
        Color startColor = new Color(1f, 1f, 1f, 0);
        impactBackgroundRenderer.color = startColor;

        float totalDuration = duration + startTransitionTime + endTransitionTime;
        
        while (true)
        {
            float deltaTime = Time.deltaTime;
            elapsedTime += deltaTime;

            if (elapsedTime <= startTransitionTime)
            {
                impactBackgroundRenderer.color = Color.Lerp(startColor, impactColor, elapsedTime / startTransitionTime);
            }
            else if (elapsedTime >= totalDuration - endTransitionTime)
            {
                impactBackgroundRenderer.color = Color.Lerp(impactColor, startColor, (elapsedTime - startTransitionTime - duration) / endTransitionTime);
            }
            else
            {
                impactBackgroundRenderer.color = impactColor;
            }
            
            yield return new WaitForSeconds(deltaTime);
            if (elapsedTime >= totalDuration)
            {
                break;
            }
        }
        impactCamera.gameObject.SetActive(false);
    }

    public void ShakeCamera(float force)
    {
        if (mainCamera.TryGetComponent(out CinemachineImpulseSource cinemachineImpulseSource))
        {
            cinemachineImpulseSource.GenerateImpulse(force);
        }
    }
}
