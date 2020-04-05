using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsperetaTextObject : MonoBehaviour
{
    public GameObject m_textOverlay, m_textUnderlay;
    public bool m_isCanvasUI;
    string m_currentText;

    TMP_Text m_tmpOverlay, m_tmpUnderlay;
    string ASPERETA_TEXT_GUIDE = "!\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~";

    void Awake()
    {
        if (m_isCanvasUI) {
            if (m_textOverlay != null) {
                m_tmpOverlay = m_textOverlay.GetComponent<TextMeshProUGUI>();
            }
            if (m_textUnderlay != null) {
                m_tmpUnderlay = m_textUnderlay.GetComponent<TextMeshProUGUI>();
            }
        }
        else {
            if (m_textOverlay != null) {
                m_tmpOverlay = m_textOverlay.GetComponent<TextMeshPro>();
            }
            if (m_textUnderlay != null) {
                m_tmpUnderlay = m_textUnderlay.GetComponent<TextMeshPro>();
            }
        }
    }

    public void SetTextColor(Color color) {
        if (m_tmpOverlay != null) {
            m_tmpOverlay.color = color;
        }
    }

    public void SetText(string text) {
        string asperetaText = ConvertToAsperetaText(text);
        m_currentText = text;
        if (m_tmpOverlay != null) {
            m_tmpOverlay.text = asperetaText;
        }
        if (m_tmpUnderlay != null) {
            m_tmpUnderlay.text = asperetaText;
        }
    }

    public void SetText(int value) {
        SetText(value.ToString());
    }

    public string GetText() {
        return m_currentText;
    }

    string ConvertToAsperetaText(string text) {
        string asperetaText = "";
        if(!string.IsNullOrEmpty(text)) {
            foreach (char c in text)
            {
                if(c.Equals(' ')) {
                    asperetaText+=c;
                }
                else {
                    int characterIndex = ASPERETA_TEXT_GUIDE.IndexOf(c);
                    if(characterIndex < 0) {
                        characterIndex = 30;
                    }
                    asperetaText+="<sprite="+characterIndex+" tint=1>";
                }
            }
        }
        return asperetaText;
    }
}
