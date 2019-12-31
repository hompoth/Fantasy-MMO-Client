using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Emoticon : MonoBehaviour
{
    public Animator animator;
    public SpriteRenderer spriteRenderer;
    float m_animationSpeed = 2.5f;
    AnimatorOverrideController animatorOverrideController;
    
    // Start is called before the first frame update
    void Awake() {
        animatorOverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animator.runtimeAnimatorController = animatorOverrideController;
        animator.runtimeAnimatorController.name = "EmoteOverrideController";
        animator.speed = 1 / m_animationSpeed;
    }

    public void PlayAnimation(AnimationClip anim) {
        animator.enabled = true;
        animatorOverrideController["Emote"] = anim;
        animator.Play("Emote", -1, 0);
        CancelInvoke("StopAnimation");
        Invoke("StopAnimation", anim.length * m_animationSpeed);
    }

    void StopAnimation() {
        spriteRenderer.sprite = null;
        animator.enabled = false;
    }
}
