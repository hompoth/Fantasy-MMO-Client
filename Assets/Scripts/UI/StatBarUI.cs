using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatBarUI : WindowUI
{
    public GameObject statBarSpriteMask;
    public AsperetaTextObject asperetaTextObject; 

    public void SetStatAmount(int statAmount) {
        asperetaTextObject.SetText(statAmount.ToString());
    }

    public void SetStatPercent(int statPercent) {
        Vector3 scale = statBarSpriteMask.transform.localScale;
        scale.x = statPercent / 100f;
        statBarSpriteMask.transform.localScale = scale;
    }
}
