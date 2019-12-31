using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessageEvent : Event
{
	public override void Run(GameManager manager, string message) {
		manager.AddColorChatMessage(1, message);
	}
}
