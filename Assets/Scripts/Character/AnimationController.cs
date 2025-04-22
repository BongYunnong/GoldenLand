using System;
using System.Collections;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    private float savedAnimatorTime;
    private string currentStateName; 
    
    private Coroutine hitStopCoroutine;
    
    private Animator animator;

    public Animator Animator
    {
        get
        {
            if (animator == null)
            {
                animator = GetComponent<Animator>();
                animator.runtimeAnimatorController = overrideController;
            }
            return animator;
        }
    }

    [SerializeField] private AnimatorOverrideController baseOverrideController;
    private AnimatorOverrideController overrideController; // 캐릭터별 인스턴스화된 OverrideController

    private void Awake()
    {
        overrideController = new AnimatorOverrideController(baseOverrideController);
    }
    public void PlayAnimationOverride(string stateName, AnimationClip clip)
    {
        SetupAnimationOverride(stateName, clip);
        Animator.CrossFadeInFixedTime(stateName, 0f);
    }
    
    public void SetupAnimationOverride(string stateName, AnimationClip clip)
    {
        overrideController[stateName] = clip;
    }
    
    
    
    public void TriggerHitstop(float duration)
    {
        if (hitStopCoroutine != null)
        {
            StopCoroutine(hitStopCoroutine);
        }
        hitStopCoroutine = StartCoroutine(HitstopCoroutine(duration));
    }
    private IEnumerator HitstopCoroutine(float duration)
    {
        Animator animator = Animator;
        // 현재 애니메이션 상태와 진행 시간 저장
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        savedAnimatorTime = stateInfo.normalizedTime % 1f;  // 0~1 사이의 값
        currentStateName = stateInfo.shortNameHash.ToString();

        float delay = duration * 0.2f;
        yield return new WaitForSeconds(delay);
        // 애니메이션 멈춤
        animator.speed = 0f;

        // Hitstop 지속 시간 동안 대기
        yield return new WaitForSeconds(duration-delay);

        // 애니메이션을 저장된 프레임부터 재생
        animator.Play(currentStateName, 0, savedAnimatorTime);
        animator.speed = 1f;
    }
}