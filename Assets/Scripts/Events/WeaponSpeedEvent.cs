using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSpeedEvent : Event
{
	public override void Run(GameManager manager, string message) {	
		if (!Int32.TryParse(message, out int weaponSpeed)) {
			weaponSpeed = 1000;
		}
		manager.SetMainPlayerAttackSpeed(weaponSpeed);
	}
}
