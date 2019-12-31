using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombineWindowUI : WindowUI
{
    public SlotUI[] m_itemSlots;
    public ButtonUI m_combineButton, m_okayButton;

    void Awake() {
        for (int index = 0; index < m_itemSlots.Length; ++index) {
            m_itemSlots[index].SetSlotIndex(index + 1);
        }
    }

    public override void AddButtons(bool combineEnabled, bool closeEnabled, bool backEnabled, bool nextEnabled, bool okEnabled) {
        if(combineEnabled) {
            m_combineButton.gameObject.SetActive(true);
        }
        if(okEnabled) {
            m_okayButton.gameObject.SetActive(true);
        }
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
