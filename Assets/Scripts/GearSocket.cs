using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SocketType { Weapon, Shield, Helm, Hair, Top, Bottom, Shoes, Face, Body };

public class GearSocket : MonoBehaviour
{
    string ANIMATOR_ISMOVING = "IsMoving";
    string ANIMATOR_ISATTACKING = "IsAttacking";
    string ANIMATOR_HORIZTONAL = "Horizontal";
    string ANIMATOR_VERTICAL = "Vertical";
    string ANIMATOR_MOVEMENTSPEED = "MovementSpeed";
    string ANIMATOR_ATTACKSPEED = "AttackSpeed";
    public SocketType m_socketType;

    public SpriteRenderer m_spriteRenderer;
    public Animator m_animator;
    public AnimGroup m_animGroup;
    public AnimAttackType m_animAttackType;
    public Color m_animColor = Color.white;
    public Color m_tintColor = Color.white;
    int m_originalSortingOrder;
    AnimatorOverrideController m_animatorOverrideController;
    
    void Awake() {
        m_animatorOverrideController = new AnimatorOverrideController(m_animator.runtimeAnimatorController);
        m_animator.runtimeAnimatorController = m_animatorOverrideController;
        m_animator.runtimeAnimatorController.name = "GearOverrideController";
        m_spriteRenderer = GetComponent<SpriteRenderer>();
        m_originalSortingOrder = m_spriteRenderer.sortingOrder;
        m_animColor.a = 0f;
    }

    public void Equip(AnimGroup animGroup, AnimAttackType animAttackType, Color color) {
        float flashAmount = color.a;
        Color flashColor = new Color(color.r, color.g, color.b, 1f);
        m_spriteRenderer.material.SetFloat("_FlashAmount", flashAmount);
        m_spriteRenderer.material.SetColor("_FlashColor", flashColor);
        m_spriteRenderer.material.SetColor("_Color", m_tintColor);
        m_spriteRenderer.color = Color.white;
        m_spriteRenderer.sprite = null;

        m_animGroup = animGroup;
        m_animAttackType = animAttackType;
        m_animColor = color;

        if(m_animGroup != null) {
            float spriteHeight = m_animGroup.GetSpriteHeight();
            Vector3 pos = transform.localPosition;
            pos.y = -Mathf.Max((spriteHeight - 1.5f) / 2, 0f);
            transform.localPosition = pos;

            animGroup.UpdateAnimator(m_animatorOverrideController, m_animAttackType);
        }
        else {
            ClearAnimator(m_animatorOverrideController, m_animAttackType);
        }
    }

    void ClearAnimator(AnimatorOverrideController animator, AnimAttackType animAttackType) {
        foreach(AnimDirectionType directionType in Enum.GetValues(typeof(AnimDirectionType))) {
            animator[directionType.ToString()] = null;
        }
    }

    public void Equip(AnimGroup animGroup, AnimAttackType animAttackType) {
        Equip(animGroup, animAttackType, m_animColor);
    }

    public void SetAttackType(AnimAttackType animAttackType) {
        m_animAttackType = animAttackType;

        if(m_animGroup != null) {
            m_animGroup.UpdateAnimator(m_animatorOverrideController, m_animAttackType);
        }
    }

    public void SetInvisible(bool invis, bool canSeeInvis) {
        if(invis) {
            if(canSeeInvis) {
                m_tintColor.a = 92f / 255f;
            }
            else {
                m_tintColor.a = 0f;
            }
        }
        else {            
            m_tintColor.a = 1f;
        }
        m_spriteRenderer.material.SetColor("_Color", m_tintColor);
    }

    public void AnimatorFacing(float horizontal, float vertical) {
        if (horizontal != 0 || vertical != 0){
            m_animator.SetFloat(ANIMATOR_HORIZTONAL, horizontal);
            m_animator.SetFloat(ANIMATOR_VERTICAL, vertical);
            if(m_socketType.Equals(SocketType.Weapon) || m_socketType.Equals(SocketType.Shield)) {
                if(vertical < 0 || horizontal > 0 && m_socketType.Equals(SocketType.Weapon) || horizontal < 0 && m_socketType.Equals(SocketType.Shield)) {
                    m_spriteRenderer.sortingOrder = m_originalSortingOrder;
                }
                else {
                    m_spriteRenderer.sortingOrder = 0;
                }
            }
        }
    }

    public void AnimatorMoving(bool isMoving) {
        m_animator.SetBool(ANIMATOR_ISMOVING, isMoving);
    }

    public void AnimatorAttacking(bool isAttacking) {
        m_animator.SetBool(ANIMATOR_ISATTACKING, isAttacking);
    }

    public void AnimatorMovementSpeed(float movementSpeed) {
        m_animator.SetFloat(ANIMATOR_MOVEMENTSPEED, movementSpeed);
    }

    public void AnimatorAttackSpeed(float attackSpeed) {
        m_animator.SetFloat(ANIMATOR_ATTACKSPEED, attackSpeed);
    }
}
