using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowWindowEvent : Event
{
	public override void Run(GameManager manager, string message) {
		Int32.TryParse(message, out int windowId);
		PlayerState.ShowWindow(windowId);
	}
}
