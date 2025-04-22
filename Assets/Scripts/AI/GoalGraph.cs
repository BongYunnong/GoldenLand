using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(MapBase))]
public class GoalGraph : MonoBehaviour
{
    public GoalNode mainGoalNode;

    public void InitializeGoalGraph()
    {
    }

    public void GetAvailableGoalNode(ref List<GoalNode> availableGoalNodes)
    {
        mainGoalNode.Traversal(ref availableGoalNodes);
    }
}
