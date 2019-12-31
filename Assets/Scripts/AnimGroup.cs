using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public enum AnimDirectionType {Walk_Up, Walk_Right, Walk_Down, Walk_Left, Attack_Up, Attack_Right, Attack_Down, Attack_Left};
public enum AnimAttackType {Fist = 1, Unknown, Staff, Sword};
[System.Serializable]
public class AnimGroup : ScriptableObject
{
    [SerializeField]
    AnimationClip[] animations;

    [SerializeField]
    float spriteHeight, spriteWidth;

    int TOTAL_DIRECTION_TYPES = 8;
    int TOTAL_ATTACK_TYPES = 4;
    
    public AnimGroup () {
        animations = new AnimationClip[TOTAL_DIRECTION_TYPES * TOTAL_ATTACK_TYPES];
    }

    #if UNITY_EDITOR
    /*public static List<Sprite> GetSpritesFromAnimator(Animator anim)
    {
        List<Sprite> _allSprites = new List<Sprite> ();
        foreach(AnimationClip ac in anim.runtimeAnimatorController.animationClips)
        {
            _allSprites.AddRange(GetSpritesFromClip(ac));
        }
        return _allSprites;
    }

    private static List<Sprite> GetSpritesFromClip(AnimationClip clip)
    {
        var _sprites = new List<Sprite> ();
        if (clip != null)
        {
            foreach (var binding in AnimationUtility.GetObjectReferenceCurveBindings (clip))
            {
                ObjectReferenceKeyframe[] keyframes = AnimationUtility.GetObjectReferenceCurve (clip, binding);
                foreach (var frame in keyframes) {
                    _sprites.Add ((Sprite)frame.value);
                }
            }
        }
        return _sprites;
    }*/

    private void UpdateAnimationClipSpriteSize(AnimationClip clip)
    {
        float animationHeight = 0f, animationWidth = 0f;
        if (clip != null)
        {
            foreach (var binding in AnimationUtility.GetObjectReferenceCurveBindings (clip))
            {
                ObjectReferenceKeyframe[] keyframes = AnimationUtility.GetObjectReferenceCurve (clip, binding);
                foreach (var frame in keyframes) {
                    Sprite sprite = (Sprite) frame.value;
                    if(sprite != null) {
                        float spriteHeight = sprite.bounds.size.y;
                        float spriteWidth = sprite.bounds.size.x;
                        if(spriteHeight > animationHeight) {
                            animationHeight = spriteHeight;
                        }
                        if(spriteWidth > animationWidth) {
                            animationWidth = spriteWidth;
                        }
                    }
                }
            }
        }
        SetSpriteHeight(animationHeight);
        SetSpriteWidth(animationWidth);
    }

    public void AddAnim(int animationId, AnimDirectionType animDirectionType, AnimAttackType animAttackType) {
        if(animAttackType.Equals(AnimAttackType.Unknown)) { // Unknown
            return;
        }
        int directionType = EnumHelper.GetIndex<AnimDirectionType>(animDirectionType);
        int attackType = EnumHelper.GetIndex<AnimAttackType>(animAttackType);
        string separator = Path.DirectorySeparatorChar.ToString();
        AnimationClip anim = Resources.Load<AnimationClip>("Animations" + separator + animationId);
        animations[TOTAL_DIRECTION_TYPES * attackType + directionType] = anim;
        UpdateAnimationClipSpriteSize(anim);
    }
    #endif

    public AnimationClip GetAnim(AnimDirectionType animDirectionType, AnimAttackType animAttackType) {
        int directionType = EnumHelper.GetIndex<AnimDirectionType>(animDirectionType);
        int attackType = EnumHelper.GetIndex<AnimAttackType>(animAttackType);
        return animations[TOTAL_DIRECTION_TYPES * attackType + directionType];
    }

    public void UpdateAnimator(AnimatorOverrideController animator, AnimAttackType animAttackType) {
        foreach(AnimDirectionType directionType in Enum.GetValues(typeof(AnimDirectionType))) {
            animator[directionType.ToString()] = GetAnim(directionType, animAttackType);
        }
    }

    public float GetSpriteHeight() {
        return spriteHeight;
    }

    public float GetSpriteWidth() {
        return spriteWidth;
    }
    
    void SetSpriteHeight(float height) {
        if(height > spriteHeight) {
            spriteHeight = height;
        }
    }
    
    void SetSpriteWidth(float width) {
        if(width > spriteWidth) {
            spriteWidth = width;
        }
    }
}
