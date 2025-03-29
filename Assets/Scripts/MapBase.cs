using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

using UnityEditor;

public class MapBase : MonoBehaviour
{
    GoalGraph goalGraph;
    public GoalGraph GoalGraph { get { return goalGraph; } }

    private void Start()
    {
        goalGraph = GetComponent<GoalGraph>();
        if(goalGraph)
        {
            goalGraph.InitializeGoalGraph();
        }
    }
}

