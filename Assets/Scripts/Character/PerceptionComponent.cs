using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public enum EPerceptionState
{
    Idle,
    Suspicion,
    Target,
    Goal
};


[System.Serializable]
public class PerceptionInfo
{
    public Character owner;
    public GoalNode target;
    public Vector2 targetPos;
    public EPerceptionState perceptionState;
    public EPerceptionState prevPerceptionState;

    public PerceptionInfo(Character _owner,
        GoalNode _target, Vector2 _targetPos,
        EPerceptionState _perceptionState,
        EPerceptionState _prevPerceptionState)
    {
        owner = _owner;
        target = _target;
        targetPos = _targetPos;
        perceptionState = _perceptionState;
        prevPerceptionState = _prevPerceptionState;
    }
}


public class PerceptionComponent : MonoBehaviour
{
    private Character ownerCharacter;

    public GoalNode goalTarget;
    public GoalNode target;

    private GoalNode prevTarget;

    private Vector3? GoalTargetPos;
    private Vector3? TargetPos;
    private Vector3? patrolPos;

    public Vector3 LastSearchPos;

    public float recognitionVal;
    public float maxRecognitionVal = 6.0f;
    public float recognitionReduceByTimeVal = 1.0f;
    public float recognitionRatioByAttack = 1.0f;
    public float recognitionAddValueByStuff = 1.0f;
    public float recognizeCutLine = 4f;
    public float suspicionCutLine = 2f;

    private float goalResearchCycle = 3;
    private float currGoalResearchCycle = 3;

    EPerceptionState perceptionState;
    public EPerceptionState PerceptoinState { get { return perceptionState; } }

    private Animator animator;

    FieldOfPerception fieldOfPerception;
    public FieldOfPerception FieldOfPerception { get { return fieldOfPerception; } }

    [SerializeField]
    private float velocityRecognitionMagnitude = 2.0f;

    private float pathCalcCooldownTIme = 2.0f;
    private float currPathCalcCooldownTIme = 0.0f;

    [SerializeField] LayerMask wallLayer;

    [SerializeField] List<SpriteRenderer> directionArrowSprites = new List<SpriteRenderer>();

    public void InitializePerceptionComponent(Character character)
    {
        ownerCharacter = character;

        fieldOfPerception = GetComponent<FieldOfPerception>();

        animator = GetComponent<Animator>();

        ResetRecognitionData();
    }

    void Update()
    {
        if (ownerCharacter == null || ownerCharacter.IsControllable() == false) return;

        UpdatePathCooldownTime(currPathCalcCooldownTIme - Time.deltaTime);

        fieldOfPerception.startAngle = ownerCharacter.GetViewQauternion().eulerAngles.y;
        UpdatePerceptionAngle(); 

        CalculateRecognitionVal();

        UpdatePerceptionState();

        RequestProperPathWithState();

        prevTarget = target;
        if(goalTarget == null || fieldOfPerception.visibleTargets.Contains(goalTarget.transform) == false)
        {
            if (target == null && prevTarget != null && recognitionVal >= recognizeCutLine)
            {
                SetTarget(prevTarget);
            }
        }
        else
        {
            currGoalResearchCycle -= Time.deltaTime;
            SetGoalTarget(null);
        }


        if (recognitionVal < suspicionCutLine && recognitionVal > 0 && TargetPos.HasValue)
        {
            TargetPos = null;
        }
    }

    private void RequestProperPathWithState()
    {
        if (ownerCharacter.GetComponent<Player>() != null) return;

        GoalNode currentTarget = GetCurrentTarget();
        if (perceptionState == EPerceptionState.Target)
        {
            if (currentTarget && currentTarget.TryGetComponent(out Character targetCharacter))
            {
                SearchPath(currentTarget.transform.position);
            }
        }
        else if (perceptionState == EPerceptionState.Goal)
        {
            if (currentTarget != null)
            {
                SearchPath(currentTarget.transform.position);
                if (currentTarget.Completed)
                {
                    SetGoalTarget(null);
                }
            }
            else
            {
                SearchPath(GoalTargetPos.Value);
            }
        }
        else if (perceptionState == EPerceptionState.Suspicion)
        {
            SearchPath(TargetPos.Value);
        }
        else
        {
            if (patrolPos.HasValue)
            {
                SearchPath(patrolPos.Value);
            }
        }
    }


    public void ModifyInput(ref Vector2 InInput)
    {
        if(Physics.BoxCast(transform.position, Vector3.one * 0.3f, new Vector3(InInput.x,0, InInput.y), out RaycastHit hit, transform.rotation, 1.0f, wallLayer))
        {
            InInput += new Vector2(hit.normal.x, hit.normal.z);
        }
    }

    private void CalculateRecognitionVal()
    {
        float tmpAdditionalRecognition = 0;
        bool foundedSomething = false;

        for (int i = 0; i < fieldOfPerception.visibleTargets.Count; i++)
        {
            if (fieldOfPerception.visibleTargets[i] == null) continue;
            Character currTargetCharacter = fieldOfPerception.visibleTargets[i].GetComponent<Character>();
            if (currTargetCharacter)
            {
                bool recognizedMovingObject = currTargetCharacter.GetVelocity().magnitude >= velocityRecognitionMagnitude;
                float DistanceRatio = 1.0f - (Vector3.Distance(this.transform.position, currTargetCharacter.transform.position) / fieldOfPerception.GetViewRadius());
                tmpAdditionalRecognition += (recognizedMovingObject ? 2.0f : 1.0f) * DistanceRatio;
                foundedSomething = true;
            }
            else
            {
                //���� �����̴� ���� ������ Recognition ����
                Rigidbody2D currTargetRigidbody = fieldOfPerception.visibleTargets[i].GetComponent<Rigidbody2D>();
                if (currTargetRigidbody && currTargetRigidbody.linearVelocity.magnitude >= velocityRecognitionMagnitude)
                {
                    tmpAdditionalRecognition += recognitionAddValueByStuff;
                    foundedSomething = true;
                }
            }
        }

        if (foundedSomething == false) tmpAdditionalRecognition  = -recognitionReduceByTimeVal;
        SetRecognitionVal(Time.deltaTime * tmpAdditionalRecognition, true);
    }


    public Vector3 GetNearestTargetPos()
    {
        Vector3 targetPos = Vector3.zero;
        if(GetCurrentTarget())
        {
            targetPos = GetCurrentTarget().GetTargetPosition();
        }
        return targetPos;
    }


    private GoalNode GetNearestEnemy()
    {
        GoalNode foundedTarget = null;
        float minDistance = Mathf.Infinity;
        for (int i = 0; i < fieldOfPerception.visibleTargets.Count; i++)
        {
            if (fieldOfPerception.visibleTargets[i] == null) continue;
            Character currTargetCharacter = fieldOfPerception.visibleTargets[i].GetComponent<Character>();

            if (currTargetCharacter != null)
            {
                float tmpDistance = Vector3.Distance(transform.position, currTargetCharacter.transform.position);
                if (tmpDistance <= minDistance)
                {
                    minDistance = tmpDistance;
                    if(currTargetCharacter.TryGetComponent(out GoalNode goalNode))
                    {
                        if(goalNode.CanGoallable())
                        {
                            foundedTarget = goalNode;
                        }
                    }
                }
            }
        }
        return foundedTarget;
    }


    private GoalNode GetNearestPlayer()
    {
        return GameManager.GetInstance().PlayerController.DefaultPlayerCharacter.GetComponent<GoalNode>();
    }

    public void ApplyHitReact(float _stimulus, GameObject _stimulusObject, bool _dead)
    {
        if(_dead)
        {
            fieldOfPerception.viewMeshFilter.gameObject.SetActive(false);
            fieldOfPerception.enabled = false;
        }
        else
        {
            ApplyStimulusReact(_stimulus * recognitionRatioByAttack, _stimulusObject);
        }
    }

    public void SetPatrolPos(Vector3 _patrolPos)
    {
        patrolPos = _patrolPos;
    }

    public void ApplyStimulusReact(float _stimulus, GameObject _stimulusObject)
    {
        if (_stimulus == 0) return;

        SetRecognitionVal(_stimulus, true);
        if(TargetPos.HasValue)
        {
            TargetPos = TargetPos.Value;
            return;
        }
        TargetPos = (_stimulusObject != null) ? _stimulusObject.transform.position : null;
    }

    private void SetRecognitionVal(float _value, bool _add = false)
    {
        recognitionVal = Mathf.Clamp((_add ? recognitionVal + _value : _value), 0.0f, maxRecognitionVal);
    }


    public void SetGoalTarget(GoalNode _target)
    {
        currGoalResearchCycle = goalResearchCycle;
        if (_target == null)
        {
            if (goalTarget)
            {
                goalTarget.SetTargeted(ownerCharacter, false);
            }
            goalTarget = null;
            GoalTargetPos = null;
        }
        else
        {
            if (goalTarget)
            {
                goalTarget.SetTargeted(ownerCharacter, false);
            }
            goalTarget = _target;
            if (goalTarget)
            {
                goalTarget.SetTargeted(ownerCharacter, true);
            }
            SetGoalTarget(_target.GetTargetPosition());
            FillPathCooldownTime();
        }
    }
    public void SetGoalTarget(Vector3 targetPos)
    {
        if (GoalTargetPos != targetPos)
        {
            GoalTargetPos = targetPos;
        }
    }
    public void SetTarget(GoalNode target)
    {
        if(target == null)
        {
            prevTarget = this.target;
            this.target = null;
        }
        else
        {
            this.target = target;
            SetTarget(target.transform.position);
        }
    }

    public void SetTarget(Vector3 targetPos)
    {
        if(TargetPos != targetPos)
        {
            TargetPos = targetPos;
        }
    }

    private void HandleCharacterDead()
    {
        ResetRecognitionData();
    }

    public void ResetComponent()
    {
        ResetRecognitionData();
        fieldOfPerception.viewMeshFilter.gameObject.SetActive(true);
        fieldOfPerception.enabled = true;
    }

    public void ResetRecognitionData()
    {
        StopAllCoroutines();
        recognitionVal = 0;
        perceptionState = EPerceptionState.Idle;
        TargetPos = null;
        SetGoalTarget(null);
        SetTarget(null);
        UpdatePerceptionState();
    }


    /// <summary>
    /// 
    /// </summary>
    private void SearchPath(Vector3 _movePos)
    {
        LastSearchPos = _movePos;
        ownerCharacter.SearchPath(_movePos);
    }

    public void SetMaxPathCooldownTime(float maxCooldownTime)
    {
        pathCalcCooldownTIme = maxCooldownTime;
    }
    public void UpdatePathCooldownTime(float currCooldownTime)
    {
        currPathCalcCooldownTIme = currCooldownTime;
    }
    public void FillPathCooldownTime()
    {
        currPathCalcCooldownTIme = pathCalcCooldownTIme;
    }
    public float GetPathCooldownTime()
    {
        return currPathCalcCooldownTIme;
    }

    private void UpdatePerceptionState()
    {
        EPerceptionState prevState = perceptionState;
        if (goalTarget != null || GoalTargetPos.HasValue)
        {
            perceptionState = EPerceptionState.Goal;
        }
        else if (target != null) //  && recognitionVal >= suspicionCutLine
        {
            perceptionState = EPerceptionState.Target;
        }
        else if (TargetPos.HasValue)
        {
            perceptionState = EPerceptionState.Suspicion;
        }
        else
        {
            perceptionState = EPerceptionState.Idle;
        }

        if(prevState != perceptionState)
        {
            if(perceptionState != EPerceptionState.Idle)
            {
                animator.SetTrigger("Recognize");
                animator.SetFloat("RecognitionType", (int)perceptionState);
            }
        }
    }

    public GoalNode GetCurrentTarget()
    {
        if (perceptionState == EPerceptionState.Target)
        {
            return target;
        }
        if (perceptionState == EPerceptionState.Goal)
        {
            return goalTarget;
        }
        return null;
    }
    public Vector3 GetCurrentTargetPos(out bool OutSuccess)
    {
        GoalNode currentTarget = GetCurrentTarget();
        OutSuccess = true;
        if (perceptionState == EPerceptionState.Goal)
        {
            if (currentTarget)
            {
                return currentTarget.GetTargetPosition();
            }
            else if (GoalTargetPos.HasValue)
            {
                return GoalTargetPos.Value;
            }
        }
        if (perceptionState == EPerceptionState.Target)
        {
            if(currentTarget)
            {
                return currentTarget.GetTargetPosition();
            }
            else if(TargetPos.HasValue)
            {
                return TargetPos.Value;
            }
        }
        if (perceptionState == EPerceptionState.Suspicion && patrolPos.HasValue)
        {
            return patrolPos.Value;
        }
        OutSuccess = false;
        return Vector3.zero;
    }


    public GoalNode GetCurrentAttackableTarget()
    {
        GoalNode node = GetCurrentTarget();
        if (node)
        {
            return node;
        }
        return null;
    }

    public bool IsReachedPatrolPos()
    {
        return patrolPos.HasValue ? (Vector3.Distance(patrolPos.Value, transform.position) <= Bot.cBotMaxPositionError) : true;
    }


    public static float CalculateAngle(Vector3 from, Vector3 to)
    {
        return Quaternion.FromToRotation(Vector3.up, to - from).eulerAngles.z;
    }

    private void UpdatePerceptionAngle()
    {
        if(ownerCharacter != null)
        {
            fieldOfPerception.viewAngle = 60;
        }
    }
}
