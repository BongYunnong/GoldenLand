using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum EAimTargtType
{
    Instigator,
    Raycast,
    CircleCast,
    OverlapCircle
};

[System.Serializable]
public class ConstActionAimInfo
{
    public string Id;
    public EAimTargtType AimTargtType;
    public float Range;
    public float CircleRadius;
    public Vector2 Direction;
    public LayerMask TargetLayer;
    public int Count;
    public float Duration;
    
    public ConstActionAimInfo(ActionAimDataSet.TableData data)
    {
        this.Id = data.ID;
        this.AimTargtType = data.AimTargtType;
        this.Range = data.Range;
        this.CircleRadius = data.CircleRadius;
        this.Direction = data.Direction.normalized;  // 방향 벡터는 정규화
        this.TargetLayer = LayerMask.GetMask(data.LayerMaskStrings.ToArray());
        this.Count = data.Count;
        this.Duration =data. Duration;
    }
}

public class ActionAim : MonoBehaviour
{
    private ConstActionAimInfo aimInfo;
    public List<IHittable> hittables = new List<IHittable>();
    private Coroutine searchCoroutine;

    public void StartSearch(IHittable instigator, ConstActionAimInfo info)
    {
        StartSearch(info);
        if (info.AimTargtType == EAimTargtType.Instigator)
        {
            hittables.Add(instigator);
        }
    }

    public void StartSearch(ConstActionAimInfo info)
    {
        this.aimInfo = info;
        hittables.Clear();
        
        // 기존 Coroutine 중지 후 새로 시작
        CancelSearch();
        
        searchCoroutine = StartCoroutine(RepeatedSearch());
    }

    // 반복 탐색 Coroutine
    private IEnumerator RepeatedSearch()
    {
        yield return new WaitForEndOfFrame();
        for (int i = 0; i < aimInfo.Count; i++)
        {
            DoSearch();
            yield return new WaitForSeconds(aimInfo.Duration);
        }
    }

    private void DoSearch()
    {
        switch (aimInfo.AimTargtType)
        {
            case EAimTargtType.Instigator:
            {
                break;
            }
            case EAimTargtType.Raycast:
            {
                RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position,
                    aimInfo.Direction, aimInfo.Range, aimInfo.TargetLayer);
                for (int i = 0; i < hits.Length; i++)
                {
                    RaycastHit2D hit = hits[i];
                    if (hit.collider != null && hit.collider.TryGetComponent(out IHittable hittable))
                    {
                        hittables.Add(hittable);
                        Debug.Log($"Raycast Hit: {hit.collider.gameObject.name}");
                    }
                }
                break;
            }
            case EAimTargtType.CircleCast:
            {
                RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position,
                    aimInfo.CircleRadius, aimInfo.Direction, aimInfo.Range, aimInfo.TargetLayer);
                for (int i = 0; i < hits.Length; i++)
                {
                    RaycastHit2D hit = hits[i];
                    if (hit.collider != null && hit.collider.TryGetComponent(out IHittable hittable))
                    {
                        hittables.Add(hittable);
                        Debug.Log($"CircleCast Hit: {hit.collider.gameObject.name}");
                    }
                }
                break;
            }
            case EAimTargtType.OverlapCircle:
            {
                Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position,
                    aimInfo.CircleRadius, aimInfo.TargetLayer);
                foreach (var collider in colliders)
                {
                    if (collider != null && collider.TryGetComponent(out IHittable hittable))
                    {
                        hittables.Add(hittable);
                        Debug.Log($"OverlapCircle Hit: {collider.gameObject.name}");
                    }
                }
                break;
            }
        }
    }

    public void CancelSearch()
    {
        if (searchCoroutine != null)
        {
            StopCoroutine(searchCoroutine);
        }
    }
    
    // 디버깅을 위해 탐색 영역을 시각화
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        if (Application.isPlaying)
        {
            switch (aimInfo.AimTargtType)
            {
                case EAimTargtType.Instigator:
                    if (hittables.Count > 0)
                    {
                        Gizmos.DrawSphere(hittables[0].GetGameObject().transform.position, 0.1f);
                    }
                    break;
                case EAimTargtType.Raycast:
                    Gizmos.DrawLine(transform.position, transform.position + (Vector3)(aimInfo.Direction * aimInfo.Range));
                    break;
                case EAimTargtType.CircleCast:
                case EAimTargtType.OverlapCircle:
                    Gizmos.DrawWireSphere(transform.position, aimInfo.CircleRadius);
                    break;
            }
        }
    }
}
