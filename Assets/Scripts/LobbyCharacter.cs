using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AIAgentComponent))]
public class LobbyCharacter : MonoBehaviour
{
    private AIAgentComponent aiAgentComponent;
    private Coroutine randomOrderCommandCoroutine;

    private void Start()
    {
        aiAgentComponent = GetComponent<AIAgentComponent>();
        aiAgentComponent.Perception.SetMaxPathCooldownTime(Random.Range(3.0f, 6.0f));
        aiAgentComponent.Perception.FillPathCooldownTime();

        randomOrderCommandCoroutine = StartCoroutine("RandomOrderCoroutine");
    }

    private void Update()
    {
        if (aiAgentComponent && aiAgentComponent.IsPathFindable())
        {
            List<GoalNode> availableGoals = new List<GoalNode>();
            GameManager.GetInstance().Map.GoalGraph.GetAvailableGoalNode(ref availableGoals);
            if (availableGoals.Count > 0)
            {
                int targetGoalNodeIndex = Random.Range(0, availableGoals.Count);
                aiAgentComponent.Perception.SetGoalTarget(availableGoals[targetGoalNodeIndex]);
            }
        }
    }

    IEnumerator RandomOrderCoroutine()
    {
        if (aiAgentComponent.IsPathFindable())
        {
            LobbyManager lobbyGameManager = GameManager.GetInstance() as LobbyManager;
            if (lobbyGameManager)
            {
                MapBase map = lobbyGameManager.Map;

                if(lobbyGameManager.lobbyCharacters.Count <= 0 || Random.value > 0.1f)
                {
                    Node currNode = map.GoalGraph.GetComponent<PathGrid>().GetRandomNodeFromWorldPointInRange(Vector3.zero, 30);
                    aiAgentComponent.Perception.SetGoalTarget(null);
                    aiAgentComponent.Perception.SetGoalTarget(currNode.worldPosition);
                }
                else
                {
                    int randomIndex = Random.Range(0, lobbyGameManager.lobbyCharacters.Count);
                    aiAgentComponent.Perception.SetGoalTarget(lobbyGameManager.lobbyCharacters[randomIndex].GetComponent<GoalNode>());
                }
            }
        }
        yield return new WaitForSeconds(Random.Range(5.0f, 10.0f));
        randomOrderCommandCoroutine = StartCoroutine("RandomOrderCoroutine");
    }
}
