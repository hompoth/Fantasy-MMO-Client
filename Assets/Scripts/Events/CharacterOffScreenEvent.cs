using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterOffScreenEvent : Event
{
	public override void Run(GameManager manager, string message) {
		Int32.TryParse(message, out int playerId);
		manager.RemovePlayer(playerId);
	}
}
