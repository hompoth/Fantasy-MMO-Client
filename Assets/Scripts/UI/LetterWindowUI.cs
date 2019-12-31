using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LetterWindowUI : WindowUI
{
    public AsperetaTextObject[] m_textLines;
    public ButtonUI m_closeButton, m_okayButton;

    void Start() {
        foreach(AsperetaTextObject textObject in m_textLines) {
            textObject.SetTextColor(AsperetaTextColor.orange);
        }
    }

    public override void AddButtons(bool combineEnabled, bool closeEnabled, bool backEnabled, bool nextEnabled, bool okEnabled) {
        if(closeEnabled) {
            m_closeButton.gameObject.SetActive(true);
        }
        if(okEnabled) {
            m_okayButton.gameObject.SetActive(true);
        }
    }

    public override void SetSlotDescription(int index, string description) {
        index--;
        if(0 <= index && index < m_textLines.Length) {
            m_textLines[index].SetText(description);
        }
    }
}
