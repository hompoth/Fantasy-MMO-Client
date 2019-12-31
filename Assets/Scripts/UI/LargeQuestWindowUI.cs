using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LargeQuestWindowUI : WindowUI
{
    public AsperetaTextObject m_title;
    public ButtonUI m_closeButton, m_okayButton;

    void Start() {
        m_title.SetTextColor(AsperetaTextColor.white);
    }

    public override void AddButtons(bool combineEnabled, bool closeEnabled, bool backEnabled, bool nextEnabled, bool okEnabled) {
        if(closeEnabled) {
            m_closeButton.gameObject.SetActive(true);
        }
        if(okEnabled) {
            m_okayButton.gameObject.SetActive(true);
        }
    }

    public override void SetTitle(string title) {
        m_title.SetText(title);
    }
}
