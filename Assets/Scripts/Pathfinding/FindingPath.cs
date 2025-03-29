using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FindingPath
{
    public Vector3[] lookPoints;
    public PathLine[] turnBoundaries;
    public int finishLineIndex;

    public FindingPath(Vector3[] waypoints, Vector3 startPos, float turnDst)
    {
        lookPoints = waypoints;
        turnBoundaries = new PathLine[lookPoints.Length];
        finishLineIndex = turnBoundaries.Length - 1;

        Vector2 previousPoint = V3ToV2(startPos);
        for (int i = 0; i < lookPoints.Length; i++)
        {
            Vector2 currentPoint = V3ToV2(lookPoints[i]);
            Vector2 dirToCurrentPoint = (currentPoint - previousPoint).normalized;
            Vector2 turnBoundaryPoint = (i == finishLineIndex) ? currentPoint : currentPoint - dirToCurrentPoint * turnDst;
            turnBoundaries[i] = new PathLine(turnBoundaryPoint, previousPoint - dirToCurrentPoint * turnDst);
            previousPoint = turnBoundaryPoint;
        }
    }


    Vector2 V3ToV2(Vector3 v3)
    {
        return new Vector2(v3.x, v3.z);
    }

    public void DrawWithGizmos()
    {
        if (lookPoints == null) return;
        if (turnBoundaries == null) return;
        Gizmos.color = Color.black;
        foreach(Vector3 p in lookPoints)
        {
            Gizmos.DrawCube(p + Vector3.up, Vector3.one*0.1f);
        }
        Gizmos.color = Color.white;
        foreach(PathLine l in turnBoundaries)
        {
            l.DrawWithGizmos(5);
        }
    }
}
