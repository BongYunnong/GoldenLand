using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpriteTrailStruct
{
    public GameObject Container;

    public List<SpriteRenderer> spriteRenderers = new List<SpriteRenderer>();
}

public class GhostTrail : MonoBehaviour
{
    #region Variables & Initializer
    [Header("[PreRequisite]")]
    private Transform TrailContainer;
    [SerializeField] private GameObject GhostTrailPrefab;
    [SerializeField] List<SpriteTrailStruct> SpriteTrailStructs = new List<SpriteTrailStruct>();
    private SpriteRenderer[] spriteRenderers = new SpriteRenderer[0];

    [Header("[Trail Info]")]
    [SerializeField] bool PlayOnStart = false;
    [SerializeField] private float trailDisappearSpeed = 3f;
    [SerializeField] private float TrailGap = 0.2f;
    [SerializeField] private Color frontColor;
    [SerializeField] private Color backColor;


    #endregion
    bool started = false;
    #region MotionTrail

    float currTime = 0;

    bool bInitialized = false;

    public void InitializeGhostTrail(CharacterBase character)
    {
        TrailContainer = new GameObject("TrailContainer").transform;

        if (PlayOnStart)
        {
            StartMotionTrail();
        }

        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        bInitialized = true;
    }

    public void DisableGhostTrail()
    {
        SpriteTrailStructs.Clear();
        TrailContainer.gameObject.SetActive(false);
        bInitialized = false;
    }

    public void Update()
    {
        if (started)
        {
            currTime += Time.deltaTime;
            if (TrailGap <= currTime)
            {
                currTime -= TrailGap;

                CreateNewGhostImage();
            }
        }
        else
        {
            currTime = 0;
        }

        AgeAfterImages();
    }

    public void CreateNewGhostImage()
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

    private void AgeAfterImages()
    {
        for (int i = SpriteTrailStructs.Count - 1; i >= 0; i--)
        {
            bool ended = false;
            for (int j = 0; j < SpriteTrailStructs[i].spriteRenderers.Count; j++)
            {
                Color tmpColor = SpriteTrailStructs[i].spriteRenderers[j].color;
                tmpColor = Color.Lerp(tmpColor, backColor, Time.deltaTime * trailDisappearSpeed);
                SpriteTrailStructs[i].spriteRenderers[j].color = tmpColor;

                float dist = tmpColor.a - backColor.a;
                if (dist <= 0.01f)
                {
                    ended = true;
                }
            }
            if (ended)
            {
                Destroy(SpriteTrailStructs[i].Container);
                SpriteTrailStructs.RemoveAt(i);
            }
        }
    }

    public void StartMotionTrail()
    {
        if (started)
            return;
        started = true;
    }

    public void EndMotionTrail()
    {
        started = false;
    }
    #endregion
}