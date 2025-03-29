using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class Bot : Character
{
    private Vector3 oldPosition;

    private Vector2 halfSize;

    public const float cBotMaxPositionError = 1.0f;

    int pathTargetIndex = 0;

    public UnityAction OnReachedToTarget;

    [SerializeField] private PathRenderer pathRendererPrefab;
    private PathRenderer pathRenderer;


    [SerializeField] float turnSpeed = 5;

    [SerializeField] FindingPath myPath;
    float turnDst = 5;

    private Vector2 moveDestOffset;
    Coroutine moveDestOffsetCoroutine;

    Coroutine donationCoroutine;
    Coroutine dialogueCoroutine;

    private string generatedNickName;

    public override void InitializeCharacter()
    {
        BoxCollider2D boxCollider2D = GetComponent<BoxCollider2D>();
        if(boxCollider2D)
        {
            halfSize = boxCollider2D.size * 0.5f;
            centerOffset = boxCollider2D.offset;
        }

        if(moveDestOffsetCoroutine != null)
        {
            StopCoroutine(moveDestOffsetCoroutine);
        }
        moveDestOffsetCoroutine = StartCoroutine("UpdateMoveOffset");
    }


    IEnumerator UpdateMoveOffset()
    {
        moveDestOffset = new Vector2(Random.value - 0.5f, Random.value - 0.5f).normalized;

        yield return new WaitForSeconds(Random.Range(1.0f, 3.0f));

        moveDestOffsetCoroutine = StartCoroutine("UpdateMoveOffset");
    }

    protected void MoveInputFunction()
    {
        mInputs[(int)EKeyInput.Left] = false;
        mInputs[(int)EKeyInput.Right] = false;
        mInputs[(int)EKeyInput.Up] = false;
        mInputs[(int)EKeyInput.Down] = false;
        mInputs[(int)EKeyInput.Dodge] = false;

        Vector2 currentInput = Vector2.zero;

        if (IsControllable())
        {
            //get the position of the bottom of the bot's aabb, this will be much more useful than the center of the sprite (mPosition)
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
        }


        SetInput(currentInput);
        oldPosition = transform.position;
    }


    private void HandleConfrontContext(ref Vector2 OutInput)
    {
        GoalNode attackableTarget = perceptionComponent.GetCurrentAttackableTarget();
        if (attackableTarget)
        {
            Vector3 diff = attackableTarget.transform.position - perceptionComponent.transform.position;
            diff.y = 0;
            perceptionComponent.transform.LookAt(attackableTarget.transform.position+ diff);
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
            Vector3 currentDest = perceptionComponent.GetCurrentTargetPos(out bool bSuccess);
            if(bSuccess)
            {
                Vector3 diff = currentDest - perceptionComponent.transform.position;
                if (diff.magnitude > cBotMaxPositionError)
                {
                    diff.y = 0;
                    Quaternion targetRot = Quaternion.LookRotation(diff);
                    perceptionComponent.transform.rotation = Quaternion.Lerp(perceptionComponent.transform.rotation, targetRot, Time.deltaTime * turnSpeed);
                }
                Vector3 currentDir = GetViewDirection() + new Vector3(moveDestOffset.x, 0, moveDestOffset.y) * 0.2f;
                OutInput = Vector2.Lerp(input, new Vector2(currentDir.x, currentDir.z).normalized, Time.deltaTime * turnSpeed);
            }
        }
    }


    private void MoveAlongPath(ref Vector2 OutInput)
    {
        Vector3 currentDest = myPath.lookPoints[pathTargetIndex];

        Vector3 diff = currentDest - perceptionComponent.transform.position;
        if (diff.magnitude > cBotMaxPositionError)
        {
            diff.y = 0;
            Quaternion targetRot = Quaternion.LookRotation(diff);
            perceptionComponent.transform.rotation = Quaternion.Lerp(perceptionComponent.transform.rotation, targetRot, Time.deltaTime * turnSpeed);
        }
        Vector3 currentDir = GetViewDirection() + new Vector3(moveDestOffset.x, 0, moveDestOffset.y) * 0.2f;
        if (diff.magnitude < cBotMaxPositionError)
        {
            pathTargetIndex++;
        }
        OutInput = Vector2.Lerp(input, new Vector2(currentDir.x, currentDir.z).normalized, Time.deltaTime * turnSpeed);
        // GC�ɷ��� �ϴ� ����
        // perceptionComponent.ModifyInput(ref OutInput);
    }

    bool searchingPath = false;

    public override void SearchPath(Vector3 destination)
    {
        if (platformerComponent.collisions.below == false)
        {
            mInputs[(int)EKeyInput.Dodge] = false;
            mInputs[(int)EKeyInput.Crounch] = false;
            return;
        }

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
            GoalNode currGoalNode = perceptionComponent.GetCurrentTarget();
            if (currGoalNode && currGoalNode.TryGetComponent(out Player targetPlayer))
            {
                pathRenderer.gameObject.SetActive(true);
                pathRenderer.DrawPathLines(targetPath, perceptionComponent.GetCurrentTarget(), destination, offset);
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

        float distToCurrTarget = Vector3.Distance(this.transform.position, perceptionComponent.GetCurrentTargetPos(out bool OutSuccess));
        bool bInAttackRange = false;

        if (pathSuccessful)
        {
            ReachedToTarget();
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

        if(perceptionComponent)
        {
            GoalNode currentTraget = perceptionComponent.GetCurrentTarget();
            if (currentTraget == null)
            {
                switch (perceptionComponent.PerceptoinState)
                {
                    case EPerceptionState.Target:
                        perceptionComponent.SetTarget(null);
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


    public override void Respawn()
    {
        base.Respawn();
        if (pathRenderer)
        {
            pathRenderer.gameObject.SetActive(true);
        }
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

    public void OnDrawGizmos()
    {
        if (myPath != null)
        {
            myPath.DrawWithGizmos();
        }
    }
}
