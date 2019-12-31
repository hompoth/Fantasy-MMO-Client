using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthManaEvent : Event
{
	public override void Run(GameManager manager, string message) {
		string[] tokens = message.Split(',');
		int index = 0;

		TryGetTokenValue(tokens, ref index, out int playerId);
		TryGetTokenValue(tokens, ref index, out int hpPercent);
		TryGetTokenValue(tokens, ref index, out int mpPercent);
		manager.SetPlayerHPMP(playerId, hpPercent, mpPercent);
	}
}
