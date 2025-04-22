using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(SpriteRenderer))]
public class LayeredSpriteRenderer : MonoBehaviour
{
    private SpriteRenderer mainSpriteRenderer;
    public SpriteRenderer MainSpriteRenderer{get{return mainSpriteRenderer;}}
    private SpriteRenderer subSpriteRenderer;
    [SerializeField] private GameObject layerSpritePrefab;
    [SerializeField] private Material layerMaterial = null;

   private Material originalMaterial;

    private void Awake()
    {
        GraphicsSettings.useScriptableRenderPipelineBatching = false;
        mainSpriteRenderer = GetComponent<SpriteRenderer>();
        originalMaterial = mainSpriteRenderer.material;
        subSpriteRenderer = Instantiate(layerSpritePrefab, transform).GetComponent<SpriteRenderer>();
        subSpriteRenderer.material = layerMaterial;
        EnableOutline(false);
    }

    public void EnableOutline(bool bUseOutline)
    {
        subSpriteRenderer.gameObject.SetActive(bUseOutline);
    }

    public void SetEnabled(bool enabled)
    {
        mainSpriteRenderer.enabled = enabled;
        subSpriteRenderer.enabled = enabled;
    }
    public bool GetEnabled()
    {
        return mainSpriteRenderer.enabled;
    }


    public void SetSprite(Sprite sprite)
    {
        mainSpriteRenderer.sprite = sprite;
        subSpriteRenderer.sprite = sprite;
    }

    public void SetMainMaterial(Material material)
    {
        if (material == null)
        {
            mainSpriteRenderer.material = originalMaterial;
        }
        else
        {
            mainSpriteRenderer.material = material;
        }
    }

    public void SetFlipY(bool value)
    {
        mainSpriteRenderer.flipY = value;
        subSpriteRenderer.flipY = value;
    }

    public void SetColor(Color color)
    {
        mainSpriteRenderer.color = color;
        //subSpriteRenderer.color = color;
    }

    public void SetOutlineColor(Color color)
    {
        subSpriteRenderer.material.SetColor("_Color", color);
    }
    
    public Color GetColor()
    {
        return mainSpriteRenderer.color;
    }
}