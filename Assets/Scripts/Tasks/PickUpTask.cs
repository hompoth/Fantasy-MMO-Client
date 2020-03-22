using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class PickUpTask : AutoTask
{
	public override bool IsActive(GameManager gameManager, PathManager pathManager, AutoControllerState state) {
		return !IsSurrounded(gameManager, state);
	}
	public override async Task Move(GameManager gameManager, PathManager pathManager, AutoControllerState state) {
        await Task.Yield();
	}
}
