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

    public void LoadDefaults() {
        for (int index = 0; index < m_slots.Length; ++index) {
            int referenceIndex = UserPrefs.GetCommandBarReferenceIndex(index);
            WindowType referenceWindowType = UserPrefs.GetCommandBarReferenceWindowType(index);
            SlotUI slot = PlayerState.GetWindowSlot(referenceWindowType, referenceIndex);
            if(slot != null) {
                m_slots[index].CopySlot(slot, true);
            }
        }
    }

    public SlotUI GetSlot(int index) {
        index--;
        if(0 <= index && index < m_slots.Length) {
            return m_slots[index];
        }
        return null;
    }
    
    public void SwapSlot(int index, int newIndex) {
        index--;
        newIndex--;
        if(0 <= index && index < m_slots.Length && 0 <= newIndex && newIndex < m_slots.Length) {
            m_slots[index].SwapWithSlot(m_slots[newIndex]);
            UpdateSlotPrefs(index, m_slots[index]);
            UpdateSlotPrefs(newIndex, m_slots[newIndex]);
        }
    }

    public void CopySlot(int index, SlotUI slot) {
        if(slot != null) {
            index--;
            if(0 <= index && index < m_slots.Length) {
                m_slots[index].CopySlot(slot, true);
                UpdateSlotPrefs(index, m_slots[index]);
            }
        }
        else {
            ClearSlot(index);
        }
    }

    void UpdateSlotPrefs(int index, SlotUI slot) {
        if(slot != null && slot.GetSlotGraphicId() > 0) {
            WindowType referenceWindowType = slot.GetReferenceWindow().GetWindowType();
            int referenceIndex = slot.GetReferenceIndex();
            UserPrefs.SetCommandBarReferenceIndex(index, referenceIndex);
            UserPrefs.SetCommandBarReferenceWindowType(index, referenceWindowType);
        }
        else {
            ClearSlot(index + 1);
        }
    }

    public void ClearSlot(int index) {
        index--;
        if(0 <= index && index < m_slots.Length) {
            m_slots[index].ClearSlot();
            UserPrefs.ClearCommandBarIndex(index);
        }
    }
}
