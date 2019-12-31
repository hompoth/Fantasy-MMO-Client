﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupUpdateEvent : Event
{
	public override void Run(GameManager manager, string message) {
		string[] tokens = message.Split(',');
		int index = 0;

		TryGetTokenValue(tokens, ref index, out int partyIndex);
		TryGetTokenValue(tokens, ref index, out int playerId);
		TryGetTokenValue(tokens, ref index, out string name);
		TryGetTokenValue(tokens, ref index, out int level);
		TryGetTokenValue(tokens, ref index, out string className);
		manager.UpdatePartyIndex(partyIndex, playerId, name, level, className);
	}
}