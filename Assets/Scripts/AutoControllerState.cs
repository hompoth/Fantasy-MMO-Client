using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AutoControllerState
{
    // Cache value
    public bool IsSurrounded(GameManager gameManager) {
		return gameManager.MainPlayerIsSurrounded();
	}
}
