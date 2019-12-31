using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffBarUI : WindowUI
{
    public SlotUI[] m_spellSlots;

    void Awake() {
        for (int index = 0; index < m_spellSlots.Length; ++index) {
            m_spellSlots[index].SetSlotIndex(index + 1);
        }
    }
    
    public void SetSpellName(int index, string spellName) {
        index--;
        if(0 <= index && index < m_spellSlots.Length) {
            m_spellSlots[index].SetSlotName(spellName);
        }
    }
    
    public void UpdateSpellGraphic(int index, int spellSlotId) {
        index--;
        if(0 <= index && index < m_spellSlots.Length) {
            m_spellSlots[index].UpdateSlotGraphic(spellSlotId);
        }
    }

    public override void SetTitle(string title) {}

    public override void AddButtons(bool combineEnabled, bool closeEnabled, bool backEnabled, bool nextEnabled, bool okEnabled) {}

    public override void SetSlotId(int index, int slotId) {}

    public override void SetSlotDescription(int index, string description) {}

    public override void SetSlotAmount(int index, int amount) {}

    public override void UpdateSlotGraphic(int index, int graphicId, Color slotColor) {}
}
