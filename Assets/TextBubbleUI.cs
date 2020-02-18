using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BubbleType { ItemDrop, Player, Slot, ChatBubble };

public class TextBubbleUI : MonoBehaviour
{
    public AsperetaTextObject[] textObject;
    public GameObject m_innerBorder, m_middleBorder, m_outerMiddleBorder, m_outerBorder;
    public GameObject m_innerBackground, m_middleBackground, m_outerMiddleBackground;
    public BubbleType m_bubbleType;
    Vector3 m_textPosition;

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
                case BubbleType.ChatBubble:
                    textLength = Math.Min(text.Length, 30);
                    offsetX = (int) (-(textLength / 2f) * 6 + 1);
                    offsetX = offsetX + 3;
                    offsetY = 3;
                    break;
            }
            int totalLines = UpdateText(text, offsetX, offsetY);
            FormatBubble(totalLines, textLength, offsetX, offsetY);
        }
    }

    private void FormatBubble(int totalLines, int textLength, int offsetX, int offsetY) {
        if(m_bubbleType.Equals(BubbleType.ChatBubble)) {
            m_outerMiddleBorder.SetActive(true);
            m_outerMiddleBackground.SetActive(true);
            UpdateBubbleObjectSize(m_innerBorder, textLength * 6 + 2, 17 + 11 * (totalLines - 1), -9 + offsetX, -12 + offsetY);
            UpdateBubbleObjectSize(m_innerBackground, textLength * 6 + 2, 15 + 11 * (totalLines - 1), -9 + offsetX, -11 + offsetY);
            UpdateBubbleObjectSize(m_middleBorder, textLength * 6 + 6, 15 + 11 * (totalLines - 1), -11 + offsetX, -11 + offsetY);
            UpdateBubbleObjectSize(m_middleBackground, textLength * 6 + 6, 13 + 11 * (totalLines - 1), -11 + offsetX, -10 + offsetY);
            UpdateBubbleObjectSize(m_outerMiddleBorder, textLength * 6 + 8, 13 + 11 * (totalLines - 1), -12 + offsetX, -10 + offsetY);
            UpdateBubbleObjectSize(m_outerMiddleBackground, textLength * 6 + 8, 9 + 11 * (totalLines - 1), -12 + offsetX, -8 + offsetY);
            UpdateBubbleObjectSize(m_outerBorder, textLength * 6 + 10, 9 + 11 * (totalLines - 1), -13 + offsetX, -8 + offsetY);
        }
        else {
            m_outerMiddleBorder.SetActive(false);
            m_outerMiddleBackground.SetActive(false);
            UpdateBubbleObjectSize(m_innerBorder, textLength * 6 + 4, 17, -9 + offsetX, -12 + offsetY);
            UpdateBubbleObjectSize(m_innerBackground, textLength * 6 + 4, 15, -9 + offsetX, -11 + offsetY);
            UpdateBubbleObjectSize(m_middleBorder, textLength * 6 + 6, 15, -10 + offsetX, -11 + offsetY);
            UpdateBubbleObjectSize(m_middleBackground, textLength * 6 + 6, 13, -10 + offsetX, -10 + offsetY);
            UpdateBubbleObjectSize(m_outerBorder, textLength * 6 + 8, 13, -11 + offsetX, -10 + offsetY);
        }
    }

    private int UpdateText(string text, int offsetX, int offsetY) {
        for(int index = 0; index < textObject.Length; ++index) {
            SetText(index, "");
        }
        int textObjectIndex = 0;
        if(m_bubbleType.Equals(BubbleType.ChatBubble)) {
            string[] splitText = text.Split(' ');
            string currentTextLine = "";
            for(int index = 0; index < splitText.Length; ++index) {
                string currentWord = splitText[index];
                if(currentWord.Length > 30) {
                    for(int wordIndex = 0; wordIndex < Math.Ceiling(currentWord.Length / 30f); ++wordIndex) {
                        if(!string.IsNullOrEmpty(currentTextLine)) {
                            SetText(textObjectIndex++, currentTextLine);
                        }
                        currentTextLine = SafeSubstring(currentWord, wordIndex * 30, 30);
                    }
                }
                else {
                    int totalAppendLength = (currentTextLine.Length > 0) ? currentTextLine.Length + currentWord.Length + 1 : currentTextLine.Length;
                    if(totalAppendLength > 30) {
                        SetText(textObjectIndex++, currentTextLine);
                        currentTextLine = currentWord;
                    }
                    else {
                        currentTextLine = string.IsNullOrEmpty(currentTextLine) ? currentWord : currentTextLine + " " + currentWord;
                    }
                }
            }
            if(!string.IsNullOrEmpty(currentTextLine)) {
                SetText(textObjectIndex++, currentTextLine);
            }
        }
        else {
            SetText(textObjectIndex++, text);
        }
        OffsetText(textObjectIndex, offsetX, offsetY);
        return textObjectIndex;
    }

    private string SafeSubstring(string word, int index, int size) {
        if(word.Length < index + size) {
            return word.Substring(index);
        }
        return word.Substring(index, size);
    }

    private void SetText(int textObjectIndex, string currentTextLine) {
        if(textObjectIndex < textObject.Length) {
            textObject[textObjectIndex].SetText(currentTextLine);
        }
    }

    private void OffsetText(int totalLines, int offsetX, int offsetY) {
        for(int index = 0; index < textObject.Length; ++index) {
            Vector3 position = m_textPosition;
            Transform gameObjectTransform = textObject[index].gameObject.transform;
            if(position == default(Vector3)) {
                position = gameObjectTransform.localPosition;
                m_textPosition = position;
            }
            position.x += offsetX * 1/32f;
            position.y += offsetY * 1/32f + 11/32f * (totalLines - index - 1);
            gameObjectTransform.localPosition = position;
        }
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
