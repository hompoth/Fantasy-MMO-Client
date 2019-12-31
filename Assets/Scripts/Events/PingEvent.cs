using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PingEvent : Event
{
	public override void Run(GameManager manager, string message) {
		manager.SendMessageToServer(Packet.Pong());
	}
}
