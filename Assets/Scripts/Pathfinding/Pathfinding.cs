using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Pathfinding : MonoBehaviour
{
    PathGrid pathGrid;

    private void Awake()
    {
        pathGrid = GetComponent<PathGrid>();
    }
    
    public void FindPath(PathRequest request, Action<PathResult> callback)
    {
        Vector3[] wayPoints = new Vector3[0];
        bool pathSuccess = false;

        Node startNode = pathGrid.NodeFromWorldPoint(request.pathStart);
        Node targetNode = pathGrid.NodeFromWorldPoint(request.pathEnd);
        if(startNode != null && startNode.walkable && targetNode !=null && targetNode.walkable)
        {
            Heap<Node> openSet = new Heap<Node>(pathGrid.MaxSize);
            HashSet<Node> closedSet = new HashSet<Node>();
            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                Node currentNode = openSet.RemoveFirst();
                closedSet.Add(currentNode);

                if (currentNode == targetNode)
                {
                    pathSuccess = true;
                    break;
                }

                foreach (Node neighbour in pathGrid.GetNeighbors(currentNode))
                {
                    if (!neighbour.walkable || closedSet.Contains(neighbour))
                    {
                        continue;
                    }

                    int newMovementCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbour) + neighbour.movementPenalty;
                    if (newMovementCostToNeighbor < neighbour.gCost || !openSet.Contains(neighbour))
                    {
                        neighbour.gCost = newMovementCostToNeighbor;
                        neighbour.hCost = GetDistance(neighbour, targetNode);
                        neighbour.parent = currentNode;

                        if (!openSet.Contains(neighbour))
                        {
                            openSet.Add(neighbour);
                        }
                        else
                        {
                            openSet.UpdateItem(neighbour);
                        }
                    }
                }
            }
        }
        if (pathSuccess)
        {
            wayPoints = RetracePath(startNode, targetNode);
        }
        callback(new PathResult(wayPoints, pathSuccess, request.callback));
    }

    Vector3[] RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        Vector3[] waypoints = SimplifyPath(path);
        Array.Reverse(waypoints);
        return waypoints;
    }

    Vector3[] SimplifyPath(List<Node> path)
    {
        List<Vector3> waypoints = new List<Vector3>();
        Vector2 directionOld = Vector2.zero;
        for (int i=1;i<path.Count;i++)
        {
            Vector2 directionNew = new Vector2(path[i - 1].gridX - path[i].gridX, path[i - 1].gridY - path[i].gridY);
            if(directionNew != directionOld)
            {
                waypoints.Add(path[i].worldPosition);
            }
            directionOld = directionNew;
        }
        return waypoints.ToArray();
    }

    int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }

    public void UpdateGrid(Vector3 InPos, float InSize)
    {
        Node centerNode = pathGrid.NodeFromWorldPoint(InPos);
        for(float i= centerNode.gridX - InSize *0.5f; i <= centerNode.gridX + InSize * 0.5f; i++)
        {
            for (float j = centerNode.gridY - InSize * 0.5f; j <= centerNode.gridY + InSize * 0.5f; j++)
            {
                pathGrid.UpdateGrid(new Vector2Int((int)i, (int)j));
            }
        }
    }
}
