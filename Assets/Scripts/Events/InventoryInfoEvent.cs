using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryInfoEvent : Event
{
	public override void Run(GameManager manager, string message) {
		string[] tokens = message.Split(',');
		int index = 0;

		TryGetTokenValue(tokens, ref index, out int slotIndex);
		TryGetTokenValue(tokens, ref index, out int itemId);
		TryGetTokenValue(tokens, ref index, out string itemName);
		TryGetTokenValue(tokens, ref index, out int amount, 1);
		TryGetTokenValue(tokens, ref index, out int itemSlotId);
		TryGetTokenValue(tokens, ref index, out Color itemSlotColor);
		manager.UpdateItemSlot(slotIndex, itemId, itemName, amount, itemSlotId, itemSlotColor);
	}
}
