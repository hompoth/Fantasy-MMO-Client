using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffInfoEvent : Event
{
	public override void Run(GameManager manager, string message) {
		string[] tokens = message.Split(',');
		int index = 0;
		
		TryGetTokenValue(tokens, ref index, out int slotIndex);
		TryGetTokenValue(tokens, ref index, out int spellId);
		TryGetTokenValue(tokens, ref index, out string spellName);
		manager.UpdateBuffSlot(slotIndex, spellName, spellId);
	}
}
