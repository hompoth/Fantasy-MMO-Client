using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class IdleTask : AutoTask
{
	public async override Task<bool> IsActive(GameManager gameManager, PathManager pathManager, AutoControllerState state) {
		await Task.Yield();
        return !IsSurrounded(gameManager, state);
	}
	public override async Task Move(GameManager gameManager, PathManager pathManager, AutoControllerState state) { 
        await Task.Yield();
    }
}
