using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorMessageEvent : Event
{
	public override void Run(GameManager manager, string message) {
		int messageType = 0;

		Int32.TryParse(message.Substring(0, 1), out messageType);
		string chatMessage = message.Substring(1);
		manager.AddColorChatMessage(messageType, chatMessage);
	}
}
