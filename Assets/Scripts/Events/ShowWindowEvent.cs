using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowWindowEvent : Event
{
	public override void Run(GameManager manager, string message) {
		if(Int32.TryParse(message, out int windowId)) {
			manager.ShowPlayerWindow(windowId);
		}
	}
}
