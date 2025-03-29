using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    public struct ViewCastInfo
    {
        public bool hit;
        public Vector3 point;
        public float dst;
        public float angle;

        public ViewCastInfo(bool _hit, Vector3 _point, float _dst, float _angle)
        {
            hit = _hit;
            point = _point;
            dst = _dst;
            angle = _angle;
        }
    }

    public struct EdgeInfo
    {
        public Vector3 pointA;
        public Vector3 pointB;

        public EdgeInfo(Vector3 _pointA, Vector3 _pointB)
        {
            pointA = _pointA;
            pointB = _pointB;
        }
    }

    [SerializeField] protected bool updateMesh = false;
    [SerializeField] protected float viewRadius;
    [SerializeField] protected float circleViewRadius;

    [Range(0, 360)]
    public float startAngle;
    [Range(0, 360)]
    public float viewAngle;

    public LayerMask obstacleMask;

    public float meshResolution;
    public int edgeResolveInterations;
    public float edgeDistanceThreshold;
    public MeshFilter viewMeshFilter;
    Mesh viewMesh;

    protected Character ownerCharacter;
    [SerializeField] protected bool effectedByOwnerCharacter = false;

    int stepCount;
    float stepAngleSize;

    private List<Vector3> viewPoints = new List<Vector3>();
    protected List<Transform> coverPositions = new List<Transform>();

    private void Start()
    {
        viewMesh = new Mesh();
        viewMesh.name = "View Mesh";
        viewMeshFilter.mesh = viewMesh;

        ownerCharacter = ownerCharacter == null ? GetComponentInParent<Character>() : ownerCharacter;


        stepCount = Mathf.RoundToInt(360 * meshResolution);
        stepAngleSize = 360.0f / stepCount;
    }

    public void InitializeFieldOfView(Character inCharacter)
    {
        ownerCharacter = inCharacter;
    }

    protected virtual void Update()
    {
        if (viewAngle > 0 && updateMesh)
        {
            DrawFieldOfView();
        }
    }
    void DrawFieldOfView()
    {
        viewPoints.Clear();
        coverPositions.Clear();
        ViewCastInfo oldViewCast = new ViewCastInfo();
        for (int i = 0; i <= stepCount; i++)
        {
            float angle = stepAngleSize * i;
            float currRadius = (Mathf.Abs(Mathf.DeltaAngle(angle, startAngle)) <= viewAngle * 0.5f) ? GetViewRadius() : GetCircleViewRadius();
            ViewCastInfo newViewCast = ViewCast(angle, currRadius, out RaycastHit outHit);
            if(outHit.collider)
            {
                coverPositions.Add(outHit.collider.transform);
            }
            if (i > 0)
            {
                bool edgeDstThresholdExceeded = Mathf.Abs(oldViewCast.dst - newViewCast.dst) > edgeDistanceThreshold;
                if (oldViewCast.hit != newViewCast.hit || (oldViewCast.hit && newViewCast.hit && edgeDstThresholdExceeded))
                {
                    EdgeInfo edge = FindEdge(oldViewCast, newViewCast, currRadius);
                    if (edge.pointA != Vector3.zero)
                    {
                        viewPoints.Add(edge.pointA);
                    }
                    if (edge.pointB != Vector3.zero)
                    {
                        viewPoints.Add(edge.pointB);
                    }
                }
            }
            viewPoints.Add(newViewCast.point);

            oldViewCast = newViewCast;
        }
        int vertexCount = viewPoints.Count + 1;
        Vector3[] verticies = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        verticies[0] = Vector3.zero;
        for (int i = 0; i < vertexCount - 1; i++)
        {
            verticies[i + 1] = viewMeshFilter.transform.InverseTransformPoint(viewPoints[i]);

            if (i < vertexCount - 2)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }
        viewMesh.Clear();
        viewMesh.vertices = verticies;
        viewMesh.triangles = triangles;
        viewMesh.RecalculateNormals();
    }

    EdgeInfo FindEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast, float _viewRadius)
    {
        float minAngle = minViewCast.angle;
        float maxAngle = maxViewCast.angle;
        Vector3 minPoint = Vector3.zero;
        Vector3 maxPoint = Vector3.zero;

        for (int i = 0; i < edgeResolveInterations; i++)
        {
            float angle = (minAngle + maxAngle) / 2;
            ViewCastInfo newViewCast = ViewCast(angle, _viewRadius, out RaycastHit outHit);

            bool edgeDstThresholdExceeded = Mathf.Abs(minViewCast.dst - newViewCast.dst) > edgeDistanceThreshold;
            bool angleIsMin = newViewCast.hit == minViewCast.hit && !edgeDstThresholdExceeded;
            if (angleIsMin)
            {
                minAngle = angle;
                minPoint = newViewCast.point;
            }
            else
            {
                maxAngle = angle;
                maxPoint = newViewCast.point;
            }
        }
        return new EdgeInfo(minPoint, maxPoint);
    }

    protected ViewCastInfo ViewCast(float globalAngle, float viewRadius, out RaycastHit hit)
    {
        Vector3 dir = DirectionFromAngle(globalAngle, true);
        if (Physics.Raycast(transform.position, dir, out hit, viewRadius, obstacleMask))
        {
            return new ViewCastInfo(true, hit.point + dir *0.5f, hit.distance, globalAngle);
        }
        else
        {
            return new ViewCastInfo(false, transform.position + dir * viewRadius, viewRadius, globalAngle);
        }
    }

    public Vector3 DirectionFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad),0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    public float GetViewRadius()
    {
        return  viewRadius;
    }
    public float GetCircleViewRadius()
    {
        return circleViewRadius;
    }
}
