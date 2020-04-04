using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandBarUI : WindowUI
{
    public SlotUI[] m_slots;

    new void Awake() {
        base.Awake();
        for (int index = 0; index < m_slots.Length; ++index) {
            m_slots[index].SetSlotIndex(index + 1);
        }
    }

    public void LoadDefaults(PlayerState state) {
        string playerName = state.GetMainPlayerName();
        for (int index = 0; index < m_slots.Length; ++index) {
            if(UserPrefs.HasCommandBarIndex(index, playerName)) {
                int referenceIndex = UserPrefs.GetCommandBarReferenceIndex(index, playerName);
                WindowType referenceWindowType = UserPrefs.GetCommandBarReferenceWindowType(index, playerName);
                SlotUI slot = state.GetWindowSlot(referenceWindowType, referenceIndex);
                if(slot != null) {
                    m_slots[index].CopySlot(slot, true);
                }
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
    
    public void SwapSlot(int index, int newIndex, string playerName) {
        index--;
        newIndex--;
        if(0 <= index && index < m_slots.Length && 0 <= newIndex && newIndex < m_slots.Length) {
            m_slots[index].SwapWithSlot(m_slots[newIndex]);
            UpdateSlotPrefs(index, m_slots[index], playerName);
            UpdateSlotPrefs(newIndex, m_slots[newIndex], playerName);
        }
    }

    public void CopySlot(int index, SlotUI slot, string playerName) {
        if(slot != null) {
            index--;
            if(0 <= index && index < m_slots.Length) {
                m_slots[index].CopySlot(slot, true);
                UpdateSlotPrefs(index, m_slots[index], playerName);
            }
        }
        else {
            ClearSlot(index, playerName);
        }
    }

    void UpdateSlotPrefs(int index, SlotUI slot, string playerName) {
        Debug.Log("UpdateSlotPrefs");
        if(slot != null && slot.GetSlotGraphicId() > 0) {
            WindowType referenceWindowType = slot.GetReferenceWindow().GetWindowType();
            int referenceIndex = slot.GetReferenceIndex();
            UserPrefs.SetCommandBarReferenceIndex(index, referenceIndex, playerName);
            UserPrefs.SetCommandBarReferenceWindowType(index, referenceWindowType, playerName);
        }
        else {
            ClearSlot(index + 1, playerName);
        }
    }

    public void ClearSlot(int index, string playerName) {
        index--;
        if(0 <= index && index < m_slots.Length) {
            m_slots[index].ClearSlot();
            UserPrefs.ClearCommandBarIndex(index, playerName);
        }
    }
}
