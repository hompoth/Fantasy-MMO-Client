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

    public void UseButton() {
        ButtonType buttonType = GetButtonType();
        switch(buttonType) {
            case ButtonType.Close:
            case ButtonType.Combine:
            case ButtonType.unknown:
            case ButtonType.Back:
            case ButtonType.Next:
                WindowUI window = GetWindow();
                int windowId = window.GetWindowId();
                int npcId = window.GetNPCId();
                int unknown = window.GetUnknownId();
                int unknown2 = window.GetUnknown2Id();
                GameManager.instance?.SendMessageToServer(Packet.WindowButtonClicked(buttonType, windowId, npcId, unknown, unknown2));
                if(buttonType.Equals(ButtonType.Close) || buttonType.Equals(ButtonType.Back) || buttonType.Equals(ButtonType.Next)) {
                    GameManager.instance?.RemoveWindow(windowId);
                }
                break;
            case ButtonType.TradeAccept:
                GameManager.instance?.SendMessageToServer(Packet.TradeAccept());
                break;
            case ButtonType.TradeCancel:
                GameManager.instance?.SendMessageToServer(Packet.TradeCancel());
                break;
            case ButtonType.PickUp:
                GameManager.instance?.SendMessageToServer(Packet.PickUp());
                break;
            case ButtonType.ChatText:
                PlayerState.ToggleChatWindow();
                break;
            case ButtonType.Help:
                GameManager.instance?.SendMessageToServer(Packet.Help());
                break;
            case ButtonType.CombineBag:
                GameManager.instance?.SendMessageToServer(Packet.OpenCombineBag());
                break;
            case ButtonType.Inventory:
                PlayerState.ToggleInventory();
                break;
            case ButtonType.ToggleTrade:
                GameManager.instance?.SendMessageToServer(Packet.ToggleTrade());
                break;
            case ButtonType.Spellbook:
                PlayerState.ToggleSpellsWindow();
                break;
            case ButtonType.Exit:
                GameManager.instance.Disconnect();
                break;
        }
    }
}
