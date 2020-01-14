using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CycleMapsTask : AutoTask
{
	public override bool IsActive(GameManager gameManager, AutoControllerState state) {
		return !IsSurrounded(gameManager, state);
	}
	public override void Move(GameManager gameManager, AutoControllerState state) {

	}
}
