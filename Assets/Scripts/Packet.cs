using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Packet
{
    public static string Login(string username, string password, string version) {
        return "LOGIN" + username + "," + password + "," + version;
    }
    public static string Pong() {
        return "PONG";
    }
    public static string LoginContinued() {
		return "LCNT";
	}

    public static string DoneLoadingMap(int mapId) {
        return "DLM" + mapId;
    }

    public static string Face(FacingDirection direction) {
        return "F" + EnumHelper.GetNameValue<FacingDirection>(direction);
    }

    public static string Move(MovingDirection direction) {
        return "M" + EnumHelper.GetNameValue<MovingDirection>(direction);
    }

    public static string Attack() {
        return "ATT";
    }

    public static string PickUp() {
        return "GET";
    }

    public static string ChatMessage(string message) {
        return ";" + message;
    }

    public static string ChatCommand(string message) {
        return message;
    }

    public static string Use(int index) {
        return "USE" + index;
    }

    public static string Cast(int index, int playerId) {
        return "CAST" + index + "," + playerId;
    }

    public static string Drop(int index, int amount) {
        return "DRP" + index + "," + amount;
    }

    public static string DropFromWindow(int windowId, int index, int amount) {
        return "DWI" + windowId + "," + index + "," + amount;
    }

    public static string DiscardItem(int index) {
        return "DITM" + index;
    }

    public static string DiscardSpell(int index) {
        return "DSPL" + index;
    }

    public static string Split(int index, int newIndex) {
        return "SPLIT" + index + "," + newIndex;
    }

    public static string Change(int index, int newIndex) {
        return "CHANGE" + index + "," + newIndex;
    }

    public static string Swap(int index, int newIndex) {
        return "SWAP" + index + "," + newIndex;
    }

    public static string LeftClick(int x, int y) {
        return "LC" + x + "," + y;
    }

    public static string RightClick(int x, int y) {
        return "RC" + x + "," + y;
    }

    public static string TradeWith(int x, int y) {
        return "TRADEWITH" + x + "," + y;
    }

    public static string InventoryToWindow(int index, int windowId, int newIndex) {
        return "ITW" + index + "," + windowId + "," + newIndex;
    }

    public static string WindowToWindow(int windowId, int index, int newWindowId, int newIndex) {
        return "WTW" + windowId + "," + index + "," + newWindowId + "," + newIndex;
    }

    public static string WindowToInventory(int index, int windowId, int newIndex) {
        return "WTI" + index + "," + windowId + "," + newIndex;
    }

    public static string InventorySellToVendor(int npcId, int index, int amount) {
        return "VSI" + npcId + "," + index + "," + amount;
    }

    public static string WindowSellToVendor(int npcId, int windowId, int index, int amount) {
        return "VSW" + npcId + "," + windowId + "," + index + "," + amount;
    }

    public static string VendorPurchaseToInventory(int npcId, int index) {
        return "VPI" + npcId + "," + index;
    }

    public static string AddTradeItem(int newIndex, int index) {
        return "TRADEADD" + newIndex + "," + index;
    }

    public static string DeleteTradeItem(int index) {
        return "TRADEDEL" + index;
    }

    public static string TradeAccept() {
        return "TRADEACCEPT";
    }

    public static string TradeCancel() {
        return "TRADECANCEL";
    }

    public static string ToggleTrade() {
        return "/TOGGLETRADE";
    }

    public static string Help() {
        return "/help";
    }

    public static string OpenCombineBag() {
        return "OCB";
    }

    public static string ItemInfo(int slotId) {
        return "GID" + slotId;
    }

    public static string SpellInfo(int slotId) {
        return "SID" + slotId;
    }

    public static string KillBuff(int index) {
        return "KBUF" + index;
    }

    public static string Emote(int emote) {
        return "EMOT" + emote;
    }

    public static string WindowButtonClicked(ButtonType buttonType, int windowId, int npcId, int unknown, int unknown2) {
        return "WBC" + EnumHelper.GetNameValue<ButtonType>(buttonType) + "," + windowId + "," + npcId + "," + unknown + "," + unknown2;
    }
}
