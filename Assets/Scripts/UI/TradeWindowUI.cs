using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO Support updating millions/thousands/ones with buttons
public class TradeWindowUI : WindowUI
{
    public SlotUI[] m_sourceItemSlots;
    public SlotUI[] m_recipientItemSlots;
    public AsperetaTextObject[] m_textObjects;
    public AsperetaTextObject m_title, m_millions, m_thousands, m_ones, m_sourceGold, m_recipientGold;
    public ButtonUI m_closeButton, m_okayButton;
    int m_millionsCount, m_thousandsCount, m_onesCount;

    void Awake() {
        for (int index = 0; index < m_sourceItemSlots.Length; ++index) {
            m_sourceItemSlots[index].SetSlotIndex(index + 1);
        }
        for (int index = 0; index < m_recipientItemSlots.Length; ++index) {
            m_recipientItemSlots[index].SetSlotIndex(index + 1);
        }
    }

    void Start() {
        foreach(AsperetaTextObject textObject in m_textObjects) {
            textObject.SetTextColor(AsperetaTextColor.white);
        }
        SetMillionsCount(0);
        SetThousandsCount(0);
        SetOnesCount(0);
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

    public void SetMillionsCount(int count) {
        m_millionsCount = count;
        m_millions.SetText(count);
    }

    public void SetThousandsCount(int count) {
        m_thousandsCount = count;
        m_thousands.SetText(count);
    }

    public void SetOnesCount(int count) {
        m_onesCount = count;
        m_ones.SetText(count);
    }

    public int GetGoldTotal() {
        return m_millionsCount * 1_000_000 + m_thousandsCount * 1_000 + m_onesCount;
    }

    public void SetTradeEnabled(bool updateSource, bool isEnabled) {
        Color color = (isEnabled ? AsperetaTextColor.white : AsperetaTextColor.yellow);
        if(updateSource) {
            for (int index = 0; index < m_sourceItemSlots.Length; ++index) {
                m_sourceItemSlots[index].SetSlotEnabled(isEnabled);
            }
            m_sourceGold.SetTextColor(color);
        }
        else {
            for (int index = 0; index < m_recipientItemSlots.Length; ++index) {
                m_recipientItemSlots[index].SetSlotEnabled(isEnabled);
            }
            m_recipientGold.SetTextColor(color);
        }
    }

    public void SetTradeGold(bool updateSource, int goldAmount) {
        if(updateSource) {
            m_sourceGold.SetText(goldAmount);
        }
        else {
            m_recipientGold.SetText(goldAmount);
        }
    }

    public void SetTradeSlotId(bool updateSource, int index, int itemId) {
        index--;
        if(updateSource) {
            if(0 <= index && index < m_sourceItemSlots.Length) {
                m_sourceItemSlots[index].SetSlotId(itemId);
            }
        }
        else {
            if(0 <= index && index < m_recipientItemSlots.Length) {
                m_recipientItemSlots[index].SetSlotId(itemId);
            }
        }
    }
    
    public void SetTradeSlotDescription(bool updateSource, int index, string itemName) {
        index--;
        if(updateSource) {
            if(0 <= index && index < m_sourceItemSlots.Length) {
                m_sourceItemSlots[index].SetSlotName(itemName);
            }
        }
        else {
            if(0 <= index && index < m_recipientItemSlots.Length) {
                m_recipientItemSlots[index].SetSlotName(itemName);
            }
        }
    }

    public void SetTradeSlotAmount(bool updateSource, int index, int amount) {
        index--;
        if(updateSource) {
            if(0 <= index && index < m_sourceItemSlots.Length) {
                m_sourceItemSlots[index].SetSlotAmount(amount);
            }
        }
        else {
            if(0 <= index && index < m_recipientItemSlots.Length) {
                m_recipientItemSlots[index].SetSlotAmount(amount);
            }
        }
    }
    
    public void UpdateTradeSlotGraphic(bool updateSource, int index, int graphicId, Color color) {
        index--;
        if(updateSource) {
            if(0 <= index && index < m_sourceItemSlots.Length) {
                m_sourceItemSlots[index].UpdateSlotGraphic(graphicId, color);
            }
        }
        else {
            if(0 <= index && index < m_recipientItemSlots.Length) {
                m_recipientItemSlots[index].UpdateSlotGraphic(graphicId, color);
            }
        }
    }
}
