using Unity.VisualScripting;
using UnityEngine;

public class Character : CharacterBase
{
    [System.Serializable]
    public enum ECharacterMoveState
    {
        Idle,
        Run,
        Dodge
    };

    public enum ECharacterContext
    {
        None = 0,
        MoveTo,
        Confront
    };
    
    public enum EKeyInput
    {
        Left = 0,
        Right,
        Up,
        Down,
        Dodge,
        Crounch,
        Attack,
        Interact,
        MAX
    }
    protected PlatformerComponent platformerComponent;
    
    protected PerceptionComponent perceptionComponent;
    public PerceptionComponent Perception { get { return perceptionComponent; } }

    protected Animator anim;
    protected Rigidbody rb;
    public Transform CharacterBody;
    
    protected Vector3 centerOffset;
    public Vector3 CenterOffset { get { return centerOffset; } }
    
    protected float CharScale;
    public float dirX { get { return Vector3.Dot(transform.forward, Camera.main.transform.right); } }
    protected float scaleDirX=1;

    public ECharacterMoveState currentMoveState = ECharacterMoveState.Idle;
    public ECharacterContext currentContext = ECharacterContext.None;

    [Header("[Character - Movement]")]
    public bool CanGetInput = true;
    protected Vector2 input;
    protected bool[] mInputs;
    protected bool[] mPrevInputs;

    protected Vector3 velocity;
    public Vector3 Velocity { get { return velocity; } }
    protected Vector3 targetVelocity;
    protected Vector3 additionalForceVelocity;

    protected float velocityXSmoothing;
    protected float velocityZSmoothing;

    public bool dashInput = false;
    
    [Header("[AdditionalMovement]")]
    protected float accelerationTimeAirborne = 0.2f;
    protected float acceleratingTimeGrounded = .1f;
    
    protected float fCrounchRemember = 0;
    protected float fCrounchRememberTime = 0.2f;
    
    protected float f_Dodge;
    
    protected bool initialized = false;
    
    public virtual void InitializeCharacter()
    {
        platformerComponent = GetComponent<PlatformerComponent>();
        anim = CharacterBody.GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        
        initialized = true;
        
        Respawn();
    }
    
    protected virtual void Update()
    {
        UpdatePrevInputs();
        
        additionalForceVelocity = Vector2.Lerp(additionalForceVelocity, Vector2.zero, Time.deltaTime * 10f);
        
        BaseMoveFunc();
        MoveInputFunction();
        
        FlipCharacter();
    }

    protected virtual void MoveInputFunction()
    {
        if (f_Dodge <= 0)
        {
            float InputX = 0;
            if (mInputs[(int)EKeyInput.Right]) InputX = 1;
            else if (mInputs[(int)EKeyInput.Left]) InputX = -1;
            float InputZ = 0;
            if (mInputs[(int)EKeyInput.Up]) InputZ = 1;
            else if (mInputs[(int)EKeyInput.Down]) InputZ = -1;

            SetInput(new Vector2(InputX, InputZ));

            if (mInputs[(int)EKeyInput.Left] || mInputs[(int)EKeyInput.Right] ||
                mInputs[(int)EKeyInput.Up] || mInputs[(int)EKeyInput.Down])
            {
                dashInput = false;
            }
        }
        else
        {
            SetInput(Vector2.zero);
        }
    }
    
    private void BaseMoveFunc()
    {
        if (f_Dodge > 0.0f)
        {
            f_Dodge -= Time.deltaTime;
        }

        if (f_Dodge > 0.0f)
        {
            platformerComponent.Move(input * GetDodgeSpeed());
            return;
        }

        if (input.magnitude <= 0.1f)
        {
            dashInput = false;
        }

        targetVelocity.x = input.x * GetMoveSpeed();
        targetVelocity.z = input.y * GetMoveSpeed();

        fCrounchRemember -= Time.deltaTime;
    }

    protected virtual void FlipCharacter()
    {
        Vector3 projectedInput = Vector3.ProjectOnPlane(perceptionComponent.transform.forward, Camera.main.transform.forward).normalized;
        if (projectedInput.x > projectedInput.y) scaleDirX = 1.0f;
        else if (projectedInput.x < projectedInput.y) scaleDirX = -1.0f;
        CharacterBody.localScale = new Vector3(scaleDirX * CharScale, Mathf.Abs(CharScale), 1f);
    }
    
    public virtual void Respawn()
    {
        StopImmediately(Vector2.zero);
    }
    
    public void StopImmediately(Vector2 input, bool bResetOnlyMovement = false)
    {
        CanGetInput = true;

        SetInput(Vector2.zero);
        mInputs = new bool[(int)EKeyInput.MAX];
        mPrevInputs = new bool[(int)EKeyInput.MAX];

        targetVelocity = Vector3.zero;
        velocity = Vector3.zero;
        additionalForceVelocity = Vector2.zero;

        perceptionComponent.transform.rotation = Quaternion.Euler(this.input.x, 0, this.input.y);
        CharacterBody.localScale = new Vector3(dirX * CharScale, Mathf.Abs(CharScale), 1f);
    }

    public void SetInput(Vector2 input)
    {
        this.input = input;
    }

    public void UpdatePrevInputs()
    {
        for (byte i = 0; i < (byte)EKeyInput.MAX; ++i)
        {
            mPrevInputs[i] = mInputs[i];
        }
    }

    public virtual void SearchPath(Vector3 destination)
    {
    }

    public Vector3 GetVelocity()
    {
        return velocity;
    }
    public float GetMoveSpeed()
    {
        return 10;
    }

    public float GetDodgeSpeed()
    {
        return 10;
    }
    
    public Vector3 GetCharacterBodyPosition()
    {
        return transform.position + centerOffset;
    }
    
    public Vector3 GetViewDirection()
    {
        return perceptionComponent.transform.TransformDirection(Vector3.forward).normalized;
    }

    public virtual Quaternion GetViewQauternion()
    {
        return perceptionComponent.transform.rotation;
    }
    
    public virtual bool IsControllable()
    {
        return true;
    }
}
