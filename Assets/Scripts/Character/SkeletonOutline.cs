using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonOutline : MonoBehaviour
{
    #region Variables & Initializer
    [Header("[PreRequisite]")]
    private Transform TrailContainer;
    [SerializeField] private GameObject GhostTrailPrefab;
    [SerializeField] List<SpriteTrailStruct> SpriteTrailStructs = new List<SpriteTrailStruct>();
    private SpriteRenderer[] spriteRenderers = new SpriteRenderer[0];

    [Header("[Trail Info]")]
    [SerializeField] private Color frontColor;
    [SerializeField] private Color backColor;


    #endregion

    #region MotionTrail
    bool bInitialized = false;

    public void InitializeOutline(CharacterBase character)
    {
        TrailContainer = new GameObject("TrailContainer").transform;

        CreateNewAfterImage();

        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        bInitialized = true;
    }

    public void DisableOutline()
    {
        SpriteTrailStructs.Clear();
        TrailContainer.gameObject.SetActive(false);
        bInitialized = false;
    }

    public void CreateNewAfterImage()
    {
        if (bInitialized == false) return;
        SpriteTrailStruct pss = new SpriteTrailStruct();
        pss.Container = Instantiate(GhostTrailPrefab, TrailContainer);
        for (int j = 0; j < spriteRenderers.Length; j++)
        {
            GameObject tmpObj = Instantiate(pss.Container.transform.GetChild(0).gameObject, pss.Container.transform);
            tmpObj.SetActive(true);
            pss.spriteRenderers.Add(pss.Container.transform.GetChild(j + 1).GetComponent<SpriteRenderer>());
            pss.spriteRenderers[pss.spriteRenderers.Count - 1].sprite = spriteRenderers[j].sprite;
            pss.spriteRenderers[pss.spriteRenderers.Count - 1].flipX = spriteRenderers[j].flipX;
            tmpObj.transform.position = spriteRenderers[j].transform.position;
            tmpObj.transform.rotation = spriteRenderers[j].transform.rotation;
            pss.spriteRenderers[j].color = frontColor;
        }
        SpriteTrailStructs.Add(pss);
    }
    #endregion
}
