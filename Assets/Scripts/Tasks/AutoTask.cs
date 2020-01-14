using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AutoTask
{
	public abstract bool IsActive(GameManager gameManager, AutoControllerState state);
	public abstract void Move(GameManager gameManager, AutoControllerState state);

	protected bool IsSurrounded(GameManager gameManager, AutoControllerState state) {
		return state.IsSurrounded(gameManager);
	}
}
