using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartyPlayerUI : WindowUI
{
    public AsperetaTextObject asperetaTextObject;
    public AsperetaStatBar asperetaStatBar;
    int m_playerId;

    public int GetPlayerId() {
        return m_playerId;
    }

    public void SetPlayerId(int playerId) {
        m_playerId = playerId;
    }

    public void SetHPBar(int hpPercent) {
        asperetaStatBar.SetHPBar(hpPercent);
    }

    public void SetMPBar(int mpPercent) {
        asperetaStatBar.SetMPBar(mpPercent);
    }

    public void SetText(string text) {
        asperetaTextObject.SetText(text);
    }
}
