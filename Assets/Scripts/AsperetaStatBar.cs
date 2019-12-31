using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsperetaStatBar : MonoBehaviour
{
    public GameObject m_hpBar, m_mpBar, m_background;
    public SpriteRenderer m_hpBarSpriteRenderer;
    float m_statBarWidth = 1f;
    int m_hpPercent, m_mpPercent;

    public void SetHPBar(int hpPercent) {
        m_hpPercent = hpPercent;
        UpdateStatScale(m_hpBar, m_hpPercent);

        if(hpPercent > 66) {
            m_hpBarSpriteRenderer.color = new Color(0, 1f, 0, 1);
        }
        else if(hpPercent > 33) {
            m_hpBarSpriteRenderer.color = new Color(1f, 1f, 0, 1);
        }
        else {
            m_hpBarSpriteRenderer.color = new Color(1f, 0, 0, 1);
        }
    }

    public void SetMPBar(int mpPercent) {
        m_mpPercent = mpPercent;
        UpdateStatScale(m_mpBar, m_mpPercent);
    }

    public void UpdateStatBarWidth(float width) {
        m_statBarWidth = width;
        UpdateStatBarSize(m_background, m_statBarWidth);
        UpdateStatBarSize(m_hpBar, m_statBarWidth);
        UpdateStatBarSize(m_mpBar, m_statBarWidth);
        UpdateStatScale(m_hpBar, m_hpPercent);
        UpdateStatScale(m_mpBar, m_mpPercent);
    }

    private void UpdateStatBarSize(GameObject gameObject, float width) {
        Transform gameObjectTransform = gameObject.transform;
        Vector3 position = gameObjectTransform.localPosition;
        Vector3 scale = gameObjectTransform.localScale;
        position.x = -width / 2;
        scale.x = width;
        gameObjectTransform.localPosition = position;
        gameObjectTransform.localScale = scale;
    }

    private void UpdateStatScale(GameObject gameObject, int statPercent) {
        float statWidth = (m_statBarWidth * statPercent / 100f);
        statWidth = Mathf.Round(statWidth * 32) / 32;
        Vector3 scale = gameObject.transform.localScale;
        scale.x = statWidth;
        gameObject.transform.localScale = scale;
    }
}
