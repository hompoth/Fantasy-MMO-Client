using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartyPlayerUI : MonoBehaviour
{
    public AsperetaTextObject asperetaTextObject;
    public StatBarUI statBar;
    int m_playerId;

    public void SetPlayerId(int playerId) {
        m_playerId = playerId;
    }

    public int GetPlayerId() {
        return m_playerId;
    }

    public void SetHPBar(int hpPercent) {
        statBar.SetHPBar(hpPercent);
    }

    public int GetHPBar() {
        return statBar.GetHPBar();
    }

    public void SetMPBar(int mpPercent) {
        statBar.SetMPBar(mpPercent);
    }

    public int GetMPBar() {
        return statBar.GetMPBar();
    }

    public void SetText(string text) {
        asperetaTextObject.SetText(text);
    }

    public string GetText(string text) {
        return asperetaTextObject.GetText();
    }
}
