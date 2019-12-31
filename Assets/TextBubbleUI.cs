using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BubbleType { ItemDrop, Player, Slot };

public class TextBubbleUI : MonoBehaviour
{
    public AsperetaTextObject asperetaTextObject;
    public GameObject m_innerBorder, m_middleBorder, m_outerBorder;
    public GameObject m_innerBackground, m_middleBackground;
    public BubbleType m_bubbleType;

    public void UpdateBubbleText(string text) {
        if(!string.IsNullOrEmpty(text)) {
            int textLength = text.Length;
            int offsetX = (int) (-(textLength / 2f) * 6 + 1);
            int offsetY = 0;
            switch(m_bubbleType) {
                case BubbleType.ItemDrop:
                    offsetY = -11;
                    break;
                case BubbleType.Player:
                    offsetY = 9;
                    break;
                case BubbleType.Slot:
                    offsetY = 9;
                    break;
            }
            asperetaTextObject.SetText(text);
            OffsetText(offsetX, offsetY);
            UpdateBubbleObjectSize(m_innerBorder, textLength * 6 + 4, 17, -9 + offsetX, -12 + offsetY);
            UpdateBubbleObjectSize(m_innerBackground, textLength * 6 + 4, 15, -9 + offsetX, -11 + offsetY);
            UpdateBubbleObjectSize(m_middleBorder, textLength * 6 + 6, 15, -10 + offsetX, -11 + offsetY);
            UpdateBubbleObjectSize(m_middleBackground, textLength * 6 + 6, 13, -10 + offsetX, -10 + offsetY);
            UpdateBubbleObjectSize(m_outerBorder, textLength * 6 + 8, 13, -11 + offsetX, -10 + offsetY);
        }
    }

    private void OffsetText(int offsetX, int offsetY) {
        Transform gameObjectTransform = asperetaTextObject.gameObject.transform;
        Vector3 position = gameObjectTransform.localPosition;
        position.x += offsetX * 1/32f;
        position.y += offsetY * 1/32f;
        gameObjectTransform.localPosition = position;
    }

    private void UpdateBubbleObjectSize(GameObject gameObject, int width, int height, int offsetX, int offsetY) {
        Transform gameObjectTransform = gameObject.transform;
        Vector3 position = gameObjectTransform.localPosition;
        Vector3 scale = gameObjectTransform.localScale;
        position.x = offsetX * 1/32f;
        position.y = offsetY * 1/32f;
        scale.x = width * 1/32f;
        scale.y = height * 1/32f;
        gameObjectTransform.localPosition = position;
        gameObjectTransform.localScale = scale;
    }
}
