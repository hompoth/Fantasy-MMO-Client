
#if UNITY_EDITOR
/*The MIT License (MIT)
Copyright (c) 2016 Edward Rowe (@edwardlrowe)
Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public static class AnimClipBuilder
{
    public static AnimationClip CreateClip(Sprite[] sprites, string clipName, int sampleRate = 12, bool isLooping = false) 
    {
        // Output nothing if there is no clip name
        if (string.IsNullOrEmpty(clipName))
        {
            return null;
        }

        // Create a new Clip
        AnimationClip clip = new AnimationClip();

        // Apply the name and framerate
        clip.name = clipName;
        clip.frameRate = sampleRate;

        // Apply Looping Settings
        AnimationClipSettings clipSettings = new AnimationClipSettings();
        clipSettings.loopTime = isLooping;
        AnimationUtility.SetAnimationClipSettings(clip, clipSettings);

        // Initialize the curve property for the animation clip
        EditorCurveBinding curveBinding = new EditorCurveBinding();
        curveBinding.propertyName = "m_Sprite";
        // Assumes user wants to apply the sprite property to the root element
        curveBinding.path = "";
        curveBinding.type = typeof(SpriteRenderer);

        // Build keyframes for the property using the supplied Sprites
        ObjectReferenceKeyframe[] keys = CreateKeysForSprites(sprites, sampleRate);

        // Build the clip if valid
        if (keys.Length > 0)
        {
            // Set the keyframes to the animation
            AnimationUtility.SetObjectReferenceCurve(clip, curveBinding, keys);
        }

        return clip;
    }

    private static ObjectReferenceKeyframe[] CreateKeysForSprites(Sprite[] sprites, int samplesPerSecond)
    {
        List<ObjectReferenceKeyframe> keys = new List<ObjectReferenceKeyframe>();
        float timePerFrame = 1.0f / samplesPerSecond;
        float currentTime = 0.0f;
        foreach (Sprite sprite in sprites)
        {
            ObjectReferenceKeyframe keyframe = new ObjectReferenceKeyframe();
            keyframe.time = currentTime;
            keyframe.value = sprite;
            keys.Add(keyframe);

            currentTime += timePerFrame;
        }

        return keys.ToArray();
    }
}
#endif