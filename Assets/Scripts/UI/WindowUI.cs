using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public enum WindowType { OptionsBar=1, InventoryWindow, SpellsWindow, CommandBar, BuffBar, FpsBar, HealthBar,
	ManaBar, SpiritBar, ExperienceBar, CharacterWindow, ChatWindow, VendorWindow, PartyWindow, Combine2,
	Combine4, Combine6, Combine8, Combine10, QuestWindow, LargeQuestWindow, TextWindow, DiscardButton, 
	LetterWindow, TradeWindow }

public abstract class WindowUI : MonoBehaviour
{
    public WindowType m_windowType;
    private int m_windowId, m_npcId, m_unknownId, m_unknown2Id;

    public void MouseOver(Vector3 worldPosition) {

    }

    public void MouseEnter(Vector3 worldPosition) {
        
    }

    public void MouseExit(Vector3 worldPosition) {

    }

    public WindowType GetWindowType() {
        return m_windowType;
    }

    public int GetWindowId() {
        return m_windowId;
    }

    public void SetWindowId(int windowId) {
        m_windowId = windowId;
    }

    public int GetNPCId() {
        return m_npcId;
    }

    public void SetNPCId(int npcId) {
        m_npcId = npcId;
    }

    public int GetUnknownId() {
        return m_unknownId;
    }

    public void SetUnknownId(int unknown) {
        m_unknownId = unknown;
    }

    public int GetUnknown2Id() {
        return m_unknown2Id;
    }

    public void SetUnknown2Id(int unknown) {
        m_unknown2Id = unknown;
    }

    public virtual void Copy(WindowUI window) {}

    public virtual void SetTitle(string title) {}

    public virtual void AddButtons(bool combineEnabled, bool closeEnabled, bool backEnabled, bool nextEnabled, bool okEnabled) {}

    public virtual void SetSlotId(int index, int slotId) {}

    public virtual void SetSlotDescription(int index, string description) {}

    public virtual void SetSlotAmount(int index, int amount) {}

    public virtual void UpdateSlotGraphic(int index, int graphicId, Color slotColor) {}
}
