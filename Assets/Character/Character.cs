using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class Character : CharacterBase
{
    protected PlatformerComponent platformerComponent;
    public PlatformerComponent PlatformerComponent { get
        {
            if (platformerComponent == null)
            {
                platformerComponent = GetComponentInChildren<PlatformerComponent>();
            }
            return platformerComponent;
        } 
    }
    
    protected PerceptionComponent perceptionComponent;
    public PerceptionComponent Perception {get
        {
            if (perceptionComponent == null)
            {
                perceptionComponent = GetComponentInChildren<PerceptionComponent>();
            }
            return perceptionComponent;
        } 
    }

    public Dictionary<string, bool> bookIds = new Dictionary<string, bool>();
    private Dictionary<EBookmarkType, string> bookmarks = new Dictionary<EBookmarkType, string>();
    public Dictionary<EBookmarkType, string> Bookmarks {get {return bookmarks;}}

    protected Rigidbody ribidgeBody;
    public Rigidbody Rigidbody { get { return Rigidbody; } }

    [Header("[Movement]")]
    protected float velocityXSmoothing = 1;
    protected float velocityZSmoothing = 1;
    
    protected bool jumping = false;
    [Range(0, 1f)] public float f_CutJumpHeight = 0.5f;
    protected float jumpVelocity;
    protected float gravity;
    public const float cMaxFallingSpeed = -50.0f;

    protected int mFramesFromJumpStart = 0;
    public float f_JumpPressedRemember = 0;
    [HideInInspector] public float f_JumpPressedRememberTime = 0.1f;

    protected float f_GroundedRemember;
    protected float f_GroundedRememberTime = 0.2f;
    
    public static float TimeToJumpApex = 0.4f;
    public static int MaxJumpLevel = 3;
    
    protected DodgeInfo dodgeInfo = null;
    public bool dashing = false;
    [SerializeField] protected float dashSpeedMultiplier = 1.5f;
    [SerializeField] protected TrailRenderer dashTrail;
    private Tween hitShakeTween =  null;

    
    [Header("[AdditionalMovement]")]
    [SerializeField] private float fallDamageVelocityBound = 25f;
    [SerializeField] private float fallDamageMul = 0.1f;
    [SerializeField] private float fallDamageBounceForce = 3;

    protected float accelerationTimeAirborne = 0.2f;
    protected float acceleratingTimeGrounded = .1f;
    protected float additionalVelocityDrag = 0.95f;
    
    public UnityAction<string> BookEquipped;
    public UnityAction<string> BookUnequipped;
    public UnityAction<EBookmarkType, string> BookmarkChanged;
    
    protected override void InitializeComponents()
    {
        base.InitializeComponents();
        platformerComponent = GetComponent<PlatformerComponent>();
        ribidgeBody = GetComponent<Rigidbody>();
    }
    
    public override void Respawn()
    {
        base.Respawn();
        if (perceptionComponent)
        {
            perceptionComponent.ResetComponent();
        }
        if(characterCustomize)
        {
            characterCustomize.SetSprite(constCharacterInfo);
            characterCustomize.ResetColor();
        }
    }
    
    public override void StopImmediately(Vector2 input, bool bResetOnlyMovement = false)
    {
        base.StopImmediately(input, bResetOnlyMovement);

        gravity = -(2 * MaxJumpLevel) / Mathf.Pow(TimeToJumpApex, 2);
        jumpVelocity = Mathf.Abs(gravity) * TimeToJumpApex;
        f_JumpPressedRemember = 0;
        f_GroundedRemember = 0;
        CancelJump();
        SetDash(false);
    }

    protected override void Update()
    {
        base.Update();
        
        ControlCharacter();
        UpdateAnimation();
    }

    private void ControlCharacter()
    {
        if (PlatformerComponent.collisions.above || PlatformerComponent.collisions.below)
        {
            velocity.y = 0;
        }

        if (PlatformerComponent.collisions.below)
        {
            f_GroundedRemember = f_GroundedRememberTime;

            bool hasBoundTag = GameplayTagContainer.HasTag(EGameplayTag.CanBound);
            //낙뎀
            if (hasBoundTag == false && velocity.y <= -fallDamageVelocityBound)
            {
                GameplayTagContainer.AddTag(EGameplayTag.CanBound);
            }

            if (hasBoundTag)
            {
                StartGroundBounce();
            }
        }
        
        float smoothingValue = (PlatformerComponent.collisions.below || dashing) ? acceleratingTimeGrounded : accelerationTimeAirborne;
        
        additionalForceVelocity *= additionalVelocityDrag;
        if (additionalForceVelocity.magnitude < 0.01f) {
            additionalForceVelocity = Vector2.zero;
        }

        if (CanGetInput && IsControllableWithSystem())
        {
            BaseMoveFunc();
            DashFunction();
            JumpFunction();
        }
        else
        {
            ribidgeBody.linearVelocity = Vector3.zero;
            targetVelocity = Vector3.zero;
        }

        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocity.x, ref velocityXSmoothing, smoothingValue);
        velocity.z = Mathf.SmoothDamp(velocity.z, targetVelocity.z, ref velocityZSmoothing, smoothingValue);
        if (IsGravityBlocked() == false)
        {
            velocity.y += gravity * Time.deltaTime;
        }
        if (dodgeInfo != null)
        {
            UpdateDodge();
        }
        else
        {
            Vector3 resultVelocity = (velocity + additionalForceVelocity) * Time.deltaTime;
            PlatformerComponent.Move(resultVelocity);
        }
        
        FlipCharacter();
    }

    

    private void BaseMoveFunc()
    {
        if (IsControllable() == false) return;

        if (PlatformerComponent.collisions.below)
        {
            CancelJump();
        }

        float speedMultiplier = 10.0f;
        float inputScale = speedMultiplier * inputMultiplier;
        if (dashing)
        {
            targetVelocity.x = moveInput.x * inputScale * dashSpeedMultiplier;
            targetVelocity.z = moveInput.y * inputScale * dashSpeedMultiplier;
        }
        else if (PlatformerComponent.collisions.below == false)
        {
            targetVelocity.x = Mathf.Lerp(targetVelocity.x, moveInput.x * inputScale, Time.deltaTime);
            targetVelocity.z = Mathf.Lerp(targetVelocity.z, moveInput.y * inputScale, Time.deltaTime);
        }
        else
        {
            targetVelocity.x = moveInput.x * inputScale;
            targetVelocity.z = moveInput.y * inputScale;
        }

        f_JumpPressedRemember -= Time.deltaTime;
        f_GroundedRemember -= Time.deltaTime;
    }
    
    private void StartGroundBounce()
    {
        GameplayTagContainer.RemoveTag(EGameplayTag.CanBound);

        SetVelocity(Vector2.zero);
        SetTargetVelocity(Vector2.zero);
        SetAdditionalVelocity(Vector2.up * 15);
    }


    private void UpdateDodge()
    {
        // 경과 시간 비율
        float prevElapsedTime = dodgeInfo.elapsedTime;
        dodgeInfo.elapsedTime += Time.deltaTime;
        float prevT = prevElapsedTime / dodgeInfo.dashDuration;
        float t = dodgeInfo.elapsedTime / dodgeInfo.dashDuration;
        
        switch (dodgeInfo.dodgeType)
        {
            case EDodgeType.Base:
            {
                if (dodgeInfo.dashCurve != null)
                {
                    // AnimationCurve에 따라 t 값 보정
                    float prevValue = dodgeInfo.dashCurve.Evaluate(prevT);
                    float curveValue = dodgeInfo.dashCurve.Evaluate(t);
                    // 다음 이동 위치 계산
                    Vector2 currentPosition = Vector2.Lerp(dodgeInfo.startPosition, dodgeInfo.targetPosition, prevValue);
                    Vector2 nextPosition = Vector2.Lerp(dodgeInfo.startPosition, dodgeInfo.targetPosition, curveValue);
                    Vector2 diff = nextPosition - currentPosition;
                    PlatformerComponent.Move(diff);
                }
                break;
            }
            case EDodgeType.Force:
            {
                Vector2 diff = dodgeInfo.targetPosition - dodgeInfo.startPosition;
                PlatformerComponent.Move(diff * Time.deltaTime);
                break;
            }
            case EDodgeType.Teleport:
            {
                transform.position = dodgeInfo.targetPosition;
                break;
            }
        }
        // 대시 종료 처리
        if (t >= 1.0f)
        {
            dodgeInfo = null;
        }
    }

    protected virtual void DashFunction()
    {
        if (IsControllable() == false)
        {
            SetDash(false);
            return;
        }

        bool leftInput = GetControlInput(EInputAction.Left);
        bool rightInput = GetControlInput(EInputAction.Right);
        SetDash(leftInput || rightInput);
    }

    protected virtual void JumpFunction()
    {
        if (IsControllable() == false || IsJumpBlocked())
        {
            CancelJump();
            return;
        }
        
        bool jumpInput = GetControlInput(EInputAction.Jump);
        if (jumpInput)
        {
            f_JumpPressedRemember = f_JumpPressedRememberTime;
        }
        if (GetControlInput(EInputAction.JumpRelease))
        {
            if (velocity.y > 0)
            {
                velocity = new Vector2(velocity.x, velocity.y * f_CutJumpHeight);
            }
        }

        if (f_JumpPressedRemember > 0 && f_GroundedRemember > 0)
        {
            Jump();
        }
    }

    private void Jump()
    {
        if (GameplayTagContainer.HasTag(EGameplayTag.Dodging))
        {
            return;
        }
        jumping = true;
        f_JumpPressedRemember = 0;
        f_GroundedRemember = 0;
        velocity.y = jumpVelocity;
        anim.SetTrigger("Jump");
        GameplayTagContainer.AddTag(EGameplayTag.Jumping);
    }

    private void CancelJump()
    {
        if (jumping)
        {
            jumping = false;
            GameplayTagContainer.RemoveTag(EGameplayTag.Jumping);
        }
    }

    
    private void UpdateAnimation()
    {
        anim.SetBool("IsGrounded", PlatformerComponent.collisions.below);
        anim.SetFloat("Vel", new Vector2(velocity.x,velocity.z).magnitude);
        anim.SetFloat("DirMatch", (targetPos.x - transform.position.x) * velocity.x);
    }

    protected virtual void FlipCharacter()
    {
        float scaleDirX = 1.0f;
        Vector3 projectedInput = Vector3.ProjectOnPlane(Perception.transform.forward, Camera.main.transform.forward).normalized;
        if (projectedInput.x > projectedInput.y) scaleDirX = 1.0f;
        else if (projectedInput.x < projectedInput.y) scaleDirX = -1.0f;
        CharacterBody.localScale = new Vector3(scaleDirX * CharScale, Mathf.Abs(CharScale), 1f);
    }
    
    public void SetDodgeInfo(DodgeInfo dodgeInfo)
    {
        this.dodgeInfo = dodgeInfo;
        if (dodgeInfo != null)
        {
            Dodge_Impl();
        }
        else
        {
            GameplayTagContainer.RemoveTag(EGameplayTag.Dodging);
            SetForceViewInput(0);
        }
    }

    public void Dodge(EDodgeType dodgeType, Vector2 targetPos, float duration, bool triggerAnim)
    {
        if (triggerAnim)
        {
            anim.SetTrigger("Dodge");
        }
        additionalForceVelocity = Vector2.zero;

        Vector2 currentPosition = transform.position;
        AnimationCurve myCurve = new AnimationCurve();
        // 키프레임 추가 (시간, 값)
        myCurve.AddKey(0.0f, 0.0f);
        myCurve.AddKey(duration, 1.0f);
        // 키프레임의 탄젠트(곡률) 설정
        myCurve.keys[1].inTangent = 0f;   // 중간점의 들어오는 곡선 방향
        myCurve.keys[1].outTangent = 0f;  // 중간점의 나가는 곡선 방향
        
        SetDodgeInfo(new DodgeInfo(dodgeType, currentPosition, targetPos, duration, myCurve));
    }
    
    protected virtual void Dodge_Impl()
    {
        SetDash(false);
        GameplayTagContainer.AddTag(EGameplayTag.Dodging);
        velocity = Vector3.zero;
        targetVelocity = Vector2.zero;
        additionalForceVelocity = Vector2.zero;
        SetForceViewInput(this.dodgeInfo.targetPosition.x - dodgeInfo.startPosition.x);
    }



    protected void SetDash(bool value)
    {
        if (dashing == value) return;
        if (value)
        {
            if (GameplayTagContainer.HasTag(EGameplayTag.Dodging))
            {
                return;
            }
            if (CurrentCharacterState != ECharacterState.Idle)
            {
                return;
            }
        }
        
        dashing = value;
        dashTrail.emitting = dashing;

        if (dashing)
        {
            Dash_Impl();
        }
        else
        {
            GameplayTagContainer.RemoveTag(EGameplayTag.Dashing);
        }
    }

    protected virtual void Dash_Impl()
    {
        GameplayTagContainer.AddTag(EGameplayTag.Dashing);
            
        float moveSpeed = 5.0f;
        targetVelocity.x = moveInput.x * moveSpeed * dashSpeedMultiplier * 0.75f;
        targetVelocity.z = moveInput.y * moveSpeed * dashSpeedMultiplier * 0.75f;
        additionalForceVelocity = Vector2.zero;
            
        anim.SetTrigger("Dash");
    }
    
    public virtual void SearchPath(Vector3 destination)
    {
        if (TryGetComponent(out AIAgentComponent aiAgent))
        {
            aiAgent.SearchPath(destination);
        }
    }
    
    public Vector3 GetViewDirection()
    {
        return perceptionComponent.transform.TransformDirection(Vector3.forward).normalized;
    }

    public virtual Quaternion GetViewQauternion()
    {
        return perceptionComponent.transform.rotation;
    }
    
    public override void HandleActionFinish()
    {
        SetDodgeInfo(null);
    }
}
