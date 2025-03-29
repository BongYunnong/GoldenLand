using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FieldOfPerception : FieldOfView
{
    public LayerMask targetMask;
    public List<Transform> visibleTargets = new List<Transform>();

    [SerializeField] LineRenderer debugLineRender;

    private void OnEnable()
    {
        StopCoroutine("FindTargetsWithDelay");
        StartCoroutine("FindTargetsWithDelay", 0.1f);
    }

    private void OnDisable()
    {
        StopCoroutine("FindTargetsWithDelay");
    }


    IEnumerator FindTargetsWithDelay(float delay)
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(delay);
            FindVisibleTargets();
        }
    }

    public void FindVisibleTargets()
    {
        visibleTargets.Clear();

        List<Transform> targetsInViewRadius = new List<Transform>();
        Collider[] colls = Physics.OverlapSphere(transform.position, Mathf.Max(viewRadius, circleViewRadius), targetMask);
        for (int i = 0; i < colls.Length; i++)
        {
            targetsInViewRadius.Add(colls[i].transform);
        }

        for (int i = 0; i < targetsInViewRadius.Count; i++)
        {
            Transform target = targetsInViewRadius[i];

            if (ownerCharacter && target == ownerCharacter.transform) continue;
            if (visibleTargets.Contains(target)) continue;

            Vector3 dirToTarget = (target.position - transform.position).normalized;
            float dstToTarget = Vector3.Distance(transform.position, target.position );

            bool hitted = Physics.Raycast(transform.position, dirToTarget,out RaycastHit hit, dstToTarget, obstacleMask);
            if (!hitted)
            {
                if (dstToTarget <= circleViewRadius)
                {
                    visibleTargets.Add(target);
                }
                else if (Mathf.Abs(Vector3.SignedAngle(transform.forward, dirToTarget, Vector3.up)) <= viewAngle / 2)
                {
                    visibleTargets.Add(target);
                }
            }
        }
    }

    public bool GetProperCoverPosition(Vector3 _movePos, out Vector3 OutDestPos)
    {
        coverPositions = coverPositions.Distinct().ToList();
        float targetDist = Vector3.Distance(_movePos, transform.position);
        for (int i = coverPositions.Count - 1; i >=0; i--)
        {
            if (Vector3.Dot(coverPositions[i].position - transform.position, _movePos - transform.position) < 0)
            {
                coverPositions.RemoveAt(i);
                continue;
            }

            if (Vector3.Distance(coverPositions[i].position, transform.position) > targetDist)
            {
                coverPositions.RemoveAt(i);
                continue;
            }
        }

        if (coverPositions.Count > 0)
        {
            coverPositions.Sort(delegate (Transform a, Transform b)
            {
                return (transform.position - a.position).magnitude > (transform.position - b.position).magnitude ? 1 : -1;
            });

            Debug.DrawLine(transform.position, coverPositions[0].position);
            Debug.DrawLine(coverPositions[0].position, coverPositions[0].position + transform.forward * -1.5f);

            OutDestPos = coverPositions[0].position + transform.forward * -1.5f;
            return true;
        }
        OutDestPos = ownerCharacter.transform.position;
        return false;
    }
}
