using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PacketExtensions
{
    static void Send(GameManager manager, string packet) {
        manager?.SendMessageToServer(packet);
    }
    public static void SendLogin(this GameManager manager, string username, string password, string version) {
        Send(manager, "LOGIN" + username + "," + password + "," + version);
    }
    public static void SendPong(this GameManager manager) {
        Send(manager, "PONG");
    }
    public static void SendLoginContinued(this GameManager manager) {
		Send(manager, "LCNT");
	}

    public static void SendDoneLoadingMap(this GameManager manager, int mapId) {
        Send(manager, "DLM" + mapId);
    }

    public static void SendFace(this GameManager manager, FacingDirection direction) {
        Send(manager, "F" + EnumHelper.GetNameValue<FacingDirection>(direction));
    }

    public static void SendMove(this GameManager manager, MovingDirection direction) {
        Send(manager, "M" + EnumHelper.GetNameValue<MovingDirection>(direction));
    }

    public static void SendAttack(this GameManager manager) {
        Send(manager, "ATT");
    }

    public static void SendPickUp(this GameManager manager) {
        Send(manager, "GET");
    }

    public static void SendChatMessage(this GameManager manager, string message) {
        Send(manager, ";" + message + " ");
    }

    public static void SendChatCommand(this GameManager manager, string message) {
        Send(manager, message);
    }

    public static void SendUse(this GameManager manager, int index) {
        Send(manager, "USE" + index);
    }

    public static void SendCast(this GameManager manager, int index, int playerId) {
        Send(manager, "CAST" + index + "," + playerId);
    }

    public static void SendDrop(this GameManager manager, int index, int amount) {
        Send(manager, "DRP" + index + "," + amount);
    }

    public static void SendDropFromWindow(this GameManager manager, int windowId, int index, int amount) {
        Send(manager, "DWI" + windowId + "," + index + "," + amount);
    }

    public static void SendDiscardItem(this GameManager manager, int index) {
        Send(manager, "DITM" + index);
    }

    public static void SendDiscardSpell(this GameManager manager, int index) {
        Send(manager, "DSPL" + index);
    }

    public static void SendSplit(this GameManager manager, int index, int newIndex) {
        Send(manager, "SPLIT" + index + "," + newIndex);
    }

    public static void SendChange(this GameManager manager, int index, int newIndex) {
        Send(manager, "CHANGE" + index + "," + newIndex);
    }

    public static void SendSwap(this GameManager manager, int index, int newIndex) {
        Send(manager, "SWAP" + index + "," + newIndex);
    }

    public static void SendLeftClick(this GameManager manager, int x, int y) {
        Send(manager, "LC" + x + "," + y);
    }

    public static void SendRightClick(this GameManager manager, int x, int y) {
        Send(manager, "RC" + x + "," + y);
    }

    public static void SendTradeWith(this GameManager manager, int x, int y) {
        Send(manager, "TRADEWITH" + x + "," + y);
    }

    public static void SendInventoryToWindow(this GameManager manager, int index, int windowId, int newIndex) {
        Send(manager, "ITW" + index + "," + windowId + "," + newIndex);
    }

    public static void SendWindowToWindow(this GameManager manager, int windowId, int index, int newWindowId, int newIndex) {
        Send(manager, "WTW" + windowId + "," + index + "," + newWindowId + "," + newIndex);
    }

    public static void SendWindowToInventory(this GameManager manager, int index, int windowId, int newIndex) {
        Send(manager, "WTI" + index + "," + windowId + "," + newIndex);
    }

    public static void SendInventorySellToVendor(this GameManager manager, int npcId, int index, int amount) {
        Send(manager, "VSI" + npcId + "," + index + "," + amount);
    }

    public static void SendWindowSellToVendor(this GameManager manager, int npcId, int windowId, int index, int amount) {
        Send(manager, "VSW" + npcId + "," + windowId + "," + index + "," + amount);
    }

    public static void SendVendorPurchaseToInventory(this GameManager manager, int npcId, int index) {
        Send(manager, "VPI" + npcId + "," + index);
    }

    public static void SendAddTradeItem(this GameManager manager, int newIndex, int index) {
        Send(manager, "TRADEADD" + newIndex + "," + index);
    }

    public static void SendDeleteTradeItem(this GameManager manager, int index) {
        Send(manager, "TRADEDEL" + index);
    }

    public static void SendTradeAccept(this GameManager manager) {
        Send(manager, "TRADEACCEPT");
    }

    public static void SendTradeCancel(this GameManager manager) {
        Send(manager, "TRADECANCEL");
    }

    public static void SendToggleTrade(this GameManager manager) {
        Send(manager, "/TOGGLETRADE");
    }

    public static void SendHelp(this GameManager manager) {
        Send(manager, "/help");
    }

    public static void SendOpenCombineBag(this GameManager manager) {
        Send(manager, "OCB");
    }

    public static void SendItemInfo(this GameManager manager, int slotId) {
        Send(manager, "GID" + slotId);
    }

    public static void SendSpellInfo(this GameManager manager, int slotId) {
        Send(manager, "SID" + slotId);
    }

    public static void SendKillBuff(this GameManager manager, int index) {
        Send(manager, "KBUF" + index);
    }

    public static void SendEmote(this GameManager manager, int emote) {
        Send(manager, "EMOT" + emote);
    }

    public static void SendWindowButtonClicked(this GameManager manager, ButtonType buttonType, int windowId, int npcId, int unknown, int unknown2) {
        Send(manager, "WBC" + EnumHelper.GetNameValue<ButtonType>(buttonType) + "," + windowId + "," + npcId + "," + unknown + "," + unknown2);
    }
}
