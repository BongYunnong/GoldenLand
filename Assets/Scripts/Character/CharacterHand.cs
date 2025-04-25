using System.Collections.Generic;
using UnityEngine;

public enum EHandType
{
    Front,
    Back,
    Thumb,
    Index,
    Middle,
    Ring,
    Little
}

public class CharacterHand : MonoBehaviour
{
    private Color currentColor;
    [SerializeField] private SerializableDictionary<EHandType, LayeredSpriteRenderer> HandSRs;

    public Color GetColor()
    {
        return currentColor;
    }
    
    public void SetSprites(List<Sprite> sprites)
    {
        int index = 0;
        foreach (var handSR in HandSRs)
        {
            handSR.Value.SetSprite(sprites[index]);
            index++;
        }
    }
    
    public void EnableOutline(bool bUseOutline)
    {
        foreach (LayeredSpriteRenderer handSR in HandSRs.Values)
        {
            handSR.EnableOutline(bUseOutline);
        }
    }

    public void SetOutlineColor(Color color)
    {
        foreach (LayeredSpriteRenderer handSR in HandSRs.Values)
        {
            handSR.SetOutlineColor(color);
        }
    }

    public void SetMainMaterial(Material material)
    {
        foreach (LayeredSpriteRenderer handSR in HandSRs.Values)
        {
            handSR.SetMainMaterial(material);
        }
    }

    public void ResetColor()
    {
        foreach (LayeredSpriteRenderer handSR in HandSRs.Values)
        {
            handSR.SetColor(Color.white);
        }
    }

    public void SetColor(Color color)
    {
        foreach (LayeredSpriteRenderer handSR in HandSRs.Values)
        {
            handSR.SetColor(color);
        }
    }

    public void SetTemporalSpriteColor(Color targetColor, float speed)
    {
        currentColor = Color.Lerp(currentColor, targetColor, speed * Time.deltaTime);
        foreach (LayeredSpriteRenderer handSR in HandSRs.Values)
        {
            handSR.SetColor(currentColor);
        }
    }
}
