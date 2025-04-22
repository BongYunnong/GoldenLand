using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GoalNode : MonoBehaviour
{
    [SerializeField] Color nodeColor = Color.white;
    [SerializeField] Sprite nodeIcon;
    [SerializeField] bool autoCompleteWithChildren = false;
    public List<GoalNode> childNodes = new List<GoalNode>();
    private bool completed;
    private int childCompleteCount = 0;
    public int targetCapacity = 1;
    public bool Completed { get { return completed; } }
    private List<Character> targetingCharacter = new List<Character>();

    [SerializeField] Vector3 offset = new Vector3(0,0.5f,0.0f);

    public UnityAction<bool> onGoalCompleteChanged;

    private void Start()
    {
        for(int i=0;i<childNodes.Count;i++)
        {
            if (childNodes[i])
            {
                childNodes[i].onGoalCompleteChanged += UpdateChildCompleteProgress;
            }
        }
    }

    private void UpdateChildCompleteProgress(bool _completed)
    {
        childCompleteCount += (_completed ? 1 : -1);
        if(autoCompleteWithChildren && childCompleteCount >= childNodes.Count)
        {
            SetCompleted(true);
        }
    }

    public void SetTargeted(Character targetingCharacter, bool target)
    {
        if(target)
        {
            this.targetingCharacter.Add(targetingCharacter);
        }
        else
        {
            this.targetingCharacter.Remove(targetingCharacter);
        }
    }


    public void SetCompleted(bool completed)
    {
        this.completed = completed;
        if(this.completed && onGoalCompleteChanged != null)
        {
            onGoalCompleteChanged.Invoke(this.completed);
        }
    }


    public void Traversal(ref List<GoalNode> goalNodes)
    {
        if (CanGoallable())
        {
            if (goalNodes.Contains(this) == false)
            {
                goalNodes.Add(this);
            }
        }
        else
        {
            for (int i = 0; i < childNodes.Count; i++)
            {
                childNodes[i].Traversal(ref goalNodes);
            }
        }
    }

    public bool CanGoallable()
    {
        bool canGollable = !completed;
        canGollable &= gameObject.activeInHierarchy;
        for (int i = 0; i < childNodes.Count; i++)
        {
            canGollable &= childNodes[i].completed;
        }
        int targetingCount = 0;
        for(int i=0;i< targetingCharacter.Count;i++)
        {
            if (targetingCharacter[i].Perception.GetCurrentTarget() == this)
            {
                targetingCount++;
            }
        }

        canGollable &= (targetingCount < targetCapacity);
        return canGollable;
    }

    public Vector3 GetTargetPosition()
    {
        return transform.position + offset;
    }

    [ExecuteInEditMode]
    private void OnDrawGizmos()
    {
        if(nodeIcon)
        {
            Gizmos.DrawIcon(transform.position, nodeIcon.name + ".png");
        }
        Gizmos.color = nodeColor;
        for (int i=0;i< childNodes.Count;i++)
        {
            if (childNodes[i])
            {
                Gizmos.DrawLine(transform.position, childNodes[i].transform.position);
            }
        }
    }
}
