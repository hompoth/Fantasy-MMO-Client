using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandBarUI : WindowUI
{
    public SlotUI[] m_slots;

    void Awake() {
        for (int index = 0; index < m_slots.Length; ++index) {
            m_slots[index].SetSlotIndex(index + 1);
        }
    }

    public SlotUI GetSlot(int index) {
        index--;
        if(0 <= index && index < m_slots.Length) {
            return m_slots[index];
        }
        return null;
    }

    public void LoadDefaults() {
        for (int index = 0; index < m_slots.Length; ++index) {
            int referenceIndex = PlayerPrefs.GetInt("CommandBar-ReferenceIndex-" + index, 0);
            int referenceWindowTypeIndex = PlayerPrefs.GetInt("CommandBar-ReferenceWindowType-" + index, 0);
            WindowType referenceWindowType = EnumHelper.GetName<WindowType>(referenceWindowTypeIndex);
            SlotUI slot = PlayerState.GetWindowSlot(referenceWindowType, referenceIndex);
            if(slot != null) {
                m_slots[index].CopySlot(slot, true);
            }
        }
    }

    public void CopySlot(int index, SlotUI slot) {
        if(slot != null) {
            index--;
            if(0 <= index && index < m_slots.Length) {
                m_slots[index].CopySlot(slot, true);
                WindowType referenceWindowType = m_slots[index].GetReferenceWindow().GetWindowType();
                int referenceWindowTypeIndex = EnumHelper.GetIndex<WindowType>(referenceWindowType);
                int referenceIndex = m_slots[index].GetReferenceIndex();
                PlayerPrefs.SetInt("CommandBar-ReferenceIndex-" + index, referenceIndex);
                PlayerPrefs.SetInt("CommandBar-ReferenceWindowType-" + index, referenceWindowTypeIndex);
            }
        }
        else {
            ClearSlot(index);
        }
    }

    public void ClearSlot(int index) {
        index--;
        if(0 <= index && index < m_slots.Length) {
            m_slots[index].ClearSlot();
            PlayerPrefs.DeleteKey("CommandBar-ReferenceIndex-" + index);
            PlayerPrefs.DeleteKey("CommandBar-ReferenceWindowType-" + index);
        }
    }
}
