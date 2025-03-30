using System;
using UnityEngine;

public class RythmComponent : MonoBehaviour
{
    [SerializeField] private Animator animator;
    
    private void Start()
    {
        // TimingManager.Instance.beatFrameUpdated += ProcessRythm;
    }

    public static bool HasParameter(string paramName, Animator animator)
    {
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName)
                return true;
        }
        return false;
    }
    
    public void ProcessRythm(int index)
    {
        if (this.gameObject.activeInHierarchy == false) return;
        if (HasParameter("Rythm", animator))
        {
            animator.SetTrigger("Rythm");
        }
    }
}
