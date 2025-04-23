using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using System.IO;
using System.Text;
using System;
using UnityEngine.UIElements;

public class CharacterCustomize : MonoBehaviour
{
    #region Variables & Initializer
    [SerializeField] private LayeredSpriteRenderer HairSR;
    [SerializeField] private LayeredSpriteRenderer BackHairSR;
    [SerializeField] private LayeredSpriteRenderer LeftBackHairSR;
    [SerializeField] private LayeredSpriteRenderer RightBackHairSR;
    [SerializeField] private LayeredSpriteRenderer HeadSR;
    [SerializeField] private LayeredSpriteRenderer FaceSR;
    [SerializeField] private LayeredSpriteRenderer HatSR;
    [SerializeField] private LayeredSpriteRenderer BackHatSR;
    [SerializeField] private LayeredSpriteRenderer AccessorySR;

    [SerializeField] private LayeredSpriteRenderer RobeSR;
    [SerializeField] private LayeredSpriteRenderer RobeLeftArmSR;
    [SerializeField] private LayeredSpriteRenderer RobeRightArmSR;
    [SerializeField] private LayeredSpriteRenderer UpperBodySR;
    [SerializeField] private LayeredSpriteRenderer LeftArmSR;
    [SerializeField] private LayeredSpriteRenderer LeftHandSR;
    [SerializeField] private LayeredSpriteRenderer LeftHandStuffSR;
    [SerializeField] private LayeredSpriteRenderer RightArmSR;
    [SerializeField] private LayeredSpriteRenderer RightHandSR;
    [SerializeField] private LayeredSpriteRenderer RightHandStuffSR;
    [SerializeField] private LayeredSpriteRenderer LowerBodySR;
    [SerializeField] private LayeredSpriteRenderer LeftLegSR;
    [SerializeField] private LayeredSpriteRenderer LeftFootSR;
    [SerializeField] private LayeredSpriteRenderer RightLegSR;
    [SerializeField] private LayeredSpriteRenderer RightFootSR;
    [SerializeField] private LayeredSpriteRenderer BackStuffSR;


    private SerializableDictionary<EEquipmentType, string> equipments = new SerializableDictionary<EEquipmentType, string>();

    private Sprite myFaceSprite;
    public List<Color> artifactColors = new List<Color>();

    private Coroutine myEmotionCoroutine;

    [SerializeField] private AudioSource fooStepAudioSource;
    [SerializeField] private AudioClip[] footStepAudioClips;
    [SerializeField] private SerializableDictionary<string, Sprite> emotionSprites;

    
    // MaterialRequest 클래스 정의
    public class MaterialRequest
    {
        public Material material;
        public int priority;
        public float duration;

        public MaterialRequest(Material material, int priority, float duration)
        {
            this.material = material;
            this.priority = priority;
            this.duration = duration;
        }
    }
    private Coroutine materialCoroutine;
    private Dictionary<string, MaterialRequest> requestedMaterials = new Dictionary<string, MaterialRequest>();

    
    private SortingGroup sortingGroup;
    public SortingGroup SortingGroup
    {
        get
        {
            if(sortingGroup == null)
            {
                sortingGroup = GetComponentInChildren<SortingGroup>();
            }
            return sortingGroup;
        }
    }
    
    #endregion

    private void Awake()
    {
        for(EEquipmentType type = EEquipmentType.Hair; type < EEquipmentType.MAX; type++)
        {
            equipments.Add(type, "");
        }
    }

    private void Update()
    {
        UpdateRequestedMaterials();
    }

    private void UpdateRequestedMaterials()
    {
        // Duration은 매터리얼이 적용되지 않더라도 duration이 진행되어야한다.
        List<string> invalidRequests = new List<string>();
        foreach (var requestedMaterial in requestedMaterials)
        {
            requestedMaterial.Value.duration -= Time.deltaTime;
            if(requestedMaterial.Value.duration <= 0)
            {
                invalidRequests.Add(requestedMaterial.Key);
            }
        }
        for (int i = 0; i < invalidRequests.Count; i++)
        {
            invalidRequests.Remove(invalidRequests[i]);
        }
    }
    
    public void ChangeLayer(string name)
    {
        ChangeLayersRecursively(transform, name);
    }

    private void ChangeLayersRecursively(Transform transform, string name)
    {
        transform.gameObject.layer = LayerMask.NameToLayer(name);
        foreach (Transform child in transform)
        {
            ChangeLayersRecursively(child, name);
        }
    }

    public void GetSpriteRendererByItemType(EEquipmentType equipmentType, out List<LayeredSpriteRenderer> outSRs)
    {
        outSRs = new List<LayeredSpriteRenderer>();
        switch (equipmentType)
        {
            case EEquipmentType.Hair: outSRs.Add(HairSR); break;
            case EEquipmentType.BackHair: outSRs.Add(BackHairSR);  outSRs.Add(LeftBackHairSR); outSRs.Add(RightBackHairSR);break;
            case EEquipmentType.Head: outSRs.Add(HeadSR); break;
            case EEquipmentType.Face: outSRs.Add(FaceSR); break;
            case EEquipmentType.Hat: outSRs.Add(HatSR); outSRs.Add(BackHatSR); break;
            case EEquipmentType.Accessory: outSRs.Add(AccessorySR); break;
            case EEquipmentType.Robe: outSRs.Add(RobeSR); outSRs.Add(RobeLeftArmSR); outSRs.Add(RobeRightArmSR); break;
            case EEquipmentType.UpperBody: outSRs.Add(UpperBodySR); outSRs.Add(LeftArmSR); outSRs.Add(RightArmSR); outSRs.Add(LeftHandSR); outSRs.Add(RightHandSR); break;
            case EEquipmentType.LowerBody: outSRs.Add(LowerBodySR); outSRs.Add(LeftLegSR); outSRs.Add(RightLegSR); outSRs.Add(LeftFootSR); outSRs.Add(RightFootSR); break;
            case EEquipmentType.BackStuff: outSRs.Add(BackStuffSR); break;
            case EEquipmentType.HandStuff: outSRs.Add(LeftHandStuffSR); outSRs.Add(RightHandStuffSR); break;
        }
    }

    public void ToggleSpriteRenderer(EEquipmentType equipmentType)
    {
        List<LayeredSpriteRenderer> foundedSRs = new List<LayeredSpriteRenderer>();
        GetSpriteRendererByItemType(equipmentType, out foundedSRs);
        foreach (var SR in foundedSRs)
        {
            SR.SetEnabled(!SR.GetEnabled());
        }
    }

    public void ShowSpriteRenderer(EEquipmentType equipmentType, bool bActivate)
    {
        List<LayeredSpriteRenderer> foundedSRs = new List<LayeredSpriteRenderer>();
        GetSpriteRendererByItemType(equipmentType, out foundedSRs);
        foreach(var SR in foundedSRs)
        {
            SR.SetEnabled(bActivate);
        }
    }


    public void EnableOutline(bool bUseOutline)
    {
        HairSR.EnableOutline(bUseOutline);
        BackHairSR.EnableOutline(bUseOutline);
        LeftBackHairSR.EnableOutline(bUseOutline);
        RightBackHairSR.EnableOutline(bUseOutline);
        HeadSR.EnableOutline(bUseOutline);
        FaceSR.EnableOutline(bUseOutline);
        HatSR.EnableOutline(bUseOutline);
        BackHatSR.EnableOutline(bUseOutline);
        AccessorySR.EnableOutline(bUseOutline);

        RobeSR.EnableOutline(bUseOutline);
        RobeLeftArmSR.EnableOutline(bUseOutline);
        RobeRightArmSR.EnableOutline(bUseOutline);
        UpperBodySR.EnableOutline(bUseOutline);
        LeftArmSR.EnableOutline(bUseOutline);
        LeftHandSR.EnableOutline(bUseOutline);
        LeftHandStuffSR.EnableOutline(bUseOutline);
        RightArmSR.EnableOutline(bUseOutline);
        RightHandSR.EnableOutline(bUseOutline);
        RightHandStuffSR.EnableOutline(bUseOutline);
        LowerBodySR.EnableOutline(bUseOutline);
        LeftLegSR.EnableOutline(bUseOutline);
        LeftFootSR.EnableOutline(bUseOutline);
        RightLegSR.EnableOutline(bUseOutline);
        RightFootSR.EnableOutline(bUseOutline);
        BackStuffSR.EnableOutline(bUseOutline);
    }

    public void SetOutlineColor(Color color)
    {
        HairSR.SetOutlineColor(color);
        BackHairSR.SetOutlineColor(color);
        LeftBackHairSR.SetOutlineColor(color);
        RightBackHairSR.SetOutlineColor(color);
        HeadSR.SetOutlineColor(color);
        FaceSR.SetOutlineColor(color);
        HatSR.SetOutlineColor(color);
        BackHatSR.SetOutlineColor(color);
        AccessorySR.SetOutlineColor(color);

        RobeSR.SetOutlineColor(color);
        RobeLeftArmSR.SetOutlineColor(color);
        RobeRightArmSR.SetOutlineColor(color);
        UpperBodySR.SetOutlineColor(color);
        LeftArmSR.SetOutlineColor(color);
        LeftHandSR.SetOutlineColor(color);
        LeftHandStuffSR.SetOutlineColor(color);
        RightArmSR.SetOutlineColor(color);
        RightHandSR.SetOutlineColor(color);
        RightHandStuffSR.SetOutlineColor(color);
        LowerBodySR.SetOutlineColor(color);
        LeftLegSR.SetOutlineColor(color);
        LeftFootSR.SetOutlineColor(color);
        RightLegSR.SetOutlineColor(color);
        RightFootSR.SetOutlineColor(color);
        BackStuffSR.SetOutlineColor(color);
    }

    public void SetMaterial(Material material)
    {
        HairSR.SetMainMaterial(material);
        BackHairSR.SetMainMaterial(material);
        LeftBackHairSR.SetMainMaterial(material);
        RightBackHairSR.SetMainMaterial(material);
        HeadSR.SetMainMaterial(material);
        FaceSR.SetMainMaterial(material);
        HatSR.SetMainMaterial(material);
        BackHatSR.SetMainMaterial(material);
        AccessorySR.SetMainMaterial(material);

        RobeSR.SetMainMaterial(material);
        RobeLeftArmSR.SetMainMaterial(material);
        RobeRightArmSR.SetMainMaterial(material);
        UpperBodySR.SetMainMaterial(material);
        LeftArmSR.SetMainMaterial(material);
        LeftHandSR.SetMainMaterial(material);
        LeftHandStuffSR.SetMainMaterial(material);
        RightArmSR.SetMainMaterial(material);
        RightHandSR.SetMainMaterial(material);
        RightHandStuffSR.SetMainMaterial(material);
        LowerBodySR.SetMainMaterial(material);
        LeftLegSR.SetMainMaterial(material);
        LeftFootSR.SetMainMaterial(material);
        RightLegSR.SetMainMaterial(material);
        RightFootSR.SetMainMaterial(material);
        BackStuffSR.SetMainMaterial(material);
    }

    public void RequestMaterial(string reason, Material material, int priority, float duration)
    {
        if (requestedMaterials.TryGetValue(reason, out MaterialRequest requset))
        {
            if (requset.priority <= priority)
            {
                requset.material = material;
                requset.duration = duration;
            }
        }
        else
        {
            MaterialRequest newRequest = new MaterialRequest(material, priority, duration);
            requestedMaterials.Add(reason, newRequest);
        }
        ProcessMaterialRequest();
    }
    
    /// <summary>
    /// 지금 가장 적절한 material을 적용
    /// </summary>
    public void ProcessMaterialRequest()
    {
        if (materialCoroutine != null)
        {
            StopCoroutine(materialCoroutine);
        }
        if (requestedMaterials.Count > 0)
        {
            int maxPriority = -1;
            string pickedRequest = null;
            foreach (var requestedMaterial in requestedMaterials)
            {
                if (requestedMaterial.Value.priority >= maxPriority)
                {
                    pickedRequest = requestedMaterial.Key;
                    maxPriority = requestedMaterial.Value.priority;
                }
            }
            if (pickedRequest != null)
            {
                MaterialRequest pickedMaterial = requestedMaterials[pickedRequest];
                requestedMaterials.Remove(pickedRequest);
                float duration = pickedMaterial.duration;
                if (duration > Mathf.Epsilon)
                {
                    materialCoroutine = StartCoroutine(ApplyMaterial(pickedMaterial));
                }
                else
                {
                    SetMaterial(pickedMaterial.material);
                }
                return;
            }
        }
        SetMaterial(null);
    }
    
    private IEnumerator ApplyMaterial(MaterialRequest request)
    {
        SetMaterial(request.material);
        yield return new WaitForSeconds(request.duration); // 지정된 시간 대기

        SetMaterial(null);
        materialCoroutine = null;
        ProcessMaterialRequest(); // 다음 요청 처리
    }

    #region Set Sprite
    public void SetSprite(ConstCharacterInfo characterData)
    {
        if (string.IsNullOrEmpty(characterData.equipmentPresetId) == false)
        {
            characterData.SetupDefaultEquipmentIds();
        }
        if(characterData.defaultEquipmentIds == null)
        {
            return;
        }
        for (EEquipmentType type = EEquipmentType.Hair; type < EEquipmentType.MAX; type++)
        {
            if (characterData.defaultEquipmentIds.TryGetValue(type, out string value))
            {
                equipments[type] = value;
            }
            else
            {
                equipments[type] = "";
            }
            SetSprite(type, equipments[type]);
        }
        for (EEquipmentType type = EEquipmentType.Hair; type < EEquipmentType.MAX; type++)
        {
            if (characterData.defaultEquipmentIds.TryGetValue(type, out string value))
            {
                equipments[type] = value;
            }
            else
            {
                equipments[type] = "";
            }
            SetSprite(type, equipments[type]);
        }
    }
    public void SetSprite(ConstEquipmentPresetInfo equipmentPresetInfo)
    {
        SetSprite(EEquipmentType.Hair, equipmentPresetInfo.Hair);
        SetSprite(EEquipmentType.BackHair, equipmentPresetInfo.BackHair);
        SetSprite(EEquipmentType.Head, equipmentPresetInfo.Head);
        SetSprite(EEquipmentType.Face, equipmentPresetInfo.Face);
        SetSprite(EEquipmentType.Hat, equipmentPresetInfo.Hat);
        SetSprite(EEquipmentType.Robe, equipmentPresetInfo.Robe);
        SetSprite(EEquipmentType.UpperBody, equipmentPresetInfo.UpperBody);
        SetSprite(EEquipmentType.LowerBody, equipmentPresetInfo.LowerBody);
        SetSprite(EEquipmentType.BackStuff, equipmentPresetInfo.BackStuff);
        SetSprite(EEquipmentType.HandStuff, equipmentPresetInfo.HandStuff);
    }
    public string GetEquipmentId(EEquipmentType equipmentType)
    {
        return equipments[equipmentType];
    }

    public void SetSprite(EEquipmentType type, string equipmentId)
    {
        DataManager dataManager = DataManager.Instance;
        ConstEquipmentInfo equipmentInfo = null;
        bool isNillOrEmpty = dataManager.equipmentDict.ContainsKey(equipmentId) == false;
        if (isNillOrEmpty == false)
        {
            equipmentInfo = dataManager.equipmentDict[equipmentId];
        }
        
        Color artifactColor = Color.white;
        artifactColors[(int)type] = artifactColor;
        equipments[type] = equipmentId;

        switch (type)
        {
            case EEquipmentType.BackHair:
                LeftBackHairSR.SetSprite(null);
                RightBackHairSR.SetSprite(null);
                if (isNillOrEmpty)
                {
                    BackHairSR.SetSprite(null);
                }
                else
                {
                    BackHairSR.SetSprite(equipmentInfo.sprites[0]);
                    if (equipmentInfo.sprites.Count > 1)
                    {
                        LeftBackHairSR.SetSprite(equipmentInfo.sprites[1]);
                    }
                    if (equipmentInfo.sprites.Count > 2)
                    {
                        RightBackHairSR.SetSprite(equipmentInfo.sprites[2]);
                    }
                }
                Color tmpColor = HairSR.GetColor();
                tmpColor.r -= 0.1f;
                tmpColor.g -= 0.1f;
                tmpColor.b -= 0.1f;
                BackHairSR.SetColor(tmpColor);
                LeftBackHairSR.SetColor(tmpColor);
                RightBackHairSR.SetColor(tmpColor);
                artifactColors[(int)EEquipmentType.BackHair] = artifactColors[(int)EEquipmentType.Hair] - new Color(0.1f, 0.1f, 0.1f, -100f);
                break;
            case EEquipmentType.Hair:
                if (isNillOrEmpty)
                    HairSR.SetSprite(null);
                else
                    HairSR.SetSprite(equipmentInfo.sprites[0]);
                HairSR.SetColor(artifactColor);
                break;
            case EEquipmentType.Head:
                if (isNillOrEmpty)
                    HeadSR.SetSprite(null);
                else
                    HeadSR.SetSprite(equipmentInfo.sprites[0]);
                HeadSR.SetColor(artifactColor);
                break;
            case EEquipmentType.Face:
                if (isNillOrEmpty)
                    FaceSR.SetSprite(null);
                else
                    FaceSR.SetSprite(equipmentInfo.sprites[0]);
                FaceSR.SetColor(Color.white);
                myFaceSprite = FaceSR.MainSpriteRenderer.sprite;
                break;
            case EEquipmentType.Hat:
                if (isNillOrEmpty)
                {
                    HatSR.SetSprite(null);
                    BackHatSR.SetSprite(null);
                }
                else
                {
                    HatSR.SetSprite(equipmentInfo.sprites[0]);
                    if (equipmentInfo.sprites.Count > 1)
                    {
                        BackHatSR.SetSprite(equipmentInfo.sprites[1]);
                    }
                }
                HatSR.SetColor(artifactColor);
                BackHatSR.SetColor(artifactColor);
                break;
            case EEquipmentType.Accessory:
                if (isNillOrEmpty)
                    AccessorySR.SetSprite(null);
                else
                    AccessorySR.SetSprite(equipmentInfo.sprites[0]);
                AccessorySR.SetColor(artifactColor);
                break;
            case EEquipmentType.Robe:
                if (isNillOrEmpty)
                {
                    RobeSR.SetSprite(null);
                    RobeRightArmSR.SetSprite(null);
                    RobeLeftArmSR.SetSprite(null);
                }
                else
                {
                    RobeSR.SetSprite(equipmentInfo.sprites[0]);
                    if (equipmentInfo.sprites.Count > 1)
                    {
                        RobeRightArmSR.SetSprite(equipmentInfo.sprites[1]);
                    }
                    if (equipmentInfo.sprites.Count > 2)
                    {
                        RobeLeftArmSR.SetSprite(equipmentInfo.sprites[2]);
                    }
                }
                RobeSR.SetColor(artifactColor);
                RobeRightArmSR.SetColor(artifactColor);
                RobeLeftArmSR.SetColor(artifactColor);
                break;
            case EEquipmentType.UpperBody:
                if (isNillOrEmpty)
                {
                    UpperBodySR.SetSprite(null);
                    RightArmSR.SetSprite(null);
                    RightHandSR.SetSprite(null);
                    LeftArmSR.SetSprite(null);
                    LeftHandSR.SetSprite(null);
                }
                else
                {
                    UpperBodySR.SetSprite(equipmentInfo.sprites[0]);
                    RightArmSR.SetSprite(equipmentInfo.sprites[1]);
                    RightHandSR.SetSprite(equipmentInfo.sprites[2]);
                    LeftArmSR.SetSprite(equipmentInfo.sprites[3]);
                    LeftHandSR.SetSprite(equipmentInfo.sprites[4]);
                }
                UpperBodySR.SetColor(artifactColor);
                RightArmSR.SetColor(artifactColor);
                RightHandSR.SetColor(artifactColor);
                LeftArmSR.SetColor(artifactColor);
                LeftHandSR.SetColor(artifactColor);
                break;
            case EEquipmentType.LowerBody:
                if (isNillOrEmpty)
                {
                    LowerBodySR.SetSprite(null);
                    RightLegSR.SetSprite(null);
                    RightFootSR.SetSprite(null);
                    LeftLegSR.SetSprite(null);
                    LeftFootSR.SetSprite(null);
                }
                else
                {
                    LowerBodySR.SetSprite(equipmentInfo.sprites[0]);
                    if (equipmentInfo.sprites.Count <= 3)
                    {
                        RightLegSR.SetSprite(equipmentInfo.sprites[1]);
                        LeftLegSR.SetSprite(equipmentInfo.sprites[2]);
                    }
                    else
                    {
                        RightLegSR.SetSprite(equipmentInfo.sprites[1]);
                        RightFootSR.SetSprite(equipmentInfo.sprites[2]);
                        LeftLegSR.SetSprite(equipmentInfo.sprites[3]);
                        LeftFootSR.SetSprite(equipmentInfo.sprites[4]);
                    }
                }
                LowerBodySR.SetColor(artifactColor);
                RightLegSR.SetColor(artifactColor);
                RightFootSR.SetColor(artifactColor);
                LeftLegSR.SetColor(artifactColor);
                LeftFootSR.SetColor(artifactColor);
                break;
            case EEquipmentType.BackStuff:
                if (isNillOrEmpty)
                    BackStuffSR.SetSprite(null);
                else
                    BackStuffSR.SetSprite(equipmentInfo.sprites[0]);
                BackStuffSR.SetColor(artifactColor);
                break;

            case EEquipmentType.HandStuff:
                if (isNillOrEmpty)
                {
                    LeftHandStuffSR.SetSprite(null);
                    RightHandStuffSR.SetSprite(null);
                }
                else
                {
                    LeftHandStuffSR.SetSprite(equipmentInfo.sprites[0]);
                    RightHandStuffSR.SetSprite(equipmentInfo.sprites[1]);
                }
                LeftHandStuffSR.SetColor(artifactColor);
                RightHandStuffSR.SetColor(artifactColor);
                break;
        }
    }
    #endregion

    #region Effects & Emotions
    public void ResetColor()
    {
        BackHairSR.SetColor(artifactColors[(int)EEquipmentType.BackHair]);
        LeftBackHairSR.SetColor(artifactColors[(int)EEquipmentType.BackHair]);
        RightBackHairSR.SetColor(artifactColors[(int)EEquipmentType.BackHair]);
        HairSR.SetColor(artifactColors[(int)EEquipmentType.Hair]);
        HeadSR.SetColor(artifactColors[(int)EEquipmentType.Head]);
        FaceSR.SetSprite(myFaceSprite);
        FaceSR.SetColor(artifactColors[(int)EEquipmentType.Face]);

        HatSR.SetColor(artifactColors[(int)EEquipmentType.Hat]);
        BackHatSR.SetColor(artifactColors[(int)EEquipmentType.Hat]);
        AccessorySR.SetColor(artifactColors[(int)EEquipmentType.Accessory]);

        RobeSR.SetColor(artifactColors[(int)EEquipmentType.Robe]);
        RobeLeftArmSR.SetColor(artifactColors[(int)EEquipmentType.Robe]);
        RobeRightArmSR.SetColor(artifactColors[(int)EEquipmentType.Robe]);

        UpperBodySR.SetColor(artifactColors[(int)EEquipmentType.UpperBody]);
        LeftArmSR.SetColor(artifactColors[(int)EEquipmentType.UpperBody]);
        LeftHandSR.SetColor(artifactColors[(int)EEquipmentType.UpperBody]);
        LeftHandStuffSR.SetColor(artifactColors[(int)EEquipmentType.HandStuff]);

        RightArmSR.SetColor(artifactColors[(int)EEquipmentType.UpperBody]);
        RightHandSR.SetColor(artifactColors[(int)EEquipmentType.UpperBody]);
        RightHandStuffSR.SetColor(artifactColors[(int)EEquipmentType.HandStuff]);

        LowerBodySR.SetColor(artifactColors[(int)EEquipmentType.LowerBody]);
        LeftLegSR.SetColor(artifactColors[(int)EEquipmentType.LowerBody]);
        LeftFootSR.SetColor(artifactColors[(int)EEquipmentType.LowerBody]);
        RightLegSR.SetColor(artifactColors[(int)EEquipmentType.LowerBody]);
        RightFootSR.SetColor(artifactColors[(int)EEquipmentType.LowerBody]);

        BackStuffSR.SetColor(artifactColors[(int)EEquipmentType.BackStuff]);
    }

    public void SetTemporalSpriteColor(Color targetColor,float speed)
    {
        BackHairSR.SetColor(Color.Lerp(BackHairSR.GetColor(),targetColor,speed*Time.deltaTime));
        LeftBackHairSR.SetColor(Color.Lerp(LeftBackHairSR.GetColor(),targetColor,speed*Time.deltaTime));
        RightBackHairSR.SetColor(Color.Lerp(RightBackHairSR.GetColor(),targetColor,speed*Time.deltaTime));
        HairSR.SetColor(Color.Lerp(HairSR.GetColor(), targetColor, speed * Time.deltaTime));
        HeadSR.SetColor(Color.Lerp(HeadSR.GetColor(), targetColor, speed * Time.deltaTime));
        FaceSR.SetColor(Color.Lerp(FaceSR.GetColor(), targetColor, speed * Time.deltaTime));

        HatSR.SetColor(Color.Lerp(HatSR.GetColor(), targetColor, speed * Time.deltaTime));
        BackHatSR.SetColor(Color.Lerp(BackHatSR.GetColor(), targetColor, speed * Time.deltaTime));
        AccessorySR.SetColor(Color.Lerp(AccessorySR.GetColor(), targetColor, speed * Time.deltaTime));

        RobeSR.SetColor(Color.Lerp(RobeSR.GetColor(), targetColor, speed * Time.deltaTime));
        RobeLeftArmSR.SetColor(Color.Lerp(RobeLeftArmSR.GetColor(), targetColor, speed * Time.deltaTime));
        RobeRightArmSR.SetColor(Color.Lerp(RobeRightArmSR.GetColor(), targetColor, speed * Time.deltaTime));

        UpperBodySR.SetColor(Color.Lerp(UpperBodySR.GetColor(), targetColor, speed * Time.deltaTime));
        LeftArmSR.SetColor(Color.Lerp(LeftArmSR.GetColor(), targetColor, speed * Time.deltaTime));
        LeftHandSR.SetColor(Color.Lerp(LeftHandSR.GetColor(), targetColor, speed * Time.deltaTime));
        LeftHandStuffSR.SetColor(Color.Lerp(LeftHandStuffSR.GetColor(), targetColor, speed * Time.deltaTime));
        RightArmSR.SetColor(Color.Lerp(RightArmSR.GetColor(), targetColor, speed * Time.deltaTime));
        RightHandSR.SetColor(Color.Lerp(RightHandSR.GetColor(), targetColor, speed * Time.deltaTime));
        RightHandStuffSR.SetColor(Color.Lerp(RightHandStuffSR.GetColor(), targetColor, speed * Time.deltaTime));

        LowerBodySR.SetColor(Color.Lerp(LowerBodySR.GetColor(), targetColor, speed * Time.deltaTime));
        LeftLegSR.SetColor(Color.Lerp(LeftLegSR.GetColor(), targetColor, speed * Time.deltaTime));
        LeftFootSR.SetColor(Color.Lerp(LeftFootSR.GetColor(), targetColor, speed * Time.deltaTime));
        RightLegSR.SetColor(Color.Lerp(RightLegSR.GetColor(), targetColor, speed * Time.deltaTime));
        RightFootSR.SetColor(Color.Lerp(RightFootSR.GetColor(), targetColor, speed * Time.deltaTime));

        BackStuffSR.SetColor(Color.Lerp(BackStuffSR.GetColor(), targetColor, speed * Time.deltaTime));
    }

    public void SetEmotionFace(string emotionId)
    {
        ForceEmotionFace_Inner(emotionId);

        myEmotionCoroutine = StartCoroutine(EmotionCoroutine());
    }
    public void ForceEmotionFace_Inner(string emotionId)
    {
        if (myEmotionCoroutine != null)
        {
            StopCoroutine(myEmotionCoroutine);
        }

        if (string.IsNullOrEmpty(emotionId) == false)
        {
            FaceSR.SetSprite(emotionSprites[emotionId]);
        }
        FaceSR.SetColor(Color.white);
        equipments[EEquipmentType.Face] = emotionId;
    }

    IEnumerator EmotionCoroutine()
    {
        //Sprite _curFace = myFaceSprite;
        yield return new WaitForSeconds(3f);
        SetSprite(EEquipmentType.Face, equipments[EEquipmentType.Face]);
    }
    public void PlayFootStep()
    {
        fooStepAudioSource.clip = footStepAudioClips[UnityEngine.Random.Range(0, footStepAudioClips.Length)];
        fooStepAudioSource.Play();
    }

    #endregion

#if UNITY_EDITOR
    public void SaveCurrentCustomize()
    {
        string filePath = Application.dataPath + "/DataTables/CharacterDataSet.csv";
        string delimiter = ",";

        StringBuilder sb = new StringBuilder();
        sb.Append(System.IO.File.ReadAllText(filePath));

        List<string> equipmentStrings = new List<string>();
        for (EEquipmentType type = EEquipmentType.Hair; type < EEquipmentType.MAX; type++)
        {
            equipmentStrings.Add(equipments[type]);
        }
        int length = equipmentStrings.Count;
        for (int index = 0; index < length; index++)
        {
            sb.AppendLine(string.Join(delimiter, equipmentStrings[index]));
        }

        StreamWriter outStream = System.IO.File.CreateText(filePath);
        outStream.WriteLine(sb);
        outStream.Close();
    }
#endif
}