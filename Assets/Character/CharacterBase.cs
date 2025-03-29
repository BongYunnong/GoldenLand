using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

[System.Serializable]
public enum ECharacterState
{
    Idle,
    Action,
    Stagger,
    Stun,
    Down,
    WallHit,
    Airborne,
};

public enum EDodgeType
{
    Base,
    Force,
    Teleport
}

[System.Serializable]
public class DodgeInfo
{
    public EDodgeType dodgeType;
    public Vector2 startPosition;
    public Vector2 targetPosition;
    public AnimationCurve dashCurve;
    public float dashDuration;
    public float elapsedTime;
    public bool hitted;

    public DodgeInfo(EDodgeType dodgeType, Vector2 startPosition, Vector2 targetPosition, float dashDuration, AnimationCurve dashCurve)
    {
        this.dodgeType = dodgeType;
        this.startPosition = startPosition;
        this.targetPosition = targetPosition;
        if (dashDuration == 0)
        {
            Debug.LogWarning("DodgeInfo: dashDuration is zero");
            dashDuration = 1;
        }
        this.dashDuration = dashDuration;
        this.dashCurve = dashCurve;
        elapsedTime = 0;
        hitted = false;
    }
}


[System.Serializable]
public struct ConstCharacterInfo
{
    public string equipmentPresetId;

    public SerializableDictionary<EEquipmentType, string> defaultEquipmentIds;

    public ConstCharacterInfo(CharacterDataSet.TableData data)
    {
        equipmentPresetId = data.EquipmentPresetId;
        
        defaultEquipmentIds = new SerializableDictionary<EEquipmentType, string>();
        if (DataManager.Instance.equipmentPresetDict.TryGetValue(equipmentPresetId,
                out ConstEquipmentPresetInfo equipmentPresetInfo))
        {
            defaultEquipmentIds[EEquipmentType.Hair] = equipmentPresetInfo.Hair;
            defaultEquipmentIds[EEquipmentType.BackHair] = equipmentPresetInfo.BackHair;
            defaultEquipmentIds[EEquipmentType.Head] = equipmentPresetInfo.Head;
            defaultEquipmentIds[EEquipmentType.Face] = equipmentPresetInfo.Face;
            defaultEquipmentIds[EEquipmentType.Hat] = equipmentPresetInfo.Hat;
            defaultEquipmentIds[EEquipmentType.Accessory] = equipmentPresetInfo.Accessory;
            defaultEquipmentIds[EEquipmentType.Robe] = equipmentPresetInfo.Robe;
            defaultEquipmentIds[EEquipmentType.UpperBody] = equipmentPresetInfo.UpperBody;
            defaultEquipmentIds[EEquipmentType.LowerBody] = equipmentPresetInfo.LowerBody;
            defaultEquipmentIds[EEquipmentType.BackStuff] = equipmentPresetInfo.BackStuff;
            defaultEquipmentIds[EEquipmentType.HandStuff] = equipmentPresetInfo.HandStuff;
        }
    }
}


[System.Serializable]
public class CharacterInfo
{
    public string characterID = null;
    public List<string> bookIds = new List<string>();
    public List<string> bookMarks = new List<string>();
    public List<SpecInfo> specInfos = new List<SpecInfo>();
    public IReadOnlyList<SpecInfo> SpecInfos => specInfos;
    public SerializableDictionary<EEquipmentType, string> equipments = new SerializableDictionary<EEquipmentType, string>();


    public void AddAllSpecExperience(int addExp)
    {
        for(int i=0; i < (int)ESpecType.MAX; i++)
        {
            AddSpecExperience((ESpecType)i, addExp);
        }
    }

    public void AddSpecExperience(ESpecType specType, int addExp)
    {
        SpecInfo currSpecInfo = GetSpecInfo(specType);
        if(currSpecInfo == null)
        {
            currSpecInfo = new SpecInfo();
            currSpecInfo.specType = specType;
            currSpecInfo.AddSpecExperience(addExp);
            specInfos.Add(currSpecInfo);
        }
        else
        {
            currSpecInfo.AddSpecExperience(addExp);
        }
    }

    public SpecInfo GetSpecInfo(ESpecType specType)
    {
        return specInfos.Find(x => x.specType == specType);
    }

    public List<string> GetPersonalityIds()
    {
        List<string> personalityIds = new List<string>();
        DataManager dataManager = DataManager.Instance;
        for (int i = 0; i < bookIds.Count; i++)
        {
            string bookId = bookIds[i];
            if (dataManager.bookDict.TryGetValue(bookId, out ConstBookInfo constBookInfo))
            {
                for (int j = 0; j < constBookInfo.PersonalityIds.Count; j++)
                {
                    string personalityId = constBookInfo.PersonalityIds[j];
                    personalityIds.Add(personalityId);
                }
            }
        }
        return personalityIds;
    }

    public bool HasBook(string bookId)
    {
        int foundedIndex = bookIds.FindIndex(x => x == bookId);
        return foundedIndex >= 0;
    }
    public void AddBookId(string bookId)
    {
        int foundedIndex = bookIds.FindIndex(x => x == bookId);
        if (foundedIndex >= 0)
        {
            return;
        }
        bookIds.Add(bookId);
    }
    public void RemoveBookId(string bookId)
    {
        int foundedIndex = bookIds.FindIndex(x => x == bookId);
        if (foundedIndex >= 0)
        {
            bookIds.RemoveAt(foundedIndex);
        }
    }

    public void SetBookmark(EBookmarkType bookmarkType, string bookId)
    {
        // Type index에 값이 없으면 추가 필요
        for (int i = bookMarks.Count; i <= (int)bookmarkType; i++)
        {
            bookMarks.Add(null);
        }
        bookMarks[(int)bookmarkType] = bookId;
    }

    public string GetBookmarkedBookId(EBookmarkType bookmarkType)
    {
        // Type index에 값이 없으면 추가 필요
        for (int i = bookMarks.Count; i <= (int)bookmarkType; i++)
        {
            bookMarks.Add(null);
        }
        return bookMarks[(int)bookmarkType];
    }
    public EBookmarkType GetBookmarkedType(string bookId)
    {
        for (int i = 0; i < bookMarks.Count; i++)
        {
            if (bookMarks[i] == bookId)
            {
                return (EBookmarkType)i;
            }
        }
        return EBookmarkType.MAX;
    }
}

public class CharacterBase : MonoBehaviour
{
    [SerializeField] protected ConstCharacterInfo constCharacterInfo;
    public ConstCharacterInfo ConstCharacterInfo { get { return constCharacterInfo; } }

    [SerializeField] protected CharacterInfo characterInfo;
    public CharacterInfo CharacterInfo { get { return characterInfo; } }
    
    [HideInInspector] public CharacterCustomize characterCustomize;
    public CharacterCustomize CharacterCustomize { get { return characterCustomize; } }
    
    [HideInInspector] public CharacterSocketComponent characterSocketComponent;
    public CharacterSocketComponent CharacterSocketComponent { get { return characterSocketComponent; } }

    protected Animator anim;
    
    protected ActionComponent actionComponent;
    public ActionComponent ActionComponent { get
        {
            if (actionComponent == null)
            {
                actionComponent = GetComponentInChildren<ActionComponent>();
                actionComponent.InitializeActionComponent(this);
            }
            return actionComponent;
        } 
    }
    
    private GameplayTagContainer gameplayTagContainer = new GameplayTagContainer();
    public GameplayTagContainer GameplayTagContainer {get { return gameplayTagContainer; } }
    
    public Transform CharacterBody;
    [SerializeField] protected Transform centerSocket;
    public Transform CenterSocket { get { return centerSocket; } }

    
    public const float characterScaleMultiplier = 1f;
    protected float CharScale = 1;
    
    // 마우스 위치 혹은 Bot의 목표 타겟의 위치
    public Vector2 targetPos = Vector2.zero;
    // 벽 점프, 회피 등 input을 무시하는 시야 방향
    public float forceViewDirX = 0;

    [Header("[Character - Movement]")]
    public bool CanGetInput = true;
    protected Vector2 moveInput;
    public Vector2 MoveInput { get { return moveInput; } }
    protected bool[] mInputs = new bool[(int)EInputAction.MAX];
    protected bool[] mPrevInputs = new bool[(int)EInputAction.MAX];

    public float inputMultiplier = 1;
    protected Vector3 velocity;
    public Vector3 Velocity { get { return velocity; } }
    protected Vector3 targetVelocity;
    public Vector3 TargetVelocity { get { return targetVelocity; } }
    protected Vector3 additionalForceVelocity;
    public Vector3 AdditionalForceVelocity { get { return additionalForceVelocity; } }

    private Dictionary<string, int> jumpBlockCounts = new Dictionary<string, int>();
    private Dictionary<string, int> viewControlBlockCounts = new Dictionary<string, int>();
    private Dictionary<string, int> gravityBlockCounts = new Dictionary<string, int>();
    
    protected ECharacterState currentCharacterState = ECharacterState.Idle;
    public ECharacterState CurrentCharacterState { get { return currentCharacterState; } }
    protected State<CharacterBase, ECharacterState>[] states;
    private StateMachine<CharacterBase, ECharacterState> stateMachine;
    public StateMachine<CharacterBase, ECharacterState> StateMachine  { get { return stateMachine; } }
    // 설정된 조건이 모두 충족되면 상태 변경을 위함 
    private Dictionary<ECharacterState, List<System.Func<bool>>> stateTriggers = new Dictionary<ECharacterState, List<System.Func<bool>>>();
    public UnityAction<ECharacterState> StateChanged;

    
    public UnityAction<CharacterInfo> CharacterInfoUpdated;
    
    protected bool initialized = false;

    protected virtual void Awake()
    {
        CharScale = CharacterBody.transform.localScale.x * characterScaleMultiplier;
        SetCharacterBodyScale(1);

        anim = GetComponentInChildren<Animator>();
        characterCustomize = CharacterBody.GetComponent<CharacterCustomize>();
        characterSocketComponent = CharacterBody.GetComponent<CharacterSocketComponent>();
    }

    private void Start()
    {
        InitializeCharacter();
    }

    public virtual void InitializeCharacter()
    {
        SetupStates();
        TryChangeState(ECharacterState.Idle);
        
        InitializeComponents();

        Respawn();
        initialized = true;
    }

    private void SetupStates()
    {
        states = new State<CharacterBase, ECharacterState>[7];
        states[(int)ECharacterState.Idle] = new CharacterStates.Idle();
        states[(int)ECharacterState.Action] = new CharacterStates.Action();
        states[(int)ECharacterState.Stagger] = new CharacterStates.Stagger();
        states[(int)ECharacterState.Stun] = new CharacterStates.Stun();
        states[(int)ECharacterState.Down] = new CharacterStates.Down();
        states[(int)ECharacterState.WallHit] = new CharacterStates.WallHit();
        states[(int)ECharacterState.Airborne] = new CharacterStates.Airborne();

        stateMachine = new StateMachine<CharacterBase, ECharacterState>();
        stateMachine.Setup(this, states[(int)ECharacterState.Idle]);
        
        StateChanged += HandleStateChanged;
    }
    
    public bool TryChangeState(ECharacterState stateType, bool force = false)
    {
        if (force == false && 
            (currentCharacterState == stateType ||
             stateMachine.CanTransitionTo(stateType) == false))
        {
            return false;
        }
        
        currentCharacterState = stateType;
        stateMachine.ChangeState(states[(int)stateType]);
        StateChanged?.Invoke(currentCharacterState);
        return true;
    }
    
    protected virtual void HandleStateChanged(ECharacterState characterState)
    {
    }
    
    protected virtual void InitializeComponents()
    {
    }

    public virtual void Respawn()
    {
        if (anim)
        {
            anim.Rebind();
        }
        StopImmediately(Vector2.zero);
    }

    public virtual void StopImmediately(Vector2 input, bool bResetOnlyMovement = false)
    {
        CanGetInput = true;

        SetMoveInput(Vector2.zero);
        SetTargetPos(targetPos);
        mInputs = new bool[(int)EInputAction.MAX];
        mPrevInputs = new bool[(int)EInputAction.MAX];

        targetVelocity = Vector3.zero;
        velocity = Vector3.zero;
        additionalForceVelocity = Vector2.zero;

        SetForceViewInput(0);
    }

    protected virtual void Update()
    {
        if (stateMachine == null)
        {
            return;
        }
        stateMachine.Execute();

        UpdateInputs();
    }
    
    public void UpdateInputs()
    {
        UpdatePrevInputs();
    }
    
    public void SetMoveInput(Vector2 input, bool bUpdateViewDIr = true)
    {
        this.moveInput = Vector2.ClampMagnitude(input, 1);
        if (bUpdateViewDIr && Mathf.Abs(input.x) > 0)
        {
            SetTargetPos(new Vector2(transform.position.x + input.x, transform.position.y));
        }
    }
    public void ResetControlInput()
    {
        for (EInputAction inputAction = EInputAction.None;
             inputAction < EInputAction.MAX;
             inputAction++)
        {
            SetControlInput(inputAction, false);
        }
    }

    public void SetControlInput(EInputAction inputActionType, bool value)
    {
        mInputs[(int)inputActionType] = value;
    }
    public bool GetControlInput(EInputAction inputActionType)
    {
        return mInputs[(int)inputActionType];
    }
    public bool GetPrevControlInput(EInputAction inputActionType)
    {
        return mPrevInputs[(int)inputActionType];
    }

    public void SetForceViewInput(float dirX = 0)
    {
        forceViewDirX = dirX;
        if (dirX == 0)
        {
            SetTargetPos(targetPos);
        }
        else
        {
            SetCharacterBodyScale(Mathf.Sign(forceViewDirX));
        }
    }


    public void SetTargetPos(Vector2 targetPos)
    {
        this.targetPos = targetPos;
        if (forceViewDirX != 0) return;
        if (IsControllable() == false) return;
        float resultDir = Mathf.Sign(this.targetPos.x - transform.position.x);
        SetCharacterBodyScale(resultDir);
    }

    private void SetCharacterBodyScale(float dirX)
    {
        CharacterBody.localScale = new Vector3(dirX * CharScale, Mathf.Abs(CharScale), 1f);
    }

    public float GetTargetViewDirX()
    {
        return Mathf.Sign(this.targetPos.x - transform.position.x);
    }

    public void UpdatePrevInputs()
    {
        for (byte i = 0; i < (byte)EInputAction.MAX; ++i)
        {
            mPrevInputs[i] = mInputs[i];
        }
    }


    public void SetVelocity(Vector2 velocity)
    {
        this.velocity = velocity;
    }
    public void SetTargetVelocity(Vector2 velocity)
    {
        this.targetVelocity = velocity;
    }
    public void SetAdditionalVelocity(Vector2 velocity)
    {
        this.additionalForceVelocity = velocity;
    }

    public void TryPlayAnimTrigger(string animName)
    {
        anim.SetTrigger(animName);
    }
    public void TryPlayAnimBool(string animName, bool active)
    {
        anim.SetBool(animName, active);
    }

    public void TryPlayAnimInteger(string animName, int index)
    {
        anim.SetInteger(animName, index);
    }
    public void TryPlayAnimFloat(string animName, float value)
    {
        anim.SetFloat(animName, value);
    }

    
    public bool IsControllable()
    {
        return CurrentCharacterState != ECharacterState.Stagger &&
               CurrentCharacterState != ECharacterState.Stun && 
               CurrentCharacterState != ECharacterState.Airborne &&
               CurrentCharacterState != ECharacterState.WallHit &&
               CurrentCharacterState != ECharacterState.Down && 
               IsControllableWithSystem();
    }
    
    public virtual bool IsControllableWithSystem()
    {
        return true;
    }
    
    public Vector2 GetCenterSocketPosition()
    {
        return centerSocket.position;
    }
    public Vector2 GetCenterSocketLocalPosition()
    {
        return centerSocket.localPosition;
    }
    public float GetHeight()
    {
        return 1;
    }
    
    
    
    public void BlockView(bool block, string reason)
    {
        if (viewControlBlockCounts.ContainsKey(reason))
        {
            viewControlBlockCounts[reason] += block ? 1 : -1;
        }
        else if(block)
        {
            viewControlBlockCounts.Add(reason, 1);
        }
        else
        {
            Debug.LogError($"BlockView 카운트가 존재하지 않는데 block을 해제하려 하였습니다. reason: {reason}");
        }
    }


    public bool IsViewControlBlocked()
    {
        foreach (var viewControlBlockCount in viewControlBlockCounts)
        {
            if (viewControlBlockCount.Value > 0)
            {
                return true;
            }
        }
        return false;
    }


    public void BlockGravity(bool block, string reason)
    {
        if (gravityBlockCounts.ContainsKey(reason))
        {
            gravityBlockCounts[reason] += block ? 1 : -1;
        }
        else if(block)
        {
            gravityBlockCounts.Add(reason, 1);
        }
        else
        {
            Debug.LogError($"Gravity Block 카운트가 존재하지 않는데 block을 해제하려 하였습니다. reason: {reason}");
        }
    }
    public bool IsGravityBlocked()
    {
        foreach (var gravityBlockCount in gravityBlockCounts)
        {
            if (gravityBlockCount.Value > 0)
            {
                return true;
            }
        }

        return false;
    }
    
    
    public void BlockJump(bool block, string reason)
    {
        if (jumpBlockCounts.ContainsKey(reason))
        {
            jumpBlockCounts[reason] += block ? 1 : -1;
        }
        else if(block)
        {
            jumpBlockCounts.Add(reason, 1);
        }
        else
        {
            Debug.LogError($"BlockView 카운트가 존재하지 않는데 block을 해제하려 하였습니다. reason: {reason}");
        }
    }


    public bool IsJumpBlocked()
    {
        foreach (var jumpBlockCount in jumpBlockCounts)
        {
            if (jumpBlockCount.Value > 0)
            {
                return true;
            }
        }
        return false;
    }

    
    public void CancelCurrentAction()
    {
        ActionComponent.CancelCurrentAction();
    }

    public virtual void HandleActionFinish()
    {
        
    }
}
