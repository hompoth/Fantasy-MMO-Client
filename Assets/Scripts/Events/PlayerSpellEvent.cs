using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpellEvent : Event
{
	public override void Run(GameManager manager, string message) {
		string[] tokens = message.Split(',');
		int index = 0;

		TryGetTokenValue(tokens, ref index, out int playerId);
		TryGetTokenValue(tokens, ref index, out int animationId);
		manager.LoadPlayerSpell(playerId, animationId);
	}
}
