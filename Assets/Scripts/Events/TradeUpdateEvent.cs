using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TradeUpdateEvent : Event
{
	public override void Run(GameManager manager, string message) {
		string[] tokens = message.Split(',');
		int index = 0;
		bool updateForSelf;

		updateForSelf = true;
		TryUpdateTradeEnabled(tokens, ref index, manager, updateForSelf);
		for(int slotIndex = 1; slotIndex <= 8; ++slotIndex) {
			TryUpdateTradeSlot(tokens, ref index, manager, updateForSelf, slotIndex);
		}
		TryUpdateTradeGold(tokens, ref index, manager, updateForSelf);
		
		updateForSelf = false;
		TryUpdateTradeEnabled(tokens, ref index, manager, updateForSelf);
		for(int slotIndex = 1; slotIndex <= 8; ++slotIndex) {
			TryUpdateTradeSlot(tokens, ref index, manager, updateForSelf, slotIndex);
		}
		TryUpdateTradeGold(tokens, ref index, manager, updateForSelf);
	}

	private void TryUpdateTradeEnabled(string[] tokens, ref int index, GameManager manager, bool updateForSelf) {
		TryGetTokenValue(tokens, ref index, out bool tradeDisabled);
		manager.UpdateTradeEnabled(updateForSelf, !tradeDisabled);
	}

	private void TryUpdateTradeSlot(string[] tokens, ref int index, GameManager manager, bool updateForSelf, int slotIndex) {
		TryGetTokenValue(tokens, ref index, out string slotInfo);
		string[] slotInfoTokens = slotInfo.Split('|');
		int slotInfoIndex = 0;

		TryGetTokenValue(slotInfoTokens, ref slotInfoIndex, out string itemName);
		TryGetTokenValue(slotInfoTokens, ref slotInfoIndex, out int amount);
		TryGetTokenValue(slotInfoTokens, ref slotInfoIndex, out int graphicId);
		TryGetTokenValue(slotInfoTokens, ref slotInfoIndex, out int slotId);
		TryGetTokenValue(slotInfoTokens, ref slotInfoIndex, out Color slotColor);
		manager.UpdateTradeSlot(updateForSelf, slotIndex, itemName, amount, graphicId, slotId, slotColor);
	}

	private void TryUpdateTradeGold(string[] tokens, ref int index, GameManager manager, bool updateForSelf) {
		TryGetTokenValue(tokens, ref index, out int amount);
		manager.UpdateTradeGold(updateForSelf, amount);
	}
}
