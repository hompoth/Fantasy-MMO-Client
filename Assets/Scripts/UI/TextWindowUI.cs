using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextWindowUI : WindowUI
{
    public AsperetaTextObject m_title;
    public AsperetaTextObject[] m_textLines;

    void Start() {
        m_title.SetTextColor(AsperetaTextColor.white);
        foreach(AsperetaTextObject textObject in m_textLines) {
            textObject.SetTextColor(AsperetaTextColor.white);
        }
    }

    public override void SetTitle(string title) {
        m_title.SetText(title);
    }

    public override void SetSlotDescription(int index, string description) {
        index--;
        if(0 <= index && index < m_textLines.Length) {
            m_textLines[index].SetText(description);
        }
    }
}