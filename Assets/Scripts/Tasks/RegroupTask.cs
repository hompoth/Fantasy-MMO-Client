using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MapTile = System.Tuple<int, int, int>;
using WarpDevice = System.Tuple<string, int>;

public class RegroupTask : AutoTask
{
	public override bool IsActive(GameManager gameManager, AutoControllerState state) {
        // If in group &&
        // -- If a member of your group has been gone for 5 seconds or more (customizable)
        // -- -- If leader go to waypoint, otherwise go to leader. Leader is the first gamemanager in list
		return !IsSurrounded(gameManager, state);
	}
	public override void Move(GameManager gameManager, AutoControllerState state) {
        MapTile goal = getTargetLocation(gameManager);
        MoveToTile(gameManager, state, goal);
	}

    private MapTile getTargetLocation(GameManager gameManager) {
        GameManager currentGameManager = ClientManager.GetCurrentGameManager();
        if(gameManager.Equals(currentGameManager)) {
            // TODO go to waypoint instead
            gameManager.GetMainPlayerPosition(out int map, out int x, out int y);
            return Tuple.Create(map, x, y);
        }
        else {
            currentGameManager.GetMainPlayerPosition(out int map, out int x, out int y);
            return Tuple.Create(map, x, y);
        }
    }
}
