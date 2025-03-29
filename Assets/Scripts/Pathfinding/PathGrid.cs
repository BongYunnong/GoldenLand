using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathGrid : MonoBehaviour
{

    [System.Serializable]
    public class TerrainType
    {
        public LayerMask terrainMask;
        public int terrainPenalty;
    }

    public LayerMask unwalkableMask;
    public Vector2 gridWorldSize;
    public float nodeRadius;
    public List<TerrainType> terrainTypes;
    LayerMask walkableMask;
    Dictionary<int, int> walkableRegionsDictionary = new Dictionary<int, int>();
    Node[,] pathGrid;

    float nodeDiameter;
    int gridSizeX, gridSizeY;

    [SerializeField] bool bShowGridWireframe = true;

    public int MaxSize
    {
        get
        {
            return gridSizeX * gridSizeY;
        }
    }

    private void Awake()
    {
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);

        foreach(TerrainType terrain in terrainTypes)
        {
            walkableMask.value |= terrain.terrainMask.value;
            walkableRegionsDictionary.Add((int)Mathf.Log(terrain.terrainMask.value,2), terrain.terrainPenalty);
        }
    }

    private void Start()
    {
        Invoke("CreateGrid", 1.0f);
    }

    void CreateGrid()
    {
        pathGrid = new Node[gridSizeX, gridSizeY];
        Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;
        for (int i = 0; i < gridSizeX; i++)
        {
            for (int j = 0; j < gridSizeY; j++)
            {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (i * nodeDiameter + nodeRadius) + Vector3.forward * (j * nodeDiameter + nodeRadius);
                bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius-0.1f, unwalkableMask));

                int movementPenalty = 0;

                Ray ray = new Ray(worldPoint + Vector3.up * 50, Vector3.down);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 100, walkableMask))
                {
                    walkableRegionsDictionary.TryGetValue(hit.collider.gameObject.layer, out movementPenalty);
                }
                else
                {
                    walkable = false;
                }
                if (!walkable)
                {
                    movementPenalty += 50;
                }
                pathGrid[i, j] = new Node(walkable, worldPoint, i, j, movementPenalty);
            }
        }
        BlurPenaltyMap(3);
    }

    public void UpdateGrid(Vector2Int InPos)
    {
        Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;
        Vector3 worldPoint = worldBottomLeft + Vector3.right * (InPos.x * nodeDiameter + nodeRadius) + Vector3.forward * (InPos.y * nodeDiameter + nodeRadius);
        bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius - 0.1f, unwalkableMask));

        int movementPenalty = 0;

        Ray ray = new Ray(worldPoint + Vector3.up * 50, Vector3.down);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100, walkableMask))
        {
            walkableRegionsDictionary.TryGetValue(hit.collider.gameObject.layer, out movementPenalty);
        }
        else
        {
            walkable = false;
        }
        if (!walkable)
        {
            movementPenalty += 50;
        }
        pathGrid[InPos.x, InPos.y].walkable = walkable;
        pathGrid[InPos.x, InPos.y].movementPenalty = movementPenalty;
    }

    void BlurPenaltyMap(int blurSize)
    {
        int kernelSize = blurSize * 2 + 1;
        int kernelExtents = (int)((kernelSize - 1) * 0.5f);
        float[,] penalties = new float[gridSizeX, gridSizeY];
        for(int y = 0;y<gridSizeY;y++)
        {
            for (int x = 0; x < gridSizeX; x++)
            {
                float sum = 0;
                float weight = 0;
                for (int i = -kernelExtents; i <= kernelExtents; i++)
                {
                    for (int j = -kernelExtents; j <= kernelExtents; j++)
                    {
                        int sampleX = Mathf.Clamp(i + x, 0, gridSizeX - 1);
                        int sampleY = Mathf.Clamp(j + y, 0, gridSizeY - 1);
                        weight = 1;
                        //gauss(i, j, 1);
                        penalties[x,y] += pathGrid[sampleX, sampleY].movementPenalty * weight;
                        sum += weight;

                        penalties[x, y] += pathGrid[x, y].movementPenalty;
                    }
                }
                penalties[x,y] *= (1.0f / sum);
            }
        }
        for (int y = 0; y < gridSizeY; y++)
        {
            for (int x = 0; x < gridSizeX; x++)
            {
                pathGrid[x,y].movementPenalty = (int)penalties[x,y];
            }
        }
    }

    public List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0)
                    continue;

                int checkX = node.gridX + i;
                int checkY = node.gridY + j;

                if (CheckIndex(checkX , checkY))
                {
                    if (Mathf.Abs(i) + Mathf.Abs(j) >= 2)
                    {
                        if (CheckWalkableIndex(node.gridX + i, node.gridY) == false)
                            continue;
                        if (CheckWalkableIndex(node.gridX, node.gridY + j) == false)
                            continue;
                    }

                    neighbors.Add(pathGrid[checkX, checkY]);
                }
            }
        }
        return neighbors;
    }

    private bool CheckIndex(int InGridX, int InGridY)
    {
        return (InGridX >= 0 && InGridX < gridSizeX && InGridY >= 0 && InGridY < gridSizeY);
    }
    private bool CheckWalkableIndex(int InGridX, int InGridY)
    {
        return CheckIndex(InGridX , InGridY) && pathGrid[InGridX, InGridY].walkable;
    }

    public Node NodeFromWorldPoint(Vector3 worldPosition)
    {
        float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float percentY = (worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);

        if (pathGrid != null)
        {
            return pathGrid[x, y];
        }
        
        return null;
    }


    public Node GetRandomNodeFromWorldPointInRange(Vector3 worldPosition, float InSize)
    {
        Vector3 checkingPos = worldPosition + new Vector3(InSize * (Random.value-0.5f),0, InSize * (Random.value - 0.5f));
        float percentX = (checkingPos.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float percentY = (checkingPos.z + gridWorldSize.y / 2) / gridWorldSize.y;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);

        if (pathGrid != null)
        {
            return pathGrid[x, y];
        }

        return null;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));
        if (pathGrid != null && bShowGridWireframe)
        {
            foreach (Node n in pathGrid)
            {
                Gizmos.color = Color.Lerp(Color.white, Color.black, Mathf.InverseLerp(0, 10,n.movementPenalty));
                Gizmos.color = (n.walkable) ? Gizmos.color : Color.red;
                Gizmos.DrawWireCube(n.worldPosition, Vector3.one * (nodeDiameter - 0.1f));
            }
        }
    }
#endif
}
