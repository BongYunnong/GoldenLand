using UnityEditor;
using UnityEngine;
using Unity.Collections;
using System.Collections.Generic;
using UnityEngine.Events;



[System.Serializable]
public struct SpritePath
{
    public string path;
    public string subName;
    [ReadOnly] private Sprite sprite;

    public SpritePath(string path, string subName)
    {
        this.path = path;
        this.subName = subName;
        sprite = null;
        UpdateSpriteAsset();
    }
    public SpritePath(string[] args)
    {
        this.path = args[0];
        this.subName = "";
        if (args.Length > 1)
        {
            this.subName = args[1];
        }
        sprite = null;
        UpdateSpriteAsset();
    }
    private void UpdateSpriteAsset()
    {
        Sprite spriteAsset = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (spriteAsset != null)
        {
            Object[] sprites = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);
            if (sprites.Length > 1)
            {
                for (int i = 0; i < sprites.Length; i++)
                {
                    if (sprites[i].name == subName)
                    {
                        spriteAsset = sprites[i] as Sprite;
                        break;
                    }
                }
            }
        }
        sprite = spriteAsset;
    }

    public Sprite GetSprite()
    {
        if(sprite == null)
        {
            UpdateSpriteAsset();
        }
        return sprite;
    }
}


[System.Serializable]
public class AssetPath<T>  where T : Object
{
    public string path;
    public T value;

    public AssetPath(string path)
    {
        this.path = path;
        value = AssetDatabase.LoadAssetAtPath<T>(path);
    }

    public T GetValue()
    {
        return value;
    }
}

[System.Serializable]
public class StructInfoBase
{
    public string title;
    public Sprite icon;
    public string description;
}

/// <summary>
/// SO �������� ��� LocalizeVariableInfo�� object������ value �� ���� ����� Ȱ���ϱ� �����Ƿ� �� ����ü�� ����Ѵ�
/// </summary>
[System.Serializable]
public struct LocalizedStructInfoBase
{
    public string title;
    public Sprite icon;
    public string description { get { return title + "_Desc"; } }
    public string stringTableKey;

    public LocalizedStructInfoBase(
        string title,
        Sprite sprite,
        string stringTableKey = "")
    {
        this.title = title;
        icon = sprite;
        this.stringTableKey = stringTableKey;
    }
}

[System.Serializable]
public struct LocalizedStructInfo
{
    public string title;
    public Sprite icon;
    public string description { get { return title + "_Desc"; } }
    public string stringTableKey;
    public LocalizeVariableInfo[] localizeVariableInfos;

    public LocalizedStructInfo(
        string title,
        Sprite sprite,
        string stringTableKey = "", params LocalizeVariableInfo[] arguments)
    {
        this.title = title;
        icon = sprite;
        this.stringTableKey = stringTableKey;
        localizeVariableInfos = arguments;
    }
}




public class DataManager : PersistentSingletonMonoBehavior<DataManager>
{
    public Dictionary<string, ConstEquipmentPresetInfo> equipmentPresetDict = new Dictionary<string, ConstEquipmentPresetInfo>();
    public Dictionary<string, ConstEquipmentInfo> equipmentDict = new Dictionary<string, ConstEquipmentInfo>();
    public Dictionary<EEquipmentType, Dictionary<string, ConstEquipmentInfo>> equipmentTypeDict = new Dictionary<EEquipmentType, Dictionary<string, ConstEquipmentInfo>>();

    public Dictionary<string, ConstActionInfo> actionDict = new Dictionary<string, ConstActionInfo>();
    public Dictionary<string, ConstWeaponInfo> weaponDict = new Dictionary<string, ConstWeaponInfo>();
    public Dictionary<string, ConstActionInputInfo> actionInputDict = new Dictionary<string, ConstActionInputInfo>();
    public Dictionary<string, ConstActionModifierInfo> actionModifierDict = new Dictionary<string, ConstActionModifierInfo>();
    public Dictionary<string, ConstActionEffectInfo> actionEffectDict = new Dictionary<string, ConstActionEffectInfo>();
    public Dictionary<string, ConstActionAreaInfo> actionAreaDict = new Dictionary<string, ConstActionAreaInfo>();

    public Dictionary<string, ConstStatusEffectInfo> statusEffectDict = new Dictionary<string, ConstStatusEffectInfo>();
    
    public Dictionary<string, ConstBookInfo> bookDict = new Dictionary<string, ConstBookInfo>();

    public Dictionary<string, ConstAnimationClipInfo> animationClipDict = new Dictionary<string, ConstAnimationClipInfo>();
    public Dictionary<string, ConstEffectGroupInfo> effectGroupInfos = new Dictionary<string, ConstEffectGroupInfo>();
    public Dictionary<string, ConstSoundInfo> soundDict = new Dictionary<string, ConstSoundInfo>();
    public Dictionary<string, ConstVisualEffefctInfo> visualEffectDict = new Dictionary<string, ConstVisualEffefctInfo>();
    
    public Dictionary<string, LocalizedStructInfoBase> extraInfoDict = new Dictionary<string, LocalizedStructInfoBase>();
    public Dictionary<string, GameObject> extraGameObjectDict = new Dictionary<string, GameObject>();

    public UnityAction OnAllDataLoaded;
    public bool IsAllDataLoaded {  get; private set; }

    private void Awake()
    {
        SetupAllData();
    }

    public void SetupAllData()
    {
        IsAllDataLoaded = false;
        
        SetupEquipmentPresetDataSet();
        SetupEquipmentDataSet();

        SetupBookDataSet();
        SetupWeaponDataSet();
        
        SetupActionInputDataSet();
        SetupActionModifierDataSet();
        SetupActionEffectDataSet();
        SetupActionAreaDataSet();

        SetupStatusEffectDataSet();
        
        SetupAnimationClipDictDataSet();
        SetupSoundDataSet();
        SetupVisualEffectDataSet();
        
        SetupExtraInfoDataSet();
        SetupExtraGameObjectDataSet();

        OnAllDataLoaded?.Invoke();
        IsAllDataLoaded = true;
    }

    private void SetupEquipmentPresetDataSet()
    {
        equipmentPresetDict.Clear();
        
        foreach (var equipmentPresetData in EquipmentPresetDataSet.Instance.datas)
        {
            ConstEquipmentPresetInfo equipmentPresetInfo = new ConstEquipmentPresetInfo(equipmentPresetData);
            equipmentPresetDict.Add(equipmentPresetInfo.id, equipmentPresetInfo);
        }
    }
    
    private void SetupEquipmentDataSet()
    {
        equipmentDict.Clear();
        equipmentTypeDict.Clear();
        for (EEquipmentType type = EEquipmentType.Hair; type < EEquipmentType.MAX; type++)
        {
            equipmentTypeDict.Add(type, new Dictionary<string, ConstEquipmentInfo>());
        }
        
        foreach (var equipmentData in EquipmentDataSet.Instance.datas)
        {
            ConstEquipmentInfo equipmentInfo = new ConstEquipmentInfo(equipmentData);
            equipmentDict.Add(equipmentData.ID, equipmentInfo);
            equipmentTypeDict[equipmentData.EquipmentType].Add(equipmentData.ID, equipmentInfo);
        }
    }

    private void SetupBookDataSet()
    {
        bookDict.Clear();

        foreach (var bookData in BookDataSet.Instance.datas)
        {
            ConstBookInfo bookInfo = new ConstBookInfo(bookData);
            bookDict.Add(bookData.ID, bookInfo);
        }
    }
    private void SetupWeaponDataSet()
    {
        weaponDict.Clear();

        foreach (var weaponData in WeaponDataSet.Instance.datas)
        {
            ConstWeaponInfo weaponInfo = new ConstWeaponInfo(weaponData);
            weaponDict.Add(weaponData.ID, weaponInfo);
        }
    }

    private void SetupActionInputDataSet()
    {
        actionInputDict.Clear();

        foreach (var actionInputData in ActionInputDataSet.Instance.datas)
        {
            ConstActionInputInfo actionInputInfo = new ConstActionInputInfo(actionInputData);
            actionInputDict.Add(actionInputData.ID, actionInputInfo);
        }
    }

    private void SetupActionModifierDataSet()
    {
        actionModifierDict.Clear();
        foreach (var actionModifierData in ActionModifierDataSet.Instance.datas)
        {
            ConstActionModifierInfo actionModifierInfo = new ConstActionModifierInfo(actionModifierData);
            actionModifierDict.Add(actionModifierInfo.id, actionModifierInfo);
        }
    }
    
    private void SetupActionEffectDataSet()
    {
        actionEffectDict.Clear();
        foreach (var actionEffectData in ActionEffectDataSet.Instance.datas)
        {
            ConstActionEffectInfo actionEffectInfo = new ConstActionEffectInfo(actionEffectData);
            actionEffectDict.Add(actionEffectInfo.ID, actionEffectInfo);
        }
    }
    
    private void SetupActionAreaDataSet()
    {
        actionAreaDict.Clear();
        foreach (var actionAreaData in ActionAreaDataSet.Instance.datas)
        {
            ConstActionAreaInfo actionAreaInfo = new ConstActionAreaInfo(actionAreaData);
            actionAreaDict.Add(actionAreaInfo.Id, actionAreaInfo);
        }
    }
    
    private void SetupStatusEffectDataSet()
    {
        statusEffectDict.Clear();

        foreach (var statusEffectData in StatusEffectDataSet.Instance.datas)
        {
            ConstStatusEffectInfo statusEffectInfo = new ConstStatusEffectInfo(statusEffectData);
            statusEffectDict.Add(statusEffectData.ID, statusEffectInfo);
        }
    }
    
    private void SetupAnimationClipDictDataSet()
    {
        animationClipDict.Clear();
        foreach (var animationClipData in AnimationClipDataSet.Instance.datas)
        {
            ConstAnimationClipInfo animationClipInfo = new ConstAnimationClipInfo(animationClipData);
            animationClipDict.Add(animationClipData.ID, animationClipInfo);
        }
    }
    private void SetupVisualEffectDataSet()
    {
        visualEffectDict.Clear();
        foreach (var visualEffectData in VisualEffectDataSet.Instance.datas)
        {
            ConstVisualEffefctInfo visualEffefctInfo = new ConstVisualEffefctInfo(visualEffectData);
            visualEffectDict.Add(visualEffectData.ID, visualEffefctInfo);
        }
    }
    private void SetupSoundDataSet()
    {
        soundDict.Clear();
        foreach (var soundData in SoundDataSet.Instance.datas)
        {
            ConstSoundInfo soundInfo = new ConstSoundInfo(soundData);
            soundDict.Add(soundData.ID, soundInfo);
        }
    }

    
    private void SetupExtraInfoDataSet()
    {
        extraInfoDict.Clear();
        foreach (var extraSpriteData in ExtraInfoDataSet.Instance.datas)
        {
            extraInfoDict.Add(extraSpriteData.ID, new LocalizedStructInfoBase(extraSpriteData.ID, extraSpriteData.SpritePath.GetSprite(), extraSpriteData.StringTableKey));
        }
    }
    private void SetupExtraGameObjectDataSet()
    {
        extraGameObjectDict.Clear();
    }
}




#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(SpritePath))]
public class SpritePatheDrawer : PropertyDrawer
{
    protected const float imageHeight = 50;
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var spritePath = property.FindPropertyRelative("path").stringValue;
        Sprite spriteAsset = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        if (spriteAsset != null)
        {
            var subName = property.FindPropertyRelative("subName").stringValue;
            Object[] sprites = AssetDatabase.LoadAllAssetRepresentationsAtPath(spritePath);
            if(sprites.Length > 1)
            {
                for (int i = 0; i < sprites.Length; i++)
                {
                    if (sprites[i].name == subName)
                    {
                        spriteAsset = sprites[i] as Sprite;
                        break;
                    }
                }
            }
        }
        if(spriteAsset != null)
        {
            return EditorGUI.GetPropertyHeight(property, label, true) + imageHeight * 0.5f;
        }
        return EditorGUI.GetPropertyHeight(property, label, true);
    }
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        //Draw the normal property field

        var spritePath = property.FindPropertyRelative("path").stringValue;
        Sprite spriteAsset = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        if (spriteAsset != null)
        {
            var subName = property.FindPropertyRelative("subName").stringValue;
            Object[] sprites = AssetDatabase.LoadAllAssetRepresentationsAtPath(spritePath);
            if (sprites.Length > 1)
            {
                for (int i = 0; i < sprites.Length; i++)
                {
                    if (sprites[i].name == subName)
                    {
                        spriteAsset = sprites[i] as Sprite;
                        break;
                    }
                }
            }
        }

        if (spriteAsset != null)
        {
            position.xMin += 10;
            position.height = imageHeight;
            DrawTexturePreview(position, spriteAsset);
        }
        position.xMin += 20;
        EditorGUI.PropertyField(position, property, label, true);
    }

    private void DrawTexturePreview(Rect position, Sprite sprite)
    {
        Vector2 fullSize = new Vector2(sprite.texture.width, sprite.texture.height);
        Vector2 size = new Vector2(sprite.textureRect.width, sprite.textureRect.height);

        Rect coords = sprite.textureRect;
        coords.x /= fullSize.x;
        coords.width /= fullSize.x;
        coords.y /= fullSize.y;
        coords.height /= fullSize.y;

        Vector2 ratio;
        ratio.x = position.width / size.x;
        ratio.y = position.height / size.y;
        float minRatio = Mathf.Min(ratio.x, ratio.y);

        Vector2 center = position.center;
        position.width = size.x * minRatio;
        position.height = size.y * minRatio;
        //position.center = new Vector2(position.xMin, position.center.y);
        position.position = new Vector2(position.xMin - 20, position.yMin + 20);
        GUI.DrawTextureWithTexCoords(position, sprite.texture, coords);
    }
}



[CustomPropertyDrawer(typeof(LocalizedStructInfoBase))]
public class LocalizedStructInfoBaseDrawer : PropertyDrawer
{
    protected const float imageHeight = 50;
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (property.FindPropertyRelative("icon").propertyType == SerializedPropertyType.ObjectReference &&
            (property.FindPropertyRelative("icon").objectReferenceValue as Sprite) != null)
        {
            return EditorGUI.GetPropertyHeight(property, label, true) + imageHeight * 0.5f;
        }
        return EditorGUI.GetPropertyHeight(property, label, true);
    }
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        //Draw the normal property field
        if (property.FindPropertyRelative("icon").propertyType == SerializedPropertyType.ObjectReference)
        {
            var sprite = property.FindPropertyRelative("icon").objectReferenceValue as Sprite;
            if (sprite != null)
            {
                position.xMin += 10;
                position.height = imageHeight;
                DrawTexturePreview(position, sprite);
            }
        }
        position.xMin += 20;
        EditorGUI.PropertyField(position, property, label, true);
    }

    private void DrawTexturePreview(Rect position, Sprite sprite)
    {
        Vector2 fullSize = new Vector2(sprite.texture.width, sprite.texture.height);
        Vector2 size = new Vector2(sprite.textureRect.width, sprite.textureRect.height);

        Rect coords = sprite.textureRect;
        coords.x /= fullSize.x;
        coords.width /= fullSize.x;
        coords.y /= fullSize.y;
        coords.height /= fullSize.y;

        Vector2 ratio;
        ratio.x = position.width / size.x;
        ratio.y = position.height / size.y;
        float minRatio = Mathf.Min(ratio.x, ratio.y);

        Vector2 center = position.center;
        position.width = size.x * minRatio;
        position.height = size.y * minRatio;
        //position.center = new Vector2(position.xMin, position.center.y);
        position.position = new Vector2(position.xMin - 20, position.yMin + 20);
        GUI.DrawTextureWithTexCoords(position, sprite.texture, coords);
    }
}
#endif
