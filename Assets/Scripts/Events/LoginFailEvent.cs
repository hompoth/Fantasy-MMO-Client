using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoginFailEvent : Event
{
	public override void Run(GameManager manager, string message) {
		ClientManager.ConnectionIssue(message);
	}
}
