using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestWindowUI : WindowUI
{
    public AsperetaTextObject m_title;
    public AsperetaTextObject[] m_textLines;
    public ButtonUI m_closeButton, m_backButton, m_nextButton, m_okayButton;

    void Start() {
        m_title.SetTextColor(AsperetaTextColor.white);
        foreach(AsperetaTextObject textObject in m_textLines) {
            textObject.SetTextColor(AsperetaTextColor.white);
        }
    }

    public override void AddButtons(bool combineEnabled, bool closeEnabled, bool backEnabled, bool nextEnabled, bool okEnabled) {
        if(closeEnabled) {
            m_closeButton.gameObject.SetActive(true);
        }
        if(backEnabled) {
            m_backButton.gameObject.SetActive(true);
        }
        if(nextEnabled) {
            m_nextButton.gameObject.SetActive(true);
        }
        if(okEnabled) {
            m_okayButton.gameObject.SetActive(true);
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
