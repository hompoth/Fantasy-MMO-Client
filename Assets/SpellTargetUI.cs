using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellTargetUI : MonoBehaviour
{
    public GameObject m_outerTargetLeft, m_outerTargetRight, m_outerTargetUp, m_outerTargetDown;
    public GameObject m_innerTargetLeft, m_innerTargetRight, m_innerTargetUp, m_innerTargetDown;

    public void UpdateTargetSize(float width, float height) {
        float innerWidth = width - 1/32f, outerWidth = width + 1/32f;
        float innerHeight = height - 1/32f, outerHeight = height + 1/32f;
        UpdateTargetSideSize(m_outerTargetLeft, 1/32f, outerHeight, -width/2, 0);
        UpdateTargetSideSize(m_outerTargetRight, 1/32f, outerHeight, width/2, 0);
        UpdateTargetSideSize(m_outerTargetUp, outerWidth, 1/32f, -width/2, height);
        UpdateTargetSideSize(m_outerTargetDown, outerWidth, 1/32f, -width/2, 0);
        UpdateTargetSideSize(m_innerTargetLeft, 1/32f, innerHeight, -width/2 + 1/32f, 1/32f);
        UpdateTargetSideSize(m_innerTargetRight, 1/32f, innerHeight, width/2 - 1/32f, 1/32f);
        UpdateTargetSideSize(m_innerTargetUp, innerWidth, 1/32f, -width/2 + 1/32f, height - 1/32f);
        UpdateTargetSideSize(m_innerTargetDown, innerWidth, 1/32f, -width/2 + 1/32f, 1/32f);
    }

    private void UpdateTargetSideSize(GameObject gameObject, float width, float height, float offsetX, float offsetY) {
        Transform gameObjectTransform = gameObject.transform;
        Vector3 position = gameObjectTransform.localPosition;
        Vector3 scale = gameObjectTransform.localScale;
        position.x = offsetX;
        position.y = offsetY;
        scale.x = width;
        scale.y = height;
        gameObjectTransform.localPosition = position;
        gameObjectTransform.localScale = scale;
    }
}
