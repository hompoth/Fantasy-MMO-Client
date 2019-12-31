using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VendorWindowUI : WindowUI
{
    public AsperetaTextObject m_title;
    public ButtonUI m_closeButton;
    public SlotUI[] m_itemSlots;

    void Awake() {
        for (int index = 0; index < m_itemSlots.Length; ++index) {
            m_itemSlots[index].SetSlotIndex(index + 1);
        }
    }

    void Start() {
        m_title.SetTextColor(AsperetaTextColor.white);
    }

    public override void AddButtons(bool combineEnabled, bool closeEnabled, bool backEnabled, bool nextEnabled, bool okEnabled) {
        if(closeEnabled) {
            m_closeButton.gameObject.SetActive(true);
        }
    }

    public override void SetTitle(string title) {
        m_title.SetText(title);
    }

    public override void SetSlotId(int index, int itemId) {
        index--;
        if(0 <= index && index < m_itemSlots.Length) {
            m_itemSlots[index].SetSlotId(itemId);
        }
    }
    
    public override void SetSlotDescription(int index, string itemName) {
        index--;
        if(0 <= index && index < m_itemSlots.Length) {
            m_itemSlots[index].SetSlotName(itemName);
        }
    }

    public override void SetSlotAmount(int index, int amount) {
        index--;
        if(0 <= index && index < m_itemSlots.Length) {
            m_itemSlots[index].SetSlotAmount(amount);
        }
    }
    
    public override void UpdateSlotGraphic(int index, int graphicId, Color color) {
        index--;
        if(0 <= index && index < m_itemSlots.Length) {
            m_itemSlots[index].UpdateSlotGraphic(graphicId, color);
        }
    } 
}
