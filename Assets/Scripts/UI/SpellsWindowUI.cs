using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellsWindowUI : WindowUI
{
    public SlotUI[] m_spellSlots;

    void Awake() {
        for (int index = 0; index < m_spellSlots.Length; ++index) {
            m_spellSlots[index].SetSlotIndex(index + 1);
        }
    }

    public SlotUI GetSlot(int index) {
        index--;
        if(0 <= index && index < m_spellSlots.Length) {
            return m_spellSlots[index];
        }
        return null;
    }

    public override void Copy(WindowUI window) {
        for (int index = 0; index < m_spellSlots.Length; ++index) {
            m_spellSlots[index].SetSlotIndex(index + 1);
        }
    }
 
    public override void SetSlotId(int index, int spellId) {
        index--;
        if(0 <= index && index < m_spellSlots.Length) {
            m_spellSlots[index].SetSlotId(spellId);
        }
    }
    
    public override void SetSlotDescription(int index, string spellName) {
        index--;
        if(0 <= index && index < m_spellSlots.Length) {
            m_spellSlots[index].SetSlotName(spellName);
        }
    }
    
    public override void UpdateSlotGraphic(int index, int graphicId, Color color = default(Color)) {
        index--;
        if(0 <= index && index < m_spellSlots.Length) {
            m_spellSlots[index].UpdateSlotGraphic(graphicId, color);
        }
    }

    public void SetSoundId(int index, int soundId) {
        index--;
        if(0 <= index && index < m_spellSlots.Length) {
            m_spellSlots[index].SetSoundId(soundId);
        }
    }

    public void SetSpellTarget(int index, string spellTarget) {
        index--;
        if(0 <= index && index < m_spellSlots.Length) {
            m_spellSlots[index].SetSpellTarget(spellTarget);
        }
    }
}
