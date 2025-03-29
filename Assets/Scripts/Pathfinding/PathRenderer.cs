using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathRenderer : MonoBehaviour
{
    private Character ownerCharacter;
    private LineRenderer lineRenderer;
    [SerializeField] SpriteRenderer targetMarkerSpriteRenderer;
    private Animator targetMarkerAnimator;

    [SerializeField] private Material[] pathMaterials;

    private void Awake()
    {
        lineRenderer = GetComponentInChildren<LineRenderer>();
    }
    public void InitializePathRenderer(Character _character)
    {
        ownerCharacter = _character;

        lineRenderer.enabled = false;
        targetMarkerSpriteRenderer.enabled = false;

        targetMarkerAnimator = GetComponentInChildren<Animator>();
    }

    public void DisablePathRenderer()
    {
        gameObject.SetActive(false);
    }

    public void ClearPathLines()
    {
        targetMarkerAnimator.SetBool("Activate", false);
        lineRenderer.enabled = false;
        lineRenderer.positionCount = 0;
        targetMarkerSpriteRenderer.enabled = false;
    }

    public void ChangeLineColor(Color _lineColor)
    {
        _lineColor.a = 0.5f;
        lineRenderer.startColor = _lineColor;
        lineRenderer.endColor = _lineColor;
    }

    public void DrawPathLines(List<Vector3> _targetPath, GoalNode _target, Vector3 _requestedPoint, Vector3 InOffset)
    {
        if (lineRenderer == null) return;

        lineRenderer.enabled = true;
        if (_targetPath.Count > 0)
        {
            lineRenderer.material = pathMaterials[0];
            lineRenderer.positionCount = (_target != null) ? _targetPath.Count + 1 : _targetPath.Count;

            Vector3 destTargetPos = _requestedPoint;
            for (var i = 0; i < _targetPath.Count; ++i)
            {
                lineRenderer.SetPosition(i, _targetPath[i] + InOffset);
                if(i == _targetPath.Count-1)
                {
                    destTargetPos = _targetPath[i];
                }
            }
            if(_target != null)
            {
                destTargetPos = _target.transform.position;
                lineRenderer.SetPosition(_targetPath.Count, destTargetPos + InOffset);
            }
            targetMarkerSpriteRenderer.enabled = true;
            targetMarkerAnimator.transform.position = destTargetPos;
            targetMarkerAnimator.SetBool("Activate", true);
        }
        else
        {
            lineRenderer.material = pathMaterials[1];
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, ownerCharacter.transform.position + InOffset);
            lineRenderer.SetPosition(1, _requestedPoint + InOffset);

            targetMarkerSpriteRenderer.enabled = true;
            targetMarkerAnimator.transform.position = _requestedPoint;
            targetMarkerAnimator.SetBool("Activate", true);
        }

        if (_targetPath.Count == 0)
        {
            targetMarkerAnimator.SetFloat("TargetIndex", 4);
        }
        else if (_target)
        {
            if (_target.TryGetComponent(out Character targetCharacter))
            {
                targetMarkerAnimator.SetFloat("TargetIndex", 3);
            }
        }
        else
        {
            targetMarkerAnimator.SetFloat("TargetIndex", 0);
        }
    }
}
