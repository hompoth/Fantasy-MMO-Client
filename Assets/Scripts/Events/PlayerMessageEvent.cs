using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMessageEvent : Event
{
	public override void Run(GameManager manager, string message) {
		int commaIndex = message.IndexOf(",");
		Int32.TryParse(message.Substring(0, commaIndex), out int playerId);
		string chatMessage = message.Substring(commaIndex + 1);
		manager.AddPlayerChatMessage(playerId, chatMessage);
	}
}
