using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spell : MonoBehaviour
{
    public Animator animator;
    public SpriteRenderer spriteRenderer;
    public bool destroyOnStop;
    AnimatorOverrideController animatorOverrideController;
    
    // Start is called before the first frame update
    void Awake() {
        animatorOverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animator.runtimeAnimatorController = animatorOverrideController;
        animator.runtimeAnimatorController.name = "SpellOverrideController";
    }

    public void PlayAnimation(AnimationClip anim) {
        animator.enabled = true;
        animatorOverrideController["Spell"] = anim;
        animator.Play("Spell", -1, 0);
        CancelInvoke("StopAnimation");
        Invoke("StopAnimation", anim.length);
    }

    void StopAnimation() {
        if(destroyOnStop) {
            Destroy(gameObject);
        }
        else {
            spriteRenderer.sprite = null;
            animator.enabled = false;
        }
    }
}
