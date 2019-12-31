﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDropEvent : Event
{
	public override void Run(GameManager manager, string message) {
		string[] tokens = message.Split(',');
		int index = 0;

		TryGetTokenValue(tokens, ref index, out int spriteId);
		TryGetTokenValue(tokens, ref index, out int x);
		TryGetTokenValue(tokens, ref index, out int y);
		TryGetTokenValue(tokens, ref index, out string itemName);
		TryGetTokenValue(tokens, ref index, out int count, 1);
		TryGetTokenValue(tokens, ref index, out Color itemColor);
		manager.LoadItemDrop(spriteId, x, y, itemName, count, itemColor);
	}
}