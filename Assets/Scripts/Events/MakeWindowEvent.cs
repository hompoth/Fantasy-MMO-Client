using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakeWindowEvent : Event
{
	public override void Run(GameManager manager, string message) {
		string[] tokens = message.Split(',');
		int index = 0;

		TryGetTokenValue(tokens, ref index, out int windowId);
		TryGetTokenValue<WindowType>(tokens, ref index, out WindowType windowType);
		TryGetTokenValue(tokens, ref index, out string title);
		TryGetTokenValue(tokens, ref index, out bool combineEnabled);
		TryGetTokenValue(tokens, ref index, out bool closeEnabled);
		TryGetTokenValue(tokens, ref index, out bool backEnabled);
		TryGetTokenValue(tokens, ref index, out bool nextEnabled);
		TryGetTokenValue(tokens, ref index, out bool okEnabled);
		TryGetTokenValue(tokens, ref index, out int npcId);
		TryGetTokenValue(tokens, ref index, out int unknown);
		TryGetTokenValue(tokens, ref index, out int unknown2);
		manager.LoadWindow(windowId, windowType, title, combineEnabled, closeEnabled, backEnabled, nextEnabled, okEnabled, npcId, unknown, unknown2);
	}
}
