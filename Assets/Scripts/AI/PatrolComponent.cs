using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PerceptionComponent))]
public class PatrolComponent : MonoBehaviour
{
    private PerceptionComponent perceptionComponent;
    [SerializeField] private Transform PatrolWaypointsParent;
    private List<Transform> patrolWayPoints = new List<Transform>();
    private int currPatrolIndex;

    private bool calculatingNextMove;

    Coroutine nextPatrolCoroutine;

    private void Start()
    {
        perceptionComponent = GetComponent<PerceptionComponent>();
        SetPatrolWayPoint(PatrolWaypointsParent);
    }

    public void SetPatrolWayPoint(Transform PatrolWayPointContainer)
    {
        PatrolWaypointsParent = PatrolWayPointContainer;
        patrolWayPoints.Clear();
        if (PatrolWaypointsParent)
        {
            foreach (Transform eachChild in PatrolWaypointsParent)
            {
                patrolWayPoints.Add(eachChild);
            }
        }
    }

    private void Update()
    {
        if (perceptionComponent.PerceptoinState == EPerceptionState.Idle &&
            perceptionComponent.IsReachedPatrolPos())
        {
            if (!calculatingNextMove)
            {
                if (nextPatrolCoroutine != null)
                {
                    StopCoroutine(nextPatrolCoroutine);
                }
                nextPatrolCoroutine = StartCoroutine("CalculateNextPatrolPosCoroutine");
            }
        }
    }

    IEnumerator CalculateNextPatrolPosCoroutine()
    {
        calculatingNextMove = true;

        yield return new WaitForSeconds(2f);
        if(patrolWayPoints.Count > 0)
        {
            currPatrolIndex = (currPatrolIndex + 1) % patrolWayPoints.Count;
            perceptionComponent.SetPatrolPos(patrolWayPoints[currPatrolIndex].position);
        }

        calculatingNextMove = false;
    }
}
