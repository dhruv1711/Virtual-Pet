using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AnimationPlayer : MonoBehaviour
{
    Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        animator = gameObject.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Animator GetTargetAnimator()
    {
        if(animator == null)
        {
            animator = gameObject.GetComponent<Animator>();
            return animator;
        }
        else
        {
            return animator;
        }
        
    }

    public void PlayAnimation(string animationName)
    {
        if(animationName != "")
        {
            animator.StopPlayback();
            animator.Play(animationName);
        }

        
    }


}
