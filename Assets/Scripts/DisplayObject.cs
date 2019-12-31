using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayObject : MonoBehaviour
{
    AsperetaTextObject m_asperetaText;
    float m_originalYPosition;

    float DISPLAY_TIME = 1f;

    void Awake()
    {
        StartCoroutine(MoveUpAndDestroy());
    }

    public void SetTextObject(AsperetaTextObject asperetaText) {
        m_asperetaText = asperetaText;
    }

    public void SetYOffset(float yOffset) {
        m_originalYPosition = yOffset;
    }

    IEnumerator MoveUpAndDestroy() {
        float currentTimeElapsed = 0f;
        while(m_asperetaText == null) yield return null;
        while (currentTimeElapsed < DISPLAY_TIME)
        {
            float yOffset = (currentTimeElapsed / DISPLAY_TIME);
            Vector3 pos = m_asperetaText.gameObject.transform.localPosition;
            pos.y = m_originalYPosition + yOffset;
            m_asperetaText.gameObject.transform.localPosition = pos;
            currentTimeElapsed += Time.deltaTime;

            yield return null;
        }
        Destroy(gameObject);
    }
}
