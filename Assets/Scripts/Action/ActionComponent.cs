using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;



[System.Serializable]
public class ActionInfo
{
    public int uid;
    public static int Seq = 0;
    public ActionBase action;
    // ActionBase는 재사용되므로, Action의 정보 중 저장해야하는 것은 여기에서 처리가 필요

    public ActionInfo(ActionBase action)
    {
        this.action = action;
        uid = Seq++;
    }
}


[System.Serializable]
public class AttackInfo
{
    public int uid;
    public static int Seq = 0;
    
    public ActionInfo actionInfo;
    public IAttackable attacker;
    public CharacterBase attackInstigator;
    public IHittable victim;
    public float elapsedTime;
    public float totalDamage;
    public float baseDamage;
    public float statusAddDamage;
    public float criticalRatio;
    public float staggerTime;
    public Vector2 force;
    public bool isStrike;
    public bool isStun;
    public bool isBound;
    public bool isGuardBreak;
    public ETargetFilterType targetFilterType;
    public EAttackObjectType attackObjectType;
    public bool critical = false;
    public bool guarded = false;
    public bool isDeadly = false;
    
    public AttackInfo(ActionInfo actionInfo, IAttackable attacker, CharacterBase attackInstigator,
        IHittable victim,
        ETargetFilterType targetFilterType,
        EAttackObjectType attackObjectType,
        float damage, Vector2 force, 
        bool isStrike, bool isStun, bool isGuardBreak, bool isBound,
        float staggerTime,
        bool guarded = false,
        bool forceCritical = false,
        float elapsedTime = 0.0f)
    {
        this.actionInfo = actionInfo;
        this.attacker = attacker;
        this.attackInstigator = attackInstigator;
        this.victim = victim;
        this.targetFilterType = targetFilterType;
        this.attackObjectType = attackObjectType;
        this.guarded = guarded;
        this.isGuardBreak = isGuardBreak;
        this.isStrike = isStrike;
        this.isStun = isStun;
        this.isBound = isBound;
        this.force = force;
        this.staggerTime = staggerTime;
        this.elapsedTime = elapsedTime;
        
        baseDamage = damage;

        totalDamage = baseDamage;
        statusAddDamage = 0;
        criticalRatio = 1.0f;

        critical = forceCritical;
        
        uid = Seq++;
    }
    public void SetElapsedTime(float newElapsedTime)
    {
        elapsedTime = newElapsedTime;
    }
}


[System.Serializable]
public class ReloadInfo
{
    public ActionBase action;
    public CharacterBase instigator;
    public int reloadedCount;

    public ReloadInfo(ActionBase action, CharacterBase instigator, int reloadedCount)
    {
        this.action = action;
        this.instigator = instigator;
        this.reloadedCount = reloadedCount;
    }
}


public class BookActionInfo
{
    public string  wepaonId = null;
    public Dictionary<EInputAction, List<string>> bindedActionIds = new Dictionary<EInputAction, List<string>>();
    public Dictionary<string, ActionBase> actions = new Dictionary<string, ActionBase>();
}   

public class WeaponInfo
{
    public Weapon weapon = null;
    public Dictionary<string, int> bookIds = new Dictionary<string, int>();

    public WeaponInfo(Weapon weapon, string bookId)
    {
        this.weapon = weapon;
        bookIds.Add(bookId, 1);
    }
}   

public class ActionComponent : MonoBehaviour
{
    private CharacterBase ownerCharacter;

    public CharacterBase OwnerCharacter { get { return ownerCharacter; } }

    public UnityAction<ActionBase, ActionBase> CurrentActionChanged;

    private ActionBase currentAction;
    public ActionBase CurrentAction{ get { return currentAction; }}
    
    private LinkedList<ActionBase> actionQueue = new LinkedList<ActionBase>();
    public LinkedList<ActionBase> ActionQueue{get { return actionQueue; }}
    
    // 콤보 등을 위해서 하나의 InputAction에 여러 ActionId가 들어갈 수 있다.
    private Dictionary<string, BookActionInfo> bookActionInfos = new Dictionary<string, BookActionInfo>();
    public Dictionary<string, BookActionInfo> BookActionInfos { get{ return bookActionInfos;} }
    private Dictionary<string, WeaponInfo> weaponInfos = new Dictionary<string, WeaponInfo>();
    public Dictionary<string, WeaponInfo> WeaponInfos { get{ return weaponInfos;} }
    
    private Coroutine actionCoroutine;
    
    public float remainedCooldownTime { get; private set; }
    private float cooldownTime = 0;
    
    public UnityAction<string, string> ActionTriggered;     // <0> BookId, <1> ActionId
    public UnityAction<EActionProgressType> ActionProgressChanged;
    public UnityAction<float, float> ActionCooldownChanged;
    public UnityAction ActionCanceled;
    public UnityAction EquippedBookChanged;
    public UnityAction<LinkedListNode<ActionBase>> ActionQueueAdded;
    public UnityAction<LinkedListNode<ActionBase>> ActionQueueRemoved;

    private Dictionary<EInputAction, EBookmarkType> inputActionBookmarkBindings = new Dictionary<EInputAction, EBookmarkType>();

    private void Start()
    {
        inputActionBookmarkBindings.Clear();
        inputActionBookmarkBindings.Add(EInputAction.BaseAction, EBookmarkType.Main);
        inputActionBookmarkBindings.Add(EInputAction.BaseActionHold, EBookmarkType.Main);
        inputActionBookmarkBindings.Add(EInputAction.Reload, EBookmarkType.Main);
        
        inputActionBookmarkBindings.Add(EInputAction.SubAction, EBookmarkType.Sub);
        inputActionBookmarkBindings.Add(EInputAction.SubActionHold, EBookmarkType.Sub);
        
        inputActionBookmarkBindings.Add(EInputAction.CastAction0, EBookmarkType.Signature);
        inputActionBookmarkBindings.Add(EInputAction.CastAction1, EBookmarkType.Signature);
        inputActionBookmarkBindings.Add(EInputAction.CastAction2, EBookmarkType.Signature);
    }

    public void InitializeActionComponent(CharacterBase characterBase)
    {
        ownerCharacter = characterBase;
        Character character = ownerCharacter as Character;
        if(character != null)
        {
            character.BookEquipped += HandleEquippedBook;
            character.BookUnequipped += HandleUnequippedBook;
            foreach (var book in character.bookIds)
            {
                HandleEquippedBook(book.Key);
            }
        }
    }

    public void ResetWepaons()
    {
        foreach (var weaponInfo in weaponInfos)
        {
            weaponInfo.Value.weapon.ResetWeapon();
        }
    }

    public void HideWeapons(float duration)
    {
        foreach (var weaponInfo in weaponInfos)
        {
            weaponInfo.Value.weapon.HideWeapon(duration);
        }
    }

    private void Update()
    {
        HandleCooldownTime();
    }

    private void HandleCooldownTime()
    {
        if (cooldownTime <= 0)
        {
            return;
        }
        remainedCooldownTime -= Time.deltaTime;
        ActionCooldownChanged?.Invoke(remainedCooldownTime, cooldownTime);
        // cooldownTime이 끝나면 setting된 CooldownTime을 리셋한다.
        if (remainedCooldownTime <= 0)
        {
            cooldownTime = 0;
        }
    }

    private void HandleEquippedBook(string bookId)
    {
        DataManager dataManager = DataManager.Instance;
        if (dataManager.bookDict.TryGetValue(bookId, out ConstBookInfo bookInfo))
        {
            BookActionInfo bookActionInfo = new BookActionInfo();
            RegisterActions(bookInfo, bookActionInfo);
            bookActionInfos.Add(bookId, bookActionInfo);
        }
        else
        {
            Debug.LogWarning("[Character] (SetAgentData) AgentData의 Book Id가 유효하지 않습니다.. " + bookId);
        }
        EquippedBookChanged?.Invoke();
    }

    private void HandleUnequippedBook(string bookId)
    {
        if (bookActionInfos.TryGetValue(bookId, out BookActionInfo bookActionInfo))
        {
            UnEquipWeapon(bookActionInfo.wepaonId , bookId);
            bookActionInfos.Remove(bookId);
        }
        EquippedBookChanged?.Invoke();
    }
    
    public void RegisterActions(ConstBookInfo bookInfo, BookActionInfo bookActionInfo)
    {
        DataManager dataManager = DataManager.Instance;
        // Action은 Book에 의해 결정되고, Book은 Tool을 가진다.
        string toolId = bookInfo.Tool;
        bookActionInfo.wepaonId = toolId;
        if (dataManager.weaponDict.TryGetValue(toolId, out ConstWeaponInfo toolInfo) == false)
        {
            return;
        }

        // Register된 정보는 BookActionInfo에 들어간다.
        Dictionary<EInputAction, List<string>> bindedActionIds = bookActionInfo.bindedActionIds;
        Dictionary<string, ActionBase> actions = bookActionInfo.actions;
        
        // Book에 따른 Weapon(Tool)도 장착
        WeaponInfo weaponInfo = GetWeaponInfo(toolId);
        Weapon weapon = weaponInfo == null ? null : weaponInfo.weapon;
        if (weapon == null)
        {
            weapon = EquipWeapon(toolId);
            weaponInfos.Add(toolId, new WeaponInfo(weapon, bookInfo.ID));
        }
        else
        {
            if (weaponInfos[toolId].bookIds.ContainsKey(bookInfo.ID) == false)
            {
                weaponInfos[toolId].bookIds.Add(bookInfo.ID, 1);
            }
            else
            {
                weaponInfos[toolId].bookIds[bookInfo.ID] ++;
            }
        }
        
        // Tool은 Action과 Key를 바인딩해준다.
        foreach (var toolActionKeyBind in bookInfo.ActionKeyBinding)
        {
            if(Enum.TryParse(toolActionKeyBind.Key, out EInputAction inputAction) == false)
            {
                Debug.LogWarning("[ActionComponent] 알맞은 InputAction Enum이 존재하지 않습니다 " + toolActionKeyBind.Key);
                continue;
            }
            
            // ActionKeyBinding 데이터에 따라 실제 바인딩
            if (bindedActionIds.ContainsKey(inputAction) == false)
            {
                bindedActionIds.Add(inputAction, new List<string>());
            }
            
            // Tool에 묶인 Action Id들을 Priority 기준으로 정렬
            List<string> actionIds = toolActionKeyBind.Value;
            actionIds.Sort((x, y) =>
            {
                return dataManager.actionDict[y].Priority.CompareTo(dataManager.actionDict[x].Priority);
            });
            for (int j = 0; j < actionIds.Count; j++)
            {
                string actionId = actionIds[j];
                RegisterAction_Implementation(actionId, inputAction, weapon, bookInfo.ID, ref bindedActionIds, ref actions);
            }
        }
    }

    public void RegisterAction_Implementation(string actionId, EInputAction inputAction, Weapon weapon, string bookId, 
        ref Dictionary<EInputAction, List<string>> bindedActionIds,
        ref Dictionary<string, ActionBase> actions)
    {
        DataManager dataManager = DataManager.Instance;
        if (dataManager.actionDict.TryGetValue(actionId, out ConstActionInfo actionInfo))
        {
            bindedActionIds[inputAction].Add(actionId);
            if (actions.ContainsKey(actionId) == false)
            {
                actions.Add(actionId, ActionFactory.CreateStruct(actionInfo, this, weapon, inputAction, bookId));
            }
            else
            {
                Debug.LogWarning($"[ActionComponnet] ActionId {actionId} already exists.");
            }

            // Sequence Action들도 Actions와 BindedActionIds에 포함되어야한다.
            if (string.IsNullOrEmpty(actionInfo.ActionSequence) == false)
            {
                RegisterAction_Implementation(actionInfo.ActionSequence, inputAction, weapon, bookId, ref bindedActionIds, ref actions);
            }
        }
    }

    public WeaponInfo GetWeaponInfo(string weaponId)
    {
        if (weaponInfos.TryGetValue(weaponId, out WeaponInfo weaponInfo))
        {
            return weaponInfo;
        }
        return null;
    }
    
    public Weapon EquipWeapon(string weaponId)
    {
        DataManager dataManager = DataManager.Instance;
        if (dataManager.weaponDict.TryGetValue(weaponId, out ConstWeaponInfo weaponInfo))
        {
            if (weaponInfo.WeaponPrefab != null)
            {
                Weapon weapon = Instantiate(weaponInfo.WeaponPrefab , null);
                weapon.InitializeWeapon(weaponInfo);
                weapon.Equip(ownerCharacter);
                return weapon;
            }
        }
        return null;
    }
    
    public void UnEquipWeapon(string weaponId, string bookId)
    {
        WeaponInfo weaponInfo = GetWeaponInfo(weaponId);
        if (weaponInfo != null)
        {
            if (weaponInfo.bookIds.ContainsKey(bookId))
            {
                weaponInfo.bookIds[bookId]--;
            }

            if (weaponInfo.bookIds[bookId] <= 0)
            {
                weaponInfo.bookIds.Remove(bookId);
                weaponInfo.weapon.DropWeapon(Vector2.up * 5);
                weaponInfo.weapon = null;
                weaponInfos.Remove(weaponId);
            }
        }
    }
    
    public void StartCooldown(float cooldownTime)
    {
        if (cooldownTime <= 0)
        {
            return;
        }
        this.cooldownTime = cooldownTime;
        remainedCooldownTime = cooldownTime;
    }
    
    /// <summary>
    /// actionID를 입력하면 그에 맞는 ActionBase를 생성하여 실행한다.
    /// </summary>
    public void TryExecuteAction(EInputAction key)
    {
        if (key == EInputAction.Cancel)
        {
            CancelAllInputsAndAction();
            return;
        }
        inputActionBookmarkBindings.TryGetValue(key, out EBookmarkType bookmarkType);
        string markedBookId = OwnerCharacter.AgentData.GetBookmarkedBookId(bookmarkType);

        // Bookmark가 설정이 안 되어있는 그냥 장착된 Book은 Action을 취할 수 없는 것으로 결정
        if (markedBookId != null && bookActionInfos.TryGetValue(markedBookId, out BookActionInfo markedBookActionInfo))
        {
            // Bookmarked된 book이 존재한다면 그것에 한해서 액션을 취함
            TryExecuteAction_Implementation(key,markedBookId, markedBookActionInfo);
        }
    }


    private void TryExecuteAction_Implementation(EInputAction key, string bookId, BookActionInfo bookActionInfo)
    {
        if (bookActionInfo.bindedActionIds.TryGetValue(key, out List<string> actionIds))
        {
            for (int i = 0; i < actionIds.Count; i++)
            {
                if (bookActionInfo.actions.TryGetValue(actionIds[i], out ActionBase action))
                {
                    // Toggle이면 같은 Action일 때 새로운 Action 불가케 하고 기존 것 Cancel함
                    if (currentAction == action &&
                        currentAction.ActionInfo.StrategyType == EActionStrategyType.Toggle)
                    {
                        CancelCurrentAction();
                        ownerCharacter.SetControlInput(key, false);
                        return;
                    }
                    
                    // EInputAction이 입력되면 바인딩 된 Action을 실행하려 한다.
                    if (TryExecuteAction(action))
                    {
                        ActionTriggered?.Invoke(bookId, actionIds[i]);
                        return;
                    }
                }
            }
        }
    }

    public ActionBase GetAction(string actionId)
    {
        foreach (var bookActionInfo in bookActionInfos)
        {
            foreach (var action in bookActionInfo.Value.actions)
            {
                if (action.Key == actionId)
                {
                    return action.Value;
                }
            }
        }

        return null;
    }
    
    public void ExecuteSequenceAction(string bookId, string actionId)
    {
        if (bookActionInfos.TryGetValue(bookId, out BookActionInfo bookActionInfo))
        {
            if (bookActionInfo.actions.TryGetValue(actionId, out ActionBase action))
            {
                // 시퀀스를 통해 실행되는 액션들은 딱히 제한이 없다.
                ExecuteAction(action, null, null, null);
                ActionTriggered?.Invoke(bookId, actionId);
            }
        }
    }

    public bool CanExecuteAction(ActionBase action)
    {
        if (action.ActionInfo.RequireCast)
        {
            return false;
        }

        bool hasAmmo = true;
        if (hasAmmo == false)
        {
            TryExecuteAction(EInputAction.Reload);
            return false;
        }
        foreach (var modifier in action.Modifiers)
        {
            if (modifier.CanExeuteAction() == false)
            {
                return false;
            }
        }
        // 쿨다운을 사용하는 액션들은 액션 컴포넌트에 쿨다운이 있으면 발동할 수 없다.
        if (remainedCooldownTime > 0 && action.ActionInfo.CooldownTime > 0)
        {
            return false;
        }
        
        if (OwnerCharacter.GameplayTagContainer.MatchesAll(action.ActionInfo.RequiredGameplayTags) == false)
        {
            return false;
        }

        // 보통 currentAction가 비워져있으면 어떤 액션이던 실행
        if (currentAction == null)
        {
            // TODO : 만약 첫 시작으로 실행될 수 없는 Action이라면 여기에서 처리
            return action.ActionInfo.RequiredActions.Count == 0;
        }
        else
        {
            if (currentAction.CanTransitionTo(action.ActionInfo))
            {
                return true;
            }
        }
        return false;
    }

    public void TryStopAction(EInputAction key)
    {
        if (currentAction == null)
        {
            return;
        }
        if (currentAction.RequirekeyInputAction == key)
        {
            currentAction.Stop();
        }
    }

    public bool TryExecuteAction(ActionBase action)
    {
        if (CanExecuteAction(action))
        {
            if (action.ActionInfo.ProcessType == EActionProcessType.Sequential)
            {
                ExecuteAction(action, null, null, null);
            }
            else if (action.ActionInfo.ProcessType == EActionProcessType.Parallel)
            {
                // parallelActionExecutor.AddAction(action);
            }

            return true;
        }
        return false;
    }
    
    public void ExecuteAction(ActionBase action, System.Action onComplete, System.Action onCanceled, System.Action onFinished)
    {
        // 현재 단계의 액션 실행
        SetCurrentAction(action);
        onFinished += () => { 
            SetCurrentAction(null);
            OwnerCharacter.TryChangeState(ECharacterState.Idle);
        };
        if (actionCoroutine != null)
        {
            StopCoroutine(actionCoroutine);
        }
        actionCoroutine = StartCoroutine(action.Execute(onComplete, onCanceled, onFinished));
    }

    public void HandleActionProgressChanged(EActionProgressType progressType)
    {
        if (progressType == EActionProgressType.PreAction)
        {
            // TODO : PreAction일 때 Ammo를 감소시킨다.
        }
        OwnerCharacter.TryChangeState(ECharacterState.Action);
        ActionProgressChanged?.Invoke(progressType);
    }
    
    public void CancelAllInputsAndAction()
    {
        ActionCanceled?.Invoke();
        CancelCurrentAction();
    }
    public void CancelCurrentAction()
    {
        if (currentAction == null)
        {
            return;
        }
        Debug.LogWarning($"Cancel : {currentAction.ActionInfo.id}");
        currentAction.Cancel();
        SetCurrentAction(null);
    }

    public void SetCurrentAction(ActionBase action)
    {
        if (currentAction != null)
        {
            currentAction.Cancel();
        }
        if (CurrentActionChanged != null)
        {
            CurrentActionChanged.Invoke(currentAction, action);
        }
        currentAction = action;
    }

    public LinkedListNode<ActionBase> AddActionQueue(ActionBase action)
    {
        LinkedListNode<ActionBase> node = actionQueue.AddLast(action);
        ActionQueueAdded?.Invoke(node);
        return node;
    }
    public ActionBase PeekActionQueue()
    {
        if (actionQueue.Count > 0)
        {
            return actionQueue.First.Value;
        }
        return null;
    }
    public ActionBase PopActionQueue()
    {
        if (actionQueue.Count > 0)
        {
            LinkedListNode<ActionBase> actionNode = actionQueue.First;
            ActionBase action = actionNode.Value;
            actionQueue.Remove(actionNode);
            ActionQueueRemoved?.Invoke(actionNode);
            return action;
        }
        return null;
    }

    public void RemoveActionQueue(LinkedListNode<ActionBase> targetActionNode)
    {
        if (targetActionNode == null)
        {
            return;
        }
        if (actionQueue.Contains(targetActionNode.Value))
        {
            actionQueue.Remove(targetActionNode);
            ActionQueueRemoved?.Invoke(targetActionNode);
        }
    }
    

    #region  StateMachine
    public virtual void EnterIdle()
    {
    }
    public virtual void TickIdle()
    {
    }
    public virtual void ExitIdle()
    {
    }
    
    public virtual void EnterAction()
    {
    }
    public virtual void TickAction()
    {
    }
    public virtual void ExitAction()
    {
    }
    
    public virtual void EnterStagger()
    {
    }
    public virtual void TickStagger()
    {
    }
    public virtual void ExitStagger()
    {
    }
    #endregion
    
    public Vector2 GetStartPos(EActionStartPositionType type = EActionStartPositionType.None)
    {
        switch (type)
        {
            case EActionStartPositionType.OwnerPos:
                return transform.position;
            case EActionStartPositionType.OwnerCenter:
                return ownerCharacter.GetCenterSocketPosition();
            case EActionStartPositionType.ViewDirection:
                return ownerCharacter.CharacterBody.localScale.x * Vector2.right + (Vector2)transform.position;
            case EActionStartPositionType.ViewDirectionCenter:
                return ownerCharacter.CharacterBody.localScale.x * Vector2.right + ownerCharacter.GetCenterSocketPosition();
            case EActionStartPositionType.TargetPosDirection:
                return (ownerCharacter.targetPos - (Vector2)transform.position).normalized + (Vector2)transform.position;
            case EActionStartPositionType.TargetPosOwnerCenterDirection:
                return (ownerCharacter.targetPos - ownerCharacter.GetCenterSocketPosition()).normalized * 1000 + (Vector2)transform.position;
            case EActionStartPositionType.TargetPos:
                return ownerCharacter.targetPos;
        }
        return transform.position;
    }

    public Vector2 GetDirection(EActionStartPositionType startType = EActionStartPositionType.None,
        EActionTargetPositionType targetType = EActionTargetPositionType.None)
    {
        return GetTargetPos(targetType) - GetStartPos(startType);
    }
    
    public Vector2 GetTargetPos(EActionTargetPositionType type = EActionTargetPositionType.None)
    {
        switch (type)
        {
            case EActionTargetPositionType.OwnerPos:
                return transform.position;
            case EActionTargetPositionType.OwnerCenter:
                return ownerCharacter.GetCenterSocketPosition();
            case EActionTargetPositionType.ViewDirection:
                return ownerCharacter.CharacterBody.localScale.x * Vector2.right + (Vector2)transform.position;
            case EActionTargetPositionType.ViewDirectionCenter:
                return ownerCharacter.CharacterBody.localScale.x * Vector2.right + ownerCharacter.GetCenterSocketPosition();
            case EActionTargetPositionType.TargetPosDirection:
                return (ownerCharacter.targetPos - (Vector2)transform.position).normalized * 1000 + (Vector2)transform.position;
            case EActionTargetPositionType.TargetPosOwnerCenterDirection:
                return (ownerCharacter.targetPos - ownerCharacter.GetCenterSocketPosition()).normalized * 1000 + (Vector2)transform.position;
            case EActionTargetPositionType.TargetPos:
                return ownerCharacter.targetPos;
        }
        return ownerCharacter.targetPos;
    }
}
