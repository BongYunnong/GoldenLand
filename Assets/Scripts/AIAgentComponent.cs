using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class AIAgentComponent : MonoBehaviour
{
    public enum ECharacterContext
    {
        None = 0,
        MoveTo,
        Confront
    };

    
    private Character ownerCharacter;
    public Character OwnerCharacter
    {
        get
        {
            if (ownerCharacter == null)
            {
                ownerCharacter = GetComponentInChildren<Character>();
            }
            return ownerCharacter;
        } 
    }

    public PerceptionComponent Perception
    {
        get
        {
            return OwnerCharacter.Perception;
        }
    }
    public ECharacterContext currentContext = ECharacterContext.None;
    
    [SerializeField] FindingPath myPath;
    
    [SerializeField] private PathRenderer pathRendererPrefab;
    private PathRenderer pathRenderer;
    
    private float turnDst = 5;
    [SerializeField] float turnSpeed = 5;
    
    private Vector3 oldPosition;
    private Vector2 halfSize;

    private int pathTargetIndex = 0;
    private bool searchingPath = false;
    
    private Vector2 moveDestOffset;
    private Coroutine moveDestOffsetCoroutine;

    public UnityAction OnReachedToTarget;
    
    public const float cBotMaxPositionError = 1.0f;

    private void Awake()
    {
        ownerCharacter = GetComponent<Character>();
    }

    private void Start()
    {
        if(moveDestOffsetCoroutine != null)
        {
            StopCoroutine(moveDestOffsetCoroutine);
        }
        moveDestOffsetCoroutine = StartCoroutine("UpdateMoveOffset");
    }

    private void Update()
    {
        Vector2 currentInput = Vector2.zero;
        switch (currentContext)
        {
            case ECharacterContext.None:
                break;
            case ECharacterContext.Confront:
                HandleConfrontContext(ref currentInput);
                break;
            case ECharacterContext.MoveTo:
                HandleMoveToContext(ref currentInput);
                break;
        }

        ownerCharacter.SetMoveInput(currentInput);
        oldPosition = transform.position;
    }

    private void HandleConfrontContext(ref Vector2 OutInput)
    {
        GoalNode attackableTarget = Perception.GetCurrentAttackableTarget();
        if (attackableTarget)
        {
            Vector3 diff = attackableTarget.transform.position - Perception.transform.position;
            diff.y = 0;
            Perception.transform.LookAt(attackableTarget.transform.position+ diff);
        }
    }

    private void HandleMoveToContext(ref Vector2 OutInput)
    {
        if (myPath.lookPoints.Length > pathTargetIndex)
        {
            MoveAlongPath(ref OutInput);
        }
        else
        {
            Vector3 currentDest = Perception.GetCurrentTargetPos(out bool bSuccess);
            if(bSuccess)
            {
                Vector3 diff = currentDest - Perception.transform.position;
                if (diff.magnitude > cBotMaxPositionError)
                {
                    diff.y = 0;
                    Quaternion targetRot = Quaternion.LookRotation(diff);
                    Perception.transform.rotation = Quaternion.Lerp(Perception.transform.rotation, targetRot, Time.deltaTime * turnSpeed);
                }
                Vector3 currentDir = Perception.transform.TransformDirection(Vector3.forward).normalized + new Vector3(moveDestOffset.x, 0, moveDestOffset.y) * 0.2f;
                OutInput = Vector2.Lerp(ownerCharacter.MoveInput, new Vector2(currentDir.x, currentDir.z).normalized, Time.deltaTime * turnSpeed);
            }
        }
    }

    IEnumerator UpdateMoveOffset()
    {
        moveDestOffset = new Vector2(Random.value - 0.5f, Random.value - 0.5f).normalized;

        yield return new WaitForSeconds(Random.Range(1.0f, 3.0f));

        moveDestOffsetCoroutine = StartCoroutine("UpdateMoveOffset");
    }
    
    
    public void ChangeAction(ECharacterContext newAction)
    {
        if(currentContext != newAction)
        {
            currentContext = newAction;
        }
    }
    
    private void MoveAlongPath(ref Vector2 OutInput)
    {
        Vector3 currentDest = myPath.lookPoints[pathTargetIndex];

        Vector3 diff = currentDest - Perception.transform.position;
        if (diff.magnitude > cBotMaxPositionError)
        {
            diff.y = 0;
            Quaternion targetRot = Quaternion.LookRotation(diff);
            Perception.transform.rotation = Quaternion.Lerp(Perception.transform.rotation, targetRot, Time.deltaTime * turnSpeed);
        }
        Vector3 currentDir = Perception.transform.TransformDirection(Vector3.forward).normalized + new Vector3(moveDestOffset.x, 0, moveDestOffset.y) * 0.2f;
        if (diff.magnitude < cBotMaxPositionError)
        {
            pathTargetIndex++;
        }
        OutInput = Vector2.Lerp(ownerCharacter.MoveInput, new Vector2(currentDir.x, currentDir.z).normalized, Time.deltaTime * turnSpeed);
    }


    public void SearchPath(Vector3 destination)
    {
        if(searchingPath==false)
        {
            searchingPath = true;
            // ���� �پ��� �� ���� �� ã�� ��찡 �� ����
            Vector3 randomOffset = new Vector3(Random.value - 0.5f, 0, Random.value - 0.5f) * 0.5f;
            PathRequestManager.RequestPath(new PathRequest(transform.position + randomOffset, destination, OnPathFound));
        }

        List<Vector3> targetPath = new List<Vector3>();
        Vector3 offset = Vector3.forward * 0.2f;
        targetPath.Add(this.transform.position);
        if(myPath != null && myPath.lookPoints != null)
        {
            //for (var i = Mathf.Max(0, pathTargetIndex); i < myPath.lookPoints.Length; i++)
            for (var i = 0; i < myPath.lookPoints.Length; i++)
            {
                targetPath.Add(myPath.lookPoints[i]);
            }
        }
        if (pathRenderer)
        {
            GoalNode currGoalNode = Perception.GetCurrentTarget();
            if (currGoalNode && currGoalNode.TryGetComponent(out Player targetPlayer))
            {
                pathRenderer.gameObject.SetActive(true);
                pathRenderer.DrawPathLines(targetPath, Perception.GetCurrentTarget(), destination, offset);
            }
            else
            {
                pathRenderer.gameObject.SetActive(false);
            }
        }
    }

    public void OnPathFound(Vector3[] newPath, bool pathSuccessful)
    {
        searchingPath = false;
        if (pathSuccessful)
        {
            pathTargetIndex = 0;
            myPath = new FindingPath(newPath, transform.position, turnDst);
        }

        float distToCurrTarget = Vector3.Distance(this.transform.position, Perception.GetCurrentTargetPos(out bool OutSuccess));
        bool bInAttackRange = false;
        
        if (Perception != null && Perception.GetCurrentAttackableTarget())
        {
            bInAttackRange = Vector3.Distance(this.transform.position, Perception.GetCurrentAttackableTarget().transform.position) <= 1;
        }

        if (pathSuccessful)
        {
            if (bInAttackRange)
            {
                ChangeAction(ECharacterContext.Confront);
            }
            else if (myPath.lookPoints.Length > pathTargetIndex)
            {
                ChangeAction(ECharacterContext.MoveTo);
            }
            else
            {
                ReachedToTarget();
            }
        }
        else
        {
            ReachedToTarget();
        }
    }

    private void ReachedToTarget()
    {
        if (pathRenderer)
        {
            pathRenderer.ClearPathLines();
        }

        if(Perception)
        {
            GoalNode currentTraget = Perception.GetCurrentTarget();
            if (currentTraget == null)
            {
                switch (Perception.PerceptoinState)
                {
                    case EPerceptionState.Target:
                        Perception.SetTarget(null);
                        break;
                    default:
                        break;
                }
            }
        }

        if (OnReachedToTarget != null)
        {
            OnReachedToTarget.Invoke();
        }
    }

    public int HandleGoalPriority(List<GoalNode> _goalNodes)
    {
        if (_goalNodes.Count > 0)
        {
            int nearestIndex = -1;
            int farthestIndex = -1;
            float minDist = Mathf.Infinity;
            float maxDist = 0;
            for (int i = 0; i < _goalNodes.Count; i++)
            {
                float currDist = Vector3.Distance(transform.position, _goalNodes[i].GetTargetPosition());
                if (currDist < minDist)
                {
                    minDist = currDist;
                    nearestIndex = i;
                }
                if (currDist > maxDist)
                {
                    maxDist = currDist;
                    farthestIndex = i;
                }
            }
            return nearestIndex;
        }
        return -1;
    }


    
    public bool IsPathFindable()
    {
        return Perception && Perception.GetPathCooldownTime() <= 0;
    }

    private void OnDestroy()
    {
        if (pathRenderer)
        {
            Destroy(pathRenderer);
        }
    }
}
