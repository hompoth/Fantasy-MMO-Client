using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeeInvisEvent : Event
{
	public override void Run(GameManager manager, string message) {
		Int32.TryParse(message, out int canSeeInvisNum);
		bool canSeeInvis = false;

		if(canSeeInvisNum == 1) {
			canSeeInvis = true;
		}
		manager.SetMainPlayerCanSeeInvisible(canSeeInvis);
	}
}
