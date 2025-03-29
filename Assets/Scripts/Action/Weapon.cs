using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;


public enum EColliderType
{
    Box,
    Circle,
    Polygon
}

public enum EWeaponAttachState
{
    Stored,
    Idle,
    Action
}

[System.Serializable]
public struct ConstWeaponInfo
{
    public string ID;
    public Weapon WeaponPrefab;
    public List<Sprite> Sprites;
    public List<string> AnimStrings;
    public string Collider;
    public Dictionary<EWeaponAttachState, List<SocketChildTransformInfo>> SocketChildTransformInfos;
    public EAttackObjectType AttackObjectType;

    public ConstWeaponInfo(WeaponDataSet.TableData data)
    {
        this.ID = data.ID;
        GameObject weapon = data.WeaponPrefab.GetValue();
        if (weapon != null)
        {
            this.WeaponPrefab = weapon.GetComponent<Weapon>();
        }
        else
        {
            this.WeaponPrefab = null;
        }

        this.Collider = data.Collider;
        
        Sprites = new List<Sprite>();
        for (int i = 0; i < data.Sprites.Count; i++)
        {
            Sprites.Add(data.Sprites[i].GetSprite());
        }

        this.AnimStrings = data.AnimStrings;
        
        SocketChildTransformInfos = new Dictionary<EWeaponAttachState, List<SocketChildTransformInfo>>();
        // 무기가 비활성화 되어있는 상태
        List<SocketChildTransformInfo> storedSockets = new List<SocketChildTransformInfo>();
        for (int i = 0; i < data.IdleSocketSettings.Count; i++)
        {
            storedSockets.Add(new SocketChildTransformInfo(data.StoredSocketSettings[i]));
        }
        SocketChildTransformInfos.Add(EWeaponAttachState.Stored, storedSockets);
        // 무기가 활성화 되어있는 상태
        List<SocketChildTransformInfo> idleSockets = new List<SocketChildTransformInfo>();
        for (int i = 0; i < data.IdleSocketSettings.Count; i++)
        {
            idleSockets.Add(new SocketChildTransformInfo(data.IdleSocketSettings[i]));
        }
        SocketChildTransformInfos.Add(EWeaponAttachState.Idle, idleSockets);
        // 무기가 액션 수행중인 상태
        List<SocketChildTransformInfo> actionSockets = new List<SocketChildTransformInfo>();
        for (int i = 0; i < data.ActionSocketSettings.Count; i++)
        {
            actionSockets.Add(new SocketChildTransformInfo(data.ActionSocketSettings[i]));
        }
        SocketChildTransformInfos.Add(EWeaponAttachState.Action, actionSockets);
        // 설정이 없는 것 마저 초기화
        for (EWeaponAttachState attachState = EWeaponAttachState.Stored; attachState <= EWeaponAttachState.Action; attachState++)
        {
            if (SocketChildTransformInfos.ContainsKey(attachState) == false)
            {
                SocketChildTransformInfos.Add(attachState, null);
            }
        }

        AttackObjectType = data.AttackObjectType;
    }
}

[System.Serializable]
public enum EWeaponState
{
    Idle,
    Attack,
    Dropped
};

[System.Serializable]
public enum EAttackObjectType
{
    None,
    Melee,
    Gun,
    Slinger,
    Trap,
    KillZone
};



[System.Serializable]
public class FireInfo
{
    public ConstWeaponInfo weaponData;
    public CharacterBase fireInstigator;
    public EAttackObjectType attackObjectType;

    public FireInfo(ConstWeaponInfo weaponData, CharacterBase fireInstigator,
        EAttackObjectType attackObjectType)
    {
        this.weaponData = weaponData;
        this.fireInstigator = fireInstigator;
        this.attackObjectType = attackObjectType;
    }
}



public class Weapon : MonoBehaviour, IAttackable
{
    protected CharacterBase owner;
    public CharacterBase Owner{get { return owner; }}

    protected ConstWeaponInfo weaponInfo;
    public ConstWeaponInfo WeaponInfo{get{ return weaponInfo;}}

    protected EWeaponState weaponState;
    public EWeaponState WeaponState { get { return weaponState; }}

    [SerializeField] private Animator weaponAnimator;
    // 전투에 사용되지는 않고, 땅에 떨어지는 등의 경우
    private Collider2D weaponBaseCollilder;

    [SerializeField] private List<AttackArea> attackAreas = new List<AttackArea>();
    
    [SerializeField] protected Transform leftHandSocket;
    [SerializeField] protected Transform rightHandSocket;

    [SerializeField] private GameObject spritePrefab;
    private List<GameObject> sprites = new List<GameObject>();
    public List<GameObject> Sprites{get{return sprites;}}

    private bool activated = false;
    
    private Vector2 initialLocalPosition;
    private Quaternion initialLocalRotation;

    private Vector2 initialSpriteLocalPosition;
    private Quaternion initialSpriteLocalRotation;

    private Coroutine hideWeaponCoroutine;

    private List<string> animStateNames = new List<string>() {"Weapon_Stored","Weapon_Idle","Weapon_Action" };
    
    [HideInInspector] public UnityAction<AttackArea> OnAreaActivated;
    [HideInInspector] public UnityAction<AttackRay> OnRayActivated;
    
    public virtual void InitializeWeapon(ConstWeaponInfo weaponInfo)
    {
        this.weaponInfo = weaponInfo;
        
        initialLocalPosition = transform.localPosition;
        initialLocalRotation = transform.localRotation;
        
        string[] colliderTokens = weaponInfo.Collider.Split('=');
        EColliderType colliderType = Enum.Parse<EColliderType>(colliderTokens[0]);
        string[] pointTokens = colliderTokens[1].Split('/');
        switch (colliderType)
        {
            case EColliderType.Box:
            {
                BoxCollider2D boxCollider2D = gameObject.AddComponent<BoxCollider2D>();
                string[] offsetToken = pointTokens[0].Split('_');
                boxCollider2D.offset = new Vector2(float.Parse(offsetToken[0]), float.Parse(offsetToken[1]));
                string[] sizeToken = pointTokens[1].Split('_');
                boxCollider2D.size = new Vector2(float.Parse(sizeToken[0]), float.Parse(sizeToken[1]));
                weaponBaseCollilder = boxCollider2D;
                break;
            }
            case EColliderType.Circle:
            {
                CircleCollider2D circleCollider2D = gameObject.AddComponent<CircleCollider2D>();
                string[] offsetToken = pointTokens[0].Split('_');
                circleCollider2D.offset = new Vector2(float.Parse(offsetToken[0]), float.Parse(offsetToken[1]));
                circleCollider2D.radius = float.Parse(pointTokens[1]);
                weaponBaseCollilder = circleCollider2D;
                break;
            }
            case EColliderType.Polygon:
            {
                PolygonCollider2D polygonCollider2D = gameObject.AddComponent<PolygonCollider2D>();
                string[] offsetToken = pointTokens[0].Split('_');
                polygonCollider2D.offset = new Vector2(float.Parse(offsetToken[0]), float.Parse(offsetToken[1]));
                List<Vector2> points = new List<Vector2>();
                for (int i = 1; i < pointTokens.Length; i++)
                {
                    string[] pointToken = pointTokens[i].Split('_');
                    points.Add(new Vector2(float.Parse(pointToken[0]), float.Parse(pointToken[1])));
                }
                polygonCollider2D.points = points.ToArray();
                weaponBaseCollilder = polygonCollider2D;
                break;
            }
        }
        initialSpriteLocalPosition = weaponBaseCollilder.transform.localPosition;
        initialSpriteLocalRotation = weaponBaseCollilder.transform.localRotation;
        weaponBaseCollilder.enabled = false;

        for (int i = 0; i < sprites.Count; i++)
        {
            Destroy(sprites[i].gameObject);
        }
        sprites.Clear();
        for (int i = 0; i < weaponInfo.Sprites.Count; i++)
        {
            sprites.Add(Instantiate(spritePrefab,transform));
            SpriteRenderer spriteRenderer = sprites[i].GetComponentInChildren<SpriteRenderer>();
            if(spriteRenderer)
            {
                spriteRenderer.sprite = weaponInfo.Sprites[i];
            }
        }
        
        ResetWeapon();
    }

    public virtual void Equip(CharacterBase character)
    {
        ResetOwner();
        owner = character;
        if (owner)
        {
            owner.StateChanged += HandleStateChanged;
            if (owner.TryGetComponent(out Character ownerCharacter))
            {
                ownerCharacter.BookmarkChanged += HandleBookmarkChanged;
                if (ownerCharacter.Bookmarks.ContainsKey(EBookmarkType.Main))
                {
                    HandleBookmarkChanged(EBookmarkType.Main, ownerCharacter.Bookmarks[EBookmarkType.Main]);
                }
            }
            HandleStateChanged(owner.CurrentCharacterState);
        }

        weaponBaseCollilder.enabled = false;
        ResetWeapon();
    }
    
    public void ResetWeapon()
    {
        transform.SetParent(owner != null ? owner.CenterSocket : null);
        weaponBaseCollilder.enabled = true;
        weaponAnimator.enabled = false;
        transform.localPosition = initialLocalPosition;
        transform.localRotation = initialLocalRotation;
        weaponBaseCollilder.transform.localPosition = initialSpriteLocalPosition;
        weaponBaseCollilder.transform.localRotation = initialSpriteLocalRotation;
        Destroy(weaponBaseCollilder.gameObject.GetComponent<Rigidbody2D>());
        weaponState = EWeaponState.Idle;

        ShowWeapon();
    }

    private void ResetOwner()
    {
        if (owner != null)
        {
            owner.StateChanged -= HandleStateChanged;
            if (owner.TryGetComponent(out Character ownerCharacter))
            {
                ownerCharacter.BookmarkChanged -= HandleBookmarkChanged;
            }
        }
        owner = null;
    }

    public void ShowWeapon()
    {
        if (hideWeaponCoroutine != null)
        {
            StopCoroutine(hideWeaponCoroutine);
        }
        gameObject.SetActive(true);
    }

    public void HideWeapon(float duration = -1.0f)
    {
        if (hideWeaponCoroutine != null)
        {
            StopCoroutine(hideWeaponCoroutine);
        }
        gameObject.SetActive(false);
        if (duration > 0.0f)
        {
            hideWeaponCoroutine = StartCoroutine(HideWeaponCoroutine(duration));
        }
    }

    
    IEnumerator HideWeaponCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        gameObject.SetActive(true);
    }

    protected virtual  void Update()
    {
        // UpdateRotation();
    }

    protected void UpdateRotation()
    {
        Vector2 direction = owner.ActionComponent.GetDirection();
        switch (weaponState)
        {
            case EWeaponState.Attack:
                break;
            case EWeaponState.Idle:
                float angle = Vector2.Angle(Vector2.right, direction);
                Vector3 cross = Vector3.Cross(Vector2.right, direction);
                if (owner.transform.localScale.x < 0)
                {
                    angle = Vector2.Angle(Vector2.left, direction);
                    cross = Vector3.Cross(Vector2.left, direction);
                }
                if (cross.z < 0) angle = -angle;
                transform.rotation = Quaternion.Euler(0, 0, angle);
                break;
        }

        foreach (var sprite in sprites)
        {
            if (sprite.TryGetComponent(out LayeredSpriteRenderer spriteRenderer))
            {
                spriteRenderer.SetFlipY(Mathf.Abs(transform.rotation.eulerAngles.z - 180) < 90f);
            }
        }
    }


    // Attack Input Pressed
    public virtual void StartAttack(ActionBase action, AttackRequsetBundle attackRequsetBundle)
    {
        weaponState = EWeaponState.Attack;
        owner.TryPlayAnimBool("Reload", false);

        FireInfo currentFireInfo = new FireInfo(WeaponInfo, owner, weaponInfo.AttackObjectType);

        Vector2 startPos = owner.ActionComponent.GetStartPos(attackRequsetBundle.StartPositionType);
        Vector2 targetPos = owner.ActionComponent.GetTargetPos(attackRequsetBundle.TargetPositionType);
        Vector2 dir = targetPos - startPos;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        
        for (int i = 0; i < attackRequsetBundle.AreaInfos.Count; i++)
        {
            ConstActionAreaInfo actionAreaInfo = attackRequsetBundle.AreaInfos[i];
            
            float angleDeltaSize = actionAreaInfo.AngleDeltaSize;
            Transform parentTransform = null;
            if (actionAreaInfo.ActionSpaceType == EActionSpaceType.Local)
            {
                parentTransform = transform;
            }
            
            switch (actionAreaInfo.ActionAreaType)
            {
                case EActionAreaType.Ray:
                {
                    GameObject pooledAttackRay = ObjectPoolManager.GetObject("AttackRay", parentTransform);
                    pooledAttackRay.transform.position = startPos;
                    AttackRay currAttackRay = pooledAttackRay.GetComponent<AttackRay>();
                    currAttackRay.SetAttackable(this, action, attackRequsetBundle, attackRequsetBundle.AreaInfos[i],
                        startPos, targetPos, angleDeltaSize);
                    if (OnRayActivated != null)
                    {
                        OnRayActivated.Invoke(currAttackRay);
                    }
                    break;
                }
                case EActionAreaType.Area:
                {
                    GameObject pooledAttackArea = ObjectPoolManager.GetObject("AttackArea", parentTransform);
                    pooledAttackArea.transform.position = startPos;

                    float randomDeltaAngle = Random.Range(-angleDeltaSize, angleDeltaSize);
                    pooledAttackArea.transform.rotation = Quaternion.AngleAxis(angle + randomDeltaAngle, Vector3.forward);
            
                    AttackArea currAttackArea = pooledAttackArea.GetComponent<AttackArea>();
                    currAttackArea.SetAttackable(this, action, attackRequsetBundle, attackRequsetBundle.AreaInfos[i]);
                    if (OnAreaActivated != null)
                    {
                        OnAreaActivated.Invoke(currAttackArea);
                    }
                    break;
                }
            }
        }
    }

    // Attack Input Released
    public virtual void CancelAttack()
    {
        if(weaponState == EWeaponState.Attack)
        {
            weaponState = EWeaponState.Idle;
        }
    }

    public virtual bool Fire(string projectileId, float angleDeltaSize, float attackRange, int numOfPellets, AttackRequsetBundle attackRequsetBundle)
    {
        Vector2 dir = owner.ActionComponent.GetDirection(attackRequsetBundle.StartPositionType, attackRequsetBundle.TargetPositionType);
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        angle += Random.Range(-angleDeltaSize, angleDeltaSize);
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        if (numOfPellets <= 1)
        {
            CrateProjectie(projectileId, dir, attackRequsetBundle);
        }
        else
        {
            float targetDist = dir.magnitude == 0 ? 1.0f : dir.magnitude;

            for (int i = 0; i < numOfPellets; i++)
            {
                float circleRadius = 1.0f;
                Vector2 startPos = owner.ActionComponent.GetStartPos(attackRequsetBundle.StartPositionType);
                Vector2 randVec = new Vector2(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f)) * circleRadius;
                Vector2 endPos = startPos + (Vector2)(transform.right * attackRange) + randVec;
                CrateProjectie(projectileId, (endPos - startPos) / targetDist, attackRequsetBundle);
            }
        }

        return true;
    }

    protected void CrateProjectie(string projectileId, Vector2 dir, AttackRequsetBundle attackRequsetBundle)
    {
        /*
        GameObject pooledBullet = ObjectPoolManager.GetObject("Bullet");
        pooledBullet.transform.position = owner.ActionComponent.GetStartPos(attackRequsetBundle.StartPositionType);
        ProjectileBase currProjectileBase = pooledBullet.GetComponent<ProjectileBase>();
        currProjectileBase.InitializeProjectile(projectileId, gameObject, owner, dir, owner.team, Color.white, attackRequsetBundle);
        */
    }

    public virtual bool TryCheckAttackObstacle(Vector2 InDirToAttackableTarget, float InMagnitude)
    {
        return Physics2D.Raycast(transform.position, InDirToAttackableTarget, InMagnitude);
    }

    private void HandleCharacterSimpleDead()
    {
        CancelAttack();
    }

    private void HandleCharacterDead(AttackInfo attackInfo)
    {
        DropWeapon(attackInfo.force);
    }
    
    private void HandleCharacterAttackSucceeded(AttackInfo attackInfo)
    {
    }
    
    private void HandleStateChanged(ECharacterState actionState)
    {
        // 활성화되어있지 않으면 캐릭터 상태가 바뀌었을 때 상태와 상관없이 Stored되어야 한다.
        if (activated == false)
        {
            SetAttachState(EWeaponAttachState.Stored);
            return;
        }
        // character의 Action에 따라 weapon의 위치를 달리한다.
        switch (actionState)
        {
            case ECharacterState.Action:
                SetAttachState(EWeaponAttachState.Action);
                break;
            case ECharacterState.Idle: 
            default:
                SetAttachState(EWeaponAttachState.Idle);
                break;
        }
    }

    private void HandleCombatModeChanged(bool combatMode)
    {
        // CombatMode가 아니라면 다 Stored상태로 변경
        if (!combatMode)
        {
            SetAttachState(EWeaponAttachState.Stored);
        }
        else if(activated)
        {
            SetAttachState(EWeaponAttachState.Idle);
        }
    }

    private void HandleBookmarkChanged(EBookmarkType bookmarkType, string bookId)
    {
        if (bookmarkType != EBookmarkType.Main)
        {
            return;
        } 

        DataManager dataManager = DataManager.Instance;
        activated = false;
        if (dataManager.bookDict.TryGetValue(bookId, out ConstBookInfo bookInfo))
        {
            if (dataManager.weaponDict.TryGetValue(bookInfo.Tool, out ConstWeaponInfo constWeaponInfo))
            {
                activated = constWeaponInfo.ID == weaponInfo.ID;
            }
        }
        if (activated)
        {
            SetAttachState(EWeaponAttachState.Idle);
        }
        else
        {
            SetAttachState(EWeaponAttachState.Stored);
        }
    }

    private void SetAttachState(EWeaponAttachState attachState)
    {
        SetAnimation(attachState);
        
        AttachToSocket(attachState);
    }

    private void SetAnimation(EWeaponAttachState attachState)
    {
        if ((int)attachState >= weaponInfo.AnimStrings.Count)
        {
            return;
        }
        
        string animString = weaponInfo.AnimStrings[(int)attachState];
        string[] animStrings = animString.Split('/');
        for (int i = 0; i < sprites.Count; i++)
        {
            if (sprites[i].TryGetComponent(out AnimationController animationController))
            {
                string[] animationClipTokens = animStrings[i].Split('=');
                if (DataManager.Instance.animationClipDict.TryGetValue(animationClipTokens[0],
                        out ConstAnimationClipInfo animationClipInfo))
                {
                    AnimationClip animationClip = animationClipInfo.AnimationClip;
                    animationController.PlayAnimationOverride(animStateNames[i], animationClip);
                }
            }
        }
    }
    
    /// <summary>
    /// Weapon은 실제로 캐릭터의 중앙에 존재한다. 여기서 Socket에 부착하는 것은 WeaponSprite이다.
    /// </summary>
    public void AttachToSocket(EWeaponAttachState attachState)
    {
        if(weaponInfo.SocketChildTransformInfos.TryGetValue(attachState, out List<SocketChildTransformInfo> infos))
        {
            if (infos == null)
            {
                return;
            }
            for (int i = 0; i < sprites.Count; i++)
            {
                if (infos.Count <= i)
                {
                    return;
                }
                owner.CharacterSocketComponent.AttachToSocket(sprites[i].transform, infos[i]);
            }
        }
    }

    public void DropWeapon(Vector2 dropForceVector)
    {
        transform.SetParent(null);
        weaponBaseCollilder.enabled = true;
        weaponAnimator.enabled = false;
        Rigidbody2D rb2D = weaponBaseCollilder.gameObject.AddComponent<Rigidbody2D>();

        dropForceVector += new Vector2(Random.Range(-10,10.0f), 100.0f);
        
        rb2D.AddRelativeForce(dropForceVector.normalized * 10);
        rb2D.angularVelocity = dropForceVector.magnitude * 10;
        weaponState = EWeaponState.Dropped;

        for (int i = 0; i < sprites.Count; i++)
        {
            sprites[i].transform.SetParent(transform);
            sprites[i].transform.localPosition = Vector3.zero;
            sprites[i].transform.localRotation = Quaternion.identity;
            if (sprites[i].TryGetComponent(out Animator animator))
            {
                animator.enabled = false;
                SpriteRenderer spriteRenderer = sprites[i].GetComponentInChildren<SpriteRenderer>();
                spriteRenderer.transform.localPosition = Vector3.zero;
                spriteRenderer.transform.localRotation = Quaternion.identity;
            }
        }
        
        ResetOwner();
    }

    public virtual AttackInfo GetAttackInfo(ActionBase actionBase, IHittable hittable, AttackRequsetBundle attackRequsetBundle)
    {
        Vector3 knockbackForce = attackRequsetBundle.GetKnockbackForceVector(hittable, owner.transform.position, transform.position);
        return new AttackInfo(new ActionInfo(actionBase), this, owner, hittable, attackRequsetBundle.TargetFilterType, EAttackObjectType.None, 
            attackRequsetBundle.BaseDamage, knockbackForce,
            attackRequsetBundle.IsStrike, attackRequsetBundle.IsStun, attackRequsetBundle.IsGuardBreak, attackRequsetBundle.IsBound, attackRequsetBundle.StaggerTime);
    }

    public GameObject GetGameObject()
    {
        return this.gameObject;
    }

    public string GetFireEffectGroup()
    {
        return null;
    }

    public string GetHitEffectGroup()
    {
        return null;
    }

    public Transform GetLeftHandSocket()
    {
        return leftHandSocket;
    }
    public Transform GetRightHandSocket()
    {
        return rightHandSocket;
    }
}
