using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public enum ButtonType { Close, Combine, unknown, Back, Next, TradeAccept, TradeCancel, PickUp, ChatText, 
    Help, CombineBag, Inventory, ToggleTrade, Spellbook, Exit }

public class ButtonUI : MonoBehaviour
{
    public ButtonType m_buttonType;
    public WindowUI m_window;

    public WindowUI GetWindow() {
        return m_window;
    }

    public ButtonType GetButtonType() {
        return m_buttonType;
    }
}
